using PluginCore;
using PluginCore.Helpers;
using ProjectManager.Projects.Haxe;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace FlowCompletion
{
    /// <summary>
    /// Uses the haxe compiler together with a Haxe macro to get accurate, but slow completion results.
    /// </summary>
    class CompilerCompletionHandler : ICompletionHandler
    {
        protected Process process;
        private string macroClassPath;

        public CompilerCompletionHandler() : base()
        {
            ExtractHaxeMacro();
        }

        public void GetCompletePath(string module, ListCallback callback)
        {
            setupProcess();

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.completePath('" + module + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();
            if (rawResult == null || rawResult == "[]") return;

            var result = rawResult.Substring(1, rawResult.Length - 2).Split(','); //remove [ and ] and split
            var list = new List<string>();

            foreach (string type in result)
            {
                list.Add(type);
            }

            if (list.Count == 0)
                return;

            callback(list);
        }

        public void GetPosition(string type, PositionCallback callback)
        {
            setupProcess();

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.find('" + type + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();
            if (rawResult == null) return;

            var result = rawResult.Split(';');
            if (result.Length != 2) return;

            var file = result[0];
            var pos = result[1];

            callback(new PositionResult(file, Int32.Parse(pos), false));
        }

        /// <summary>
        /// Gets completion options for the given path, or null if none are found
        /// </summary>
        public void GetCompletion(string path, ListCallback callback)
        {
            try
            {
                setupProcess();
            }
            catch(Exception e)
            {
                return;
            }

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.getCompletion('" + path + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();
            if (rawResult == null || rawResult == "[]") return;

            var result = rawResult.Substring(1, rawResult.Length - 2).Split(','); //remove [ and ] and split
            var list = new List<string>();

            foreach (string type in result)
            {
                list.Add(type);
            }

            if (list.Count == 0)
                return;

            callback(list);
        }

        public void GetFile(string file, StringCallback callback)
        {
            setupProcess();

            var args = GetArgs();
            args.Insert(0, "--macro \"util.ReferenceMacro.getFile('" + file + "')\"");
            process.StartInfo.Arguments = String.Join(" ", args.ToArray());

            var rawResult = waitForCompiler();

            callback(rawResult);
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
           
            macroClassPath = Path.Combine(PathHelper.DataDir, "FlowCompletion/haxe_code");
            //MessageBox.Show("macroclasspath -> " + macroClassPath);
            var filename = Path.Combine(macroClassPath, "util/ReferenceMacro.hx");
            //MessageBox.Show("filename -> " + filename);
            var util = Path.GetDirectoryName(filename);
            //MessageBox.Show("util -> " + util);

            if (!Directory.Exists(util)) Directory.CreateDirectory(util);
            File.WriteAllBytes(filename, Properties.Resources.ReferenceMacro);
        }

        /// <summary>
        /// Creates a Process object for haxe completion without arguments.
        /// </summary>
        private Process CreateProcess()
        {
            if (PluginBase.CurrentSDK.Path == null)
            {
                throw new Exception("Please setup haxe SDK");
            }

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

        /// <summary>
        /// Starts the compiler and waits for the Macro to print out 
        /// </summary>
        /// <returns></returns>
        private string waitForCompiler(bool logErrors = true)
        {
            try
            {
                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.Close();

                output = output.Replace("\r\n", "\n");
                if (logErrors && error != null && error != "")
                    PluginCore.Managers.TraceManager.Add(error);

                foreach (string line in output.Split('\n', '\r'))
                {
                    if (line.StartsWith("ReferenceMacro "))
                    {
                        return line.Substring("ReferenceMacro ".Length);
                    }
                }
            }
            catch
            {
            }

            return null;
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
