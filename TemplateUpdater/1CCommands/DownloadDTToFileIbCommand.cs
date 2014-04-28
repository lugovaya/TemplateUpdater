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
    public class DownloadDTToFileIbCommand
    {
        private readonly Logger _log = LogManager.GetLogger("DownloadDTToFileIbCommand");
        
        private readonly string _templatePath;

        private readonly string _dtPath;

        private readonly string _dtLogFolder;

        private readonly string _path1CExe;

        public DownloadDTToFileIbCommand(string path1CExe, string templatePath, string dtPath,string dtLogFolder) 
        {
            _path1CExe = path1CExe;
            _templatePath = templatePath;
            _dtPath = dtPath;
            _dtLogFolder = dtLogFolder;
        }

        public string Execute() 
        {
            _log.Trace("Start to upload Dt to file ib {0} -  {1}", _templatePath, _dtPath);

            var loadLogResult = "";

            try
            {
                loadLogResult = UploadDtFile();
                
                _log.Info("Downloading dt to file ib successfully {0}", _templatePath);
                
                File.Delete(_dtPath);

                return loadLogResult + " Успешно завершено.";
            }
            catch (Exception ex) 
            {
                _log.Error("Error during downloading Dt {0} to file ib: {1}", _templatePath, ex.Message);
                return string.Format("ERROR: Ошибка при загрузке dt в файловую базу {0}: {1}", _templatePath, ex.Message);              
            }
        }

        private string UploadDtFile()
        {
            // формируем парамеры запуска
            var commandString = string.Format(@"CONFIG /F ""{0}"" /Out ""{1}"" /RestoreIB ""{2}""",
                                      _templatePath, _dtLogFolder, _dtPath);

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
                _log.Error("Error on upload dt to file ib. ExitCode != 0. Log: {0}", logResult);
                return string.Format("ERORR: Ошибка при загрузке dt в файловую базу: {0}");
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
                logResult = "ERROR: " + ex.Message;
            }

            return logResult;
        }
    }
}
