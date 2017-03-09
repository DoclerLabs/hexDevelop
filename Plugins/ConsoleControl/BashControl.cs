using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Automation;

namespace ConsoleControl
{
    // "C:\Program Files\Git\bin\bash" -c "echo hi & bash"
    public partial class BashControl : CommandControl, ConsoleProvider
    {

        ConsoleColor backColor = ConsoleColor.Black;
        ConsoleColor foreColor = ConsoleColor.White;

        public BashControl(string mintty, string args, bool init = true, string workingDirectory = null) : base(mintty, args, workingDirectory)
        {
            InitializeComponent();
            this.Resize += BashControl_Resize;
            this.Enter += this.BashControl_Enter;
            this.Paint += this.BashControl_Paint;

            if (init)
                Create();
        }

        public ConsoleColor ConsoleBackColor
        {
            get
            {
                return backColor;
            }

            set
            {
                //throw new NotImplementedException();
            }
        }

        public ConsoleColor ConsoleForeColor
        {
            get
            {
                return foreColor;
            }
            set
            {
                //throw new NotImplementedException();
            }
        }

        public string WorkingDirectory
        {
            set
            {
                lastWorkingDir = value;

                if (process == null)
                {
                    Create();
                }
                else
                {
                    SendString("cd \"" + value + "\"");
                    SendString("clear");
                }
            }
        }

        /// <summary>
        /// Creates the console window if it was closed / not created yet
        /// </summary>
        override public void Create()
        {
            base.Create();

            WinApi.SetParent(cmdHandle, this.Handle);
            SendString("clear");
            ResizeConsole();
        }

        override protected void RunCommandWithoutCache(string cmd)
        {
            window.SetFocus();
            //TODO: prevent user from clicking around while doing this
            SendKeys.SendWait(cmd);

            Focus();
        }

        private void BashControl_Resize(object sender, EventArgs e)
        {
            ResizeConsole();
        }

        private void BashControl_Enter(object sender, EventArgs e)
        {
            try
            {
                window.SetFocus();
            }
            catch { }

            ProcessCommandCache();
        }

        private void BashControl_Paint(object sender, PaintEventArgs e)
        {
            ResizeConsole();
        }

        private void ResizeConsole()
        {
            //WinApi.SetWindowLong(cmdHandle, WinApi.GWL_STYLE, 0x80000000L);
            //WinApi.ShowWindow(cmdHandle, WinApi.SW_SHOWMAXIMIZED);
            WinApi.ResizeClientRectTo(cmdHandle, new Rectangle(new Point(0, 0), Size));

            SetRealConsoleSize();

            var tooWide = realSize.Width > Width;
            var tooHigh = realSize.Height > Height;
            if (tooWide || tooHigh)
            {
                int newWidth = realSize.Width;
                int newHeight = realSize.Height;
                if (tooWide)
                {
                    newWidth -= 8;
                }
                if (tooHigh)
                {
                    newHeight -= 12;
                }

                WinApi.ResizeClientRectTo(cmdHandle, new Rectangle(0, 0, newWidth, newHeight));
                SetRealConsoleSize();
            }

            //TODO: use real size for clipping
        }
    }
}
