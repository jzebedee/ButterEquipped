using HarmonyLib;
using System;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace ButterEquipped.Patches;
using InventorySide = InventoryLogic.InventorySide;

// Token: 0x06000D56 RID: 3414 RVA: 0x00036514 File Offset: 0x00034714
[HarmonyPatch(typeof(SPItemVM), nameof(SPItemVM.RefreshWith))]
internal static class SPItemVM_RefreshWithPatch
{
    internal static event Action<SPItemVM, SPItemVM, InventorySide>? OnRefreshWith;

    public static void Postfix(SPItemVM __instance, SPItemVM itemVM, InventorySide inventorySide)
    {
        OnRefreshWith?.Invoke(__instance, itemVM, inventorySide);
    }
}
