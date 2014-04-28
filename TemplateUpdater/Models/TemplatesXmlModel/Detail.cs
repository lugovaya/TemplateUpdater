using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TemplateUpdater.Models.TemplatesXmlModel
{
    [Serializable()]
    public class Detail
    {
        [XmlAttribute]
        public string IbName { get; set; }

        [XmlAttribute]
        public string CurrentVersion { get; set; }

        [XmlAttribute]
        public string ActualVersion { get; set; }

        [XmlAttribute]
        public string TemplatePath { get; set; }

        [XmlAttribute]
        public string UpdatesPath { get; set; }

        [XmlAttribute]
        public string LastUpdateDate { get; set; }

        [XmlAttribute]
        public string UpdatesCount { get; set; }

        [XmlArray]
        [XmlArrayItem("Update")]
        public Update[] Updates { get; set; }
    }
}
