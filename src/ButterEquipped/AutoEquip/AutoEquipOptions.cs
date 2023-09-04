namespace ButterEquipped.AutoEquip;

public record class AutoEquipOptions()
{
    public bool EquipHero { get; set; } = true;
    public bool EquipCompanions { get; set; } = true;
    public bool EquipCivilian { get; set; } = true;

    public bool EquipFromLoot { get; set; } = true;
    public bool EquipFromStash { get; set; } = true;
    public bool EquipFromInventory { get; set; } = true;
    public bool EquipFromDiscard { get; set; } = true;
    public bool EquipFromTrade { get; set; } = false;

    public bool KeepWeaponClass { get; set; } = true;
    public bool KeepMountType { get; set; } = false;
    public bool KeepCrafted { get; set; } = false;

    public bool AutoEquipOnClose { get; set; } = false;
}