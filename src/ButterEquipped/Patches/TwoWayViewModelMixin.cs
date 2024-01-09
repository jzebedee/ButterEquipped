using Bannerlord.UIExtenderEx.ViewModels;
using System;
using System.Runtime.CompilerServices;
using TaleWorlds.Library;

namespace ButterEquipped.Patches;

internal class TwoWayViewModelMixin<TViewModel> : BaseViewModelMixin<TViewModel> where TViewModel : ViewModel
{
    private static readonly ConditionalWeakTable<TViewModel, WeakReference<TwoWayViewModelMixin<TViewModel>>> _twoWayMap = [];

    public TwoWayViewModelMixin(TViewModel vm) : base(vm)
    {
        //not thread safe.
        //may need to synchronize ourselves since we don't have access to the net6.0+ AddOrUpdate

        if (!_twoWayMap.TryGetValue(vm, out var weakRef))
        {
            _twoWayMap.Add(vm, new(this));
            return;
        }

        weakRef.SetTarget(this);
    }
}
