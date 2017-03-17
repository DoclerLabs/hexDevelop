using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace ConsolePanel
{
    [Serializable]
    class Settings
    {
        private ConsoleColor background = ConsoleColor.Black;
        private ConsoleColor foreground = ConsoleColor.White;

        private TerminalProvider provider = TerminalProvider.Bash;
        private string minttyCmd = "";
        private string bashCmd = "-e \"\"";

        [Category("Cmd"), DisplayName("Background Color"), DefaultValue(ConsoleColor.Black)]
        public ConsoleColor BackgroundColor
        {
            get
            {
                return this.background;
            }
            set
            {
                this.background = value;
            }
        }

        [Category("Cmd"), DisplayName("Foreground Color"), DefaultValue(ConsoleColor.White)]
        public ConsoleColor ForegroundColor
        {
            get
            {
                return this.foreground;
            }
            set
            {
                this.foreground = value;
            }
        }

        [DisplayName("Terminal"), DefaultValue(TerminalProvider.Bash)]
        public TerminalProvider TerminalProvider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        [Category("Bash"),
            Description("The path to your mintty.exe\nExample: C:\\Program Files\\Git\\usr\\bin\\mintty.exe"),
            DisplayName("Command")]
        [Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
        public string MinttyCommand
        {
            get
            {
                return this.minttyCmd;
            }
            set
            {
                this.minttyCmd = value;
            }
        }

        [Category("Bash"),
            Description("The parameter to pass to mintty. It contains the path to bash in posix path format.\nExample: -e \"/c/Program Files/Git/bin/bash\""),
            DisplayName("Parameters"),
            DefaultValue("-e \"\"")]
        public string BashCommand
        {
            get
            {
                return this.bashCmd;
            }
            set
            {
                this.bashCmd = value;
            }
        }
    }

    enum TerminalProvider
    {
        Bash,
        Cmd
    }
}
