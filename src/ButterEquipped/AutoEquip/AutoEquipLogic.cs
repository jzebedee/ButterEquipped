using HarmonyLib.BUTR.Extensions;
using Helpers;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
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

    private readonly IComparer<EquipmentElement> _eqComparer;

    private readonly IEquipmentSlotLockSource _equipmentSlotLocks;

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
        if(hero?.CharacterObject is not CharacterObject character)
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

        if(ShouldEquipOtherSide())
        {
            result |= EquipAllHeroes(InventorySide.OtherInventory).Select(t => t.result).LastOrDefault(t => t);
        }

        if(!result)
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

        Equipment GetEquipment() => civilian ? hero.FirstCivilianEquipment : hero.FirstBattleEquipment;

        TransferCommand CreateUnequipCommand(EquipmentElement equipment, EquipmentIndex index)
            => TransferCommand.Transfer(1, InventorySide.Equipment, InventorySide.PlayerInventory, new ItemRosterElement(equipment, 1), index, EquipmentIndex.None, hero, civilian);

        TransferCommand CreateEquipCommand(ItemRosterElement itemRoster, EquipmentIndex index)
            => TransferCommand.Transfer(1, side, InventorySide.Equipment, itemRoster, EquipmentIndex.None, index, hero, civilian);
    }

    private static void Message(string information)
        => InformationManager.DisplayMessage(new InformationMessage(information));


    private bool ShouldEquip(EquipmentElement eq, CharacterObject hero, EquipmentIndex index, bool civilian)
    {
        if (civilian && !eq.Item.IsCivilian)
        {
            return false;
        }

        Equipment allEq = GetEquipment();
        if (options.KeepWeaponClass && eq.Item.PrimaryWeapon is var primaryWeapon)
        {
            var initialEq = allEq[index];
            if (initialEq.Item.PrimaryWeapon.WeaponClass != primaryWeapon.WeaponClass)
            {
                return false;
            }
        }

        return index switch
        {
            EquipmentIndex.HorseHarness when allEq.Horse.IsEmpty => false,
            EquipmentIndex.HorseHarness => allEq.Horse.Item.HorseComponent.Monster.FamilyType == eq.Item.ArmorComponent.FamilyType,
            _ => true
        };

        Equipment GetEquipment() => civilian ? hero.FirstCivilianEquipment : hero.FirstBattleEquipment;
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

        var bestItems = InvLogic.GetElementsInRoster(side)
            .Where(item => item.EquipmentElement.Item.ItemType == itemType)
            .Where(item => CharacterHelper.CanUseItemBasedOnSkill(hero, item.EquipmentElement))
            .Where(item => ShouldEquip(item.EquipmentElement, hero, slotIndex, civilian))
            .Prepend(new ItemRosterElement(slotEq, 0))
            .OrderByDescending(item => item.EquipmentElement, _eqComparer);

        return bestItems.First();
    }
}
