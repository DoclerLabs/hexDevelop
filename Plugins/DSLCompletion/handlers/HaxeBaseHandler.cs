using PluginCore;
using PluginCore.Helpers;
using ProjectManager.Projects.Haxe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DSLCompletion
{
    abstract class HaxeBaseHandler : ICompletionHandler
    {
        protected Process process;
        private string macroClassPath;

        protected HaxeBaseHandler()
        {
            ExtractHaxeMacro();
        }

        public virtual PositionResult GetPosition(string type)
        {
            setupProcess();

            return null;
        }

        public virtual List<string> GetCompletion(string path)
        {
            setupProcess();

            return null;
        }

        public virtual string GetFile(string file)
        {
            setupProcess();

            return null;
        }

        private void setupProcess()
        {
            if (process == null)
            {
                process = CreateProcess();
            }
        }

        private void ExtractHaxeMacro()
        {
            macroClassPath = Path.Combine (PathHelper.DataDir, "DSLCompletion/haxe_code");
            var filename = Path.Combine(macroClassPath, "util/ReferenceMacro.hx");
            var util = Path.GetDirectoryName(filename);

            if (!Directory.Exists(util)) Directory.CreateDirectory(util);
            File.WriteAllBytes (filename, Properties.Resources.ReferenceMacro);
        }

        /// <summary>
        /// Creates a Process object for haxe completion without arguments.
        /// </summary>
        private Process CreateProcess()
        {
            var path = Path.Combine(PluginBase.CurrentSDK.Path, "haxe.exe");

            var proc = new Process();
            proc.StartInfo.FileName = path;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.EnableRaisingEvents = true;

            return proc;
        }

        #region From HaXeContext/Completion/HaxeComplete.cs
        //MIT licensed

        static readonly Regex reArg =
            new Regex("^(-cp|-resource)\\s*([^\"'].*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static readonly Regex reMacro =
            new Regex("^(--macro)\\s*([^\"'].*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        protected List<String> GetArgs()
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);

            // Build Haxe command
            var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths.ToArray();
            var hxmlArgs = new List<String>(hxproj.BuildHXML(paths, "Nothing__", true));
            RemoveComments(hxmlArgs);
            QuotePath(hxmlArgs);
            EscapeMacros(hxmlArgs);

            hxmlArgs.Insert(0, "--no-output");
            var pluginDir = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(PluginMain)).Location);

            hxmlArgs.Insert(0, "-cp \"" + macroClassPath + "\"");
            if (hxproj.TraceEnabled) hxmlArgs.Insert(0, "-debug");

            return hxmlArgs;
        }

        private static void RemoveComments(List<string> hxmlArgs)
        {
            for (int i = 0; i < hxmlArgs.Count; i++)
            {
                string arg = hxmlArgs[i];
                if (!string.IsNullOrEmpty(arg))
                {
                    if (arg.StartsWith('#')) // commented line
                        hxmlArgs[i] = "";
                }
            }
        }

        private static void QuotePath(List<string> hxmlArgs)
        {
            for (int i = 0; i < hxmlArgs.Count; i++)
            {
                string arg = hxmlArgs[i];
                if (!string.IsNullOrEmpty(arg))
                {
                    Match m = reArg.Match(arg);
                    if (m.Success)
                        hxmlArgs[i] = m.Groups[1].Value + " \"" + m.Groups[2].Value.Trim() + "\"";
                }
            }
        }

        private static void EscapeMacros(List<string> hxmlArgs)
        {
            for (int i = 0; i < hxmlArgs.Count; i++)
            {
                string arg = hxmlArgs[i];
                if (!string.IsNullOrEmpty(arg))
                {
                    Match m = reMacro.Match(arg);
                    if (m.Success)
                        hxmlArgs[i] = m.Groups[1].Value + " \"" + m.Groups[2].Value.Trim() + "\"";
                }
            }
        }

        #endregion




    }
}
