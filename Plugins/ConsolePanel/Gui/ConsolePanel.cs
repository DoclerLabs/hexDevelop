using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;

namespace ConsolePanel.Gui
{
    public partial class ConsolePanel : UserControl
    {
        Process process;
        IntPtr cmdHandle;
        Size realSize;

        Color backColor = Color.Black;
        Color foreColor = Color.White;
        List<string> commandsToDo = new List<string>();
        string lastWorkingDir;

        public event EventHandler Exited;

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

        public Color ConsoleBackColor
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

                SendString("COLOR " + trimmedBack + trimmedFore);
                SendString("cls");
            }
        }

        public Color ConsoleForeColor
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

                SendString("COLOR " + trimmedBack + trimmedFore);
                SendString("cls");
            }
        }

        /// <summary>
        /// Returns the actual size of the console window's client area
        /// (the width of a console window only changes by specific values)
        /// </summary>
        public Size ActualSize
        {
            get
            {
                return realSize;
            }
        }

        public Process Process
        {
            get
            {
                return process;
            }
        }

        public ConsolePanel(bool init = true, string workingDirectory = null)
        {
            InitializeComponent();

            lastWorkingDir = workingDirectory;

            if (init)
                Create();
        }

        /// <summary>
        /// Cancel the currently running process
        /// </summary>
        public void Cancel()
        {
            if (process != null)
            {
                if (!process.HasExited)
                {
                    SendString("^(c)", false);
                    process.Kill();
                }
                
                process = null;
            }
        }

        /// <summary>
        /// Creates the console window if it was closed / not created yet
        /// </summary>
        public void Create()
        {
            if (process != null && !process.HasExited)
            {
                return;
            }

            try
            {
                process = new Process();

                process.StartInfo.FileName = "cmd";
                if (lastWorkingDir != null)
                    process.StartInfo.WorkingDirectory = lastWorkingDir;
                process.StartInfo.UseShellExecute = false;

                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

                process.Start();
            }
            catch
            {
            }
            //Wait for cmd window
            while (process.MainWindowHandle == IntPtr.Zero)
            {
                process.Refresh();
            }
            cmdHandle = process.MainWindowHandle;

            WinApi.SetParent(cmdHandle, pnlClipping.Handle);

            SendString("cls");

            ResizeConsole();
        }

        /// <summary>
        /// Sends a String to the embedded console window
        /// </summary>
        /// <param name="str">The string to send</param>
        /// <param name="execute">if true, a "\r" is appended to the given string</param>
        public void SendString(string str, bool execute = true)
        {
            if (execute)
                str += "\r";
            ProcessCommandCache();
            try
            {
                RunCommandWithoutCache(str);
            }
            catch
            {
                commandsToDo.Add(str);
            }
        }

        private void ProcessCommandCache()
        {
            while (commandsToDo.Count > 0)
            {
                var toDo = commandsToDo[0];
                
                try
                {
                    RunCommandWithoutCache(toDo);
                    commandsToDo.RemoveAt(0);
                }
                catch
                {
                    break;
                }
            }
        }

        private void RunCommandWithoutCache(string cmd)
        {
            System.Windows.Automation.AutomationElement window = System.Windows.Automation.AutomationElement.FromHandle(cmdHandle);
            
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

            //realSize = WinApi.GetClientRect(cmdHandle).Size;
            pnlClipping.Size = realSize;
        }

        private void SetRealConsoleSize()
        {
            WINDOWINFO info;
            WinApi.GetWindowInfo(cmdHandle, out info);

            var leftBorder = info.rcClient.Left - info.rcWindow.Left;
            var topBorder = info.rcClient.Top - info.rcWindow.Top;
            var rightBorder = (int)info.cxWindowBorders;
            var bottomBorder = (int)info.cyWindowBorders;

            var width = info.rcWindow.Right - info.rcWindow.Left;
            var height = info.rcWindow.Bottom - info.rcWindow.Top;

            realSize.Width = width - leftBorder - rightBorder;
            realSize.Height = height - topBorder - bottomBorder;
        }

        private void CmdPanel_Resize(object sender, EventArgs e)
        {
            ResizeConsole();
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            cmdHandle = IntPtr.Zero;
            if (Exited != null)
                Exited(sender, e);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            Cancel();

            base.Dispose(disposing);
        }

        private void CmdPanel_Enter(object sender, EventArgs e)
        {
            ProcessCommandCache();
        }

        private void CmdPanel_Paint(object sender, PaintEventArgs e)
        {
            WinApi.ResizeClientRectTo(cmdHandle, new Rectangle(new Point(0, 0), Size));
        }
    }
}
