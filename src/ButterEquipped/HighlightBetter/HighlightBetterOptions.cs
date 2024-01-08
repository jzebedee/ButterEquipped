namespace ButterEquipped.HighlightBetter;

public record class HighlightBetterOptions
{
    public bool HighlightBetterItems { get; set; } = true;
    public bool HighlightFromLoot { get; set; } = true;
    public bool HighlightFromStash { get; set; } = true;
    public bool HighlightFromInventory { get; set; } = true;
    public bool HighlightFromDiscard { get; set; } = true;
    public bool HighlightFromTrade { get; set; } = false;
}
