using HarmonyLib;
using SandBox.GauntletUI;
using System;
using TaleWorlds.CampaignSystem.Inventory;

namespace ButterEquipped.Patches;

//TaleWorlds.CampaignSystem.Inventory.InventoryManager.CloseInventoryPresentation(bool) : void @060012C4
//// Token: 0x060012C4 RID: 4804 RVA: 0x000522C8 File Offset: 0x000504C8
[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.CloseInventoryPresentation))]
internal static class AutoEquipPatches_InventoryManager_CloseInventoryPresentation
{
    internal static event EventHandler<bool>? OnClosing;

    public static void Prefix(object __instance, bool fromCancel)
        => OnClosing?.Invoke(__instance, fromCancel);
}

//SandBox.GauntletUI.GauntletInventoryScreen.OnInitialize() : void @06000062
//// Token: 0x06000062 RID: 98 RVA: 0x0000523C File Offset: 0x0000343C
[HarmonyPatch(typeof(GauntletInventoryScreen), "OnInitialize" /* nameof(GauntletInventoryScreen.OnInitialize) */)]
internal static class AutoEquipPatches_GauntletInventoryScreen_OnInitialize
{
    internal static event EventHandler? OnInitialize;

    public static void Postfix(object __instance)
        => OnInitialize?.Invoke(__instance, EventArgs.Empty);
}