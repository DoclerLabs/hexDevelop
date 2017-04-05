using PluginCore;
using ProjectManager.Projects;
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
            catch
            {
                PluginCore.Managers.TraceManager.Add("Error running yo, please check if the command in the settings is correct: " + yoCmd, (int)TraceType.Error);
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

        public static Process GetYoGenerator(string yoCmd, string generator, string directory)
        {
            var proj = PluginCore.PluginBase.CurrentProject as Project;

            var generatorKey = "yeomanoptions_" + generator;

            var options = "";
            if (proj.Storage.ContainsKey(generatorKey))
            {
                options = proj.Storage[generatorKey];
            }
            else
            {
                #region hexMachina specific
                if (generator.StartsWith("hex:") || generator == "hex")
                {
                    options = "--currentPackage=\"$(Package)\"";
                }
                #endregion

                proj.Storage.Add(generatorKey, options);
            }

            //replace $(Placeholders)
            options = PluginCore.PluginBase.MainForm.ProcessArgString(options);
            options = options.Replace("$(Package)", GetPackage(directory));
            options = options.Replace("$(FolderName)", directory);

            var args = generator + " " + options;

            var proc = GetVisibleYo(yoCmd);
            proc.StartInfo.WorkingDirectory = directory;
            proc.StartInfo.Arguments = args;

            return proc;
        }

        public static string GetYoFile(string folder)
        {
            return Path.Combine(folder, "yo.cmd");
        }

        private static string GetPackage(string directory)
        {
            var project = PluginCore.PluginBase.CurrentProject as ProjectManager.Projects.Haxe.HaxeProject;
            var package = "";


            string classpath = project.AbsoluteClasspaths.GetClosestParent(directory);

            if (classpath == null)
            {
                var collection = new PathCollection();
                foreach (var path in ProjectManager.PluginMain.Settings.GlobalClasspaths)
                {
                    collection.Add(path);
                }
                classpath = collection.GetClosestParent(directory);
            }
            if (classpath != null)
            {
                package = ProjectPaths.GetRelativePath(classpath, directory).Replace(Path.DirectorySeparatorChar, '.');
            }

            return package;
        }
    }
}
