using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace YeomanTemplates.yeoman
{
    class YoHelper
    {
        public static Process GetYoProcess(string yoCmd)
        {
            var proc = new Process();
            proc.StartInfo.FileName = yoCmd;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.EnableRaisingEvents = true;

            return proc;
        }

        public static string GetYoFile(string folder)
        {
            return Path.Combine(folder, "yo.cmd");
        }
    }
}
