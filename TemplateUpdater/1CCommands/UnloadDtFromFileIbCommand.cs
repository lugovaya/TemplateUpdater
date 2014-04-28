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
    public class UnloadDtFromFileIbCommand
    {
        private readonly Logger _log = LogManager.GetLogger("LoadDtCommand");

        private readonly string _ibPath;

        private readonly string _dtPath;

        private readonly string _path1CExe;        

        /// <summary>
        /// Выгрузка DT из базы файлового шаблона
        /// </summary>
        /// <param name="path1Cexe">Путь к 1С</param>
        /// <param name="server1C">Сервер 1С</param>
        /// <param name="dtLogFolder">Путь к папке, содержащей логи dt</param>
        /// <param name="ibPath">Название необходимого шаблона</param>
        /// <param name="dtPath">Путь к dt</param>
        public UnloadDtFromFileIbCommand(string path1Cexe, string ibPath, string dtPath) 
        {
            _path1CExe = path1Cexe;
            _ibPath = ibPath;
            _dtPath = dtPath;
        }

        public string Execute() 
        {
            var success = false;
            
            _log.Trace("Start to unload Dt {0} -  {1}", _ibPath, _dtPath);

            // формируем парамеры запуска
            var commandString = string.Format(@"DESIGNER  /F ""{0}"" /DumpIB ""{1}"" ",
                                      _ibPath, _dtPath);

            try
            {
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

                if (process.ExitCode != 0)
                {
                    _log.Error("Error on load dt. ExitCode != 0");
                    throw new Exception("Error on load dt. ExitCode != 0" );
                }
                success = true;
            }
            catch (Exception ex)
            {
                _log.Error("Error in UnloadDtServer: {0}", ex.Message);
                success = false;
            }

            if (success)
            {
                _log.Info("Loading dt successfully {0} in {1}", _ibPath, _dtPath);
                return "Успешно завершено.";
            }
            else
            {
                _log.Error("Error during loading Dt {0} in {1}", _ibPath, _dtPath);
                return string.Format("ERROR: Error during loading Dt {0}", _ibPath);
            }
            
        }
       
    }
}
