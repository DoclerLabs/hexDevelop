using System;
using System.ComponentModel;

namespace YeomanTemplates
{
    [Serializable]
    class Settings
    {
        private string cmd = "yo";

        [DisplayName("Yo Command"), DefaultValue("yo")]
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
