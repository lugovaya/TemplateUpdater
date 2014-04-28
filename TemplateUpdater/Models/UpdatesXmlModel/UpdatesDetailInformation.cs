using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TemplateUpdater.Models.UpdatesXmlModel
{
    [Serializable()]
    [XmlRoot("UpdatesList"), XmlType("UpdatesList")]    
    public class UpdatesDetailInformation
    {
        [XmlArray]
        [XmlArrayItem("ConfigUpdate")]
        public ConfigUpdate[] Updates { get; set; }
    }
}
