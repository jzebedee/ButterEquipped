using ButterEquipped.Patches;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;

namespace ButterEquipped.HighlightBetter;

internal static class SPItemVMExtensions
{
    public static SPItemVMMixin? GetMixinForVM(this SPItemVM vm)
    {
        if(vm is null)
        {
            return null;
        }

        var mixinRef = TwoWayViewModelMixin<SPItemVM>.GetVmMixin(vm);
        if (!mixinRef.TryGetTarget(out var mixinBase) || mixinBase is not SPItemVMMixin mixinVm)
        {
            return null;
        }

        return mixinVm;
    }
}
