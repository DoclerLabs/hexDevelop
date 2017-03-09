using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Automation;

namespace ConsoleControl
{
    public partial class CmdControl : CommandControl, ConsoleProvider
    {
        ConsoleColor backColor = ConsoleColor.Black;
        ConsoleColor foreColor = ConsoleColor.White;

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
                    SendString("cls");
                }
            }
        }

        public ConsoleColor ConsoleBackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                if (backColor == value)
                    return;

                backColor = value;

                var trimmedFore = foreColor.ToString("X").TrimStart('0');
                var trimmedBack = backColor.ToString("X").TrimStart('0');
                if (trimmedFore == "")
                    trimmedFore = "0";
                if (trimmedBack == "")
                    trimmedBack = "0";

                SendString("color " + trimmedBack + trimmedFore);
                SendString("cls");
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
                if (foreColor == value)
                    return;

                foreColor = value;

                var trimmedFore = foreColor.ToString("X").TrimStart('0');
                var trimmedBack = backColor.ToString("X").TrimStart('0');
                if (trimmedFore == "")
                    trimmedFore = "0";
                if (trimmedBack == "")
                    trimmedBack = "0";

                SendString("color " + trimmedBack + trimmedFore);
                SendString("cls");
            }
        }

        /// <summary>
        /// Creates a new ConsoleControl
        /// </summary>
        /// <param name="init">If it should be created right away, if this is false, you have to call init manually</param>
        /// <param name="workingDirectory"></param>
        public CmdControl(bool init = true, string workingDirectory = null) : base("cmd", null, workingDirectory)
        {
            InitializeComponent();

            if (init)
                Create();
        }

        /// <summary>
        /// Creates the console window if it was closed / not created yet
        /// </summary>
        override public void Create()
        {
            base.Create();

            WinApi.SetParent(cmdHandle, pnlClipping.Handle);
            SendString("mode 250");
            SendString("cls");
            ResizeConsole();
        }

        override protected void RunCommandWithoutCache(string cmd)
        {
            window.SetFocus();
            //TODO: prevent user from clicking around while doing this
            SendKeys.SendWait(cmd);

            Focus();
        }

        private void ResizeConsole()
        {
            WinApi.ShowWindow(cmdHandle, WinApi.SW_SHOWMAXIMIZED);
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

            pnlClipping.Size = realSize;
        }

        private void CmdPanel_Resize(object sender, EventArgs e)
        {
            ResizeConsole();
        }

        private void CmdPanel_Enter(object sender, EventArgs e)
        {
            try
            {
                window.SetFocus();
            }
            catch { }
            
            ProcessCommandCache();
        }

        private void ConsoleControl_Paint(object sender, PaintEventArgs e)
        {
            ResizeConsole();
        }
    }
}
