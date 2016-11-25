using System;
using System.Diagnostics;
using System.IO;
using YeomanTemplates.Helper;

namespace YeomanTemplates.yeoman
{
    class YoHelper
    {
        public static NestedStringList GetYoGenerators(string yoCmd)
        {
            try
            {
                var yo = GetYoProcess(yoCmd);
                yo.StartInfo.Arguments = "--generators";
                yo.Start();

                var reader = yo.StandardOutput;

                var parser = new yeoman.GeneratorListParser();
                parser.ParseInput(reader);

                reader.Dispose();

                return parser.Result;
            }
            catch (Exception e)
            {
                PluginCore.Managers.ErrorManager.ShowError("Error running yo, please check if the command in the settings is correct: " + yoCmd, e);
            }

            return null;
        }

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

        public static Process GetVisibleYo(string yoCmd)
        {
            var proc = new Process();
            proc.StartInfo.FileName = yoCmd;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.CreateNoWindow = false;

            return proc;
        }

        public static string GetYoFile(string folder)
        {
            return Path.Combine(folder, "yo.cmd");
        }
    }
}
