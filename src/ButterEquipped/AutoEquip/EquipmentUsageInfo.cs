using System.Collections.Generic;
using TaleWorlds.Core;

namespace ButterEquipped.AutoEquip;

public record EquipmentUsageInfo(bool HasMount, bool HasShield, HashSet<WeaponClass> UsableAmmoClasses, bool CanUseAllBowsOnHorseback, BasicCultureObject? TargetCulture = null);
