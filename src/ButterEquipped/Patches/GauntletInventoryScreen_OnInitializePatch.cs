using HarmonyLib;
using SandBox.GauntletUI;
using System;

namespace ButterEquipped.Patches;

//SandBox.GauntletUI.GauntletInventoryScreen.OnInitialize() : void @06000062
//// Token: 0x06000062 RID: 98 RVA: 0x0000523C File Offset: 0x0000343C
[HarmonyPatch(typeof(GauntletInventoryScreen), "OnInitialize" /* nameof(GauntletInventoryScreen.OnInitialize) */)]
internal static class GauntletInventoryScreen_OnInitializePatch
{
    internal static event Action<GauntletInventoryScreen>? OnInitialize;

    public static void Postfix(GauntletInventoryScreen __instance)
        => OnInitialize?.Invoke(__instance);
}