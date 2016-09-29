using ProjectManager.Projects;
using System;
using System.IO;
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
            this.cmdConsole.Anchor = (((AnchorStyles.Top | AnchorStyles.Bottom)
            | AnchorStyles.Left)
            | AnchorStyles.Right);
            this.cmdConsole.Location = new System.Drawing.Point(0, 16);
            this.cmdConsole.Name = "cmdConsole";
            this.cmdConsole.Size = new System.Drawing.Size(677, 234);
            this.cmdConsole.TabIndex = 2;
            this.cmdConsole.Exited += cmdConsole_Exited;

            this.Controls.Add(this.cmdConsole);
        }

        private void cmdConsole_Exited(object sender, EventArgs e)
        {
            if (ParentForm == null || ParentForm.IsDisposed)
                return;

            if (ParentForm.InvokeRequired)
            {
                try
                {
                    ParentForm.Invoke((MethodInvoker)delegate
                    {
                        RaiseReadyEvent();
                    });
                }
                catch { }
                
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

            var proj = PluginCore.PluginBase.CurrentProject as Project;
            
            var generatorKey = "yeomanoptions_" + generator;

            var options = "";
            if (proj.Storage.ContainsKey(generatorKey))
            {
                options = proj.Storage[generatorKey];
            }
            else
            {
                #region hexMachina specific
                if (generator.StartsWith("hex:") || generator == "hex")
                {
                    options = "--currentPackage=\"$(Package)\"";
                }
                #endregion

                proj.Storage.Add(generatorKey, options);
            }

            //replace $(Placeholders)
            options = PluginCore.PluginBase.MainForm.ProcessArgString(options);
            options = options.Replace("$(Package)", GetPackage(Environment.CurrentDirectory));
            options = options.Replace("$(FolderName)", Environment.CurrentDirectory);

            var command = yoCommand + " " + generator + " " + options + " && exit";

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

        string GetPackage(string directory)
        {
            var project = PluginCore.PluginBase.CurrentProject as ProjectManager.Projects.Haxe.HaxeProject;
            var package = "";

            
            string classpath = project.AbsoluteClasspaths.GetClosestParent(directory);

            if (classpath == null)
            {
                var collection = new PathCollection();
                foreach (var path in ProjectManager.PluginMain.Settings.GlobalClasspaths)
                {
                    collection.Add(path);
                }
                classpath = collection.GetClosestParent(directory);
            }
            if (classpath != null)
            {
                package = ProjectPaths.GetRelativePath(classpath, directory).Replace(Path.DirectorySeparatorChar, '.');
            }

            return package;
        }
    }
}
