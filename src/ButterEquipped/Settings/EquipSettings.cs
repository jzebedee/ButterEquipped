using ButterEquipped.AutoEquip;
using ButterEquipped.HighlightBetter;
using MCM.Abstractions.Base;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using System;

namespace ButterEquipped.Settings;

internal static class EquipSettings
{
    private static Version? CurrentVersion => typeof(EquipSettings).Assembly.GetName().Version;

    private static string SettingsId => $"{nameof(ButterEquipped)}_v{CurrentVersion?.ToString(1)}";

    private static string SettingsName => $"Butter Equipped {CurrentVersion?.ToString(3)}";

    public static ISettingsBuilder AddEquipSettings(AutoEquipOptions eqOpt, HighlightBetterOptions hiOpt, string id)
    {
        var builder = BaseSettingsBuilder.Create(SettingsId, SettingsName)!
            .SetFormat("json2")
            .SetFolderName(nameof(ButterEquipped))
            //.SetSubFolder(id)
            .CreateGroup("{=ButterEquip001}Equip Targets", BuildEquipTargetsGroup)
            .CreateGroup("{=ButterEquip002}Equip From", BuildEquipFromGroup)
            .CreateGroup("{=ButterEquip003}Keep Items", BuildKeepItemsGroup)
            .CreateGroup("{=ButterEquip004}Auto Equip", BuildAutoEquipGroup)
            .CreateGroup("{=ButterEquip017}Highlight Better Items", BuildHighlightItemsGroup)
            //.WithoutDefaultPreset()
            .CreatePreset(BaseSettings.DefaultPresetId, BaseSettings.DefaultPresetName, builder => BuildDefaultPreset(builder, new()));

        return builder;

        static void BuildDefaultPreset(ISettingsPresetBuilder builder, AutoEquipOptions opt)
            => builder
                .SetPropertyValue("equip_player", opt.EquipHero)
                .SetPropertyValue("equip_companions", opt.EquipCompanions)
                .SetPropertyValue("equip_battle", opt.EquipBattle)
                .SetPropertyValue("equip_civilian", opt.EquipCivilian)
                .SetPropertyValue("equip_stealth", opt.EquipStealth)

                .SetPropertyValue("equip_from_loot", opt.EquipFromLoot)
                .SetPropertyValue("equip_from_stash", opt.EquipFromStash)
                .SetPropertyValue("equip_from_inventory", opt.EquipFromInventory)
                .SetPropertyValue("equip_from_discard", opt.EquipFromDiscard)
                .SetPropertyValue("equip_from_trade", opt.EquipFromTrade)

                .SetPropertyValue("keep_weapon_class", opt.KeepWeaponClass)
                .SetPropertyValue("keep_mount_type", opt.KeepMountType)
                .SetPropertyValue("keep_crafted", opt.KeepCrafted)
                .SetPropertyValue("keep_culture", opt.KeepCulture)

                .SetPropertyValue("auto_equip_on_inventory_close", opt.AutoEquipOnClose);

        void BuildEquipTargetsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("equip_player",
                         "{=ButterEquip005}Equip Player Hero",
                         new ProxyRef<bool>(() => eqOpt.EquipHero, value => eqOpt.EquipHero = value),
                         propBuilder => propBuilder.SetRequireRestart(false).SetOrder(1))
                .AddBool("equip_companions",
                         "{=ButterEquip006}Equip Companions",
                         new ProxyRef<bool>(() => eqOpt.EquipCompanions, value => eqOpt.EquipCompanions = value),
                         propBuilder => propBuilder.SetRequireRestart(false).SetOrder(2))
                .AddBool("equip_battle",
                         "{=ButterEquip_EquipBattleEquipment}Equip Battle Equipment",
                         new ProxyRef<bool>(() => eqOpt.EquipBattle, value => eqOpt.EquipBattle = value),
                         propBuilder => propBuilder.SetRequireRestart(false).SetOrder(3))
                .AddBool("equip_civilian",
                         "{=ButterEquip_EquipCivilianEquipment}Equip Civilian Equipment",
                         new ProxyRef<bool>(() => eqOpt.EquipCivilian, value => eqOpt.EquipCivilian = value),
                         propBuilder => propBuilder.SetRequireRestart(false).SetOrder(4))
                .AddBool("equip_stealth",
                         "{=ButterEquip_EquipStealthEquipment}Equip Stealth Equipment",
                         new ProxyRef<bool>(() => eqOpt.EquipStealth, value => eqOpt.EquipStealth = value),
                         propBuilder => propBuilder.SetRequireRestart(false).SetOrder(5))
                .SetGroupOrder(1);

        void BuildEquipFromGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("equip_from_loot",
                         "{=ButterEquip008}Equip from Loot",
                         new ProxyRef<bool>(() => eqOpt.EquipFromLoot, value => eqOpt.EquipFromLoot = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_stash",
                         "{=ButterEquip009}Equip from Stash",
                         new ProxyRef<bool>(() => eqOpt.EquipFromStash, value => eqOpt.EquipFromStash = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_inventory",
                         "{=ButterEquip010}Equip from Inventory",
                         new ProxyRef<bool>(() => eqOpt.EquipFromInventory, value => eqOpt.EquipFromInventory = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_discard",
                         "{=ButterEquip011}Equip from Discard",
                         new ProxyRef<bool>(() => eqOpt.EquipFromDiscard, value => eqOpt.EquipFromDiscard = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_trade",
                         "{=ButterEquip012}Equip from Trade",
                         new ProxyRef<bool>(() => eqOpt.EquipFromTrade, value => eqOpt.EquipFromTrade = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(2);

        void BuildKeepItemsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("keep_weapon_class",
                         "{=ButterEquip013}Keep Weapon Class",
                         new ProxyRef<bool>(() => eqOpt.KeepWeaponClass, value => eqOpt.KeepWeaponClass = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip013Hint}When enabled, only weapons of the same class will be equipped (e.g., One Handed Maces will only be upgraded with One Handed Maces)"))
                .AddBool("keep_mount_type",
                         "{=ButterEquip014}Keep Mount Type",
                         new ProxyRef<bool>(() => eqOpt.KeepMountType, value => eqOpt.KeepMountType = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip014Hint}When enabled, only mounts of the same type will be equipped (e.g., only horses for equipped horse, or camels for equipped camel)"))
                .AddBool("keep_crafted",
                         "{=ButterEquip015}Keep Crafted Weapons",
                         new ProxyRef<bool>(() => eqOpt.KeepCrafted, value => eqOpt.KeepCrafted = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("keep_culture",
                         "{=ButterEquip016}Keep Culture",
                         new ProxyRef<bool>(() => eqOpt.KeepCulture, value => eqOpt.KeepCulture = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip016Hint}When enabled, only equipment from the same culture as the hero will be equipped (e.g., only Khuzait armor for Khuzait-culture heroes)"))
                .SetGroupOrder(3);

        void BuildAutoEquipGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("auto_equip_on_inventory_close",
                         "{=ButterEquip004}Auto Equip",
                         new ProxyRef<bool>(() => eqOpt.AutoEquipOnClose, value => eqOpt.AutoEquipOnClose = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(4);

        void BuildHighlightItemsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("highlight_better_items",
                         "{=ButterEquip017}Highlight Better Items",
                         new ProxyRef<bool>(() => hiOpt.HighlightBetterItems, value => hiOpt.HighlightBetterItems = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("highlight_from_loot",
                         "{=ButterEquip018}Highlight from Loot",
                         new ProxyRef<bool>(() => hiOpt.HighlightFromLoot, value => hiOpt.HighlightFromLoot = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("highlight_from_stash",
                         "{=ButterEquip019}Highlight from Stash",
                         new ProxyRef<bool>(() => hiOpt.HighlightFromStash, value => hiOpt.HighlightFromStash = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("highlight_from_inventory",
                         "{=ButterEquip020}Highlight from Inventory",
                         new ProxyRef<bool>(() => hiOpt.HighlightFromInventory, value => hiOpt.HighlightFromInventory = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("highlight_from_discard",
                         "{=ButterEquip021}Highlight from Discard",
                         new ProxyRef<bool>(() => hiOpt.HighlightFromDiscard, value => hiOpt.HighlightFromDiscard = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("highlight_from_trade",
                         "{=ButterEquip022}Highlight from Trade",
                         new ProxyRef<bool>(() => hiOpt.HighlightFromTrade, value => hiOpt.HighlightFromTrade = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(5);

    }
}
