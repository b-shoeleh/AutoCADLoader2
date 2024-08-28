using System.IO;
using System.Xml.Serialization;

namespace AutoCADLoader.Models.Packages
{
    [Serializable()]
    [XmlRoot("PackageCollection")]
    public class BundleCollection
    {
        [XmlArray("Packages")]
        [XmlArrayItem("Package", typeof(Bundle))]
        public Bundle[] Packages { get; set; }
    }

    [Serializable()]
    public class Bundle
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Autocad")]
        public bool Autocad { get; set; }

        [XmlElement("Civil3d")]
        public bool Civil3d { get; set; }

        [XmlIgnore]
        public string FileName
        {
            get
            {
                return Title + ".bundle";
            }
        }

        [XmlIgnore]
        public bool Active { get; set; }


        [XmlIgnore]
        public string TargetFilePath
        {
            get
            {
                return Path.Combine(UserInfo.AppDataFolder("Autodesk\\ApplicationPlugins"), FileName);
            }
        }
    }
}
