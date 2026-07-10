using HarmonyLib;
using SandBox.GauntletUI;
using System;

namespace ButterEquipped.Patches;

//SandBox.GauntletUI.GauntletInventoryScreen.OnReady() : void @06000072
// Token: 0x06000072 RID: 114 RVA: 0x000057AC File Offset: 0x000039AC
[HarmonyPatch(typeof(GauntletInventoryScreen), "OnReady" /* nameof(GauntletInventoryScreen.OnReady) */)]
internal static class GauntletInventoryScreen_OnReadyPatch
{
    internal static event Action<GauntletInventoryScreen>? OnReady;

    public static void Postfix(GauntletInventoryScreen __instance)
        => OnReady?.Invoke(__instance);
}