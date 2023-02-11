using HarmonyLib.BUTR.Extensions;
using SandBox.GauntletUI;
using System;
using System.Collections;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library.EventSystem;
using TaleWorlds.ScreenSystem;
namespace ButterEquipped.AutoEquip;

public sealed class AutoEquipBehavior : CampaignBehaviorBase, IEquipmentSlotLockSource, IDisposable
{
    private bool eventsRegistered;
    private bool disposed;

    private Dictionary<HeroEquipmentSet, BitArray> slotLocks;
    public Dictionary<HeroEquipmentSet, BitArray> SlotLocks => slotLocks;

    private AutoEquipOptions options;

    private SPInventoryVM spInventoryVm;
    private AutoEquipViewModel eqUpVm;
    private GauntletLayer gauntletLayer;

    public AutoEquipBehavior(AutoEquipOptions options)
    {
        this.options = options;
    }

    public override void RegisterEvents()
    {
        if(Game.Current.EventManager is EventManager manager)
        {
            manager.RegisterEvent<InventoryEquipmentTypeChangedEvent>(OnInventoryEquipmentTypeChanged);
        }

        ScreenManager.OnPushScreen += ScreenManager_OnPushScreen;
        ScreenManager.OnPopScreen += ScreenManager_OnPopScreen;
        AutoEquipPatches_InventoryManager_CloseInventoryPresentation.OnClosing += HandleClose;
        eventsRegistered = true;
    }

    private void UpdateViewModel()
    {
        var hero = spInventoryVm.CharacterList?.SelectedItem?.Hero;
        if (hero is null)
        {
            return;
        }

        eqUpVm.IsEquipVisible = true;
        eqUpVm.IsEquipPartyVisible = !spInventoryVm.CharacterList!.HasSingleItem && options.EquipCompanions;
        eqUpVm.UpdateSlotLocks();
    }

    private void OnInventoryEquipmentTypeChanged(InventoryEquipmentTypeChangedEvent e)
        => UpdateViewModel();

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

        this.spInventoryVm = spInventoryVm;

        var eqUpLogic = new AutoEquipLogic(spInventoryVm, this);

        eqUpVm = new AutoEquipViewModel(spInventoryVm, this);
        eqUpVm.OnEquip += (sender, e) => _ = e switch
        {
            AutoEquipViewModel.EquipPartyEventArgs => eqUpLogic.EquipParty(options),
            AutoEquipViewModel.EquipHeroEventArgs eqHero => eqUpLogic.Equip(options, eqHero.Hero, eqHero.Civilian),
            _ => false
        };
        UpdateViewModel();

        spInventoryVm.PropertyChangedWithValue += (sender, e) =>
        {
            if (e.PropertyName is not nameof(SPInventoryVM.CurrentCharacterName))
            {
                return;
            }

            UpdateViewModel();
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
            eqUpVm.ExecuteEquipParty();
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
            if (Game.Current.EventManager is EventManager manager)
            {
                manager.UnregisterEvent<InventoryEquipmentTypeChangedEvent>(OnInventoryEquipmentTypeChanged);
            }

            ScreenManager.OnPushScreen -= ScreenManager_OnPushScreen;
            ScreenManager.OnPopScreen -= ScreenManager_OnPopScreen;
            AutoEquipPatches_InventoryManager_CloseInventoryPresentation.OnClosing -= HandleClose;
        }

        disposed = true;
    }
}
