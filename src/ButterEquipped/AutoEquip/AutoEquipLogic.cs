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
using InventorySide = InventoryLogic.InventorySide;
using ItemTypeEnum = ItemObject.ItemTypeEnum;
using ItemUsageSetFlags = ItemObject.ItemUsageSetFlags;

public class AutoEquipLogic
{
    private static InventoryLogic InvLogic => InventoryManager.InventoryLogic;

    private static InventoryMode Mode => InventoryManager.Instance.CurrentMode;

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

    private readonly Action _updateRightCharacter;

    private readonly Action _executeRemoveZeroCounts;

    private readonly Action _refreshInformationValues;

    private readonly IEquipmentSlotLockSource _equipmentSlotLocks;

    private readonly IComparer<EquipmentElement> _eqComparer;

    private AutoEquipOptions options;

    public AutoEquipLogic(SPInventoryVM spInventoryVm, IEquipmentSlotLockSource equipmentSlotLocks)
    {
        _equipmentSlotLocks = equipmentSlotLocks;
        _updateRightCharacter = AccessTools2.GetDelegate<Action>(spInventoryVm, typeof(SPInventoryVM), "UpdateRightCharacter");
        _executeRemoveZeroCounts = AccessTools2.GetDelegate<Action>(spInventoryVm, typeof(SPInventoryVM), "ExecuteRemoveZeroCounts");
        _refreshInformationValues = AccessTools2.GetDelegate<Action>(spInventoryVm, typeof(SPInventoryVM), "RefreshInformationValues");
        _eqComparer = new EquipmentElementComparer();
    }

    public bool Equip(AutoEquipOptions options, Hero hero, bool civilian)
    {
        if (hero?.CharacterObject is not CharacterObject character)
        {
            return false;
        }

        this.options = options;

        var result = false;

        if (options.EquipFromInventory)
        {
            result |= EquipHero(character, InventorySide.PlayerInventory, civilian);
        }

        if (ShouldEquipOtherSide())
        {
            result |= EquipHero(character, InventorySide.OtherInventory, civilian);
        }

        if (!result)
        {
            Message(Messages.NothingToEquip.ToString());
        }

        _updateRightCharacter();
        _refreshInformationValues();

        return result;
    }

    public bool EquipParty(AutoEquipOptions options)
    {
        this.options = options;

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

            var result = EquipHero(character, side);
            if (options.EquipCivilian)
            {
                result |= EquipHero(character, side, civilian: true);
            }
            yield return (hero, result);
        }
    }

    private bool EquipHero(CharacterObject hero, InventorySide side, bool civilian = false)
    {
        bool result = false;

        var slotLocks = _equipmentSlotLocks.GetSlotLocks(new(hero.StringId, !civilian));
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
            ItemRosterElement bestItem = FindBestItem(index, hero, side, civilian);

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

        Equipment GetEquipment() => this.GetEquipment(hero, civilian);

        TransferCommand CreateUnequipCommand(EquipmentElement equipment, EquipmentIndex index)
            => TransferCommand.Transfer(1, InventorySide.Equipment, InventorySide.PlayerInventory, new ItemRosterElement(equipment, 1), index, EquipmentIndex.None, hero, civilian);

        TransferCommand CreateEquipCommand(ItemRosterElement itemRoster, EquipmentIndex index)
            => TransferCommand.Transfer(1, side, InventorySide.Equipment, itemRoster, EquipmentIndex.None, index, hero, civilian);
    }

    private static void Message(string information)
        => InformationManager.DisplayMessage(new InformationMessage(information));

    private bool ShouldEquip(EquipmentElement eq, CharacterObject hero, EquipmentIndex index, bool civilian, EquipmentUsageInfo usageInfo)
    {
        var item = eq.Item;
        if (civilian && !item.IsCivilian)
        {
            return false;
        }

        Equipment allEq = GetEquipment();
        var initialEq = allEq[index];

        if (options.KeepCulture)
        {
            if (item.Culture != hero.Culture)
            {
                return false;
            }
        }

        if (options.KeepWeaponClass && item.HasWeaponComponent)
        {
            bool anyAllowed = false;
            for (int i = 0; i < item.Weapons.Count && i < initialEq.Item.Weapons.Count; i++)
            {
                var initialWcd = initialEq.Item.Weapons[i];
                var wcd = item.Weapons[i];

                anyAllowed |= initialWcd.WeaponClass == wcd.WeaponClass && AllowForUsage(item.ItemType, wcd);
            }

            if (!anyAllowed)
            {
                return false;
            }
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

        Equipment GetEquipment() => this.GetEquipment(hero, civilian);

        bool AllowForUsage(ItemTypeEnum itemType, WeaponComponentData weapon)
            => (itemType, weapon.GetUsageFlags()) switch
            {
                (ItemTypeEnum.Bow, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => usageInfo.HasMount || hero.GetPerkValue(DefaultPerks.Bow.HorseMaster),
                (ItemTypeEnum.Bow, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => true,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => usageInfo.HasMount,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresMount) => !usageInfo.HasMount,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => !usageInfo.HasShield,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresShield) => usageInfo.HasShield,
                _ when weapon.IsAmmo => usageInfo.UsableAmmoClasses.Contains(weapon.WeaponClass),
                _ => true
            };
    }

    private ItemRosterElement FindBestItem(EquipmentIndex slotIndex, CharacterObject hero, InventorySide side, bool civilian)
    {
        Equipment allEq = civilian ? hero.FirstCivilianEquipment : hero.FirstBattleEquipment;
        EquipmentElement slotEq = allEq[slotIndex];

        ItemTypeEnum? itemType = slotEq.IsEmpty switch
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
            false when options.KeepCrafted && slotEq.Item.IsCraftedByPlayer => null,
            false => slotEq.Item.ItemType
        };

        if (itemType is null)
        {
            return ItemRosterElement.Invalid;
        }

        EquipmentUsageInfo usageInfo = new(
            HasMount: !allEq.Horse.IsEmpty,
            HasShield: allEq.HasWeaponOfClass(WeaponClass.LargeShield, WeaponClass.SmallShield),
            UsableAmmoClasses: allEq.WeaponSlots()
                                    .Where(slot => !slot.IsEmpty)
                                    .Select(slot => slot.Item.PrimaryWeapon)
                                    .OfType<WeaponComponentData>()
                                    .Select(wcd => wcd.AmmoClass)
                                    .Where(ammoClass => ammoClass != WeaponClass.Undefined)
                                    .ToArray());
        var bestItems = InvLogic.GetElementsInRoster(side)
            .Where(item => item.EquipmentElement.Item.ItemType == itemType)
            .Where(item => Equipment.IsItemFitsToSlot(slotIndex, item.EquipmentElement.Item))
            .Where(item => CharacterHelper.CanUseItemBasedOnSkill(hero, item.EquipmentElement))
            .Where(item => ShouldEquip(item.EquipmentElement, hero, slotIndex, civilian, usageInfo))
            .Prepend(new ItemRosterElement(slotEq, 0))
            .OrderByDescending(item => item.EquipmentElement, _eqComparer);

        return bestItems.First();
    }

    private Equipment GetEquipment(CharacterObject character, bool civilian)
        => civilian ? character.FirstCivilianEquipment : character.FirstBattleEquipment;
}
