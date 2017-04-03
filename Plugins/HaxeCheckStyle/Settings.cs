using PluginCore.Localization;
using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace HaxeCheckStyle
{
    [Serializable]
    public class Settings
    {
        private string globalFile = "";
        private bool peferGlobal = false;

        [DisplayName("Global checkstyle.json")]
        [DefaultValue("")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string CustomSettingsFile
        {
            get
            {
                return globalFile;
            }
            set
            {
                globalFile = value;
            }
        }

        [DisplayName("Prefer global checkstyle.json")]
        [DefaultValue(false)]
        public bool PreferGlobalSettings
        {
            get
            {
                return peferGlobal;
            }
            set
            {
                peferGlobal = value;
            }
        }

    }
}