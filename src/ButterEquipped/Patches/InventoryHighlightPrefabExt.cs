using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;
using Bannerlord.UIExtenderEx.ResourceManager;
using System.IO;
using System.Xml;

namespace ButterEquipped.Patches;

[PrefabExtension("InventoryItemTuple", "//Constants/Constant[1]")]
internal class InventoryHighlightPrefabBrushExt : PrefabExtensionInsertPatch
{
    public InventoryHighlightPrefabBrushExt()
    {
        if(SubModule.ModuleDirectory is not string baseDir)
        {
            return;
        }

        var brushDocument = new XmlDocument();
        brushDocument.Load(Path.Combine(baseDir, @"..\..\GUI\Brushes\HighlightBetter.xml"));
        BrushFactoryManager.CreateAndRegister(brushDocument);
    }

    public override InsertType Type => InsertType.Prepend;

    // The file should have an extension of type .xml, and be located inside of the GUI folder of your module.
    // You can include or omit the extension type. I.e. both of the following would work:
    //   ExampleFileInjectedPatch
    //   ExampleFileInjectedPatch.xml
    [PrefabExtensionFileName]
    public string PatchFileName => "HighlightBetterInsert.xml";
}

