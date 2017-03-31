using LintingHelper;
using PluginCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HaxeCheckStyle
{
    class CheckStyleLinter : ILintProvider
    {
        /**
        * Match checkstyle entry
        * i.e.  src/Test.hx:11: characters 1-2 : Info: Empty block should be written as "{}"
        * or    src/Test.hx:14: character 1 : Info: Trailing whitespace
        */
        private Regex fileEntry = new Regex(@"^(?<filename>([_A-Za-z]:)?[^:*?]+):(?<line>[0-9]+): characters? (?<chars>[0-9]+(\-[0-9]+)?) : (?<type>[^:]*): (?<description>.*)$", RegexOptions.Compiled);

        public CheckStyleLinter()
        {
        }

        public void DoLintAsync(string[] files, LintCallback callback)
        {
            try
            {
                string command = "$(CompilerPath)\\haxelib.exe";
                string args = "run checkstyle";
                foreach (string file in files)
                {
                    args += " -s \"" + file + "\"";
                }
                command = PluginBase.MainForm.ProcessArgString(command);

                Process p = new Process();
                p.StartInfo.FileName = command;
                p.StartInfo.Arguments = args;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(PluginBase.CurrentProject.ProjectPath);
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.Start();

                var list = new List<LintingResult>();
                while (!p.StandardOutput.EndOfStream)
                {
                    var line = p.StandardOutput.ReadLine();
                    var result = ParseLine(line);
                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
                while (!p.StandardError.EndOfStream)
                {
                    var line = p.StandardError.ReadLine();
                    var result = ParseLine(line);
                    if (result != null)
                    {
                        list.Add(result);
                    }
                }
                p.WaitForExit();
                //PluginBase.MainForm.CallCommand("RunProcessCaptured", command);

                callback(list);
            }
            catch {
                callback(null);
            }
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

            return null;
        }
    }
}
