using HarmonyLib.BUTR.Extensions;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ButterEquipped.AutoEquip;

using EquipmentType = Equipment.EquipmentType;
using EquipmentModes = SPInventoryVM.EquipmentModes;
using InventoryMode = InventoryScreenHelper.InventoryMode;
using InventorySide = InventoryLogic.InventorySide;
using ItemTypeEnum = ItemObject.ItemTypeEnum;
using ItemUsageSetFlags = ItemObject.ItemUsageSetFlags;

public class AutoEquipLogic
{
    private static InventoryLogic InvLogic => InventoryScreenHelper.GetActiveInventoryState().InventoryLogic;

    private static InventoryMode Mode => InventoryScreenHelper.GetActiveInventoryState().InventoryMode;

    //private static readonly Dictionary<EquipmentModes, IComparer<EquipmentElement>> _comparerByMode = new()
    //{
    //    [EquipmentModes.Battle] = new EquipmentElementComparer(EquipmentType.Battle),
    //    [EquipmentModes.Civilian] = new EquipmentElementComparer(EquipmentType.Civilian),
    //    [EquipmentModes.Stealth] = new EquipmentElementComparer(EquipmentType.Stealth)
    //};

    private static readonly IComparer<EquipmentElement> _battleComparer = new EquipmentElementComparer(EquipmentType.Battle);

    private static readonly IComparer<EquipmentElement> _civilianComparer = new EquipmentElementComparer(EquipmentType.Civilian);

    private static readonly IComparer<EquipmentElement> _stealthComparer = new EquipmentElementComparer(EquipmentType.Stealth);

    private static class Messages
    {
        public static readonly TextObject NothingToEquip = new("{=ButterEquipMSG001}Nothing to equip");

        public static TextObject HeroReplacedItem(TextObject heroName, TextObject oldItemName, TextObject newItemName)
            => new("{=ButterEquipMSG002}{HERO} replaced {ITEM} with {BESTITEM}",
                new() {
                    { "HERO", heroName },
                    { "ITEM", oldItemName },
                    { "BESTITEM", newItemName }
                });

        public static TextObject HeroEquipsItem(TextObject heroName, TextObject itemName)
            => new("{=ButterEquipMSG003}{HERO} equips {BESTITEM}",
                new() {
                    { "HERO", heroName },
                    { "BESTITEM", itemName }
                });
    }

    private static class PrivateMethods
    {
        public static Action<SPInventoryVM>? UpdateRightCharacter
            = AccessTools2.GetDelegate<Action<SPInventoryVM>>(typeof(SPInventoryVM), "UpdateRightCharacter");

        public static Action<SPInventoryVM>? ExecuteRemoveZeroCounts
            = AccessTools2.GetDelegate<Action<SPInventoryVM>>(typeof(SPInventoryVM), "ExecuteRemoveZeroCounts");

        public static Action<SPInventoryVM>? RefreshInformationValues
            = AccessTools2.GetDelegate<Action<SPInventoryVM>>(typeof(SPInventoryVM), "RefreshInformationValues");

        public static HarmonyLib.AccessTools.FieldRef<SPInventoryVM, EquipmentModes>? GetEquipmentMode
            = AccessTools2.FieldRefAccess<SPInventoryVM, EquipmentModes>("_equipmentMode");
    }

    private readonly Action _updateRightCharacter;

    private readonly Action _executeRemoveZeroCounts;

    private readonly Action _refreshInformationValues;

    private readonly Func<EquipmentModes?> _getEquipmentMode;

    private readonly IEquipmentSlotLockSource _equipmentSlotLocks;

    private AutoEquipOptions options;

    public AutoEquipLogic(AutoEquipOptions options, SPInventoryVM spInventoryVm, IEquipmentSlotLockSource equipmentSlotLocks)
    {
        this.options = options;
        _equipmentSlotLocks = equipmentSlotLocks;
        _updateRightCharacter = () => PrivateMethods.UpdateRightCharacter?.Invoke(spInventoryVm);
        _executeRemoveZeroCounts = () => PrivateMethods.ExecuteRemoveZeroCounts?.Invoke(spInventoryVm);
        _refreshInformationValues = () => PrivateMethods.RefreshInformationValues?.Invoke(spInventoryVm);
        _getEquipmentMode = () => PrivateMethods.GetEquipmentMode?.Invoke(spInventoryVm);
    }

