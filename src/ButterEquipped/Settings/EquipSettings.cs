using ButterEquipped.AutoEquip;
using MCM.Abstractions.Base;
using MCM.Abstractions.FluentBuilder;
using MCM.Common;
using System;

namespace ButterEquipped.Settings;

internal static class EquipSettings
{
    private static Version CurrentVersion => typeof(EquipSettings).Assembly.GetName().Version;

    private static string SettingsId => $"{nameof(ButterEquipped)}_v{CurrentVersion.ToString(1)}";

    private static string SettingsName => $"Butter Equipped {CurrentVersion.ToString(3)}";

    public static ISettingsBuilder AddEquipSettings(AutoEquipOptions opt, string id)
    {
        var builder = BaseSettingsBuilder.Create(SettingsId, SettingsName)!
            .SetFormat("json2")
            .SetFolderName(nameof(ButterEquipped))
            //.SetSubFolder(id)
            .CreateGroup("{=ButterEquip001}Equip Targets", BuildEquipTargetsGroup)
            .CreateGroup("{=ButterEquip002}Equip From", BuildEquipFromGroup)
            .CreateGroup("{=ButterEquip003}Keep Items", BuildKeepItemsGroup)
            .CreateGroup("{=ButterEquip004}Auto Equip", BuildAutoEquipGroup)
            //.WithoutDefaultPreset()
            .CreatePreset(BaseSettings.DefaultPresetId, BaseSettings.DefaultPresetName, builder => BuildDefaultPreset(builder, new()));

        return builder;

        static void BuildDefaultPreset(ISettingsPresetBuilder builder, AutoEquipOptions opt)
            => builder
                .SetPropertyValue("equip_player", opt.EquipHero)
                .SetPropertyValue("equip_companions", opt.EquipCompanions)
                .SetPropertyValue("equip_civilian", opt.EquipCivilian)

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
                         new ProxyRef<bool>(() => opt.EquipHero, value => opt.EquipHero = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_companions",
                         "{=ButterEquip006}Equip Companions",
                         new ProxyRef<bool>(() => opt.EquipCompanions, value => opt.EquipCompanions = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_civilian",
                         "{=ButterEquip007}Equip Civilian Equipment",
                         new ProxyRef<bool>(() => opt.EquipCivilian, value => opt.EquipCivilian = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(1);

        void BuildEquipFromGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("equip_from_loot",
                         "{=ButterEquip008}Equip from Loot",
                         new ProxyRef<bool>(() => opt.EquipFromLoot, value => opt.EquipFromLoot = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_stash",
                         "{=ButterEquip009}Equip from Stash",
                         new ProxyRef<bool>(() => opt.EquipFromStash, value => opt.EquipFromStash = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_inventory",
                         "{=ButterEquip010}Equip from Inventory",
                         new ProxyRef<bool>(() => opt.EquipFromInventory, value => opt.EquipFromInventory = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_discard",
                         "{=ButterEquip011}Equip from Discard",
                         new ProxyRef<bool>(() => opt.EquipFromDiscard, value => opt.EquipFromDiscard = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_trade",
                         "{=ButterEquip012}Equip from Trade",
                         new ProxyRef<bool>(() => opt.EquipFromTrade, value => opt.EquipFromTrade = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(2);

        void BuildKeepItemsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("keep_weapon_class",
                         "{=ButterEquip013}Keep Weapon Class",
                         new ProxyRef<bool>(() => opt.KeepWeaponClass, value => opt.KeepWeaponClass = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip013Hint}When enabled, only weapons of the same class will be equipped (e.g., One Handed Maces will only be upgraded with One Handed Maces)"))
                .AddBool("keep_mount_type",
                         "{=ButterEquip014}Keep Mount Type",
                         new ProxyRef<bool>(() => opt.KeepMountType, value => opt.KeepMountType = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip014Hint}When enabled, only mounts of the same type will be equipped (e.g., only horses for equipped horse, or camels for equipped camel)"))
                .AddBool("keep_crafted",
                         "{=ButterEquip015}Keep Crafted Weapons",
                         new ProxyRef<bool>(() => opt.KeepCrafted, value => opt.KeepCrafted = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("keep_culture",
                         "{=ButterEquip016}Keep Culture",
                         new ProxyRef<bool>(() => opt.KeepCulture, value => opt.KeepCulture = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("{=ButterEquip016Hint}When enabled, only equipment from the same culture as the hero will be equipped (e.g., only Khuzait armor for Khuzait-culture heroes)"))
                .SetGroupOrder(3);

        void BuildAutoEquipGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("auto_equip_on_inventory_close",
                         "{=ButterEquip004}Auto Equip",
                         new ProxyRef<bool>(() => opt.AutoEquipOnClose, value => opt.AutoEquipOnClose = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(4);

    }
}
