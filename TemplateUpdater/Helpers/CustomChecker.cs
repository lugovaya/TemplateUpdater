using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateUpdater.Helpers
{
    public static class CustomChecker
    {
        public static bool IsProcessFailedDuringExecution(string result) 
        {
            if (result.Contains("ERROR"))
                return true;
            return false;
        }

        public static bool NeedAttentionToProcess(string result) 
        {
            if (result.StartsWith("INFO"))
                return true;
            return false;
        }
    }
}
