
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

namespace DSLCompletion
{
    public class PluginMain : IPlugin
    {
        private String pluginName = "DSLCompletion";
        private String pluginGuid = "20D4708C-D7EF-45ED-90CF-8D2FA98C5261";
        private String pluginHelp = "http://hexmachina.org/";
        private String pluginDesc = "Plugin to allow F4 and simple completion to work in DSL (XML) files.";
        private String pluginAuth = "Christoph Otter";
        private String settingFilename;
        private Settings settingObject;
        //private DockContent pluginPanel;
        //private Image pluginImage;

        private DSLComplete completion;

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
            
            this.completion = new DSLComplete(settingObject.CompletionMethod);

            PluginBase.MainForm.RegisterShortcutItem("DSLCompletion.GotoDeclaration", Keys.F12);
            EventManager.AddEventHandler(this, EventType.Keys | EventType.ApplySettings, HandlingPriority.Normal);
            UITools.Manager.OnCharAdded += new UITools.CharAddedHandler(OnChar);
        }

        public void Dispose()
        {
            SaveSettings();
        }

        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Keys:
                    KeyEvent ke = e as KeyEvent;
                    
                    if (ke.Value == PluginBase.MainForm.GetShortcutItemKeys ("DSLCompletion.GotoDeclaration"))
                    {
                        if (completion.GotoDeclaration())
                        {
                            ke.Handled = true;
                            ke.ProcessKey = false;
                        }
                    }
                    else if (ke.Value == (Keys.Control | Keys.Space))
                    {
                        if (completion.GetCompletion())
                        {
                            ke.Handled = true;
                            ke.ProcessKey = false;
                        }
                    }
                    break;
                case EventType.ApplySettings:
                    completion.SetCompletionMethod(settingObject.CompletionMethod);
                    break;
            }
        }

        private void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, "DSLCompletion");
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

        private void OnChar(ScintillaControl sci, int value)
        {
            if (DSLComplete.IsAllowedChar((char)value))
            {
                completion.GetCompletion();
            }
        }
    }
}
