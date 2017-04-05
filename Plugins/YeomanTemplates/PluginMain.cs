
using System;
using PluginCore;
using System.ComponentModel;
using PluginCore.Managers;
using PluginCore.Utilities;
using System.IO;
using PluginCore.Helpers;
using PluginCore.Controls;
using ScintillaNet;
using System.Windows.Forms;
using System.Collections.Generic;
using YeomanTemplates.yeoman;
using ProjectManager.Controls.TreeView;

namespace YeomanTemplates
{
    public class PluginMain : IPlugin
    {

        #region Addon Metadata
        private String pluginName = "YeomanTemplates";
        private String pluginGuid = "6809C145-1FCC-453F-9688-F7AC40690F64";
        private String pluginHelp = "http://hexmachina.org/";
        private String pluginDesc = "Plugin to run yeoman from FlashDevelop.";
        private String pluginAuth = "Christoph Otter";
        private String settingFilename;
        private Settings settingObject;
        //private DockContent pluginPanel;
        //private Image pluginImage;

        public int Api
        {
            get
            {
                return 1;
            }
        }

        public string Author
        {
            get
            {
                return pluginAuth;
            }
        }

        public string Description
        {
            get
            {
                return pluginDesc;
            }
        }

        public string Guid
        {
            get
            {
                return pluginGuid;
            }
        }

        public string Help
        {
            get
            {
                return pluginHelp;
            }
        }

        public string Name
        {
            get
            {
                return pluginName;
            }
        }

        [Browsable(false)]
        public object Settings
        {
            get
            {
                return settingObject;
            }
        }
        #endregion

        private List<ToolStripItem> yeomanMenu = new List<ToolStripItem>();

        public void Initialize()
        {
            InitBasics();
            LoadSettings();

            CacheGenerators();

            EventManager.AddEventHandler(this, EventType.Command, HandlingPriority.Normal);
        }

        public void Dispose()
        {
            SaveSettings();
        }

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Command:
                    var data = (DataEvent)e;
                    if (data.Action == "ProjectManager.TreeSelectionChanged" && ProjectTreeView.Instance.SelectedNode is DirectoryNode)
                    {
                        AddYeomanMenu();
                    }
                    break;
            }
        }

        private void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, pluginName);
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this.settingFilename = Path.Combine(dataPath, "Settings.fdb");
        }

        private void AddYeomanMenu()
        {
            var menu = (ProjectContextMenu)ProjectTreeView.Instance.ContextMenuStrip;

            if (menu.AddMenu.DropDownItems.Count == 0 || yeomanMenu.Count == 0) return;

            int secondSeparator = -1;
            int counter = 0;
            for (int i = 0; i < menu.AddMenu.DropDownItems.Count; i++)
            {
                var item = menu.AddMenu.DropDownItems[i];

                if (item is ToolStripSeparator && counter < 2)
                {
                    secondSeparator = i;
                    counter++;
                }
            }

            for (int i = yeomanMenu.Count - 1; i >= 0; i--)
            {
                menu.AddMenu.DropDownItems.Insert(secondSeparator, yeomanMenu[i]);
            }
            menu.AddMenu.DropDownItems.Insert(secondSeparator, new ToolStripSeparator());
        }

        private void LoadSettings()
        {
            this.settingObject = new Settings();
            if (!File.Exists(this.settingFilename)) this.SaveSettings();
            else
            {
                Object obj = ObjectSerializer.Deserialize(this.settingFilename, this.settingObject);
                this.settingObject = (Settings)obj;
            }
        }

        private void SaveSettings()
        {
            ObjectSerializer.Serialize(this.settingFilename, this.settingObject);
        }

        private void CacheGenerators()
        {
            var generators = YoHelper.GetYoGenerators(GetYoCommand());
            var icon = Properties.Resources.yeoman.ToBitmap();

            if (generators == null) return;

            foreach (var parent in generators)
            {
                var item = new ToolStripMenuItem(parent.Value);
                item.Image = icon;
                item.Click += delegate
                {
                    var dir = ProjectTreeView.Instance.SelectedNode.BackingPath;
                    var proc = YoHelper.GetYoGenerator(GetYoCommand(), parent.Value, dir);
                    proc.Start();
                };

                foreach (var child in parent)
                {
                    var childItem = new ToolStripMenuItem(child.Value);
                    childItem.Image = icon;
                    childItem.Click += delegate
                    {
                        var dir = ProjectTreeView.Instance.SelectedNode.BackingPath;
                        var proc = YoHelper.GetYoGenerator(GetYoCommand(), parent.Value + ":" + child.Value, dir);
                        proc.Start();
                    };

                    item.DropDownItems.Add(childItem);
                }
                yeomanMenu.Add(item);
            }
        }

        private String GetYoCommand()
        {
            return settingObject.YoCommand != "" ? settingObject.YoCommand : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm" + Path.DirectorySeparatorChar + "yo.cmd");
        }
    }
}
