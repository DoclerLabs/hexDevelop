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
using System.Management;

namespace ConsoleControl
{
    public abstract partial class CommandControl : UserControl
    {
        protected Process process;
        protected IntPtr cmdHandle;
        protected AutomationElement window;
        protected Size realSize;

        protected List<string> commandsToDo = new List<string>();
        protected string lastWorkingDir;
        protected string cmd;
        protected string arg;

        public event EventHandler Exited;

        public CommandControl(string command, string args, string workingDirectory)
        {
            InitializeComponent();
            SetStyle(ControlStyles.Selectable, true);
            Dock = DockStyle.Fill;
            Text = "Console";

            cmd = command;
            arg = args;
            lastWorkingDir = workingDirectory;
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

                    KillProcessAndChildren(process.Id);
                }

                process = null;
            }
        }

        private void KillProcessAndChildren(int id)
        {
            var search = new ManagementObjectSearcher("SELECT * FROM Win32_Process Where ParentProcessID=" + id);
            foreach (var m in search.Get())
            {
                var childID = Convert.ToInt32(m["ProcessID"]);
                KillProcessAndChildren(childID);
            }

            try
            {
                Process.GetProcessById(id).Kill();
            }
            catch
            {
            }
            
        }

        /// <summary>
        /// Creates the console window if it was closed / not created yet
        /// </summary>
        public virtual void Create()
        {
            if (process != null && !process.HasExited)
            {
                return;
            }

            try
            {
                process = new Process();

                process.StartInfo.FileName = cmd;
                process.StartInfo.Arguments = arg;

                if (lastWorkingDir != null)
                    process.StartInfo.WorkingDirectory = lastWorkingDir;
                process.StartInfo.UseShellExecute = false;

                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

                process.Start();

                //Wait for cmd window
                while (process.MainWindowHandle == IntPtr.Zero)
                {
                    process.Refresh();
                }
                cmdHandle = process.MainWindowHandle;
                window = AutomationElement.FromHandle(cmdHandle);
            }
            catch
            {
            }
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

        protected void ProcessCommandCache()
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

        protected abstract void RunCommandWithoutCache(string cmd);

        protected void Process_Exited(object sender, EventArgs e)
        {
            cmdHandle = IntPtr.Zero;
            if (Exited != null)
                Exited(sender, e);
        }

        protected void SetRealConsoleSize()
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


    }
}
