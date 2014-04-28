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
    public class RunFileIbTemplateCommand
    {
        private readonly Logger _log = LogManager.GetLogger("RunInfobaseTemplateCommand");

        private readonly string _path1CExe;
        private readonly string _infobasePath;
        private readonly string _mode;

        /// <summary>
        /// Запуск информационной ФАЙЛОВОЙ базы шаблона в необходимом режиме
        /// "ENTERPRISE" - режим 1С-Предприятия
        /// "DESIGNER" - режим Конфигуратора
        /// </summary>
        /// <param name="mode">Режим 1C</param>
        /// <param name="infobasePath">Путь к информационной базе</param>
        /// <param name="path1CExe">Путь к 1С</param>
        public RunFileIbTemplateCommand(string path1CExe, string infobasePath, string mode) 
        {
            _path1CExe = path1CExe;
            _infobasePath = infobasePath;
            _mode = mode;
        }

        public string Execute() 
        {
            var process = new Process();

            var infobasePath = Path.Combine(_infobasePath, "1Cv8.1CD");
            
            if (!File.Exists(infobasePath))
            {
                _log.Error("Infobase file not found: {0}", infobasePath);
                return string.Format("ERROR: Infobase file not found: {0}", infobasePath);
            }

            if (!File.Exists(_path1CExe))
            {
                _log.Error("File 1C.exe not found: {0}", _path1CExe);
                return string.Format("ERROR: File 1C.exe not found: {0}", _path1CExe);
            }

            // запуск информационной базы с консоли
            process.StartInfo.FileName = _path1CExe;
            process.StartInfo.Arguments = string.Format(@" {0} /F""{1}""", _mode.ToUpper(), _infobasePath);
            process.Start();

            while (!process.HasExited)
                process.WaitForExit(1000);

            return "Успешно завершено.";
        }
    }
}
