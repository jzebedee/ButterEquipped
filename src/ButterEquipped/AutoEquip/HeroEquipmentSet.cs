using TaleWorlds.SaveSystem;

namespace ButterEquipped.AutoEquip;

public record HeroEquipmentSet([property: SaveableProperty(1)] string StringId, [property: SaveableProperty(2)] bool IsWarSet);