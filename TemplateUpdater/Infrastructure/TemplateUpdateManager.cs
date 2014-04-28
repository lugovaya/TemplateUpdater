using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using NLog;
using TemplateUpdater._1CCommands;
using System.Xml.XPath;
using TemplateUpdater.Helpers;
using TemplateUpdater.Models.TemplateDetailXmlModel;
using TemplateUpdater.Models.TemplatesXmlModel;

namespace TemplateUpdater.Infrastructure
{
    public class TemplateUpdateManager : ITemplateUpdateManager
    {
        private readonly Logger _log = LogManager.GetLogger("TemplateUpdateManager");

        // поле для десериализируемого объекта шаблона из xml
        private readonly TemplateDetailInformation _templateDetail;

        // логин пользователя (по умалчиванию)
        private readonly string _userLogin = ConfigurationManager.AppSettings["UserName"];

        // пароль пользователя (по умалчиванию)
        private readonly string _userPassword = ConfigurationManager.AppSettings["UserPassword"];

        // код блокировки (по умалчиванию)
        private readonly string _lockCode = ConfigurationManager.AppSettings["LockCode"];

        // путь к программе запуска 1С
        private readonly string _path1CExe = ConfigurationManager.AppSettings["Path1CExe"];

        // путь к программе запуска 1С
        private readonly string _server1C = ConfigurationManager.AppSettings["Server1C"];
        
        private readonly string _dtPath = ConfigurationManager.AppSettings["DtPath"];
        
        private readonly string _dtLogFolder = ConfigurationManager.AppSettings["DtLogFolderPath"];

        // путь временной папки для запуска 1с и дальнейшего обновления
        private readonly string _infobasePath = ConfigurationManager.AppSettings["PathInfobase"];

        private readonly string _updatesNodePath = ConfigurationManager.AppSettings["UpdateNodePath"];

        private readonly string _tempateXmlPath = ConfigurationManager.AppSettings["TemplatesXmlPath"];

        private readonly string _templatesPath = ConfigurationManager.AppSettings["PathTemplate"];

        /// <summary>
        /// При создании экземпляра класса, сразу получаем выбранный шаблон из xml
        /// </summary>
        /// <param name="xmlData">Данные из xml</param>
        public TemplateUpdateManager(string xmlData)
        {
            _templateDetail = CustomXmlSerializer.GetSelectedTemplateInfoFromXml(xmlData);
        }
        
        public TemplateUpdateManager()
        {
            // TODO: Complete member initialization
        }

        // названия шаблонов, как они названы в папке для фаловых шаблонов
        enum Templates
        {
            db_1c_bp20corp_shablon,
            db_1c_bp30corp_shablon,
            db_1c_bp20_shablon,
            db_1c_bp30_shablon,
            //db_1c_do_shablon,
            db_1c_zup_shablon,
            db_1c_ca_shablon,
            db_1c_upp13_shablon,
            db_1c_ut11_shablon
        }

        // названия шаблонов, как они названы в папке для обновлений        
        Dictionary<Templates, string> templatesValue = new Dictionary<Templates, string>()
        {
            { Templates.db_1c_bp20corp_shablon, "Бухгалтерия предприятия КОРП, редакция 2.0" },
            { Templates.db_1c_bp30corp_shablon, "Бухгалтерия предприятия КОРП, редакция 3.0" },
            { Templates.db_1c_bp20_shablon, "Бухгалтерия предприятия, редакция 2.0" },
            { Templates.db_1c_bp30_shablon, "Бухгалтерия предприятия, редакция 3.0" },
            { Templates.db_1c_ca_shablon, "Комплексная автоматизация, редакция 1.1" },
            //{ Templates.db_1c_do_shablon, "Документооборот 8 КОРП, редакция 1.2" },
            { Templates.db_1c_upp13_shablon, "Управление производственным предприятием, редакция 1.3" },
            { Templates.db_1c_ut11_shablon, "Управление торговлей, редакция 11.0" },
            { Templates.db_1c_zup_shablon, "Зарплата и Управление Персоналом, редакция 2.5" } 
        };

