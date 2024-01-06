using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(SPInventoryVM), "UpdateEquipment")]
internal static class SPInventoryVM_UpdateEquipmentPatch
{
    internal static event Action<SPInventoryVM>? OnUpdateEquipment;

    public static void Postfix(SPInventoryVM __instance)
    {
        OnUpdateEquipment?.Invoke(__instance);
    }
}

[HarmonyPatch(typeof(SPInventoryVM), "UpdateCharacterEquipment")]
internal static class SPInventoryVM_UpdateCharacterEquipmentPatch
{
    internal static event Action<SPInventoryVM>? OnUpdateCharacterEquipment;

    public static void Postfix(SPInventoryVM __instance)
    {
        OnUpdateCharacterEquipment?.Invoke(__instance);
    }
}

//[HarmonyPatch(typeof(SPInventoryVM), "UpdateFilteredStatusOfItem")]
//internal static class InventoryItemTupleWidget_UpdateCivilianStatePatch
//{
//    internal static event Action<InventoryItemTupleWidget>? OnUpdateCivilianState;

//    public static void Postfix(SPInventoryVM __instance, SPItemVM item)
//    {
//        if (__instance is { ScreenWidget: null })
//        {
//            return;
//        }

//        OnUpdateCivilianState?.Invoke(__instance);
//    }
//}