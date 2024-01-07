using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(SPInventoryVM), "UpdateCharacterEquipment")]
internal static class SPInventoryVM_UpdateCharacterEquipmentPatch
{
    internal static event Action<SPInventoryVM>? OnUpdateCharacterEquipment;

    public static void Postfix(SPInventoryVM __instance)
    {
        OnUpdateCharacterEquipment?.Invoke(__instance);
    }
}