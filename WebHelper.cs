namespace TS.Service.Service.UtilFunc
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    
    public class UtilFunc
    {
        public static void OpenWebSite(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch (Exception)
            {
                var args = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                args += ",OpenAs_RunDLL " + url;
                System.Diagnostics.Process.Start("rundll32.exe", args);
            }
        }
    }
}
