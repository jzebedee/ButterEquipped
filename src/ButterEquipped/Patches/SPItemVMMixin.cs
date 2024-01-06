using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using ButterEquipped.AutoEquip;
using HarmonyLib.BUTR.Extensions;
using System;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ButterEquipped.Patches;

[ViewModelMixin]
internal class SPItemVMMixin : BaseViewModelMixin<SPItemVM>
{
    private static readonly EquipmentElementComparer _comparer = new();

    private static readonly Func<SPInventoryVM, Equipment>? ActiveEquipment
        = AccessTools2.GetPropertyGetterDelegate<Func<SPInventoryVM, Equipment>>(typeof(SPInventoryVM), nameof(ActiveEquipment));

    private static readonly Func<SPInventoryVM, EquipmentIndex, SPItemVM>? GetItemFromIndex
        = AccessTools2.GetDelegate<Func<SPInventoryVM, EquipmentIndex, SPItemVM>>(typeof(SPInventoryVM), nameof(GetItemFromIndex));

    private static bool IsValid
        => GetItemFromIndex is not null && CurrentInventory is not null;

    private static Action<SPInventoryVM>? OnEquipmentUpdate;
    private static SPInventoryVM? currentInventory;
    internal static SPInventoryVM? CurrentInventory
    {
        get => currentInventory;
        set
        {
            currentInventory = value;
            OnEquipmentUpdate?.Invoke(currentInventory);
        }
    }

    public SPItemVMMixin(SPItemVM vm) : base(vm)
    {
        OnEquipmentUpdate += spInventoryVm => OnPropertyChanged(nameof(ButterEquippedIsItemBetter));
    }

    [DataSourceProperty]
    public bool ButterEquippedIsItemBetter
    {
        get
        {
            if (!IsValid)
            {
                System.Diagnostics.Debug.Fail($"{nameof(SPItemVMMixin)} is invalid");
                return false;
            }

            if (ViewModel is { InventorySide: InventoryLogic.InventorySide.Equipment })
            {
                return false;
            }

            if (ViewModel is not
                {
                    ItemRosterElement.EquipmentElement: var itemEqEl,
                    ItemType: >= EquipmentIndex.WeaponItemBeginSlot and <= EquipmentIndex.NumEquipmentSetSlots
                })
            {
                return false;
            }

            if (GetItemFromIndex!(CurrentInventory, ViewModel.ItemType) is not SPItemVM equippedItem)
            {
                return false;
            }

            return _comparer.Compare(itemEqEl, equippedItem.ItemRosterElement.EquipmentElement) > 0;
        }
    }
}