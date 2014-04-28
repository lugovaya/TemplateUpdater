using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TemplateUpdater.Helpers
{
    public class VersionComparer : IComparer<string>
    {
        private Logger _log = LogManager.GetLogger("VersionComparer");

        /// <summary>Сравнивает версии, заданые строкой</summary>		
        /// <returns>1, 0 и -1</returns>
        public int Compare(string ver1, string ver2)
        {
            int tmp1, tmp2;
            try
            {
                string[] v1 = ver1.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                string[] v2 = ver2.Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                if (v1.Length != v2.Length) return 0;

                for (int i = 0; i < v1.Length; i++)
                {
                    tmp1 = int.Parse(v1[i]);
                    tmp2 = int.Parse(v2[i]);
                    if (tmp1 > tmp2) return 1;
                    if (tmp1 < tmp2) return -1;
                }
            }
            catch(Exception ex)
            {
                _log.Error("Error during comparing the versions: {0}", ex.Message);
            }

            return 0;
        }

        /// <summary>Статический метод сравнения версий</summary>		
        /// <returns>1, 0 и -1</returns>
        public static int CompareSt(string ver1, string ver2)
        {
            return new VersionComparer().Compare(ver1, ver2);
        }

    }
}
