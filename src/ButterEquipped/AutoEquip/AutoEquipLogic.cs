using HarmonyLib.BUTR.Extensions;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static TaleWorlds.CampaignSystem.Inventory.InventoryLogic;
using static TaleWorlds.Core.ItemObject;

namespace ButterEquipped.AutoEquip;

public class AutoEquipLogic
{
    private static InventoryLogic InvLogic => InventoryManager.InventoryLogic;

    private static InventoryMode Mode => InventoryManager.Instance.CurrentMode;

    private readonly Traverse2 _updateRightCharacter;

    private readonly Traverse2 _executeRemoveZeroCounts;

    private readonly Traverse2 _refreshInformationValues;

    private readonly IEquipmentSlotLockSource _equipmentSlotLocks;

    private readonly IComparer<EquipmentElement> _eqComparer;

    private AutoEquipOptions options;

    public AutoEquipLogic(SPInventoryVM spInventoryVm, IEquipmentSlotLockSource equipmentSlotLocks)
    {
        _equipmentSlotLocks = equipmentSlotLocks;
        _updateRightCharacter = Traverse2.Create(spInventoryVm).Method("UpdateRightCharacter");
        _executeRemoveZeroCounts = Traverse2.Create(spInventoryVm).Method("ExecuteRemoveZeroCounts");
        _refreshInformationValues = Traverse2.Create(spInventoryVm).Method("RefreshInformationValues");
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
            Message("Nothing to equip");
        }

        _updateRightCharacter.GetValue();
        _refreshInformationValues.GetValue();

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
            Message("Nothing to equip");
        }

        _updateRightCharacter.GetValue();
        _refreshInformationValues.GetValue();

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

        _executeRemoveZeroCounts.GetValue();
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

                //these duplicate if passed to the formatter without explicitly ToString()'ing them
                Message($"{hero.Name} replaced {startingEq.GetModifiedItemName().ToString()} with {bestItem.EquipmentElement.GetModifiedItemName().ToString()}");
            }
            else
            {
                Message($"{hero.Name} equips {bestItem.EquipmentElement.GetModifiedItemName().ToString()}");
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

        if (options.KeepWeaponClass && item.HasWeaponComponent)
        {
            bool anyAllowed = false;
            for (int i = 0; i < item.Weapons.Count && i < initialEq.Item.Weapons.Count; i++)
            {
                var initialWcd = initialEq.Item.Weapons[i];
                var wcd = item.Weapons[i];

                anyAllowed |= initialWcd.WeaponClass == wcd.WeaponClass && AllowForUsage(item.ItemType, wcd.GetUsageFlags());
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
            _ => true
        };

        Equipment GetEquipment() => this.GetEquipment(hero, civilian);

        bool AllowForUsage(ItemTypeEnum itemType, ItemUsageSetFlags usageFlags)
            => (itemType, usageFlags) switch
            {
                (ItemTypeEnum.Bow, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => usageInfo.HasMount || hero.GetPerkValue(DefaultPerks.Bow.HorseMaster),
                (ItemTypeEnum.Bow, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => true,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoMount) => usageInfo.HasMount,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresMount) => !usageInfo.HasMount,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresNoShield) => !usageInfo.HasShield,
                (_, var u) when u.HasFlag(ItemUsageSetFlags.RequiresShield) => usageInfo.HasShield,
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

        EquipmentUsageInfo usageInfo = new(HasMount: !allEq.Horse.IsEmpty, HasShield: allEq.HasWeaponOfClass(WeaponClass.LargeShield, WeaponClass.SmallShield));
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
