using System;
using System.Collections;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace ButterEquipped.AutoEquip;

public class AutoEquipViewModel : ViewModel
{
    [DataSourceProperty]
    public bool Weapon0Locked
    {
        get => _slotLocks[(int)EquipmentIndex.Weapon0];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Weapon0])
            {
                _slotLocks[(int)EquipmentIndex.Weapon0] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool Weapon1Locked
    {
        get => _slotLocks[(int)EquipmentIndex.Weapon1];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Weapon1])
            {
                _slotLocks[(int)EquipmentIndex.Weapon1] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool Weapon2Locked
    {
        get => _slotLocks[(int)EquipmentIndex.Weapon2];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Weapon2])
            {
                _slotLocks[(int)EquipmentIndex.Weapon2] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool Weapon3Locked
    {
        get => _slotLocks[(int)EquipmentIndex.Weapon3];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Weapon3])
            {
                _slotLocks[(int)EquipmentIndex.Weapon3] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool HelmetLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Head];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Head])
            {
                _slotLocks[(int)EquipmentIndex.Head] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool CloakLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Cape];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Cape])
            {
                _slotLocks[(int)EquipmentIndex.Cape] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool ArmorLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Body];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Body])
            {
                _slotLocks[(int)EquipmentIndex.Body] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool GloveLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Gloves];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Gloves])
            {
                _slotLocks[(int)EquipmentIndex.Gloves] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool BootLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Leg];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Leg])
            {
                _slotLocks[(int)EquipmentIndex.Leg] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool MountLocked
    {
        get => _slotLocks[(int)EquipmentIndex.Horse];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Horse])
            {
                _slotLocks[(int)EquipmentIndex.Horse] = value;
                OnPropertyChanged();
            }
        }
    }

    [DataSourceProperty]
    public bool MountArmorLocked
    {
        get => _slotLocks[(int)EquipmentIndex.HorseHarness];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.HorseHarness])
            {
                _slotLocks[(int)EquipmentIndex.HorseHarness] = value;
                OnPropertyChanged();
            }
        }
    }

    public void ExecuteEquip() => OnEquip?.Invoke(this, EventArgs.Empty);

    private readonly IEquipmentSlotLockSource _slotLockSource;

    private SPInventoryVM _SPInventoryVM;

    private BitArray _slotLocks;

    public event EventHandler? OnEquip;

    public AutoEquipViewModel(SPInventoryVM SPInventoryVM, IEquipmentSlotLockSource slotLockSource)
    {
        _SPInventoryVM = SPInventoryVM;
        _slotLockSource = slotLockSource;
    }

    public void UpdateSlotLocks()
    {
        var heroId = _SPInventoryVM.CharacterList?.SelectedItem?.CharacterID;
        if (heroId is null)
        {
            return;
        }

        var heroSet = new HeroEquipmentSet(heroId, _SPInventoryVM.IsInWarSet);
        _slotLocks = _slotLockSource.GetSlotLocks(heroSet);

        this.OnPropertyChanged(nameof(HelmetLocked));
        this.OnPropertyChanged(nameof(CloakLocked));
        this.OnPropertyChanged(nameof(ArmorLocked));
        this.OnPropertyChanged(nameof(GloveLocked));
        this.OnPropertyChanged(nameof(BootLocked));

        this.OnPropertyChanged(nameof(MountLocked));
        this.OnPropertyChanged(nameof(MountArmorLocked));

        this.OnPropertyChanged(nameof(Weapon0Locked));
        this.OnPropertyChanged(nameof(Weapon1Locked));
        this.OnPropertyChanged(nameof(Weapon2Locked));
        this.OnPropertyChanged(nameof(Weapon3Locked));
    }
}
