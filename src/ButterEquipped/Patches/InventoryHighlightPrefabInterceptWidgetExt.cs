using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.ResourceManager;
using ButterEquipped.HighlightBetter;
using System.Collections.Generic;
using System.Xml;

namespace ButterEquipped.Patches;

[PrefabExtension("InventoryItemTuple", "//InventoryItemTupleWidget")]
internal class InventoryHighlightPrefabInterceptWidgetExt : PrefabExtensionInsertPatch
{
    private const string _widgetReplacementText = """
        <InventoryItemTupleInterceptWidget 
        VisualDefinition="Container" 
        IsHidden="@IsFiltered" 
        ButtonType="Radio" 
        DragWidget="DragWidget" 
        Brush="Inventory.Tuple.SoundBrush" 
        Command.Click="ExecuteSelectItem" 
        Command.PreviewItem="{ExecutePreviewItem}" 
        Command.HoverBegin="ExecuteSetFocused" 
        Command.HoverEnd="ExecuteSetUnfocused" 
        Command.SellItem="ExecuteSellItem" 
        Command.EquipItem="ExecuteEquipItem" 
        Command.UnequipItem="ExecuteUnequipItem" 
        Command.Opened="ExecuteResetTrade" 
        Command.OnAlternateRelease="ExecuteConcept" 
        WidthSizePolicy="Fixed" 
        HeightSizePolicy="CoverChildren" 
        HorizontalAlignment="Center" 
        IsRightSide="*IsPlayerItem" 
        NameTextWidget="Main\Body\MainControls\NameText" 
        CountTextWidget="Main\Body\MainControls\CountTextParent\CountText" 
        CostTextWidget="Main\Body\MainControls\CostTextParent\CostText" 
        MainContainer="Main" 
        ExtendedControlsContainer="Extension" 
        TransferButton="Main\TransferButtonParent\TransferButton" 
        EquipButton="Extension\ExtensionCarrier\ButtonCarrier\EquipButton" 
        SliderTransferButton="Extension\ExtensionCarrier\ButtonCarrier\SliderTransferButton" 
        ViewButton="Extension\ExtensionCarrier\ButtonCarrier\PreviewButton" 
        Slider="Extension\ExtensionCarrier\SliderParent\TransferSlider" 
        SliderParent="Extension\ExtensionCarrier\SliderParent" 
        SliderTextWidget="Extension\ExtensionCarrier\SliderParent\SliderTextWidget" 
        TransactionCount="@TransactionCount" 
        IsTransferable="@IsTransferable" 
        ItemCount="@ItemCount" 
        ProfitState="@ProfitIndex" 
        IsEquipable="@IsEquipableItem" 
        CanCharacterUseItem="@CanCharacterUseItem" 
        BetterItemScore="@ButterEquippedScore" 
        IsBetterItem="@ButterEquippedIsItemBetter" 
        IsCivilian="@IsCivilianItem" 
        IsGenderDifferent="@IsGenderDifferent" 
        ItemType="@TypeId" 
        EquipmentIndex="@ItemType" 
        DefaultBrush="!Inventory.Tuple" 
        CivilianDisabledBrush="!Inventory.Tuple.Civillian" 
        CharacterCantUseBrush="!Inventory.Tuple.CharacterCantUse" 
        BetterItemHighlightBrush="!ButterEquipped.BetterItem" 
        MarginTop="2" 
        ItemID="@StringId" 
        HoveredCursorState="RightClickLink" 
        ItemImageIdentifier="Main\Body\MainControls\ImageIdentifier" 
        IsNewlyAdded="@IsNew">
        </InventoryItemTupleInterceptWidget>
        """;

    private readonly XmlDocument _widgetDocument;

    public InventoryHighlightPrefabInterceptWidgetExt()
    {
        _widgetDocument = new XmlDocument();
        _widgetDocument.LoadXml(_widgetReplacementText);
        WidgetFactoryManager.Register(typeof(InventoryItemTupleInterceptWidget));
    }

    public override InsertType Type => InsertType.ReplaceKeepChildren;
    public override int Index => 0;

    [PrefabExtensionXmlNodes]
    public IEnumerable<XmlNode> GetNodes()
    {
        yield return _widgetDocument;
    }
}

