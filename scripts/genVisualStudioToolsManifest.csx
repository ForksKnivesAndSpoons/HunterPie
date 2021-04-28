#r "../HunterPie.UI/bin/Debug/HunterPie.UI.dll"

using System.Linq;
using System.Windows;
using System.Xml;
using HunterPie.GUI;

var settings = new XmlWriterSettings();
settings.OmitXmlDeclaration = true;
settings.Indent = true;
settings.NewLineChars = "\n";


using (var writer = XmlWriter.Create("HunterPie.UI/tools/VisualStudioToolsManifest.xml", settings)) {
    writer.WriteStartElement("FileList");

    writer.WriteStartElement("File");
    writer.WriteAttributeString(null, "Reference", null, "HunterPie.UI.dll");

    writer.WriteStartElement("ToolboxItems");
    writer.WriteAttributeString(null, "UIFramework", null, "UWP");
    writer.WriteAttributeString(null, "VSCategory", null, "HunterPie.UI");
    writer.WriteAttributeString(null, "BlendCategory", null, "HunterPie.UI");

    var lib = typeof(Widget).Assembly;
    lib.GetTypes()
        .Where(t => t.IsSubclassOf(typeof(UIElement)))
        .Select(t => t.FullName)
        .Where(t => t != null)
        .ToList()
        .ForEach(t => {
            writer.WriteStartElement("Item");
            writer.WriteAttributeString(null, "Type", null, t);
            writer.WriteEndElement();
        });

    writer.WriteEndElement();
    writer.WriteEndElement();
    writer.WriteEndElement();
    writer.Flush();
}