    public bool Equip(Hero hero, EquipmentModes mode)
    {
        if (hero?.CharacterObject is not CharacterObject character)
        {
            return false;
        }

        var result = false;

        if (options.EquipFromInventory)
        {
            result |= EquipHero(character, InventorySide.PlayerInventory, mode);
        }

        if (ShouldEquipOtherSide())
        {
            result |= EquipHero(character, InventorySide.OtherInventory, mode);
        }

        if (!result)
        {
            Message(Messages.NothingToEquip.ToString());
        }

        _updateRightCharacter();
        _refreshInformationValues();

        return result;
    }

    public bool EquipParty()
    {
        var result = false;

        if (options.EquipFromInventory)
        {
            result |= EquipAllHeroes(InventorySide.PlayerInventory).Select(t => t.result).LastOrDefault(t => t);
        }

        if (ShouldEquipOtherSide())
        {
            result |= EquipAllHeroes(InventorySide.OtherInventory).Select(t => t.result).LastOrDefault(t => t);
        }

        if (!result)
        {
            Message(Messages.NothingToEquip.ToString());
        }

        _updateRightCharacter();
        _refreshInformationValues();

        return result;
    }

    private bool ShouldEquipOtherSide() => Mode switch
    {
        InventoryMode.Default when options.EquipFromDiscard => true,
        InventoryMode.Loot when options.EquipFromLoot => true,
        InventoryMode.Stash when options.EquipFromStash => true,
        InventoryMode.Trade when options.EquipFromTrade => true,
        _ => false
    };

    private IEnumerable<(Hero hero, bool result)> EquipAllHeroes(InventorySide side)
    {
        var heroes = PartyBase.MainParty.MemberRoster
            .GetTroopRoster()
            .Select(item => item.Character)
            .Where(character => character.IsHero)
            .Select(character => (character, character.HeroObject));
        foreach ((CharacterObject character, Hero hero) in heroes)
        {
            //IsPlayerCompanion is false for tutorial family
            if (!hero.IsHumanPlayerCharacter && !options.EquipCompanions)
            {
                continue;
            }

            if (hero.IsHumanPlayerCharacter && !options.EquipHero)
            {
                continue;
            }

            bool result = false;
            if (options.EquipBattle)
            {
                result |= EquipHero(character, side, mode: EquipmentModes.Battle);
            }
            if (options.EquipCivilian)
            {
                result |= EquipHero(character, side, mode: EquipmentModes.Civilian);
            }
            if (options.EquipStealth)
            {
                result |= EquipHero(character, side, mode: EquipmentModes.Stealth);
            }
            yield return (hero, result);
        }
    }

    private bool EquipHero(CharacterObject hero, InventorySide side, EquipmentModes mode)
    {
        bool result = false;

        var slotLocks = _equipmentSlotLocks.GetSlotLocks(new(hero.StringId, (int)mode));
        for (EquipmentIndex index = EquipmentIndex.WeaponItemBeginSlot; index < EquipmentIndex.NumEquipmentSetSlots; index++)
        {
            if (slotLocks[(int)index])
            {
                continue;
            }

            result |= TryEquip(index);
        }

        _executeRemoveZeroCounts();
        return result;

        bool TryEquip(EquipmentIndex index)
        {
            EquipmentElement startingEq = GetEquipment()[index];
            ItemRosterElement bestItem = FindBestItem(index, hero, side, mode);

            if (bestItem.IsEmpty || bestItem.EquipmentElement.IsEqualTo(startingEq))
            {
                return false;
            }

            if (!startingEq.IsEmpty)
            {
                var unequipCmd = CreateUnequipCommand(startingEq, index);
                InvLogic.AddTransferCommand(unequipCmd);

                Message(Messages.HeroReplacedItem(hero.Name, startingEq.GetModifiedItemName(), bestItem.EquipmentElement.GetModifiedItemName()).ToString());
            }
            else
            {
                Message(Messages.HeroEquipsItem(hero.Name, bestItem.EquipmentElement.GetModifiedItemName()).ToString());
            }

            if (index == EquipmentIndex.Horse && GetEquipment()[EquipmentIndex.HorseHarness] is var harness && !harness.IsEmpty)
            {
                //always unequip harness to avoid camels + horse harness
                var unequipHarnessCmd = CreateUnequipCommand(harness, EquipmentIndex.HorseHarness);
                InvLogic.AddTransferCommand(unequipHarnessCmd);
            }

            var equipCmd = CreateEquipCommand(bestItem, index);
            InvLogic.AddTransferCommand(equipCmd);

            return true;
        }

        Equipment? GetEquipment() => AutoEquipLogic.GetEquipment(hero, mode);

        TransferCommand CreateUnequipCommand(EquipmentElement equipment, EquipmentIndex index)
            => TransferCommand.Transfer(
                amount: 1,
                fromSide: EquipmentModeToInventorySide(mode),
                toSide: InventorySide.PlayerInventory,
                elementToTransfer: new ItemRosterElement(equipment, 1),
                fromEquipmentIndex: index,
                toEquipmentIndex: EquipmentIndex.None,
                character: hero);

        TransferCommand CreateEquipCommand(ItemRosterElement itemRoster, EquipmentIndex index)
            => TransferCommand.Transfer(
                amount: 1,
                fromSide: side,
                toSide: EquipmentModeToInventorySide(mode),
                elementToTransfer: itemRoster,
                fromEquipmentIndex: EquipmentIndex.None,
                toEquipmentIndex: index,
                character: hero);
    }

