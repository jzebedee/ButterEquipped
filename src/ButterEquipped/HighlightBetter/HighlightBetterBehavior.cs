﻿using ButterEquipped.Patches;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.HighlightBetter;
using InventorySide = InventoryLogic.InventorySide;

internal class HighlightBetterBehavior : CampaignBehaviorBase, IDisposable
{
    private static HighlightBetterOptions _options;

    private bool _eventsRegistered;
    private bool _disposed;

    private static readonly WeakReference<SPInventoryVM> _currentVm = new(null!);

    public static HighlightBetterOptions CurrentOptions => _options with { }; //shallow record clone

    public static SPInventoryVM? CurrentVm
        => _currentVm.TryGetTarget(out var vm) switch
        {
            true => vm,
            false => null
        };

    [Conditional("DEBUG")]
    static void DebugLog([CallerMemberName] string methodName = "") => System.Diagnostics.Debug.WriteLine("[{0}] {1}", DateTimeOffset.Now, methodName);

    public HighlightBetterBehavior(HighlightBetterOptions options)
    {
        _options = options;
    }

    public override void RegisterEvents()
    {
        if (_eventsRegistered)
        {
            return;
        }

        InventoryItemTupleWidget_UpdateCivilianStatePatch.OnUpdateCivilianState += OnWidgetUpdateCivilianState;
        SPInventoryVM_UpdateEquipmentPatch.OnUpdateEquipment += SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment;
        SPInventoryVM_UpdateCharacterEquipmentPatch.OnUpdateCharacterEquipment += SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment;
        SPItemVM_RefreshWithPatch.OnRefreshWith += SPItemVM_RefreshWithPatch_OnRefreshWith;
        _eventsRegistered = true;
    }

    private void SPItemVM_RefreshWithPatch_OnRefreshWith(SPItemVM instance, SPItemVM itemVM, InventorySide inventorySide)
    {
        Debug.Assert(!_disposed);

        if(instance.GetMixinForVM() is not SPItemVMMixin instanceMixin
        || itemVM.GetMixinForVM() is not SPItemVMMixin itemMixin)
        {
            return;
        }

        instanceMixin.ButterEquippedIsItemBetter = itemMixin.ButterEquippedIsItemBetter;
        DebugLog();
    }

    private void SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment(SPInventoryVM spInventoryVm)
    {
        Debug.Assert(!_disposed);

        //switching war set / hero
        _currentVm.SetTarget(spInventoryVm);
        DebugLog();

        spInventoryVm.LeftItemListVM.ApplyActionOnAllItems(vm => vm.GetMixinForVM()?.Refresh());
        spInventoryVm.RightItemListVM.ApplyActionOnAllItems(vm => vm.GetMixinForVM()?.Refresh());
    }

    private void SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment(SPInventoryVM spInventoryVm, SPItemVM spItemVm, EquipmentIndex equipmentIndex)
    {
        Debug.Assert(!_disposed);

        //moving items in or out of current character equipment
        _currentVm.SetTarget(spInventoryVm);
        DebugLog();

        UpdateItemsOfType(spInventoryVm.LeftItemListVM, equipmentIndex);
        UpdateItemsOfType(spInventoryVm.RightItemListVM, equipmentIndex);

        static void UpdateItemsOfType(TaleWorlds.Library.MBBindingList<SPItemVM> items, EquipmentIndex index)
        {
            foreach (SPItemVM itemVM in items)
            {
                if (itemVM.ItemType == index)
                {
                    var mixin = itemVM.GetMixinForVM();
                    mixin?.Refresh();
                }
            }
        }
    }

    private void OnWidgetUpdateCivilianState(InventoryItemTupleWidget widget)
    {
        Debug.Assert(!_disposed);

        if (widget is not InventoryItemTupleInterceptWidget interceptWidget)
        {
            return;
        }

        if (!widget.MainContainer.Brush.IsCloneRelated(widget.DefaultBrush))
        {
            //can't use / civilian
            return;
        }

        widget.MainContainer.Brush = interceptWidget.IsBetterItem switch
        {
            true => interceptWidget.BetterItemHighlightBrush,
            false => widget.DefaultBrush
        };
    }

    public override void SyncData(IDataStore dataStore)
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_eventsRegistered)
        {
            InventoryItemTupleWidget_UpdateCivilianStatePatch.OnUpdateCivilianState -= OnWidgetUpdateCivilianState;
            SPInventoryVM_UpdateEquipmentPatch.OnUpdateEquipment -= SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment;
            SPInventoryVM_UpdateCharacterEquipmentPatch.OnUpdateCharacterEquipment -= SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment;
            SPItemVM_RefreshWithPatch.OnRefreshWith -= SPItemVM_RefreshWithPatch_OnRefreshWith;
        }

        _disposed = true;
    }
}
