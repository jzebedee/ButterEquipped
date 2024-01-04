using System.Collections.Generic;
using TaleWorlds.Core;

namespace ButterEquipped.AutoEquip;

public record struct EquipmentUsageInfo(bool HasMount, bool HasShield, HashSet<WeaponClass> UsableAmmoClasses, bool CanUseAllBowsOnHorseback);
