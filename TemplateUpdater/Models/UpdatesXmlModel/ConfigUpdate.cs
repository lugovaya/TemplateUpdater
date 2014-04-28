using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TemplateUpdater.Models.UpdatesXmlModel
{
    [Serializable()]    
    public class ConfigUpdate
    {
        [XmlAttribute("Ver")]
        public string Version { get; set; }

        [XmlAttribute]
        public string Date { get; set; }

        [XmlArray]
        [XmlArrayItem("ApplyVersion")]
        public string[] ApplyVersions { get; set; }
    }
}
