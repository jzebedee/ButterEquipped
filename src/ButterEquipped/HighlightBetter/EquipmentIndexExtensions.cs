using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;

namespace ButterEquipped.HighlightBetter;

internal static class EquipmentIndexExtensions
{
    public static string? GetPropertyNameFromIndex(this EquipmentIndex index)
        => index switch
        {
            EquipmentIndex.Weapon0 => nameof(SPInventoryVM.CharacterWeapon1Slot),
            EquipmentIndex.Weapon1 => nameof(SPInventoryVM.CharacterWeapon2Slot),
            EquipmentIndex.Weapon2 => nameof(SPInventoryVM.CharacterWeapon3Slot),
            EquipmentIndex.Weapon3 => nameof(SPInventoryVM.CharacterWeapon4Slot),
            EquipmentIndex.ExtraWeaponSlot => nameof(SPInventoryVM.CharacterBannerSlot),
            EquipmentIndex.Head => nameof(SPInventoryVM.CharacterHelmSlot),
            EquipmentIndex.Body => nameof(SPInventoryVM.CharacterTorsoSlot),
            EquipmentIndex.Leg => nameof(SPInventoryVM.CharacterBootSlot),
            EquipmentIndex.Gloves => nameof(SPInventoryVM.CharacterGloveSlot),
            EquipmentIndex.Cape => nameof(SPInventoryVM.CharacterCloakSlot),
            EquipmentIndex.Horse => nameof(SPInventoryVM.CharacterMountSlot),
            EquipmentIndex.HorseHarness => nameof(SPInventoryVM.CharacterMountArmorSlot),
            _ => null
        };
}
