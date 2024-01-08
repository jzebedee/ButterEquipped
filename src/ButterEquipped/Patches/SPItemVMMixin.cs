using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.ViewModels;
using ButterEquipped.AutoEquip;
using ButterEquipped.HighlightBetter;
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
    private static readonly Func<SPInventoryVM, EquipmentIndex, SPItemVM>? GetItemFromIndex
        = AccessTools2.GetDelegate<Func<SPInventoryVM, EquipmentIndex, SPItemVM>>(typeof(SPInventoryVM), nameof(GetItemFromIndex));

    private static bool IsValid
        => GetItemFromIndex is not null && CurrentInventory is not null;

    private static Action<SPInventoryVM?>? OnEquipmentUpdate;
    private static SPInventoryVM? currentInventory;
    internal static SPInventoryVM? CurrentInventory
    {
        get => currentInventory;
        set
        {
            //if (currentInventory != value)
            {
                currentInventory = value;
                OnEquipmentUpdate?.Invoke(currentInventory);
            }
        }
    }

    public SPItemVMMixin(SPItemVM vm) : base(vm)
    {
        OnEquipmentUpdate += HandleEquipmentUpdate;
    }

    private void HandleEquipmentUpdate(SPInventoryVM? vm)
    {
        if (vm is null)
        {
            return;
        }

        vm.PropertyChangedWithValue += HandleVmPropertyChanged;
    }

    private void HandleVmPropertyChanged(object? sender, PropertyChangedWithValueEventArgs e)
    {
        var watchedSlotName = ViewModel?.ItemType.GetPropertyNameFromIndex();
        if (!e.PropertyName.Equals(watchedSlotName))
        {
            return;
        }

        if (ViewModel switch
        {
            { IsFiltered: true } => true,
            { IsEquipableItem: false } => true,
            //Don't skip updating the property, otherwise we will have a dirty brush in this scenario:
            //
            // 1. Party has Hero A w/ Bad Horse equipped, inventory contains Good Horse with Riding > 10
            //    IsItemBetter SHOULD be true because Good Horse > Bad Horse
            // 2. Hero A has riding 1, so Good Horse has red brush because CanCharacterUseItem: false
            // 3. Good Horse skips update because CanCharacterUseItem: false, so IsItemBetter stays false
            // 4. Party switches to Hero B w/ Good Horse equipped
            // 5. Good Horse gets updated, but IsItemBetter doesn't raise a change because it's already false
            // 6. Good Horse has a green brush even though Hero B already has Good Horse equipped
            //{ CanCharacterUseItem: false } => true,
            { InventorySide: InventoryLogic.InventorySide.Equipment } => true,
            _ => false
        })
        {
            return;
        }

        ButterEquippedIsItemBetter = CompareEquipment(e.Value as SPItemVM);
    }

    private SPItemVM? EquipmentReference
        => (IsValid, ViewModel, CurrentInventory) switch
        {
            (true, SPItemVM vm, SPInventoryVM inventoryVm) => GetItemFromIndex!(CurrentInventory, ViewModel.ItemType),
            _ => null
        };

    private bool CompareEquipment()
    {
        if (ViewModel is { InventorySide: InventoryLogic.InventorySide.Equipment }
            or { IsEquipableItem: false }
            or { CanCharacterUseItem: false })
        {
            return false;
        }

        if (EquipmentReference is not SPItemVM equippedItem)
        {
            return false;
        }

        return CompareEquipment(equippedItem);
    }

    private bool CompareEquipment(SPItemVM? equippedItem)
    {
        var isWeapon = equippedItem is { ItemType: >= EquipmentIndex.WeaponItemBeginSlot and < EquipmentIndex.NonWeaponItemBeginSlot };
        if (equippedItem is null or { ItemRosterElement.IsEmpty: true })
        {
            return !isWeapon;
        }

        //ignore weapons, their index is always Weapon0/WeaponExtra (banners) so in the future we need 
        //to fully model the auto equip logic to match them with an appropriate equipped slot
        if (isWeapon)
        {
            return false;
        }

        var eqEffectiveness = EquipmentElementComparer.CalculateEffectiveness(equippedItem.ItemRosterElement.EquipmentElement);
        return ButterEquippedScore.CompareTo(eqEffectiveness) > 0;
    }

    [DataSourceProperty]
    public float ButterEquippedScore
    {
        get
        {
            if (ViewModel is not { ItemRosterElement.EquipmentElement: var eqEl })
            {
                return -1f;
            }

            return EquipmentElementComparer.CalculateEffectiveness(eqEl);
        }
    }

    [DataSourceProperty]
    public bool ButterEquippedIsItemBetter
    {
        get => _isItemBetter ??= CompareEquipment();
        set
        {
            if (_isItemBetter != value)
            {
                _isItemBetter = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    private bool? _isItemBetter;
}