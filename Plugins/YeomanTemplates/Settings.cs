using System;
using System.ComponentModel;

namespace YeomanTemplates
{
    [Serializable]
    class Settings
    {
        private string cmd = "";

        [DisplayName("Yo Command"), DefaultValue("")]
        public string YoCommand
        {
            get
            {
                return this.cmd;
            }
            set
            {
                this.cmd = value;
            }
        }
    }
}
