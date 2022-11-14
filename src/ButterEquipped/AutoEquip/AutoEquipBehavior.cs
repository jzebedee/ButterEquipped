using HarmonyLib.BUTR.Extensions;
using SandBox.GauntletUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.ScreenSystem;

namespace ButterEquipped.AutoEquip;

public sealed class AutoEquipBehavior : CampaignBehaviorBase, IEquipmentSlotLockSource, IDisposable
{
    private bool eventsRegistered;
    private bool disposed;

    private AutoEquipViewModel eqUpVm;

    private GauntletLayer gauntletLayer;

    private Dictionary<HeroEquipmentSet, BitArray> slotLocks;
    public Dictionary<HeroEquipmentSet, BitArray> SlotLocks => slotLocks;

    private AutoEquipOptions options;

    private static readonly HashSet<string> UpdateOnProperties = new()
    {
        nameof(SPInventoryVM.CurrentCharacterName),
        nameof(SPInventoryVM.IsInWarSet)
    };

    public AutoEquipBehavior(AutoEquipOptions options)
    {
        this.options = options;
    }

    public override void RegisterEvents()
    {
        ScreenManager.OnPushScreen += ScreenManager_OnPushScreen;
        ScreenManager.OnPopScreen += ScreenManager_OnPopScreen;
        AutoEquipPatches_InventoryManager_CloseInventoryPresentation.OnClosing += HandleClose;
        eventsRegistered = true;
    }

    private void ScreenManager_OnPopScreen(ScreenBase poppedScreen)
    {
        if (poppedScreen is not GauntletInventoryScreen inventoryScreen)
        {
            return;
        }

        inventoryScreen.RemoveLayer(gauntletLayer);
    }

    private void ScreenManager_OnPushScreen(ScreenBase pushedScreen)
    {
        if (pushedScreen is not GauntletInventoryScreen inventoryScreen)
        {
            return;
        }

        var dataSourceTraverse = Traverse2.Create(inventoryScreen).Field<SPInventoryVM>("_dataSource");
        if (dataSourceTraverse.Value is not SPInventoryVM spInventoryVm)
        {
            return;
        }

        var eqUpLogic = new AutoEquipLogic(spInventoryVm, this);

        eqUpVm = new AutoEquipViewModel(spInventoryVm, this);
        eqUpVm.OnEquip += (sender, _) => eqUpLogic.EquipAll(options);
        eqUpVm.UpdateSlotLocks();

        spInventoryVm.PropertyChangedWithValue += (sender, e) =>
        {
            if (!UpdateOnProperties.Contains(e.PropertyName))
            {
                return;
            }

            eqUpVm.UpdateSlotLocks();
        };

        const string inventoryAutoEquipPrefab = "InventoryAutoEquip";
        gauntletLayer = new GauntletLayer(localOrder: 116);
        gauntletLayer.LoadMovie(inventoryAutoEquipPrefab, eqUpVm);

        inventoryScreen.AddLayer(gauntletLayer);
        gauntletLayer.InputRestrictions.SetInputRestrictions();

    }

    private void HandleClose(object sender, bool fromCancel)
    {
        if (!fromCancel && options.AutoEquipOnClose)
        {
            eqUpVm.ExecuteEquip();
        }
    }

    public BitArray GetSlotLocks(HeroEquipmentSet set)
    {
        var setLocks = slotLocks ??= new Dictionary<HeroEquipmentSet, BitArray>();

        if (!setLocks.TryGetValue(set, out var locks))
        {
            setLocks.Add(set, locks = new BitArray((int)EquipmentIndex.NumEquipmentSetSlots));
        }

        return locks;
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData<Dictionary<HeroEquipmentSet, BitArray>>("lockedSlots", ref slotLocks);
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (eventsRegistered)
        {
            ScreenManager.OnPushScreen -= ScreenManager_OnPushScreen;
            ScreenManager.OnPopScreen -= ScreenManager_OnPopScreen;
            AutoEquipPatches_InventoryManager_CloseInventoryPresentation.OnClosing -= HandleClose;
        }

        disposed = true;
    }
}