        public void CreateTemplatesXml() 
        {
            try 
            {
                var templateIndex = 0;

                var updatedTemplates = new TemplatesConfig 
                {
                    Templates = new Template[templatesValue.Values.ToArray<string>().Length]
                };

                // выбираем уже существующие данные из Templates.xml
                var templates = CustomXmlSerializer.GetAllTemplatesFromXml(_tempateXmlPath);

                // проходим по всем шаблонам, которые нужно обновлять
                foreach (var template in templatesValue)
                {
                    var updatesPath = Path.Combine(_updatesNodePath, template.Value, "info.xml");
                   
                    // выгребаем новые обновления данного шаблона из info.xml
                    var templateUpdates = CustomXmlSerializer.GetTemplateUpdatesFromXml(updatesPath).Updates;

                    // выбираем данный шаблон из уже существующих данных из Templates.xml
                    var templateDetail = templates.Templates.FirstOrDefault(x => x.Name == template.Value);

                    if (templateUpdates == null)
                    {
                        _log.Error("Error by getting updates from xml: template - {0}", template);
                        throw new ArgumentNullException();
                    }

                    if (templateDetail == null)
                    {
                        _log.Error("Template {0} not found in xml", template.Value);
                        throw new ArgumentNullException();
                    }

                    // перезаполняем (обновляем)  детальную информацию по данному шаблону
                    var info = new Detail
                    {
                        IbName = template.Key.ToString(),
                        ActualVersion = templateUpdates.OrderByDescending(x => DateTime.Parse(x.Date)).FirstOrDefault().Version,
                        CurrentVersion = templateDetail.Detail.CurrentVersion,
                        LastUpdateDate = templateDetail.Detail.LastUpdateDate,
                        TemplatePath = _templatesPath,
                        UpdatesPath = Path.Combine(_updatesNodePath, template.Value),
                        UpdatesCount = templateDetail.Detail.UpdatesCount,//templateUpdates.Length.ToString(),
                        Updates = new TemplateUpdater.Models.TemplatesXmlModel.Update[templateUpdates.Length]
                    };

                    // заполняем список обновлений в Templates.xml обновлениями из info.xml
                    for (int i = 0; i < templateUpdates.Length; i++) 
                    {
                        var update = templateUpdates[i];

                        info.Updates[i] = new Models.TemplatesXmlModel.Update() 
                        { 
                            ApplyVersions = update.ApplyVersions.OrderBy(x => x).ToArray<string>(),
                            UpdateDate = update.Date,
                            UpdateVersion = update.Version,
                            Title = update.Version
                        };
                    }

                    
                    // добавляем сформированную информацию по шаблону к обновлённому списку шаблонов
                    updatedTemplates.Templates[templateIndex] = new Template 
                    { 
                        Detail = info, 
                        Name = template.Value 
                    };

                    templateIndex++;
                    
                }
                
                // сериализуем обновлённый список шаблонов снова в Templates.xml
                CustomXmlSerializer.WriteAllTemplatesToXml(updatedTemplates, _tempateXmlPath);
            }
            catch (Exception ex) 
            { 
                _log.Error("Creating Templates.xml ERROR: {0}", ex.Message);
            }
            
        }

        /// <summary>
        /// Запуск информационной ФАЙЛОВОЙ базы шаблона в необходимом режиме
        /// "ENTERPRISE" - режим 1С-Предприятия
        /// "DESIGNER" - режим Конфигуратора
        /// </summary>
        /// <param name="mode">Режим</param>
        public string RunInfobaseTemplate(string mode)
        {
            return new RunFileIbTemplateCommand(_path1CExe, _infobasePath, mode).Execute();
        }

        
        /// <summary>
        /// Обновление шаблона с помощью *.cfu
        /// </summary>
        public string UpdateIbTemplateWithCFU()
        {
            return new UpdateIbTemplateCommand(_path1CExe, _infobasePath, _templateDetail, _tempateXmlPath).Execute();
        }
        
        public string UploadInfobaseDT()
        {
           return new UnloadDtFromFileIbCommand(
               _path1CExe,
               _infobasePath, 
               Path.Combine(_dtPath, _templateDetail.IbName + ".dt"))
               .Execute();
        }

        public void CloseDesigner()
        {
             new CloseDesignerCommand().Execute();
        }

        public void RefreshUpdatedTemplateInFolder(string ibTemplateName)
        {
            throw new NotImplementedException();
        }

        public void RunServerInfobase(string mode)
        {
            new RunServerIbTemplateCommand(_path1CExe, _templateDetail.TemplatePath, mode).Execute();
        }

        public string DownloadDTToServerIb()
        {
            return new DownloadDTToServerIbCommand(_path1CExe, _server1C
                , Path.Combine(_dtLogFolder, _templateDetail.IbName + ".log")
                , _templateDetail.IbName.Replace("db_", "")
                , Path.Combine(_dtPath, _templateDetail.IbName + ".dt")).Execute();
        }

        public string CreateServerInfobaseTemplate()
        {
            return new CreateServerIbTemplateCommand(_templateDetail.IbName).Execute();
        }

        /// <summary>
        /// Копируем файл шаблона во временную папку для того, чтобы 1С-ка его захавала и запустила
        /// </summary>
        /// <param name="ibTemplateName">Имя необходимого шаблона</param>
        public string CopyTemplateToTempFolder()
        {

            // путь обновляемого шаблона
            var templatePath = Path.Combine(_templateDetail.TemplatePath, _templateDetail.IbName + ".1CD");

            // путь временной папки для запуска 1с и дальнейшего обновления
            var infobasePath = Path.Combine(_infobasePath, "1Cv8.1CD");

            if (File.Exists(templatePath))
            {
                File.Copy(templatePath, infobasePath, true);
                return "Успешно завершено.";
            }
            else
            {
                _log.Error("Template file not found: {0}", templatePath);
                return "ERROR: Файл шаблона не был найден.";
            }
        }

        public string CopyFileIbToTemplateFolder() 
        {
            // путь временной папки для запуска 1с
            var infobasePath = Path.Combine(_infobasePath, "1Cv8.1CD");

            // путь обновляемого шаблона
            var templatePath = Path.Combine(_templateDetail.TemplatePath, _templateDetail.IbName + ".1CD");

            if (File.Exists(infobasePath))
            {
                File.Copy(infobasePath, templatePath, true);
                return "Успешно завершено.";
            }
            else
            {
                _log.Error("Infobase file not found: {0}", templatePath);
                return "ERROR: Файл базы не был найден.";
            }
        }

        public string DownloadDTToFileIb()
        {
            return new DownloadDTToFileIbCommand(_path1CExe, _templatesPath, _dtPath, _dtLogFolder).Execute();
        }
    }
}
