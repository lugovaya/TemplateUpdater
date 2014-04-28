using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateUpdater._1CCommands
{
    class LoadEmptyTemplateCommand
    {
        private readonly string _templateName;

        public LoadEmptyTemplateCommand(string templateName)
        {
            _templateName = templateName;
        }

        public void Execute()
        {
            var commandLoadTempalte = new CreateServerIbTemplateCommand(_templateName);
            
            commandLoadTempalte.Execute();
        }
    }
}

