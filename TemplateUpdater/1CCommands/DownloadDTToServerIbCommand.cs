using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateUpdater._1CCommands
{
    public class DownloadDTToServerIbCommand
    {
        private readonly Logger _log = LogManager.GetLogger("DownloadDTToServerIbCommand");

        private readonly string _templateName;

        private readonly string _dtPath;

        private readonly string _path1CExe;

        private readonly string _dtLogFolder;

        private readonly string _server1C;

        /// <summary>
        /// Загрузка DT в базу серверного шаблона
        /// </summary>
        /// <param name="path1Cexe">Путь к 1С</param>
        /// <param name="server1C">Сервер 1С</param>
        /// <param name="dtLogFolder">Путь к папке, содержащей логи dt</param>
        /// <param name="templateName">Название необходимого шаблона</param>
        /// <param name="dtPath">Путь к dt</param>
        public DownloadDTToServerIbCommand(string path1Cexe, string server1C, string dtLogFolder, string templateName, string dtPath) 
        {
            _path1CExe = path1Cexe;
            _server1C = server1C;
            _dtLogFolder = dtLogFolder;
            _templateName = templateName;
            _dtPath = dtPath;
        }

        public string Execute() 
        {
            var success = false;
            _log.Trace("Start to upload Dt {0} -  {1}", _templateName, _dtPath);

            // загружаем пустую базу sql (для очитки пользователей)
            // нужно ли это в данной ситуации
            //var commandLoadSqlTemplate = new LoadEmptyTemplateCommand(_templateName);
            //commandLoadSqlTemplate.Execute();

            //_log.Trace("Empty template has been downloaded {0}", _templateName);

            var loadLogResult = "";

            try
            {
                loadLogResult = UploadDtServer();
                success = true;
            }
            catch (Exception ex)
            {
                _log.Error("Error in UploadDtServer: {0}", ex.Message);
                loadLogResult = ex.Message;
                success = false;
            }

            _log.Info("Log Result: {0}", loadLogResult);

            if (success)
            {
                _log.Info("Downloading dt successfully {0}", _templateName);
                return "Успешно завершено.";
            }
            else
            {
                _log.Error("Error during downloading Dt {0}: {1}", _templateName, loadLogResult);
                return string.Format("ERROR: Error during downloading Dt {0}: {1}", _templateName, loadLogResult);
                
            }
            
        }

       /// <summary>
       /// Загрузка dt в серверную базу
       /// </summary>
       /// <returns>результат логирования</returns>
        private string UploadDtServer()
        {
            // формируем парамеры запуска
            _log.Info("IbName: {0}", _templateName);
            var commandString = string.Format(@"CONFIG /S ""{0}"" /RestoreIB ""{1}"" /Out ""{2}""",
                                      _server1C + "\\" + _templateName, _dtPath, _dtLogFolder);

            _log.Info("Command string {0}", commandString);

            // запускаем 1с с даданными аргументами
            var process = new Process { StartInfo = { FileName = _path1CExe, Arguments = commandString } };
            process.Start();

            process.WaitForExit(3000); 		
            if (process.CloseMainWindow()) process.Kill();

            // ждем пока не завершитсья процесс загрузки дт
            while (!process.HasExited)
            {
                process.WaitForExit(1000);
            }

            // читаем лог-файл, который вернула 1с
            var logResult = ReadLogFile(_dtLogFolder);

            if (process.ExitCode != 0)
            {
                _log.Error("Error on upload dt. ExitCode != 0. Log: {0}", logResult);
                throw new Exception("Error on upload dt. ExitCode != 0. Log: " + logResult);
            }

            // возвращаем результат
            return logResult;
        }

        private string ReadLogFile(string dtLogFolder)
        {
            var logResult = "<empty log>";

            try
            {
                var reader = new StreamReader(dtLogFolder, Encoding.GetEncoding(1251));
                logResult = reader.ReadToEnd();
                _log.Trace("Reading log file is successfully completed");
            }
            catch (Exception ex)  
            { 
                _log.Error("Error durring reading log file: {0}", ex.Message); 
            }

            return logResult;
        }
    }
}
