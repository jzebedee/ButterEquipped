using HarmonyLib;
using System.Reflection;
using System;
using TaleWorlds.MountAndBlade;

namespace ButterQuarters
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = $"{nameof(ButterQuarters)}.harmony";

        private readonly Lazy<Harmony> _harmony = new(() => new Harmony(HarmonyId));

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            _harmony.Value.PatchAll(Assembly.GetExecutingAssembly());
        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }
    }
}