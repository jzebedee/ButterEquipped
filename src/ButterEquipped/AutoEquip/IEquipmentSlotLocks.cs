using System.Collections;
using System.Collections.Generic;

namespace ButterEquipped.AutoEquip;

public interface IEquipmentSlotLockSource
{
    Dictionary<HeroEquipmentSet, BitArray> SlotLocks { get; }

    BitArray GetSlotLocks(HeroEquipmentSet set);
}