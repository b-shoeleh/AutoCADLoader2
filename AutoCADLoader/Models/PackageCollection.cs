using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutoCADLoader.Models
{
    [Serializable()]
    [System.Xml.Serialization.XmlRoot("PackageCollection")]
    public class PackageCollection
    {
        [XmlArray("Packages")]
        [XmlArrayItem("Package", typeof(Package))]
        public Package[] Packages { get; set; }



    }

    [Serializable()]
    public class Package
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        //[XmlElement("Discipline")]
        //public string Discipline { get; set; }

        //[XmlElement("Common")]
        //public bool Common { get; set; }

        [XmlElement("Autocad")]
        public bool Autocad { get; set; }

        [XmlElement("Civil3d")]
        public bool Civil3d { get; set; }

        [XmlIgnore]
        public string PackageName
        {
            get
            {
                return Title + ".bundle";
            }
        }

        [XmlIgnore]
        public bool Active { get; set; }


        [XmlIgnore]
        public string PackageDestPath
        {
            get
            {
                return Path.Combine(UserInfo.AppDataFolder("Autodesk\\ApplicationPlugins"), PackageName);
            }
        }
    }
}
