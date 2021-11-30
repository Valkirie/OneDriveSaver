using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropboxMe
{
    class Utils
    {
        public static void SetStartup(bool OnStartup, string ExecutablePath)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (OnStartup)
                rk.SetValue("DropboxMe", ExecutablePath);
            else
                rk.DeleteValue("DropboxMe", false);
        }
    }
}
