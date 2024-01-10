using ButterEquipped.Patches;
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

namespace ButterEquipped.AutoEquip;

public sealed class AutoEquipBehavior : CampaignBehaviorBase, IEquipmentSlotLockSource, IDisposable
{
    private bool eventsRegistered;
    private bool disposed;

    private Dictionary<HeroEquipmentSet, BitArray> slotLocks;
    public Dictionary<HeroEquipmentSet, BitArray> SlotLocks => slotLocks;

    private readonly AutoEquipOptions options;

    private AutoEquipLogic eqUpLogic;
    private SPInventoryVM spInventoryVm;
    private AutoEquipViewModel eqUpVm;
    private GauntletLayer gauntletLayer;

    public AutoEquipBehavior(AutoEquipOptions options)
    {
        this.options = options;
    }

    public override void RegisterEvents()
    {
        if (Game.Current.EventManager is EventManager manager)
        {
            manager.RegisterEvent<InventoryEquipmentTypeChangedEvent>(OnInventoryEquipmentTypeChanged);
        }

        GauntletInventoryScreen_OnInitializePatch.OnInitialize += HandleInventoryScreenInitialized;
        InventoryManager_CloseInventoryPresentationPatch.OnClosing += HandleClose;
        eventsRegistered = true;
    }

    private void UpdateViewModel()
    {
        //InventoryEquipmentTypeChanged can fire before the inventory screen is initialized
        //when first opening inventory in a civilian mission from a campaign / war inventory
        if (eqUpVm is null)
        {
            return;
        }

        eqUpVm.IsEquipVisible = spInventoryVm is { CharacterList.SelectedItem.Hero: not null };
        eqUpVm.IsEquipPartyVisible = options.EquipCompanions && spInventoryVm is { CharacterList.HasSingleItem: false };
        eqUpVm.UpdateSlotLocks();
    }

    private void OnInventoryEquipmentTypeChanged(InventoryEquipmentTypeChangedEvent e)
        => UpdateViewModel();

    private void HandleInventoryScreenInitialized(GauntletInventoryScreen inventoryScreen)
    {
        if (inventoryScreen is null)
        {
            return;
        }

        var dataSourceTraverse = Traverse2.Create(inventoryScreen).Field<SPInventoryVM>("_dataSource");
        if (dataSourceTraverse.Value is not SPInventoryVM spInventoryVm)
        {
            return;
        }

        this.spInventoryVm = spInventoryVm;

        eqUpLogic = new AutoEquipLogic(spInventoryVm, this);

        eqUpVm = new AutoEquipViewModel(spInventoryVm, this);
        eqUpVm.OnEquip += HandleEquip;
        UpdateViewModel();

        spInventoryVm.PropertyChangedWithValue += (sender, e) =>
        {
            if (e.PropertyName is not nameof(SPInventoryVM.CurrentCharacterName))
            {
                return;
            }

            UpdateViewModel();
        };

        inventoryScreen.AddLayer(CreateInventoryLayer());
    }

    private GauntletLayer CreateInventoryLayer()
    {
        const string inventoryAutoEquipPrefab = "InventoryAutoEquip";
        gauntletLayer = new GauntletLayer(localOrder: 116);
        gauntletLayer.LoadMovie(inventoryAutoEquipPrefab, eqUpVm);
        gauntletLayer.InputRestrictions.SetInputRestrictions();
        return gauntletLayer;
    }

    private bool HandleEquip(AutoEquipViewModel.AutoEquipEventArgs? e)
        => e switch
        {
            AutoEquipViewModel.EquipPartyEventArgs => eqUpLogic.EquipParty(options),
            AutoEquipViewModel.EquipHeroEventArgs eqHero => eqUpLogic.Equip(options, eqHero.Hero, eqHero.Civilian),
            _ => false
        };

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

            GauntletInventoryScreen_OnInitializePatch.OnInitialize -= HandleInventoryScreenInitialized;
            InventoryManager_CloseInventoryPresentationPatch.OnClosing -= HandleClose;
        }

        disposed = true;
    }
}