    private static InventorySide EquipmentModeToInventorySide(EquipmentModes equipmentMode)
        => equipmentMode switch
        {
            EquipmentModes.Civilian => InventorySide.CivilianEquipment,
            EquipmentModes.Battle => InventorySide.BattleEquipment,
            EquipmentModes.Stealth => InventorySide.StealthEquipment,
            _ => InventorySide.None,
        };

    private static void Message(string information)
        => InformationManager.DisplayMessage(new InformationMessage(information));

    private bool ShouldEquip(Equipment allEq, EquipmentElement eq, EquipmentIndex index, EquipmentUsageInfo usageInfo)
    {
        var item = eq.Item;
        if (usageInfo.TargetCulture is BasicCultureObject targetCulture
         && item.Culture != targetCulture)
        {
            return false;
        }

        if (item.ItemComponent is WeaponComponent weapon && !ValidateWeapon(weapon))
        {
            return false;
        }

        return index switch
        {
            EquipmentIndex.HorseHarness when allEq.Horse.IsEmpty => false,
            EquipmentIndex.HorseHarness => allEq.Horse.Item.HorseComponent.Monster.FamilyType == item.ArmorComponent.FamilyType, //camelizer
            EquipmentIndex.Horse when options.KeepMountType && allEq is { Horse.IsEmpty: false } => allEq.Horse.Item.HorseComponent.Monster.FamilyType == item.HorseComponent.Monster.FamilyType, //camelizer
            EquipmentIndex.ExtraWeaponSlot when allEq[EquipmentIndex.ExtraWeaponSlot].IsEmpty => false,
            EquipmentIndex.ExtraWeaponSlot => allEq[EquipmentIndex.ExtraWeaponSlot].Item.BannerComponent.BannerEffect.StringId == item.BannerComponent.BannerEffect?.StringId, //banners
            _ => true
        };

        bool ValidateWeapon(WeaponComponent weapon)
        {
            var initialEq = allEq[index];
            if (initialEq.IsEmpty)
            {
                return false;
            }

            var initialDetails = GetWeaponDetails(initialEq.Item.WeaponComponent);
            var weaponDetails = GetWeaponDetails(weapon);

            if (options.KeepWeaponClass)
            {
                //Keep weapon class will force an exact match of the current:
                // weapon class (ex: , weapon description
                return initialDetails.Intersect(weaponDetails).Any();
            }

            var initialCompare = initialDetails.Select(tup => (WeaponComponentData.GetItemTypeFromWeaponClass(tup.weaponClass), tup.weaponDescription, tup.allowed));
            var weaponCompare = weaponDetails.Select(tup => (WeaponComponentData.GetItemTypeFromWeaponClass(tup.weaponClass), tup.weaponDescription, tup.allowed));
            return initialCompare.Intersect(weaponCompare).Any();
        }

        IEnumerable<(WeaponClass weaponClass, string weaponDescription, bool allowed)> GetWeaponDetails(WeaponComponent weapon)
            => weapon.Weapons
                .OfType<WeaponComponentData>()
                .Select(wcd => (wcd.WeaponClass, wcd.WeaponDescriptionId, AllowForUsage(wcd)));

        bool AllowForUsage(WeaponComponentData weapon)
            => (weapon.WeaponClass, weapon.GetUsageFlags()) switch
            {
                (WeaponClass.Bow, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => !usageInfo.HasMount || usageInfo.CanUseAllBowsOnHorseback,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => !usageInfo.HasMount,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresMount) => usageInfo.HasMount,
                //(_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => !usageInfo.HasShield,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresShield) => usageInfo.HasShield,
                _ when weapon.IsAmmo => usageInfo.UsableAmmoClasses.Contains(weapon.WeaponClass),
                _ => true
            };
    }

