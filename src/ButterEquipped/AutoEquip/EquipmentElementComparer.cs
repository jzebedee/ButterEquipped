using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.Core.ItemObject;

namespace ButterEquipped.AutoEquip;

public sealed class EquipmentElementComparer : IComparer<EquipmentElement>
{
    public record struct ComparerUsageInfo(bool HasMount, bool HasShield);

    private readonly ComparerUsageInfo _usageInfo;

    public EquipmentElementComparer(ComparerUsageInfo usageInfo = default)
        => _usageInfo = usageInfo;

    public int Compare(EquipmentElement x, EquipmentElement y)
        => (x.IsEmpty, y.IsEmpty) switch
        {
            (true, true) => 0,
            (true, false) => -1,
            (false, true) => 1,
            (false, false) => CompareInternal(x, y),
        };

    private int CompareInternal(EquipmentElement x, EquipmentElement y)
    {
        var effX = CalculateEffectiveness(x);
        var effY = CalculateEffectiveness(y);
        return effX.CompareTo(effY);
    }

    private bool AllowForUsage(ItemUsageSetFlags usageFlags)
    => usageFlags switch
    {
        var u when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => _usageInfo.HasMount,
        var u when u.HasFlag(ItemUsageSetFlags.RequiresMount) => !_usageInfo.HasMount,
        var u when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => !_usageInfo.HasShield,
        var u when u.HasFlag(ItemUsageSetFlags.RequiresShield) => _usageInfo.HasShield,
        _ => true
    };

    private float CalculateEffectiveness(EquipmentElement eq)
    {
        var item = eq.Item;
        var type = item.ItemType;
        var weight = eq.Weight;

        return item.ItemComponent switch
        {
            HorseComponent horse => CalculateEffectivenessHorse(horse),
            WeaponComponent weapon => weapon.Weapons.Where(wcd => AllowForUsage(wcd.GetUsageFlags())).Select((wcd, i) => CalculateEffectivenessWeapon(wcd) / (i+1)*2).Sum(),
            ArmorComponent => CalculateEffectivenessArmor(),
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

        float CalculateEffectivenessWeapon(WeaponComponentData weapon)
        {
            float mod = weapon.WeaponClass switch
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

            var missileDamage = weapon.GetModifiedMissileDamage(eq.ItemModifier);
            var missileSpeed = weapon.GetModifiedMissileSpeed(eq.ItemModifier);
            var accuracy = weapon.Accuracy;
            var thrustSpeed = weapon.GetModifiedThrustSpeed(eq.ItemModifier);
            var thrustDamage = weapon.GetModifiedThrustDamage(eq.ItemModifier);
            var swingSpeed = weapon.GetModifiedSwingSpeed(eq.ItemModifier);
            var swingDamage = weapon.GetModifiedSwingDamage(eq.ItemModifier);
            var handling = weapon.GetModifiedHandling(eq.ItemModifier);
            var weaponLength = weapon.WeaponLength;
            var maxDataValue = weapon.MaxDataValue;
            if (weapon.IsRangedWeapon)
            {
                if (weapon.IsConsumable)
                {
                    return (missileDamage * missileSpeed * 1.775f + accuracy * maxDataValue * 25f + weaponLength * 4f) * 0.006944f * maxDataValue * mod;
                }
                else
                {
                    return (missileSpeed * missileDamage * 1.75f + thrustSpeed * accuracy * 0.3f) * 0.01f * maxDataValue * mod;
                }
            }
            else if (weapon.IsMeleeWeapon)
            {
                float modThrust = thrustSpeed * thrustDamage * 0.01f;
                float modSwing = swingSpeed * swingDamage * 0.01f;
                float modMax = MathF.Max(modSwing, modThrust);
                float modMin = MathF.Min(modSwing, modThrust);
                return ((modMax + modMin * modMin / modMax) * 120f + handling * 15f + weaponLength * 20f + weight * 5f) * 0.01f * mod;
            }
            else if (weapon.IsConsumable)
            {
                return (missileDamage * 550f + missileSpeed * 15f + maxDataValue * 60f) * 0.01f * mod;
            }
            else if (weapon.IsShield)
            {
                var bodyArmor = weapon.GetModifiedArmor(eq.ItemModifier);
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
