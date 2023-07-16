using ButterEquipped.AutoEquip;
using HarmonyLib;
using MCM.Abstractions.Base.PerSave;
using System;
using System.Diagnostics;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace ButterEquipped;

public class SubModule : MBSubModuleBase
{
    private const string HarmonyId = $"{nameof(ButterEquipped)}.harmony";

    private static readonly Lazy<Harmony> _harmony = new(() => new Harmony(HarmonyId));

    public static Harmony Harmony => _harmony.Value;

    private AutoEquipBehavior? eqUpBehavior;

    private FluentPerSaveSettings? settings;

    public AutoEquipOptions? Options { get; private set; }

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

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