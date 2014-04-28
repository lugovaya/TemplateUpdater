using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace TemplateUpdater.Models.TemplateDetailXmlModel
{
    [Serializable()]
    public class Update
    {
        [XmlAttribute]
        public string Title { get; set; }

        [XmlArray("ApplyVersions")]
        [XmlArrayItem("ApplyVersion")]
        public string[] ApplyVersions { get; set; }

        [XmlAttribute]
        public string UpdateDate { get; set; }

        [XmlAttribute]
        public string UpdateVersion { get; set; }
    }
}
