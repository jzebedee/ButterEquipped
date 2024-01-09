using HarmonyLib;
using System;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(InventoryItemTupleWidget), "UpdateCivilianState")]
internal static class InventoryItemTupleWidget_UpdateCivilianStatePatch
{
    internal static event Action<InventoryItemTupleWidget>? OnUpdateCivilianState;

    public static void Postfix(InventoryItemTupleWidget __instance)
    {
        if (__instance is { ScreenWidget: null })
        {
            return;
        }

        OnUpdateCivilianState?.Invoke(__instance);
    }
}
