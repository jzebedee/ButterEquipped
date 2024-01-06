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

    //private static readonly AccessTools.FieldRef<SPItemVM, InventoryLogic>? ActiveEquipment
    //    = AccessTools2.FieldRefAccess<SPInventoryVM, Equipment>("ActiveEquipment");

    private static Action<SPInventoryVM>? OnCurrentInventoryChanged;
    private static SPInventoryVM currentInventory;
    internal static SPInventoryVM CurrentInventory
    {
        get => currentInventory;
        set
        {
            //if (currentInventory != value)
            {
                currentInventory = value;
                OnCurrentInventoryChanged?.Invoke(currentInventory);
            }
        }
    }

    public SPItemVMMixin(SPItemVM vm) : base(vm)
    {
        //piggyback off of SPInventoryVM.RefreshCharacterCanUseItem()
        //ViewModel.PropertyChanged += (sender, e) =>
        //{
        //    if (e.PropertyName is not nameof(ViewModel.CanCharacterUseItem))
        //    {
        //        return;
        //    }

        //    ;
        //};
        OnCurrentInventoryChanged += spInventoryVm =>
        {
            spInventoryVm.PropertyChanged += (sender, e) =>
            {
                if(e.PropertyName is not nameof(SPInventoryVM.IsInWarSet))
                {
                    return;
                }
                OnPropertyChanged(nameof(ButterEquippedIsItemBetter));

            };

            OnPropertyChanged(nameof(ButterEquippedIsItemBetter));
        };
        //ViewModel.PropertyChangedWithBoolValue += ViewModel_PropertyChangedWithBoolValue;
    }

    private void ViewModel_PropertyChangedWithBoolValue(object sender, PropertyChangedWithBoolValueEventArgs e)
    {
        if (e.PropertyName is not nameof(ViewModel.IsEquipableItem))
        {
            return;
        }

        OnPropertyChanged(nameof(ButterEquippedIsItemBetter));
    }

    [DataSourceProperty]
    public bool ButterEquippedIsItemBetter
    {
        get
        {
            if (ViewModel is { InventorySide: InventoryLogic.InventorySide.Equipment })
            {
                return false;
            }

            if (ViewModel is not { ItemRosterElement.EquipmentElement: var itemEqEl })
            {
                return false;
            }

            if (ViewModel.ItemType is not >= EquipmentIndex.WeaponItemBeginSlot and <= EquipmentIndex.NumEquipmentSetSlots)
            {
                return false;
            }

            //if (CurrentInventory switch
            //{
            //    SPInventoryVM vm => ActiveEquipment(vm),
            //    null => InventoryManager.InventoryLogic.InitialEquipmentCharacter.Equipment
            //} is not Equipment currentEq)
            //{
            //    return false;
            //}
            if (CurrentInventory is not SPInventoryVM inventoryVm)
            {
                return false;
            }

            var otherType = ViewModel.GetItemTypeWithItemObject();
            if (otherType != ViewModel.ItemType)
            {
                ;
            }
            if (GetItemFromIndex(inventoryVm, ViewModel.ItemType) is not SPItemVM equippedItem)
            {
                return false;
            }

            return _comparer.Compare(itemEqEl, equippedItem.ItemRosterElement.EquipmentElement) > 0;
        }
    }
}