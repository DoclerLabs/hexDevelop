
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

namespace YeomanTemplates
{
    public class PluginMain : IPlugin
    {
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

        public void Initialize()
        {
            InitBasics();
            LoadSettings();

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
                    if (data.Action == "ProjectManager.CreateNewFile")
                    {
                        var table = data.Data as System.Collections.Hashtable;
                        var templatePath = table["templatePath"] as String;
                        var directory = table["inDirectory"] as String;
                        
                        if (templatePath.EndsWith("Yeoman.fdt"))
                        {
                            e.Handled = true; //make sure no other plugin generates stuff
                            
                            //show wizard
                            var cwd = Environment.CurrentDirectory;
                            Environment.CurrentDirectory = directory;

                            var yoCmd = settingObject.YoCommand != "" ? settingObject.YoCommand : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm" + Path.DirectorySeparatorChar + "yo.cmd");
                            var runYo = new frmRunYeoman(directory, yoCmd);
                            runYo.ShowDialog(PluginBase.MainForm);
                            Environment.CurrentDirectory = cwd;
                        }
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
    }
}
