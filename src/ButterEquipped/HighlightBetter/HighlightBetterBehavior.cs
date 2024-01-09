using ButterEquipped.Patches;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.HighlightBetter;
internal class HighlightBetterBehavior : CampaignBehaviorBase, IDisposable
{
    private static readonly WeakReference<SPInventoryVM> _currentVm = new(null!);

    private static HighlightBetterOptions _options;

    public static HighlightBetterOptions CurrentOptions => _options with { }; //shallow record clone

    public static SPInventoryVM? CurrentVm
        => _currentVm.TryGetTarget(out var vm) switch
        {
            true => vm,
            false => null
        };

    [Conditional("DEBUG")]
    static void DebugLog([CallerMemberName] string methodName = "") => System.Diagnostics.Debug.WriteLine("[{0}] {1}", DateTimeOffset.Now, methodName);

    private bool _eventsRegistered;
    private bool _disposed;

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
        _eventsRegistered = true;
    }


    private void SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment(SPInventoryVM spInventoryVm)
    {
        Debug.Assert(!_disposed);

        //switching war set / hero
        _currentVm.SetTarget(spInventoryVm);
        DebugLog();

        //problem:
        // Hero A and Hero B both have no / bad helms
        // open trade full of helms (all green)
        // equip Hero A with best helm
        // trade side helms turn normal (ButterEquippedIsBetterItem: false)
        // switch to Hero B
        // trade side helms don't update, even though they are better

        //solution:
        spInventoryVm.LeftItemListVM.ApplyActionOnAllItems(vm => vm.GetMixinForVM()?.Refresh());
        spInventoryVm.RightItemListVM.ApplyActionOnAllItems(vm => vm.GetMixinForVM()?.Refresh());
    }

    private void SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment(SPInventoryVM spInventoryVm, SPItemVM spItemVm, EquipmentIndex equipmentIndex)
    {
        Debug.Assert(!_disposed);

        //moving items in or out of current character equipment
        _currentVm.SetTarget(spInventoryVm);
        DebugLog();

        //problem:
        // same as OnUpdateCharacterEquipment, except that dragging in or equipping an item
        // won't update items of the same type in the inventory, only the replaced item when it gets added
        
        //we could just refresh the entire item list,
        //but since we know the index we can target only matching item types

        //solution:
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
        }

        _disposed = true;
    }
}