    private IEnumerable<ItemRosterElement> CompareItems(EquipmentIndex slotIndex, CharacterObject hero, InventorySide side, EquipmentModes eqMode)
    {
        if (GetEquipment(hero, eqMode) is not Equipment allEq)
        {
            return [];
        }

        EquipmentElement slotEq = allEq[slotIndex];

        ItemTypeEnum itemType = slotEq.IsEmpty switch
        {
            true => slotIndex switch
            {
                EquipmentIndex.Head => ItemTypeEnum.HeadArmor,
                EquipmentIndex.Body => ItemTypeEnum.BodyArmor,
                EquipmentIndex.Leg => ItemTypeEnum.LegArmor,
                EquipmentIndex.Gloves => ItemTypeEnum.HandArmor,
                EquipmentIndex.Cape => ItemTypeEnum.Cape,
                EquipmentIndex.HorseHarness when !allEq[EquipmentIndex.Horse].IsEmpty => ItemTypeEnum.HorseHarness,
                _ => ItemTypeEnum.Invalid
            },
            false when options.KeepCrafted && slotEq.Item.IsCraftedByPlayer => ItemTypeEnum.Invalid,
            false => slotEq.Item.ItemType
        };

        if (itemType is ItemTypeEnum.Invalid)
        {
            return [];
        }

        EquipmentUsageInfo usageInfo = new(
            HasMount: !allEq.Horse.IsEmpty,
            HasShield: allEq.HasWeaponOfClass(WeaponClass.LargeShield, WeaponClass.SmallShield),
            UsableAmmoClasses: [.. GetUsableAmmoClasses(allEq)],
            CanUseAllBowsOnHorseback: hero.GetPerkValue(DefaultPerks.Bow.HorseMaster),
            TargetCulture: options.KeepCulture ? hero.Culture : null);

        return InvLogic.GetElementsInRoster(side)
            .Where(item => item switch
            {
                //disallow war items in civilian mode
                { IsEmpty: false, EquipmentElement.Item.IsCivilian: false } when eqMode is EquipmentModes.Civilian => false,
                { IsEmpty: false, EquipmentElement.Item.IsStealthItem: false } when eqMode is EquipmentModes.Stealth => false,
                { IsEmpty: false } => true,
                _ => false
            })
            .Where(item => Equipment.IsItemFitsToSlot(slotIndex, item.EquipmentElement.Item))
            .Where(item => CharacterHelper.CanUseItemBasedOnSkill(hero, item.EquipmentElement))
            .Where(item => ShouldEquip(GetEquipment(hero, eqMode), item.EquipmentElement, slotIndex, usageInfo))
            .Prepend(new ItemRosterElement(slotEq, 0))
            .OrderByDescending(item => item.EquipmentElement, eqMode switch
            {
                EquipmentModes.Battle => _battleComparer,
                EquipmentModes.Civilian => _civilianComparer,
                EquipmentModes.Stealth => _stealthComparer,
                _ => _battleComparer
            });
    }

    private ItemRosterElement FindBestItem(EquipmentIndex slotIndex, CharacterObject hero, InventorySide side, EquipmentModes mode)
        => CompareItems(slotIndex, hero, side, mode).DefaultIfEmpty(ItemRosterElement.Invalid).First();

    private static IEnumerable<WeaponClass> GetUsableAmmoClasses(Equipment allEq)
        => allEq.WeaponSlots()
                .Where(slot => !slot.IsEmpty)
                .Select(slot => slot.Item.PrimaryWeapon)
                .OfType<WeaponComponentData>()
                .Select(wcd => wcd.AmmoClass)
                .Where(ammoClass => ammoClass != WeaponClass.Undefined);

    private static Equipment? GetEquipment(CharacterObject character, EquipmentModes mode)
        => mode switch
        {
            EquipmentModes.Civilian => character.FirstCivilianEquipment,
            EquipmentModes.Battle => character.FirstBattleEquipment,
            EquipmentModes.Stealth => character.FirstStealthEquipment,
            _ => null
        };
}
