using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TemplateUpdater.Helpers;
using TemplateUpdater.Models.TemplateDetailXmlModel;


namespace TemplateUpdater._1CCommands
{
    public class UpdateIbTemplateCommand
    {
        private readonly Logger _log = LogManager.GetLogger("UpdateIbTemplateCommand");

        private List<string> _cfuUpdates;

        private readonly string _path1CExe;

        private readonly TemplateDetailInformation _templateDetail;

        private readonly string _infobasePath;

        private readonly string _xmlPath;

        
        /// <summary>
        /// Обновление шаблона базы с помощью *.cfu
        /// </summary>
        /// <param name="path1CExe">Путь к 1С</param>
        /// <param name="server1C">Адресс сервера 1С</param>
        /// <param name="userLogin">Имя пользователя по умалчиванию</param>
        /// <param name="lockCode">Код блокировки базы 1С</param>
        /// <param name="templateDetail">Информация о шаблоне из xml, выбраная на форме</param>
        public UpdateIbTemplateCommand(string path1CExe, string infobasePath, TemplateDetailInformation templateDetail, string xmlPath) 
        {
            _path1CExe = path1CExe;
            _infobasePath = infobasePath;
            _templateDetail = templateDetail;
            _xmlPath = xmlPath;
        }

        public string Execute() 
        {
            //LoadUpdatesList();

            return RunUpdates();
        }

        #region Update section
        // IMPORTANT: нужно ли это, и нужно ли здесь
        private void LoadUpdatesList()
        {
            _log.Trace("Load updates list");

            _cfuUpdates = GetUpdatesList(_templateDetail.UpdatesPath, _templateDetail.CurrentVersion);

            if (_cfuUpdates.Count == 0) _log.Info("Updates are not found");
        }

