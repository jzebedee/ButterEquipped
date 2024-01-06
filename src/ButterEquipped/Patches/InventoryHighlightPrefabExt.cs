using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.ResourceManager;
using ButterEquipped.HighlightBetter;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ButterEquipped.Patches;

//[PrefabExtension("InventoryItemTuple", "//Constants/Constant[1]")]
//internal class InventoryHighlightPrefab1Ext : PrefabExtensionInsertPatch
//{
//    public override InsertType Type => InsertType.Append;

//    // The file should have an extension of type .xml, and be located inside of the GUI folder of your module.
//    // You can include or omit the extension type. I.e. both of the following would work:
//    //   ExampleFileInjectedPatch
//    //   ExampleFileInjectedPatch.xml
//    [PrefabExtensionFileName]
//    public string PatchFileName => "HighlightBetterInsert.xml";
//}

[PrefabExtension("InventoryItemTuple", "//Constants/Constant[1]")]
internal class InventoryHighlightPrefab2Ext : PrefabExtensionInsertPatch
{
    private readonly XmlDocument _brushDocument;

    public InventoryHighlightPrefab2Ext()
    {
        _brushDocument = new XmlDocument();
        _brushDocument.Load(Path.Combine(SubModule.ModuleDirectory, @"..\..\GUI\Brushes\HighlightBetter.xml"));
        BrushFactoryManager.CreateAndRegister(_brushDocument);
    }

    public override InsertType Type => InsertType.Prepend;

    // The file should have an extension of type .xml, and be located inside of the GUI folder of your module.
    // You can include or omit the extension type. I.e. both of the following would work:
    //   ExampleFileInjectedPatch
    //   ExampleFileInjectedPatch.xml
    [PrefabExtensionFileName]
    public string PatchFileName => "HighlightBetterInsert.xml";
}

[PrefabExtension("InventoryItemTuple", "//InventoryItemTupleWidget")]
internal class InventoryHighlightPrefab3Ext : PrefabExtensionSetAttributePatch
{
    public override List<Attribute> Attributes { get; } = new()
    {
        //{ new("CanCharacterUseItem","@ButterEquippedIsItemBetter") }
        //{ new("DefaultBrush","!ButterEquipped.BetterItem") }
    };
}


[PrefabExtension("InventoryItemTuple", "//InventoryItemTupleWidget")]
internal class InventoryHighlightPrefab4Ext : PrefabExtensionInsertPatch
{
    private const string _widgetReplacementText = """
        <InventoryItemTupleInterceptWidget VisualDefinition="Container" IsHidden="@IsFiltered" ButtonType="Radio" DragWidget="DragWidget" Brush="Inventory.Tuple.SoundBrush" Command.Click="ExecuteSelectItem" Command.PreviewItem="{ExecutePreviewItem}" Command.HoverBegin="ExecuteSetFocused" Command.HoverEnd="ExecuteSetUnfocused" Command.SellItem="ExecuteSellItem" Command.EquipItem="ExecuteEquipItem" Command.UnequipItem="ExecuteUnequipItem" Command.Opened="ExecuteResetTrade" Command.OnAlternateRelease="ExecuteConcept" WidthSizePolicy="Fixed" HeightSizePolicy="CoverChildren" HorizontalAlignment="Center" IsRightSide="*IsPlayerItem" NameTextWidget="Main\Body\MainControls\NameText" CountTextWidget="Main\Body\MainControls\CountTextParent\CountText" CostTextWidget="Main\Body\MainControls\CostTextParent\CostText" MainContainer="Main" ExtendedControlsContainer="Extension" TransferButton="Main\TransferButtonParent\TransferButton" EquipButton="Extension\ExtensionCarrier\ButtonCarrier\EquipButton" SliderTransferButton="Extension\ExtensionCarrier\ButtonCarrier\SliderTransferButton" ViewButton="Extension\ExtensionCarrier\ButtonCarrier\PreviewButton" Slider="Extension\ExtensionCarrier\SliderParent\TransferSlider" SliderParent="Extension\ExtensionCarrier\SliderParent" SliderTextWidget="Extension\ExtensionCarrier\SliderParent\SliderTextWidget" TransactionCount="@TransactionCount" IsTransferable="@IsTransferable" ItemCount="@ItemCount" ProfitState="@ProfitIndex" IsEquipable="@IsEquipableItem" CanCharacterUseItem="@CanCharacterUseItem" IsBetterItem="@ButterEquippedIsItemBetter" IsCivilian="@IsCivilianItem" IsGenderDifferent="@IsGenderDifferent" ItemType="@TypeId" EquipmentIndex="@ItemType" DefaultBrush="!Inventory.Tuple" CivilianDisabledBrush="!Inventory.Tuple.Civillian" CharacterCantUseBrush="!Inventory.Tuple.CharacterCantUse" BetterItemHighlightBrush="!ButterEquipped.BetterItem" MarginTop="2" ItemID="@StringId" HoveredCursorState="RightClickLink" ItemImageIdentifier="Main\Body\MainControls\ImageIdentifier" IsNewlyAdded="@IsNew"></InventoryItemTupleInterceptWidget>
        """;

    private readonly XmlDocument _widgetDocument;

    public InventoryHighlightPrefab4Ext()
    {
        _widgetDocument = new XmlDocument();
        _widgetDocument.LoadXml(_widgetReplacementText);
        WidgetFactoryManager.Register(typeof(InventoryItemTupleInterceptWidget));
        //WidgetFactoryManager.CreateAndRegister(nameof(InventoryItemTupleInterceptWidget),)
        //BrushFactoryManager.CreateAndRegister(_widgetDocument);
    }

    public override InsertType Type => InsertType.ReplaceKeepChildren;
    public override int Index => 0;

    [PrefabExtensionXmlNodes]
    public IEnumerable<XmlNode> GetNodes()
    {
        yield return _widgetDocument;
    }
}

