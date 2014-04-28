using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TemplateUpdater.Models.TemplatesXmlModel
{
    [Serializable()]
    [XmlRoot("TemplatesConfig"), XmlType("TemplatesConfig")]    
    public class TemplatesConfig
    {
        [XmlArray]
        [XmlArrayItem("Template")]        
        public Template[] Templates { get; set; }
    }
}
