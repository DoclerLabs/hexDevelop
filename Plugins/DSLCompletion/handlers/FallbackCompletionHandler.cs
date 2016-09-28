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
        public void GetPosition(string type, PositionCallback callback)
        {
            var hxproj = PluginBase.CurrentProject as HaxeProject;
            var haxeContext = (AS2Context.Context)ASCompletion.Context.ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            #region Workaround for bug
            haxeContext.CurrentModel = haxeContext.GetCachedFileModel(hxproj.CompilerOptions.MainClass);
            if (AS2Context.Context.Panel.InvokeRequired)
            {
                AS2Context.Context.Panel.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    haxeContext.CurrentFile = "";
                });
            }
            #endregion

            var classes = haxeContext.GetAllProjectClasses();
            foreach (var model in classes.Items)
            {
                var isModule = model.Name == type;
                if (isModule)
                {
                }
                else if (type.StartsWith(model.Name + "."))
                {
                    foreach (var clazz in model.InFile.Classes)
                    {
                        PluginCore.Managers.TraceManager.Add(clazz.FullName);
                    }
                }
            }

            //var hxproj = (PluginBase.CurrentProject as HaxeProject);
            //var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            //paths.AddRange(hxproj.Classpaths);

            //var classPaths = paths.ToArray();

            //type = type.Replace(".", "/");

            //foreach (string path in classPaths)
            //{
            //    //look for module with type as file
            //    var result = checkType(path, type);
            //    if (result != null)
            //    {
            //        callback(result);
            //        return;
            //    }
                    

            //    //look if class is within another module
            //    var splitter = type.LastIndexOf("/");
            //    var module = type.Substring(0, splitter);
            //    var typeName = type.Substring(splitter + 1);

            //    result = checkType(path, module);
            //    if (result != null)
            //    {
            //        callback(result);
            //        return;
            //    }
            //}
        }

        public void GetFile(string file, StringCallback callback)
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
                    callback(path);
                    return;
                }
            }
        }

        /// <summary>
        /// Gets completion options for the given path
        /// </summary>
        /// <param name="path">The path that is currently being written. It has to end with a dot</param>
        /// <returns>a list of possible completion options or null if none were found</returns>
        public void GetCompletion(string path, ListCallback callback)
        {
            var list = new List<string>();

            var hxproj = PluginBase.CurrentProject as HaxeProject;
            var haxeContext = (AS2Context.Context)ASCompletion.Context.ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            #region Workaround for bug
            haxeContext.CurrentModel = haxeContext.GetCachedFileModel(hxproj.CompilerOptions.MainClass);
            if (AS2Context.Context.Panel.InvokeRequired)
            {
                AS2Context.Context.Panel.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    haxeContext.CurrentFile = "";
                });
            }
            #endregion

            var classes = haxeContext.GetAllProjectClasses();

            foreach (var model in classes.Items)
            {
                //var isModule = model.Name == path.TrimEnd('.');
                //if (isModule)
                //{
                //}
                if (model.Name.StartsWith(path))
                {
                    var completion = model.Name.Substring(path.Length);
                    var split = completion.Split('.');

                    if (!list.Contains(split[0]))
                        list.Add(split[0]);
                }
            }


            //var hxproj = (PluginBase.CurrentProject as HaxeProject);
            //var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            //paths.AddRange(hxproj.Classpaths);

            //var classPaths = paths.ToArray();

            //path = path.Replace(".", "/");
            //PluginCore.Managers.TraceManager.Add(path);
            //var list = new List<string>();

            //foreach (string cp in classPaths)
            //{
            //    var file = Path.Combine(cp, path.TrimEnd('/') + ".hx");
            //    var folder = Path.Combine(cp, path);
            //    PluginCore.Managers.TraceManager.Add(file);
            //    if (File.Exists(file)) //the given path is a module
            //    {
            //        return null; //not smart enough to parse haxe code
            //    }
            //    if (Directory.Exists(folder))
            //    {
            //        foreach (string pack in Directory.GetDirectories(folder))
            //        {
            //            var packName = Path.GetFileName(pack);
            //            if (!list.Contains(packName))
            //                list.Add(packName);
            //        }
            //        list.Sort();

            //        var modules = new List<string>();
            //        foreach (string module in Directory.GetFiles(folder, "*.hx", SearchOption.TopDirectoryOnly))
            //        {
            //            var moduleName = Path.GetFileNameWithoutExtension(module);
            //            if (!modules.Contains(moduleName))
            //                modules.Add(moduleName);
            //        }

            //        modules.Sort();
            //        list.AddRange(modules);
            //    }
            //}

            if (list.Count == 0) return;

            callback(list);
        }

        public void GetCompletePath(string clazz, ListCallback callback)
        {
            var results = new List<string>();

            var hxproj = PluginBase.CurrentProject as HaxeProject;
            var haxeContext = (AS2Context.Context)ASCompletion.Context.ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);
            
            if (haxeContext.CurrentModel == null)
            {
                haxeContext.CurrentModel = haxeContext.GetFileModel(hxproj.CompilerOptions.MainClass);
            }
            var classes = haxeContext.GetAllProjectClasses();
            foreach (var model in classes.Items)
            {
                if (model.Name.EndsWith("." + clazz) && !results.Contains(model.Name))
                {
                    results.Add(model.Name);
                }
            }

            //var hxproj = (PluginBase.CurrentProject as HaxeProject);
            //var paths = ProjectManager.PluginMain.Settings.GlobalClasspaths;
            //paths.AddRange(hxproj.Classpaths);
            //var classPaths = paths.ToArray();

            //foreach (string cp in classPaths)
            //{
            //    if (Directory.Exists(cp))
            //    {
            //        foreach (string moduleName in Directory.GetFiles(cp, "*.hx", SearchOption.AllDirectories))
            //        {
            //            var fileName = Path.GetFileName(moduleName);
            //            if (fileName == clazz + ".hx")
            //            {
            //                var path = moduleName.Substring(cp.Length + 1); //remove cp and "."
            //                path = path.Substring(0, path.Length - 3); //remove ".hx"
            //                path = path.Replace('\\', '/').Replace('/', '.');
            //                results.Add(path);
            //            }
            //        }
            //    }
            //}

            if (results.Count == 0)
                return;

            callback(results);
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
