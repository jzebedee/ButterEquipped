using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.Inventory;

namespace ButterEquipped.Patches;

//TaleWorlds.CampaignSystem.Inventory.InventoryManager.CloseInventoryPresentation(bool) : void @060012C4
//// Token: 0x060012C4 RID: 4804 RVA: 0x000522C8 File Offset: 0x000504C8
[HarmonyPatch(typeof(InventoryManager), nameof(InventoryManager.CloseInventoryPresentation))]
internal static class InventoryManager_CloseInventoryPresentationPatch
{
    internal static event Action<bool>? OnClosing;

    public static void Prefix(object __instance, bool fromCancel)
        => OnClosing?.Invoke(fromCancel);
}
