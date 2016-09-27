using System;
using System.Windows.Forms;

namespace YeomanTemplates.gui.controls
{
    public partial class ctrlCmd : UserControl, PageControl
    {
        string yoCommand;
        string workingDirectory;

        public ctrlCmd(string workingDir, string yoCmd)
        {
            InitializeComponent();
            Dock = DockStyle.Fill;

            yoCommand = yoCmd;
            workingDirectory = workingDir;
        }

        private void ctrlCmd_Load(object sender, System.EventArgs e)
        {
            // 
            // cmdConsole
            // 
            this.cmdConsole = new ConsoleControl.ConsoleControl(true, workingDirectory);
            this.cmdConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdConsole.Location = new System.Drawing.Point(0, 16);
            this.cmdConsole.Name = "cmdConsole";
            this.cmdConsole.Size = new System.Drawing.Size(677, 234);
            this.cmdConsole.TabIndex = 2;
            this.cmdConsole.Exited += cmdConsole_Exited;

            this.Controls.Add(this.cmdConsole);
        }

        private void cmdConsole_Exited(object sender, EventArgs e)
        {
            if (ParentForm != null && ParentForm.InvokeRequired)
            {
                ParentForm.Invoke((MethodInvoker)delegate
                {
                    RaiseReadyEvent();
                });
            }
            else
            {
                RaiseReadyEvent();
            }
        }

        private void ParentForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            cmdConsole.Cancel();
        }


        public event EventHandler Ready;

        public object GetOutput()
        {
            return null;
        }

        public void setInput(object data)
        {
            var generator = (string)data;

            var form = (frmRunYeoman)ParentForm;
            form.DisableBack();

            var command = yoCommand + " " + generator + " && exit";

            cmdConsole.SendString(command);
            ParentForm.FormClosed += ParentForm_FormClosed;
        }

        public void OnCancel()
        {
            ParentForm.Close();
        }

        public void RaiseReadyEvent()
        {
            if (Ready != null) Ready(this, new EventArgs());
        }
    }
}
