using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TemplateUpdater
{
    public class MyXmlDataProvider : XmlDataProvider
    {
        public new Uri Source
        {
            get { return base.Source; }
            set
            {
                base.Source = value;

                var watcher = new FileSystemWatcher();

                watcher.Path = ConfigurationManager.AppSettings["TemplatesXmlPath"];

                watcher.Filter = value.OriginalString;

                watcher.Changed += new FileSystemEventHandler(watcher_Changed);

                watcher.EnableRaisingEvents = true;
            }
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            base.Refresh();
        }

    }
}
