using ButterEquipped.Patches;
using System;
using System.Diagnostics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.HighlightBetter;

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
    }

    private void SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment(SPInventoryVM spInventoryVm, SPItemVM spItemVm, EquipmentIndex equipmentIndex)
    {
        Debug.Assert(!_disposed);

        //moving items in or out of current character equipment
        _currentVm.SetTarget(spInventoryVm);
        spInventoryVm.OnPropertyChangedWithValue(spItemVm, equipmentIndex.GetPropertyNameFromIndex());
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
