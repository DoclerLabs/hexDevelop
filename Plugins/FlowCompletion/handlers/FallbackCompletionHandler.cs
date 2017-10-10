using System;
using PluginCore;
using ProjectManager.Projects.Haxe;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using ASCompletion.Model;
using ASCompletion.Context;
using System.Windows.Forms;

namespace FlowCompletion
{
    /// <summary>
    /// Tries to get some completion info out of FlashDevelop without using the haxe compiler
    /// </summary>
    class FallbackCompletionHandler : ICompletionHandler
    {
        public void GetPosition(string type, PositionCallback callback)
        {
            var hxproj = PluginBase.CurrentProject as HaxeProject;
            var haxeContext = (AS2Context.Context)ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            #region Workaround for bug
            haxeContext.CurrentModel = new ASCompletion.Model.FileModel();
            if (AS2Context.Context.Panel.InvokeRequired)
            {
                AS2Context.Context.Panel.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    haxeContext.CurrentFile = "";
                });
            }
            #endregion

            var classes = haxeContext.GetAllProjectClasses();

            #region Reset workaround for bug
            haxeContext.CurrentFile = null;
            #endregion
            //Try to find class with given name
            var clazz = SplitAtLastDot(type);

            var classModel = haxeContext.GetModel(clazz[0], clazz[1], "");
            if (classModel.InFile != null && classModel.InFile.FileName != "")
            {
                callback(new PositionResult(classModel.InFile.FileName, classModel.LineFrom, true));
                return;
            }

            //Found no class, look for field in class
            var field = clazz[1];
            clazz = SplitAtLastDot(clazz[0]);
            
            classModel = haxeContext.GetModel(clazz[0], clazz[1], "");
            var members = classModel.Members;
            foreach (ASCompletion.Model.MemberModel member in members)
            {
                if (member.Name == field && (member.Flags & ASCompletion.Model.FlagType.Constructor) == 0)
                {
                    callback(new PositionResult(classModel.InFile.FileName, member.LineFrom, true));
                    return;
                }
            }

            //Nothing found
            callback(null);
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
            haxeContext.CurrentModel = new ASCompletion.Model.FileModel();
            if (AS2Context.Context.Panel.InvokeRequired)
            {
                AS2Context.Context.Panel.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    haxeContext.CurrentFile = "";
                });
            }
            #endregion

            var classes = haxeContext.GetAllProjectClasses();

            #region Reset workaround for bug
            haxeContext.CurrentFile = null;
            #endregion

            //Look for classes
            string[] clazz = SplitAtLastDot(path);

            foreach (var model in classes.Items)
            {
                if (model.Name.Equals(clazz[0])) //this means we are looking for variables within a class
                {
                    var cls = SplitAtLastDot(clazz[0]);
                    var classModel = haxeContext.GetModel(cls[0], cls[1], "");
                    foreach (MemberModel member in classModel.Members.Items)
                    {
                        if (member.Name.StartsWithOrdinal(clazz[1])) //clazz[1] is the variable name in this case
                        {
                            list.Add(member.Name);
                        }
                    }
                }
                else if (model.Name.StartsWith(path))
                {
                    var completion = model.Name.Substring(path.Length);
                    var splt = completion.Split('.');

                    if (!list.Contains(splt[0]))
                        list.Add(splt[0]);
                }
            }

            //if (list.Count == 0) return;
            /*foreach (var itm in list)
                MessageBox.Show(itm.ToString());*/
            callback(list);
        }

        public void GetCompletePath(string clazz, ListCallback callback)
        {
            var results = new List<string>();

            var hxproj = PluginBase.CurrentProject as HaxeProject;
            var haxeContext = (AS2Context.Context)ASCompletion.Context.ASContext.GetLanguageContext(PluginBase.CurrentProject.Language);

            #region Workaround for bug
            haxeContext.CurrentModel = new ASCompletion.Model.FileModel();
            if (AS2Context.Context.Panel.InvokeRequired)
            {
                AS2Context.Context.Panel.Invoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    haxeContext.CurrentFile = "";
                });
            }
            #endregion

            var classes = haxeContext.GetAllProjectClasses();

            #region Reset workaround for bug
            haxeContext.CurrentFile = null;
            #endregion

            foreach (var model in classes.Items)
            {
                if (model.Name.EndsWith("." + clazz) && !results.Contains(model.Name))
                {
                    results.Add(model.Name);
                }
            }

            //if (results.Count == 0) return;

            callback(results);
        }

        /// <summary>
        /// Splits at the last '.' character.
        /// Example: SplitClassFromPackage("example.test.Main") becomes ["example.test", "Main"]
        /// </summary>
        private string[] SplitAtLastDot(string path)
        {
            var split = new List<string>(path.Split('.'));
            var clazz = split[split.Count - 1];
            split.RemoveAt(split.Count - 1);
            var package = string.Join(".", split.ToArray());

            return new string[] { package, clazz };
        }
    }
}
