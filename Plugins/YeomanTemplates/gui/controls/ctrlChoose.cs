using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System;
using System.Collections;

namespace YeomanTemplates.gui.controls
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

                var parser = new yeoman.GeneratorListParser(tvGenerator);
                parser.ParseInput(reader);

                reader.Dispose();
            }
            catch (Exception e)
            {
                PluginCore.Managers.ErrorManager.ShowError("Error running yo: " + yoCmd, e);
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
    }
}
