using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Core.ItemObject;

namespace ButterEquipped.AutoEquip;

internal static class WeaponComponentDataExtensions
{
    public static ItemUsageSetFlags GetUsageFlags(this WeaponComponentData wcd)
    => wcd.ItemUsage switch
    {
        string usage => MBItem.GetItemUsageSetFlags(usage),
        _ => default
    };
}
