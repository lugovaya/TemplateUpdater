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
    public class RunServerIbTemplateCommand
    {
        private readonly Logger _log = LogManager.GetLogger("RunServerIbTemplateCommand");

        private readonly string _path1CExe;
        private readonly string _infobasePath;
        private readonly string _mode;

        public RunServerIbTemplateCommand(string path1CExe, string infobasePath, string mode) 
        {
            _path1CExe = path1CExe;
            _infobasePath = infobasePath;
            _mode = mode;
        }

        public void Execute() 
        {
            var process = new Process();

            if (!File.Exists(_infobasePath))
            {
                _log.Error("Infobase file not found: {0}", _infobasePath);
                throw new FileNotFoundException();
            }

            if (!File.Exists(_path1CExe))
            {
                _log.Error("File 1C.exe not found: {0}", _path1CExe);
                throw new FileNotFoundException();
            }

            // запуск информационной базы с консоли
            process.StartInfo.FileName = _path1CExe;
            process.StartInfo.Arguments = string.Format(@" {0} /F""{1}""", _mode.ToUpper(), _infobasePath);
            process.Start();
        }
    }
}
