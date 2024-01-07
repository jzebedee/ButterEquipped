using Bannerlord.UIExtenderEx;
using ButterEquipped.AutoEquip;
using ButterEquipped.HighlightBetter;
using ButterEquipped.Settings;
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
    private HighlightBetterBehavior? highlightBehavior;

    private FluentPerSaveSettings? settings;

    public AutoEquipOptions? AutoEquipOptions { get; private set; }

    public HighlightBetterOptions? HighlightBetterOptions { get; private set; }

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

        AutoEquipOptions ??= new();
        HighlightBetterOptions ??= new();
        campaignGameStarter.AddBehavior(eqUpBehavior = new AutoEquipBehavior(AutoEquipOptions));
        campaignGameStarter.AddBehavior(highlightBehavior = new HighlightBetterBehavior(HighlightBetterOptions));
    }

    public override void OnAfterGameInitializationFinished(Game game, object starterObject)
    {
        if (game.GameType is not Campaign campaign)
        {
            return;
        }

        Debug.Assert(settings is null);
        var builder = EquipSettings.AddEquipSettings(AutoEquipOptions!, HighlightBetterOptions!, campaign.UniqueGameId);
        settings = builder.BuildAsPerSave();
        settings?.Register();
    }

    public override void OnGameEnd(Game game)
    {
        eqUpBehavior?.Dispose();
        eqUpBehavior = null;

        highlightBehavior?.Dispose();
        highlightBehavior = null;

        var oldSettings = settings;
        oldSettings?.Unregister();
        settings = null;

        AutoEquipOptions = null;
        HighlightBetterOptions = null;
    }
}