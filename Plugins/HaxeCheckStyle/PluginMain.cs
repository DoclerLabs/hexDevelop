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
        private string settingFilename;
        private Settings settingObject;

        public int Api => 1;

        public string Author => "Christoph Otter";

        public string Description => "Adds haxe-checkstyle support to FlashDevelop.";

        public string Guid => "B4584C60-3D44-43DE-B1D5-6F14968ADB5A";

        public string Help => "http://hexmachina.org/";

        public string Name => "HaxeCheckStyle";

        [Browsable(false)]
        public object Settings => settingObject;

        public void Initialize()
        {
            InitBasics();
            LoadSettings();

            LintingHelper.Managers.LintingManager.RegisterLinter("haxe", new CheckStyleLinter(settingObject));
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
            string dataPath = Path.Combine(PathHelper.DataDir, Name);
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
