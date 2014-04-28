using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using TemplateUpdater.Models.TemplateDetailXmlModel;
using TemplateUpdater.Models.TemplatesXmlModel;
using TemplateUpdater.Models.UpdatesXmlModel;


namespace TemplateUpdater.Helpers
{
    public class CustomXmlSerializer
    {
        private static Logger _log = LogManager.GetLogger("XmlSerializer");

        public static TemplateDetailInformation GetSelectedTemplateInfoFromXml(string innerXml) 
        {            
            var templateInfo = new TemplateDetailInformation();
            try
            {
                var reader = new StringReader(innerXml);
                var serializer = new XmlSerializer(typeof(TemplateDetailInformation));
                templateInfo = (TemplateDetailInformation)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception ex) 
            {
                _log.Error("Deserialization to TemplateDetailInformation was failed. Error: {0}", ex.Message);
            }

            return templateInfo;
        }

        public static UpdatesDetailInformation GetTemplateUpdatesFromXml(string xmlPath) 
        {
            var updatesInfo = new UpdatesDetailInformation();

            try
            {
                var reader = new StreamReader(xmlPath);
                var serializer = new XmlSerializer(typeof(UpdatesDetailInformation));
                updatesInfo = (UpdatesDetailInformation)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception ex) 
            {
                _log.Error("Deserialization to UpdatesDetailInformation was failed. Error: {0}", ex.Message);
            }

            return updatesInfo;
        }

        public static TemplatesConfig GetAllTemplatesFromXml(string xmlPath) 
        {
            var templates = new TemplatesConfig();

            try
            {        
                var reader = new StreamReader(xmlPath);
                var serializer = new XmlSerializer(typeof(TemplatesConfig));
                templates = (TemplatesConfig)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception ex)
            {
                _log.Error("Deserialization to TemplatesConfig was failed. Error: {0}", ex.Message);
            }

            return templates;
        }

        public static void WriteAllTemplatesToXml(TemplatesConfig templateConfig, string xmlPath) 
        {
            
            try
            {

                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(TemplatesConfig));

                System.IO.StreamWriter file = new System.IO.StreamWriter(
                    xmlPath);
                writer.Serialize(file, templateConfig);
                file.Close();
            }
            catch (Exception ex) 
            {
                _log.Error("Error during writing xml: {0}", ex.Message);
            }            
        }
    }
}
