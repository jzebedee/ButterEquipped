using Bannerlord.UIExtenderEx.Attributes;
using ButterEquipped.AutoEquip;
using ButterEquipped.HighlightBetter;
using HarmonyLib.BUTR.Extensions;
using System;
using System.Diagnostics;
using System.Threading;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ButterEquipped.Patches;
using InventorySide = InventoryLogic.InventorySide;

[ViewModelMixin(nameof(SPItemVM.RefreshValues))]
internal class SPItemVMMixin : TwoWayViewModelMixin<SPItemVM>
{
    private static readonly Func<SPInventoryVM, EquipmentIndex, SPItemVM>? GetItemFromIndex
        = AccessTools2.GetDelegate<Func<SPInventoryVM, EquipmentIndex, SPItemVM>>(typeof(SPInventoryVM), nameof(GetItemFromIndex));

#if DEBUG
    internal static int _totalUpdates;
#endif

    public SPItemVMMixin(SPItemVM vm) : base(vm)
    {
        //we are trying to avoid subscribing to property notifications from the underlying VM
        //to avoid having to deal with weak events or leaking a reference to our mixin class
    }

    public override void OnRefresh()
    {
        Refresh();
        base.OnRefresh();
    }

    public void Refresh()
        => ButterEquippedIsItemBetter = CompareEquipment();

    bool ShouldHighlightSide()
    {
        if (HighlightBetterBehavior.CurrentOptions is not HighlightBetterOptions options)
        {
            return false;
        }

        if (options is { HighlightBetterItems: false })
        {
            return false;
        }

        if (ViewModel is not { InventorySide: var side })
        {
            return false;
        }

        return side switch
        {
            InventorySide.PlayerInventory => options is { HighlightFromInventory: true },
            InventorySide.OtherInventory when InventoryManager.Instance is { CurrentMode: var mode } => ShouldHighlightOtherSide(options, mode),
            _ => true//false
        };

        static bool ShouldHighlightOtherSide(HighlightBetterOptions options, InventoryMode mode)
            => (options, mode) switch
            {
                ({ HighlightFromDiscard: true }, InventoryMode.Default) => true,
                ({ HighlightFromLoot: true }, InventoryMode.Loot) => true,
                ({ HighlightFromStash: true }, InventoryMode.Stash) => true,
                ({ HighlightFromTrade: true }, InventoryMode.Trade) => true,
                _ => false
            };
    }

    bool IsValid()
        => ViewModel switch
        {
            null => false,
            //{ IsNew: true } => false,
            { IsFiltered: true } => false,
            { IsEquipableItem: false } => false,
            { CanCharacterUseItem: false } => false,
            { InventorySide: not InventorySide.PlayerInventory and not InventorySide.OtherInventory } => false,
            _ => true
        };

    private SPItemVM? EquipmentReference
        => (GetItemFromIndex, ViewModel, HighlightBetterBehavior.CurrentVm) switch
        {
            (var getEq and not null, SPItemVM vm, SPInventoryVM inventoryVm) => getEq(inventoryVm, vm.ItemType),
            _ => null
        };

    private bool CompareEquipment()
    {
        if (EquipmentReference is not SPItemVM equippedItem)
        {
            return false;
        }

        return CompareEquipment(equippedItem);
    }

    private bool CompareEquipment(SPItemVM? equippedItem)
    {
        if (!IsValid())
        {
            return false;
        }

        if (!ShouldHighlightSide())
        {
            return false;
        }

        var isWeapon = ViewModel is { ItemType: >= EquipmentIndex.WeaponItemBeginSlot and < EquipmentIndex.NonWeaponItemBeginSlot };
        //ignore weapons, their index is always Weapon0/WeaponExtra (banners) so in the future we need 
        //to fully model the auto equip logic to match them with an appropriate equipped slot
        if (isWeapon)
        {
            return false;
        }

        if (equippedItem is null or { ItemRosterElement.IsEmpty: true })
        {
            return !isWeapon;
        }

        var eqEffectiveness = EquipmentElementComparer.CalculateEffectiveness(equippedItem.ItemRosterElement.EquipmentElement);
        return ButterEquippedScore.CompareTo(eqEffectiveness) > 0;
    }

    [DataSourceProperty]
    public float ButterEquippedScore
        => ViewModel switch
        {
            { ItemRosterElement.EquipmentElement: var eqEl } => EquipmentElementComparer.CalculateEffectiveness(eqEl),
            _ => -1f
        };

    [DataSourceProperty]
    public bool ButterEquippedIsItemBetter
    {
        get => _isItemBetter;
        set
        {
            if (_isItemBetter != value)
            {
                _isItemBetter = value;
                OnPropertyChangedWithValue(value);
                DebugIncrementCount();
            }

            [Conditional("DEBUG")]
            static void DebugIncrementCount() => Interlocked.Increment(ref _totalUpdates);
        }
    }

    private bool _isItemBetter;
}