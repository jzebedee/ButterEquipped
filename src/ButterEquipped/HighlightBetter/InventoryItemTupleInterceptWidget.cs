using HarmonyLib.BUTR.Extensions;
using System;
using TaleWorlds.GauntletUI;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Inventory;
using TaleWorlds.MountAndBlade.Launcher.Library;

namespace ButterEquipped.HighlightBetter;

public class InventoryItemTupleInterceptWidget : InventoryItemTupleWidget
{
    private static readonly Action<InventoryItemTupleWidget> UpdateCivilianState
        = AccessTools2.GetDelegate<Action<InventoryItemTupleWidget>>(typeof(InventoryItemTupleWidget), nameof(UpdateCivilianState));

    public InventoryItemTupleInterceptWidget(UIContext context) : base(context)
    {
    }

    [Editor(false)]
    public bool IsBetterItem
    {
        get => _isBetterItem;
        set
        {
            if (_isBetterItem != value)
            {
                _isBetterItem = value;
                OnPropertyChanged(value);
                UpdateCivilianState(this);
            }
        }
    }

    [Editor(false)]
    public Brush BetterItemHighlightBrush
    {
        get => _betterItemHighlightBrush;
        set
        {
            if (_betterItemHighlightBrush != value)
            {
                _betterItemHighlightBrush = value;
                OnPropertyChanged(value);
            }
        }
    }

    private bool _isBetterItem;
    private Brush _betterItemHighlightBrush;
}
