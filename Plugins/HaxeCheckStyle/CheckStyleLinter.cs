using LintingHelper;
using PluginCore;
using PluginCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PluginCore.Managers;

namespace HaxeCheckStyle
{
    class CheckStyleLinter : ILintProvider
    {
        private Settings settings;
        /**
        * Match checkstyle entry
        * i.e.  src/Test.hx:11: characters 1-2 : Info: Empty block should be written as "{}"
        * or    src/Test.hx:14: character 1 : Info: Trailing whitespace
        */
        private readonly Regex fileEntry = new Regex(@"^(?<filename>([_A-Za-z]:)?[^:*?]+):(?<line>[0-9]+): characters? (?<chars>[0-9]+(\-[0-9]+)?) : (?<type>[^:]*): (?<description>.*)$", RegexOptions.Compiled);

        public CheckStyleLinter(Settings settings)
        {
            this.settings = settings;
        }

        public void LintAsync(string[] files, LintCallback callback)
        {
            ProcessRunner p = new ProcessRunner();
            try
            {
                string command = FindHaxelibPath();
                if (command == null)
                {
                    callback(null);
                    return;
                }

                var list = new List<LintingResult>();

                Action<object, string> output = (sender, line) =>
                {
                    var result = ParseLine(line);
                    if (result != null)
                    {
                        list.Add(result);
                    }
                };
                Action<object, int> ended = (sender, exitCode) =>
                {
                    callback(list);
                };
                p.Output += new LineOutputHandler(output);
                p.Error += new LineOutputHandler(output);
                p.ProcessEnded += new ProcessEndedHandler(ended);

                if (PluginBase.CurrentProject != null)
                {
                    p.WorkingDirectory = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                }

                string args = "run checkstyle";

                foreach (string file in files)
                {
                    args += " -s \"" + file + "\"";
                }

                if (settings.PreferGlobalSettings && !string.IsNullOrEmpty(settings.CustomSettingsFile))
                {
                    args += " -c \"" + settings.CustomSettingsFile + "\"";
                }

                p.Run(command, args);
            }
            catch
            {
                callback(null);
                p.KillProcess();
            }
        }

        public void LintProjectAsync(IProject project, LintCallback callback)
        {
            LintAsync(project.SourcePaths, callback);
        }

        LintingResult ParseLine(string line)
        {
            var match = fileEntry.Match(line);
            if (match.Success)
            {
                var result = new LintingResult();
                result.File = match.Groups["filename"].Value;
                result.Line = int.Parse(match.Groups["line"].Value);
                result.Description = match.Groups["description"].Value;

                //chars could be a range like "0-1" or just a single position like "0"
                var chars = match.Groups["chars"].Value.Split('-');
                result.FirstChar = int.Parse(chars[0]);
                if (chars.Length > 1)
                {
                    result.Length = int.Parse(chars[1]) - result.FirstChar;
                }

                var severity = match.Groups["type"].Value;
                switch (severity.ToLower())
                {
                    case "info":
                        result.Severity = LintingSeverity.Info;
                        break;
                    case "error":
                        result.Severity = LintingSeverity.Error;
                        break;
                    case "warning":
                        result.Severity = LintingSeverity.Warning;
                        break;
                }

                return result;
            }

            if (line.StartsWithOrdinal("Error"))
                TraceManager.Add(line);
            return null;
        }

        static string FindHaxelibPath()
        {
            string command = "$(CompilerPath)\\haxelib.exe";
            command = PluginBase.MainForm.ProcessArgString(command);

            if (File.Exists(command)) return command;

            command = Path.Combine(Environment.ExpandEnvironmentVariables("%HAXEPATH%"), "haxelib.exe");

            if (File.Exists(command)) return command;
            
            TraceManager.Add("Error: Could not find haxelib.exe");

            return null;
        }
    }
}
