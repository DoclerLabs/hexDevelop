using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections;

namespace YeomanTemplates.Gui.Controls
{
    public partial class ctrlChoose : UserControl, PageControl
    {
        public ctrlChoose(string yoCmd)
        {
            InitializeComponent();

            Dock = DockStyle.Fill;

            //Check for available generators
            try
            {
                var yo = yeoman.YoHelper.GetYoProcess(yoCmd);
                yo.StartInfo.Arguments = "--generators";
                yo.Start();

                var reader = yo.StandardOutput;

                var parser = new yeoman.GeneratorListParser();
                parser.ParseInput(reader);
                //for (var p : parser.Result);

                reader.Dispose();
            }
            catch (Exception e)
            {
                PluginCore.Managers.ErrorManager.ShowError("Error running yo, please check if the command in the settings is correct: " + yoCmd, e);
            }
        }

        public event EventHandler Ready;

        public object GetOutput()
        {
            var selection = tvGenerator.SelectedNode;
            if (selection.Parent != null)
            {
                return selection.Parent.Text + ":" + selection.Text;
            }
            else
            {
                return selection.Text;
            }
        }

        public void setInput(object data)
        {
            //nothing for now, maybe folder
        }

        public void OnCancel()
        {
        }

        private void tvGenerator_AfterSelect(object sender, TreeViewEventArgs e)
        {
            RaiseReadyEvent();
        }

        public void RaiseReadyEvent()
        {
            if (Ready != null) Ready(this, new EventArgs());
        }

        private void tvGenerator_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var form = (frmRunYeoman)ParentForm;
            form.NextPage();
        }
    }
}
