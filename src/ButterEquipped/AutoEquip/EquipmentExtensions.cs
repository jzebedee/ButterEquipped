using System;
using System.Linq;
using TaleWorlds.Core;

namespace ButterEquipped.AutoEquip;

internal static class EquipmentExtensions
{
    public static bool HasWeaponOfClass(this Equipment equipment, params WeaponClass[] weaponClasses)
    {
        for (EquipmentIndex index = EquipmentIndex.WeaponItemBeginSlot; index < EquipmentIndex.NumAllWeaponSlots; index++)
        {
            var eqEl = equipment[index];
            if (eqEl.IsEmpty)
            {
                continue;
            }

            if (weaponClasses.Contains(eqEl.Item.PrimaryWeapon.WeaponClass))
            {
                return true;
            }
        }

        return false;
    }
}
