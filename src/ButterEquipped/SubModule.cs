using Bannerlord.UIExtenderEx;
using ButterEquipped.AutoEquip;
using ButterEquipped.HighlightBetter;
using HarmonyLib;
using MCM.Abstractions.Base.PerSave;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ButterEquipped;

public class SubModule : MBSubModuleBase
{
    private const string HarmonyId = $"{nameof(ButterEquipped)}.harmony";

    private static readonly Lazy<Harmony> _harmony = new(() => new Harmony(HarmonyId));

    private static readonly Lazy<UIExtender> _extender = new(() =>
    {
        var extender = new UIExtender(nameof(ButterEquipped));
        extender.Register(typeof(SubModule).Assembly);
        return extender;
    });

    public static string ModuleDirectory => Path.GetDirectoryName(typeof(SubModule).Assembly.Location);

    public static Harmony Harmony => _harmony.Value;
    public static UIExtender UIExtender => _extender.Value;

    private AutoEquipBehavior? eqUpBehavior;

    private FluentPerSaveSettings? settings;

    public AutoEquipOptions? Options { get; private set; }

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        UIExtender.Enable();
        Harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    protected override void OnGameStart(Game game, IGameStarter gameStarter)
    {
        if (gameStarter is not CampaignGameStarter campaignGameStarter)
        {
            return;
        }

        Options ??= new();
        campaignGameStarter.AddBehavior(eqUpBehavior = new AutoEquipBehavior(Options));
        campaignGameStarter.AddBehavior(new HighlightBetterBehavior());
    }

    public override void OnAfterGameInitializationFinished(Game game, object starterObject)
    {
        if (game.GameType is not Campaign campaign)
        {
            return;
        }

        Debug.Assert(settings is null);
        var builder = Settings.EquipSettings.AddEquipSettings(Options!, campaign.UniqueGameId);
        settings = builder.BuildAsPerSave();
        settings?.Register();
    }

    public override void OnGameEnd(Game game)
    {
        var oldBehavior = eqUpBehavior;
        oldBehavior?.Dispose();
        eqUpBehavior = null;

        var oldSettings = settings;
        oldSettings?.Unregister();
        settings = null;

        Options = null;
    }
}