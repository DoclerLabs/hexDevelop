using PluginCore;
using PluginCore.Helpers;
using PluginCore.Managers;
using PluginCore.Utilities;
using System;
using System.ComponentModel;
using System.IO;

namespace HaxeCheckStyle
{
    public class PluginMain : IPlugin
    {
        private string pluginName = "HaxeCheckStyle";
        private string pluginGuid = "B4584C60-3D44-43DE-B1D5-6F14968ADB5A";
        private string pluginHelp = "http://hexmachina.org/";
        private string pluginDesc = "Adds haxe-checkstyle support to FlashDevelop.";
        private string pluginAuth = "Christoph Otter";
        private string settingFilename;
        private Settings settingObject;

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

            LintingHelper.Managers.LintingManager.RegisterLinter("haxe", new CheckStyleLinter());
        }

        public void Dispose()
        {
            SaveSettings();
        }

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
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
