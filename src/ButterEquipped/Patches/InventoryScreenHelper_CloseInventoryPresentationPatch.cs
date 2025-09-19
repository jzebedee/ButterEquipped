using HarmonyLib;
using Helpers;
using System;

namespace ButterEquipped.Patches;

[HarmonyPatch(typeof(InventoryScreenHelper), "CloseInventoryPresentation")]
internal static class InventoryScreenHelper_CloseInventoryPresentationPatch
{
    internal static event Action<bool>? OnClosing;

    public static void Prefix(/*object __instance, */bool fromCancel)
        => OnClosing?.Invoke(fromCancel);
}
