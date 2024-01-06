using ButterEquipped.Patches;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.HighlightBetter;

internal class HighlightBetterBehavior : CampaignBehaviorBase, IDisposable
{
    public override void RegisterEvents()
    {
        InventoryItemTupleWidget_UpdateCivilianStatePatch.OnUpdateCivilianState += OnWidgetUpdated;
        SPInventoryVM_UpdateEquipmentPatch.OnUpdateEquipment += SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment;
        SPInventoryVM_UpdateCharacterEquipmentPatch.OnUpdateCharacterEquipment += SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment;
    }

    private void SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment(SPInventoryVM spInventoryVm)
    {
        //switching war set / hero
        SPItemVMMixin.CurrentInventory = spInventoryVm;
    }

    private void SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment(SPInventoryVM spInventoryVm)
    {
        //moving items in or out of current character equipment
        SPItemVMMixin.CurrentInventory = spInventoryVm;
    }

    private void OnWidgetUpdated(InventoryItemTupleWidget widget)
    {
        if (widget is not InventoryItemTupleInterceptWidget interceptWidget)
        {
            return;
        }

        if(!widget.MainContainer.Brush.IsCloneRelated(widget.DefaultBrush))
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
        InventoryItemTupleWidget_UpdateCivilianStatePatch.OnUpdateCivilianState -= OnWidgetUpdated;
        SPInventoryVM_UpdateEquipmentPatch.OnUpdateEquipment -= SPInventoryVM_UpdateEquipmentPatch_OnUpdateEquipment;
        SPInventoryVM_UpdateCharacterEquipmentPatch.OnUpdateCharacterEquipment -= SPInventoryVM_UpdateCharacterEquipmentPatch_OnUpdateCharacterEquipment;
    }
}
