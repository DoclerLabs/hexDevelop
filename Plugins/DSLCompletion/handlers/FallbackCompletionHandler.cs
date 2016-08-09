using PluginCore;
using ProjectManager.Projects.Haxe;
using System.IO;
using System.Collections.Generic;

namespace DSLCompletion
{
    /// <summary>
    /// Tries to get some completion info out of files without using the haxe compiler
    /// </summary>
    class FallbackCompletionHandler : ICompletionHandler
    {
        public PositionResult GetPosition(string type)
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);
            var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            paths.AddRange(hxproj.Classpaths);

            var classPaths = paths.ToArray();

            type = type.Replace(".", "/");

            foreach (string path in classPaths)
            {
                //look for module with type as file
                var result = checkType(path, type);
                if (result != null)
                    return result;

                //look if class is within another module
                var splitter = type.LastIndexOf("/");
                var module = type.Substring(0, splitter);
                var typeName = type.Substring(splitter + 1);

                result = checkType(path, module);
                if (result != null)
                    return result;
            }

            return null;
        }

        public string GetFile(string file)
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);
            var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            paths.AddRange(hxproj.Classpaths);
            paths.Reverse(); //later class paths have priority

            var classPaths = paths.ToArray();

            foreach (string cp in classPaths)
            {
                var path = Path.Combine(cp, file);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets completion options for the given path
        /// </summary>
        /// <param name="path">The path that is currently being written. It has to end with a dot</param>
        /// <returns>a list of possible completion options or null if none were found</returns>
        public List<string> GetCompletion(string path)
        {
            var hxproj = (PluginBase.CurrentProject as HaxeProject);
            var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            paths.AddRange(hxproj.Classpaths);

            var classPaths = paths.ToArray();

            path = path.Replace(".", "/");

            var list = new List<string>();

            foreach (string cp in classPaths)
            {
                var file = Path.Combine(cp, path + ".hx");
                var folder = Path.Combine(cp, path);

                if (File.Exists(file)) //the fiven path is a module
                {
                    return null; //not smart enough to parse haxe code
                }
                if (Directory.Exists(folder))
                {
                    foreach (string pack in Directory.GetDirectories(folder))
                    {
                        var packName = Path.GetFileName(pack);
                        if (!list.Contains(packName))
                            list.Add(packName);
                    }
                    list.Sort();

                    var modules = new List<string>();
                    foreach (string module in Directory.GetFiles(folder, "*.hx", SearchOption.TopDirectoryOnly))
                    {
                        var moduleName = Path.GetFileNameWithoutExtension(module);
                        if (!modules.Contains(moduleName))
                            modules.Add(moduleName);
                    }

                    modules.Sort();
                    list.AddRange(modules);
                }
            }

            if (list.Count == 0) return null;

            return list;
        }

        PositionResult checkType(string path, string type)
        {
            var file = Path.Combine(path, type + ".hx");
            if (File.Exists(file))
            {
                return new PositionResult(file, 0);
            }
            return null;
        }
    }
}
