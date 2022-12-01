using TaleWorlds.Core;

namespace ButterEquipped.AutoEquip;

public record struct EquipmentUsageInfo(bool HasMount, bool HasShield, WeaponClass[] UsableAmmoClasses);
