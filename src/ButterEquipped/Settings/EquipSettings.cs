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
            .CreateGroup("Equip Targets", BuildEquipTargetsGroup)
            .CreateGroup("Equip From", BuildEquipFromGroup)
            .CreateGroup("Keep Items", BuildKeepItemsGroup)
            .CreateGroup("Auto Equip", BuildAutoEquipGroup)
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

                .SetPropertyValue("auto_equip_on_inventory_close", opt.AutoEquipOnClose);

        void BuildEquipTargetsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("equip_player",
                         "Equip Player Hero",
                         new ProxyRef<bool>(() => opt.EquipHero, value => opt.EquipHero = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_companions",
                         "Equip Companions",
                         new ProxyRef<bool>(() => opt.EquipCompanions, value => opt.EquipCompanions = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_civilian",
                         "Equip Civilian Equipment",
                         new ProxyRef<bool>(() => opt.EquipCivilian, value => opt.EquipCivilian = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(1);

        void BuildEquipFromGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("equip_from_loot",
                         "Equip from Loot",
                         new ProxyRef<bool>(() => opt.EquipFromLoot, value => opt.EquipFromLoot = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_stash",
                         "Equip from Stash",
                         new ProxyRef<bool>(() => opt.EquipFromStash, value => opt.EquipFromStash = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_inventory",
                         "Equip from Inventory",
                         new ProxyRef<bool>(() => opt.EquipFromInventory, value => opt.EquipFromInventory = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_discard",
                         "Equip from Discard",
                         new ProxyRef<bool>(() => opt.EquipFromDiscard, value => opt.EquipFromDiscard = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .AddBool("equip_from_trade",
                         "Equip from Trade",
                         new ProxyRef<bool>(() => opt.EquipFromTrade, value => opt.EquipFromTrade = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(2);

        void BuildKeepItemsGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("keep_weapon_class",
                         "Keep Weapon Class",
                         new ProxyRef<bool>(() => opt.KeepWeaponClass, value => opt.KeepWeaponClass = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("When enabled, only weapons of the same class will be equipped (e.g., One Handed Maces will only be upgraded with One Handed Maces)"))
                .AddBool("keep_mount_type",
                         "Keep Mount Type",
                         new ProxyRef<bool>(() => opt.KeepMountType, value => opt.KeepMountType = value),
                         propBuilder =>
                            propBuilder.SetRequireRestart(false)
                                       .SetHintText("When enabled, only mounts of the same type will be equipped (e.g., only horses for equipped horse, or camels for equipped camel)"))
                .AddBool("keep_crafted",
                         "Keep Crafted Weapons",
                         new ProxyRef<bool>(() => opt.KeepCrafted, value => opt.KeepCrafted = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(3);

        void BuildAutoEquipGroup(ISettingsPropertyGroupBuilder builder)
            => builder
                .AddBool("auto_equip_on_inventory_close",
                         "Auto Equip",
                         new ProxyRef<bool>(() => opt.AutoEquipOnClose, value => opt.AutoEquipOnClose = value),
                         propBuilder => propBuilder.SetRequireRestart(false))
                .SetGroupOrder(4);

    }
}
