using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ButterEquipped.AutoEquip;

public sealed class EquipmentElementComparer : IComparer<EquipmentElement>
{
    public int Compare(EquipmentElement x, EquipmentElement y)
        => (x.IsEmpty, y.IsEmpty) switch
        {
            (true, true) => 0,
            (true, false) => -1,
            (false, true) => 1,
            (false, false) => CompareInternal(x, y),
        };

    private static int CompareInternal(EquipmentElement x, EquipmentElement y)
    {
        var effX = CalculateEffectiveness(x);
        var effY = CalculateEffectiveness(y);
        return effX.CompareTo(effY);
    }

    private static float CalculateEffectiveness(EquipmentElement eq)
    {
        var item = eq.Item;
        var type = item.ItemType;
        var weight = eq.Weight;

        return (item.HasHorseComponent, item.HasWeaponComponent, item.HasArmorComponent) switch
        {
            (true, _, _) => CalculateEffectivenessHorse(item.HorseComponent),
            (_, true, _) => CalculateEffectivenessWeapon(item.WeaponComponent.PrimaryWeapon),
            (_, _, true) => CalculateEffectivenessArmor(),
            _ => 1f
        };

        float CalculateEffectivenessArmor(/*ArmorComponent armor*/)
        {
            if (type == ItemObject.ItemTypeEnum.HorseHarness)
            {
                var bodyArmor = eq.GetModifiedMountBodyArmor();
                return bodyArmor * 1.67f;
            }
            else
            {
                var headArmor = eq.GetModifiedHeadArmor();
                var bodyArmor = eq.GetModifiedBodyArmor();
                var legArmor = eq.GetModifiedLegArmor();
                var armArmor = eq.GetModifiedArmArmor();
                return (headArmor * 34f + bodyArmor * 42f + legArmor * 12f + armArmor * 12f) * 0.03f;
            }
        }

        float CalculateEffectivenessWeapon(WeaponComponentData primaryWeapon)
        {
            float mod = primaryWeapon.WeaponClass switch
            {
                WeaponClass.Dagger => 0.4f,
                WeaponClass.OneHandedSword => 0.55f,
                WeaponClass.TwoHandedSword => 0.6f,
                WeaponClass.OneHandedAxe => 0.5f,
                WeaponClass.TwoHandedAxe => 0.55f,
                WeaponClass.Mace => 0.5f,
                WeaponClass.Pick => 0.4f,
                WeaponClass.TwoHandedMace => 0.55f,
                WeaponClass.OneHandedPolearm => 0.4f,
                WeaponClass.TwoHandedPolearm => 0.4f,
                WeaponClass.LowGripPolearm => 0.4f,
                WeaponClass.Arrow => 3f,
                WeaponClass.Bolt => 3f,
                WeaponClass.Cartridge => 3f,
                WeaponClass.Bow => 0.55f,
                WeaponClass.Crossbow => 0.57f,
                WeaponClass.Stone => 0.1f,
                WeaponClass.Boulder => 0.1f,
                WeaponClass.ThrowingAxe => 0.25f,
                WeaponClass.ThrowingKnife => 0.2f,
                WeaponClass.Javelin => 0.28f,
                WeaponClass.Pistol => 1f,
                WeaponClass.Musket => 1f,
                WeaponClass.SmallShield => 0.4f,
                WeaponClass.LargeShield => 0.5f,
                _ => 1f,
            };

            var missileDamage = primaryWeapon.GetModifiedMissileDamage(eq.ItemModifier);
            var missileSpeed = primaryWeapon.GetModifiedMissileSpeed(eq.ItemModifier);
            var accuracy = primaryWeapon.Accuracy;
            var thrustSpeed = primaryWeapon.GetModifiedThrustSpeed(eq.ItemModifier);
            var thrustDamage = primaryWeapon.GetModifiedThrustDamage(eq.ItemModifier);
            var swingSpeed = primaryWeapon.GetModifiedSwingSpeed(eq.ItemModifier);
            var swingDamage = primaryWeapon.GetModifiedSwingDamage(eq.ItemModifier);
            var handling = primaryWeapon.GetModifiedHandling(eq.ItemModifier);
            var weaponLength = primaryWeapon.WeaponLength;
            var maxDataValue = primaryWeapon.MaxDataValue;
            if (primaryWeapon.IsRangedWeapon)
            {
                if (primaryWeapon.IsConsumable)
                {
                    return (missileDamage * missileSpeed * 1.775f + accuracy * maxDataValue * 25f + weaponLength * 4f) * 0.006944f * maxDataValue * mod;
                }
                else
                {
                    return (missileSpeed * missileDamage * 1.75f + thrustSpeed * accuracy * 0.3f) * 0.01f * maxDataValue * mod;
                }
            }
            else if (primaryWeapon.IsMeleeWeapon)
            {
                float num3 = thrustSpeed * thrustDamage * 0.01f;
                float num4 = swingSpeed * swingDamage * 0.01f;
                float num5 = MathF.Max(num4, num3);
                float num6 = MathF.Min(num4, num3);
                return ((num5 + num6 * num6 / num5) * 120f + handling * 15f + weaponLength * 20f + weight * 5f) * 0.01f * mod;
            }
            else if (primaryWeapon.IsConsumable)
            {
                return (missileDamage * 550f + missileSpeed * 15f + maxDataValue * 60f) * 0.01f * mod;
            }
            else if (primaryWeapon.IsShield)
            {
                var bodyArmor = primaryWeapon.GetModifiedArmor(eq.ItemModifier);
                return (bodyArmor * 60f + thrustSpeed * 10f + maxDataValue * 40f + weaponLength * 20f) * 0.01f * mod;
            }

            return 1f;
        }

        float CalculateEffectivenessHorse(HorseComponent horse)
        {
            var chargeDamage = horse.ChargeDamage;//eq.GetModifiedMountCharge();
            var speed = horse.Speed;//eq.GetModifiedMountSpeed();
            var manuever = horse.Maneuver;//eq.GetModifiedMountManeuver();
            var bodyLength = horse.BodyLength;
            var hitPoints = eq.GetModifiedMountHitPoints();
            return (chargeDamage * speed + manuever * speed + bodyLength * weight * 0.025f) * hitPoints * 0.0001f;
        }
    }
}
