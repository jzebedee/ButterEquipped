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
using TaleWorlds.Library;
using TaleWorlds.Library.EventSystem;

namespace ButterEquipped.AutoEquip;

public sealed class AutoEquipBehavior : CampaignBehaviorBase, IEquipmentSlotLockSource, IDisposable
{
    private static class PrivateMethods
    {
        public static HarmonyLib.AccessTools.FieldRef<GauntletInventoryScreen, SPInventoryVM>? GauntletInventoryScreen_DataSource
            = AccessTools2.FieldRefAccess<GauntletInventoryScreen, SPInventoryVM>("_dataSource");
    }

    private readonly AutoEquipOptions options;

    private Dictionary<HeroEquipmentSet, BitArray>? slotLocks;
    public Dictionary<HeroEquipmentSet, BitArray> SlotLocks => slotLocks!;

    private bool eventsRegistered;
    private bool disposed;

    private AutoEquipLogic? eqUpLogic;
    private AutoEquipViewModel? eqUpVm;
    private SPInventoryVM? spInventoryVM;

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
        InventoryScreenHelper_CloseInventoryPresentationPatch.OnClosing += HandleClose;
        eventsRegistered = true;
    }

    private void UpdateViewModel()
    {
        //InventoryEquipmentTypeChanged can fire before the inventory screen is initialized
        //when first opening inventory in a civilian mission from a campaign / war inventory
        if (eqUpVm is null || this.spInventoryVM is not SPInventoryVM vm)
        {
            return;
        }

        eqUpVm.IsEquipVisible = vm is { CharacterList.SelectedItem.Hero: not null };
        eqUpVm.IsEquipPartyVisible = options.EquipCompanions && vm is { CharacterList.HasSingleItem: false };
        eqUpVm.UpdateSlotLocks();
    }

    private void HandleInventoryPropertyChanged(object sender, PropertyChangedWithValueEventArgs e)
    {
        if (e.PropertyName is not nameof(SPInventoryVM.CurrentCharacterName))
        {
            return;
        }

        UpdateViewModel();
    }

    private void OnInventoryEquipmentTypeChanged(InventoryEquipmentTypeChangedEvent e)
        => UpdateViewModel();

    private void HandleInventoryScreenInitialized(GauntletInventoryScreen inventoryScreen)
    {
        if (inventoryScreen is null)
        {
            return;
        }

        if (PrivateMethods.GauntletInventoryScreen_DataSource?.Invoke(inventoryScreen) is not SPInventoryVM spInventoryVM)
        {
            return;
        }

        this.spInventoryVM = spInventoryVM;

        eqUpLogic = new AutoEquipLogic(options with { } /* shallow record clone */, spInventoryVM, this);

        eqUpVm = new AutoEquipViewModel(spInventoryVM, this);
        eqUpVm.OnEquip += HandleEquip;
        UpdateViewModel();

        spInventoryVM.PropertyChangedWithValue += HandleInventoryPropertyChanged;

        inventoryScreen.AddLayer(CreateInventoryLayer());
    }

    private GauntletLayer CreateInventoryLayer()
    {
        const string inventoryAutoEquipPrefab = "InventoryAutoEquip";

        var gauntletLayer = new GauntletLayer("ButterEquipped", localOrder: 109);
        gauntletLayer.LoadMovie(inventoryAutoEquipPrefab, eqUpVm);
        gauntletLayer.InputRestrictions.SetInputRestrictions();
        return gauntletLayer;
    }

    private bool HandleEquip(AutoEquipViewModel.AutoEquipEventArgs? e)
        => e switch
        {
            AutoEquipViewModel.EquipPartyEventArgs => eqUpLogic?.EquipParty(),
            AutoEquipViewModel.EquipHeroEventArgs eqHero => eqUpLogic?.Equip(eqHero.Hero, eqHero.Mode),
            _ => false
        } ?? false;

    private void HandleClose(bool fromCancel)
    {
        if (eqUpVm is not AutoEquipViewModel eqVM)
        {
            return;
        }

        if (!fromCancel && options.AutoEquipOnClose)
        {
            eqVM.ExecuteEquipParty();
        }
        eqVM.OnEquip -= HandleEquip;

        if (spInventoryVM is not SPInventoryVM inventoryVM)
        {
            return;
        }
        spInventoryVM.PropertyChangedWithValue -= HandleInventoryPropertyChanged;
    }

    public BitArray GetSlotLocks(HeroEquipmentSet set)
    {
        const int SlotLockLength = (int)EquipmentIndex.NumEquipmentSetSlots;

        var setLocks = slotLocks ??= [];
        if (!setLocks.TryGetValue(set, out var locks))
        {
            setLocks.Add(set, locks = new BitArray(SlotLockLength));
        }
        else if (locks is { Length: not SlotLockLength })
        {
            locks.Length = SlotLockLength;
        }

        return locks;
    }

    public override void SyncData(IDataStore dataStore)
    {
        dataStore.SyncData<Dictionary<HeroEquipmentSet, BitArray>>("lockedSlots", ref slotLocks!);
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
            InventoryScreenHelper_CloseInventoryPresentationPatch.OnClosing -= HandleClose;
        }

        disposed = true;
    }
}
