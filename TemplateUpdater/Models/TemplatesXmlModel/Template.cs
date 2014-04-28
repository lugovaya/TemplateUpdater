using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using TemplateUpdater.Models.TemplateDetailXmlModel;


namespace TemplateUpdater.Models.TemplatesXmlModel
{
    [Serializable()]
    public class Template
    {
        [XmlAttribute]
       public string Name { get; set; }

        [XmlElement]
       public Detail Detail { get; set; }

        
    }
}