        /// <summary>
        /// Запуск загруженных обновлений для шаблона
        /// </summary>
        private string RunUpdates()
        {
            try
            {
                _log.Trace("Run updates");

                _cfuUpdates = GetUpdatesList(_templateDetail.UpdatesPath, _templateDetail.CurrentVersion);

                if (_cfuUpdates.Count == 0)
                {
                    _log.Info("Updates list is empty");
                    UpdateTemplatesXml(_xmlPath, _templateDetail.CurrentVersion);
                    return "INFO: Updates list is empty";
                }

                var result = LoadCfuList();
                return result; 
            }
            catch (Exception ex) 
            {
                _log.Error("Error during updates running: {0}", ex.Message);
                return string.Format("ERROR: Error during updates running: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Получение списка обновлений из заданной папки
        /// </summary>
        /// <param name="updatesFolder">Путь к папке с обновлениями</param>
        /// <param name="version">Текущая версия шаблона</param>
        /// <returns>Список названий обновлений</returns>
        public List<string> GetUpdatesList(string updatesFolder, string version)
        {
            _log.Trace("Get list updates");

            try
            {
                // получаем: 
                // 1) набор файлов по указанному пути и паттерну
                // 2) для каждого файла выбираем его название без расширения
                var updatesInFolder = Directory.EnumerateFiles(updatesFolder, "*.*.*.*.cfu")
                    .Select(cfuFile => Path.GetFileNameWithoutExtension(cfuFile));

                foreach (var update in updatesInFolder)
                    _log.Info("Updates in folder: {0}", update);

                
                // список детельной информации по обновлениям (из xml)
                var updateInfoListFromXml = GetInfoListUpdates(version, updatesInFolder);

                foreach (var updateXml in updateInfoListFromXml)
                    _log.Info("Updates in xml: {0}", updateXml);

                return updateInfoListFromXml.OrderBy(i => i, new VersionComparer()).ToList();
            }
            catch (Exception ex)
            {
                _log.Error("Error during getting updates list: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Получение детальной информации об обновлениях шаблона из xml
        /// </summary>
        /// <param name="prevVersion">Текущая версия</param>
        /// <param name="updatesInFolder">Список обновлений, которые находятся в папке</param>
        /// <returns>Перечисление названий обновлений, которые необходимо поставить, с учётом текущей версии</returns>
        private IEnumerable<string> GetInfoListUpdates(string prevVersion, IEnumerable<string> updatesInFolder)
        {
             _log.Trace("Get updates information from xml");

            var templateInfo = new TemplateDetailInformation();

            templateInfo.Updates = _templateDetail.Updates;

            var currentVersion = prevVersion;

            var result = new List<string>();

            try
            {

                while (true)
                {
                    // получаем версии обновлений, для которых используемая версия отличается от текущей (из xml)
                    var updateVersions = templateInfo.Updates.Where(upd =>
                                    upd.ApplyVersions.Any(version => VersionComparer.CompareSt(version, currentVersion) == 0)).
                                    Select(upd => upd.UpdateVersion);

                    _log.Info("Updates version {0}", updateVersions);

                    // далее из полученного выбираем ту версию, которая отличается от версий обновлений в папке (самую последнюю)
                    var updVersion = updateVersions.Where(version => updatesInFolder.Any(versionInFile =>
                                    VersionComparer.CompareSt(version, versionInFile) == 0)).OrderByDescending(version =>
                                    version, new VersionComparer()).FirstOrDefault();

                    _log.Info("updVersion {0}", updVersion);

                    if (!string.IsNullOrEmpty(updVersion))
                    {
                        currentVersion = updVersion;

                        // и если всё хорошо, то добавляем её к списку требуемых к загрузке обновлений
                        result.Add(updVersion);
                    }
                    else break;
                }

                _log.Trace("Getting updates information from xml has been completed");
            }
            catch (Exception ex)
            {
                _log.Error("Error during getting detail information from xml file: {0}", ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Загрузка списка файлов обновления (*.cfu)
        /// </summary>
        private string LoadCfuList()
        {
            var result = "";
                
            try
            {
                var index = 0;
                var lastVersion = "";

                _log.Info("Length cfu updates {0}", _cfuUpdates.ToArray().Length);
                foreach (var cfu in _cfuUpdates)
                {
#if DEBUG
                    if (index == 1) break;
#endif
                    _log.Info("CFU # {0} version {1}", index, cfu);

                    var cfuFilePath = Path.Combine(_templateDetail.UpdatesPath, cfu + ".cfu");

                    var information = RunConfigLoadCfu(cfuFilePath);

                    if (information.StartsWith("ERROR"))
                    {
                        _log.Error("Error during loading config cfu");
                        return information;
                    }

                    result += RunConfigUpdateTemplate() + @"\n";
                    index++;
                    lastVersion = cfu;
                }

                _log.Info(result);

                UpdateTemplatesXml(_xmlPath, lastVersion);
            }
            catch (Exception ex) 
            {
                _log.Error("Error during loading cfu: {0}", ex.Message);
                result = @"ERROR: Во время загрузки *.cfu \n" + ex.Message;
            }
            
            return result;
        }

        private void UpdateTemplatesXml(string xmlPath, string version)
        {
            try
            {
                // выбираем уже существующие данные из Templates.xml
                var templates = CustomXmlSerializer.GetAllTemplatesFromXml(xmlPath);

                // из этих данных выбираем нужный шаблон
                var selectedTemplate = templates.Templates.FirstOrDefault(x => x.Detail.IbName == _templateDetail.IbName);

                selectedTemplate.Detail.LastUpdateDate = DateTime.Now.ToString("s"); //       s: 2008-06-15T21:15:07

                selectedTemplate.Detail.CurrentVersion = version;
                _log.Info("Updated current version - {0}", version);

                selectedTemplate.Detail.UpdatesCount = GetAllowedUpdates(version);

                // сериализуем обновлённый список шаблонов снова в Templates.xml
                CustomXmlSerializer.WriteAllTemplatesToXml(templates, xmlPath);
            }
            catch (Exception ex) 
            {
                _log.Error("Error during updating templatex XML {0}", ex.Message);
            }
        }

        private string GetAllowedUpdates(string version)
        {
            var updates = GetUpdatesList(_templateDetail.UpdatesPath, version);
            return (updates != null) ? updates.Count.ToString() : "";
        }

        private string RunConfigLoadCfu(string cfuFile)
        {
            _log.Trace("Start running config load cfu... ");

            var result = "";
            var logFilePath = Path.GetTempFileName();

            var process = new Process();
            process.StartInfo.FileName = _path1CExe;
            process.StartInfo.Arguments = string.Format(@"CONFIG /F ""{0}"" /UpdateCfg ""{1}"" /Out ""{2}""",
                _infobasePath, cfuFile, logFilePath);
            process.Start();

            while (!process.HasExited)
                process.WaitForExit(1000);

            result = ReadLog(logFilePath);

            if (process.ExitCode > 0)
            {
                result = "ERROR: " + result;
                _log.Error("Error during loading cfu {0}", result);
            }

            _log.Trace("Finish. Result: {0}", result);

            return result;
        }

        // Практически 2 одинаковых метода - зачем нужны???
        private string RunConfigUpdateTemplate()
        {
            _log.Trace("Run config update...");

            var result = "";
            var logFilePath = Path.GetTempFileName();

            var process = new Process();
            process.StartInfo.FileName = _path1CExe;
            process.StartInfo.Arguments = string.Format(@"CONFIG /F ""{0}"" /UpdateDBCfg /Out ""{1}""",
                _infobasePath, logFilePath);
            process.Start();

            while (!process.HasExited)
                process.WaitForExit(1000);

            result = ReadLog(logFilePath);

            if (process.ExitCode > 0)
            { 
                result = "ERROR: " + result;
                _log.Error("Error during runing cfu {0}", result);
            }

            return result;
        }

        private string ReadLog(string logFilePath)
        {
            var result = "";

            try
            {
                var reader = new StreamReader(logFilePath, Encoding.GetEncoding(1251));
                result = reader.ReadToEnd();
                reader.Close();
                File.Delete(logFilePath);
            }
            catch (Exception ex)
            {
                result += "Error: " + ex.Message;
                _log.Error("Error during reading log: {0}", ex.Message);
            }

            return result;
        }

        #endregion
    }
}
