using System;
using System.Collections;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace ButterEquipped.AutoEquip;

public class AutoEquipViewModel : ViewModel
{
    private static class Messages
    {
        public static readonly TextObject Equip = new("{=ButterEquipVM001}Equip");

        public static readonly TextObject Party = new("{=ButterEquipVM002}Party");
    }

    [DataSourceProperty]
    public bool Weapon0Locked
    {
        get => _slotLocks[(int)EquipmentIndex.Weapon0];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.Weapon0])
            {
                _slotLocks[(int)EquipmentIndex.Weapon0] = value;
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool ExtraWeaponLocked
    {
        get => _slotLocks[(int)EquipmentIndex.ExtraWeaponSlot];
        set
        {
            if (value != _slotLocks[(int)EquipmentIndex.ExtraWeaponSlot])
            {
                _slotLocks[(int)EquipmentIndex.ExtraWeaponSlot] = value;
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
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
                OnPropertyChangedWithValue(value);
            }
        }
    }

    private bool _isEquipVisible;

    [DataSourceProperty]
    public bool IsEquipVisible
    {
        get => _isEquipVisible;
        set
        {
            if (value != _isEquipVisible)
            {
                _isEquipVisible = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    private bool _isEquipPartyVisible;

    [DataSourceProperty]
    public bool IsEquipPartyVisible
    {
        get => _isEquipPartyVisible;
        set
        {
            if (value != _isEquipPartyVisible)
            {
                _isEquipPartyVisible = value;
                OnPropertyChangedWithValue(value);
            }
        }
    }

    [DataSourceProperty]
    public bool IsInWarSet => _spInventoryVM.IsInWarSet;

    [DataSourceProperty]
    public string EquipText { get; } = Messages.Equip.ToString();

    [DataSourceProperty]
    public string PartyText { get; } = Messages.Party.ToString();

    public void ExecuteEquip()
        => OnEquip?.Invoke(new EquipHeroEventArgs(_spInventoryVM.CharacterList.SelectedItem.Hero, !_spInventoryVM.IsInWarSet));

    public void ExecuteEquipParty()
        => OnEquip?.Invoke(EquipPartyEventArgs.Empty);

    public event Func<AutoEquipEventArgs?, bool> OnEquip;

    private readonly IEquipmentSlotLockSource _slotLockSource;

    private readonly SPInventoryVM _spInventoryVM;

    private BitArray _slotLocks;

    public record AutoEquipEventArgs;

    public record EquipPartyEventArgs : AutoEquipEventArgs
    {
        public static readonly EquipPartyEventArgs Empty = new();
    }

    public record EquipHeroEventArgs(Hero Hero, bool Civilian) : AutoEquipEventArgs;

    public AutoEquipViewModel(SPInventoryVM spInventoryVM, IEquipmentSlotLockSource slotLockSource)
    {
        _spInventoryVM = spInventoryVM;
        _slotLockSource = slotLockSource;
    }

    public void UpdateSlotLocks()
    {
        if(_spInventoryVM is not { CharacterList.SelectedItem.CharacterID: string heroId })
        {
            return;
        }

        var heroSet = new HeroEquipmentSet(heroId, _spInventoryVM.IsInWarSet);
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
        this.OnPropertyChanged(nameof(ExtraWeaponLocked));

        this.OnPropertyChanged(nameof(IsInWarSet));
    }
}
