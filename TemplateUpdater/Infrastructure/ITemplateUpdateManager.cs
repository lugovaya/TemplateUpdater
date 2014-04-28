using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateUpdater.Infrastructure
{
    public interface ITemplateUpdateManager
    {
        string RunInfobaseTemplate(string mode);

        string UpdateIbTemplateWithCFU();

        string UploadInfobaseDT();

        void CloseDesigner();

        void RefreshUpdatedTemplateInFolder(string ibTemplateName);

        void RunServerInfobase(string mode);

        string DownloadDTToFileIb();

        string DownloadDTToServerIb();

        string CreateServerInfobaseTemplate();
    }
}
