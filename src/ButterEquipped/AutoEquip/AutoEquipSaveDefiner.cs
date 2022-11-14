using System;
using System.Collections;
using System.Collections.Generic;
using TaleWorlds.SaveSystem;

namespace ButterEquipped.AutoEquip;

public class AutoEquipSaveDefiner : SaveableTypeDefiner
{
    private const int SaveBaseId = 0xBE * 0xBAD * 0xAF;

    public AutoEquipSaveDefiner() : base(SaveBaseId) { }

    protected override void DefineClassTypes()
    {
        AddClassDefinition(typeof(HeroEquipmentSet), 1);
        AddClassDefinitionWithCustomFields(typeof(BitArray), 2, GetCustomFields_BitArray());

        static IEnumerable<Tuple<string, short>> GetCustomFields_BitArray()
        {
            yield return new("m_array", 1);
            yield return new("m_length", 2);
            yield return new("_version", 3);
        }
    }

    protected override void DefineContainerDefinitions()
    {
        ConstructContainerDefinition(typeof(Dictionary<HeroEquipmentSet, BitArray>));
    }
}
