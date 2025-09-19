using HarmonyLib;
using System;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(InventoryItemTupleWidget), "UpdateEquipmentTypeState")]
internal static class InventoryItemTupleWidget_UpdateEquipmentTypeStatePatch
{
    internal static event Action<InventoryItemTupleWidget>? OnUpdateEquipmentTypeState;

    public static void Postfix(InventoryItemTupleWidget __instance)
    {
        if (__instance is { ScreenWidget: null })
        {
            return;
        }

        OnUpdateEquipmentTypeState?.Invoke(__instance);
    }
}
