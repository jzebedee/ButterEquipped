using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Library;

namespace ButterEquipped.Patches;

internal class TwoWayViewModelMixin<TViewModel> : BaseViewModelMixin<TViewModel> where TViewModel : ViewModel
{
    private static readonly ConditionalWeakTable<TViewModel, WeakReference<TwoWayViewModelMixin<TViewModel>>> _twoWayMap = new();

    internal static WeakReference<TwoWayViewModelMixin<TViewModel>> GetVmMixin(TViewModel viewModel)
        => _twoWayMap.GetValue(viewModel, static _ => new(null!));

    public TwoWayViewModelMixin(TViewModel vm) : base(vm)
    {
        //not thread safe.
        //may need to synchronize ourselves since we don't have access to the net6.0+ AddOrUpdate

        var weakRef = _twoWayMap.GetValue(vm, /* avoid closure */ static _ => new(null!));
        weakRef.SetTarget(this);
    }
}
