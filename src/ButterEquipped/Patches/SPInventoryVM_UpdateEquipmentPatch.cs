using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(SPInventoryVM), "UpdateEquipment")]
internal static class SPInventoryVM_UpdateEquipmentPatch
{
    internal static event Action<SPInventoryVM, /*SPItemVM,*/ EquipmentIndex>? OnUpdateEquipment;

    public static void Postfix(SPInventoryVM __instance, /*SPItemVM itemVM,*/ EquipmentIndex itemType)
    {
        OnUpdateEquipment?.Invoke(__instance, /*itemVM,*/ itemType);
    }
}