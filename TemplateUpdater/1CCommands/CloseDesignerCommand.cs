using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateUpdater._1CCommands
{
    public class CloseDesignerCommand
    {
        private readonly Logger _log = LogManager.GetLogger("CloseDesignerCommand");

        public CloseDesignerCommand() { }

        public void  Execute() 
        {
            try
            {
                System.Threading.Thread.Sleep(3000);
                
                var processes1C = Process.GetProcessesByName("1cv8");



                if (processes1C.Length == 0)
                    return;

                foreach (var process in processes1C) 
                {
                    process.Kill();
                    process.WaitForExit();
                    process.Dispose();
                }

                _log.Trace("Designer has been successfully closed");
                
            }
            catch (Exception ex) 
            {
                _log.Error("Error during designer closing: {0}", ex.Message);
                
            }
        
        }
    }
}
