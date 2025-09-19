using TaleWorlds.SaveSystem;

namespace ButterEquipped.AutoEquip;

public record HeroEquipmentSet(
    [property: SaveableProperty(1)] string HeroStringId,
    [property: SaveableProperty(2)] int SetMode); //TaleWorlds.CampaignSystem.ViewModelCollection.Inventory.SPInventoryVM.EquipmentModes