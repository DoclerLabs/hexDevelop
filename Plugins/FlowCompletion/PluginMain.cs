using System;
using System.IO;
using PluginCore.Utilities;
using PluginCore.Managers;
using PluginCore.Helpers;
using PluginCore;
using System.Windows.Forms;
using PluginCore.Controls;
using ScintillaNet;

namespace FlowCompletion
{
    public class PluginMain : IPlugin
    {
        private String pluginName = "Flow Completion";
        private String pluginGuid = "42ac7fab-421b-1f38-a985-72a03534f731";
        private String pluginHelp = "www.flashdevelop.org/community/";
        private String pluginDesc = "Plugin to allow simple completion to work in DSL (Flow) files";
        private String pluginAuth = "Anthony Kondratuk";
        private String settingFilename;
        private Settings settingObject;

        private FlowCompletion completion;


        //private DockContent pluginPanel;
        //private PluginUI pluginUI;
        //private Image pluginImage;

        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public Int32 Api
        {
            get { return 1; }
        }

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public String Name
        {
            get { return this.pluginName; }
        }

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public String Guid
        {
            get { return this.pluginGuid; }
        }

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public String Author
        {
            get { return this.pluginAuth; }
        }

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public String Description
        {
            get { return this.pluginDesc; }
        }

        /// <summary>
        /// Web address for help
        /// </summary> 
        public String Help
        {
            get { return this.pluginHelp; }
        }

        public object Settings
        {
            get
            { return this.settingObject; }
        }

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        /*[Browsable(false)]
        public Object Settings
        {
            get { return this.settingObject; }
        }*/

        #endregion

        #region Required Methods

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            this.InitBasics();
            this.LoadSettings();

            this.completion = new FlowCompletion(CompletionMethod.Combined);

            //PluginBase.MainForm.RegisterShortcutItem("Flow.TriggerCompletion", Keys.Space);
            PluginBase.MainForm.RegisterShortcutItem("Flow.SelectItemForCompletion", Keys.Enter);
            PluginBase.MainForm.RegisterShortcutItem("Flow.GoToDeclaration", Keys.F11);
            EventManager.AddEventHandler(this, EventType.Keys | EventType.ApplySettings, HandlingPriority.Normal);
            UITools.Manager.OnCharAdded += new UITools.CharAddedHandler(OnChar);
            this.AddEventHandlers();
        }

        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose()
        {
            this.SaveSettings();
        }

        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority prority)
        {
            switch (e.Type)
            {
                case EventType.Keys:

                    KeyEvent ke = e as KeyEvent;
                    if (ke.Value == PluginBase.MainForm.GetShortcutItemKeys("Flow.TriggerCompletion"))
                    {
                        //completion.getCompletion(true);
                        //completion.Type = CompletionType.NewDeclaration;
                        //completion.choseCompletion();
                    }
                    else if (ke.Value == PluginBase.MainForm.GetShortcutItemKeys("Flow.GoToDeclaration"))
                    {
                        //MessageBox.Show("GoTo");
                        completion.GoToDeclaration();
                    }
                    break;

                    // Catches FileSwitch event and displays the filename it in the PluginUI.
                    /*case EventType.FileSwitch:
                        string fileName = PluginBase.MainForm.CurrentDocument.FileName;
                        pluginUI.Output.Text += fileName + "\r\n";
                        TraceManager.Add("Switched to " + fileName); // tracing to output panel
                        break;

                    // Catches Project change event and display the active project path
                    case EventType.Command:
                        string cmd = (e as DataEvent).Action;
                        if (cmd == "ProjectManager.Project")
                        {
                            IProject project = PluginBase.CurrentProject;
                            if (project == null)
                                pluginUI.Output.Text += "Project closed.\r\n";
                            else
                                pluginUI.Output.Text += "Project open: " + project.ProjectPath + "\r\n";
                        }
                        break;*/
            }
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        public void InitBasics()
        {
            String dataPath = Path.Combine(PathHelper.DataDir, "Flow Completion");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            this.settingFilename = Path.Combine(dataPath, "Settings.fdb");
            //this.pluginImage = PluginBase.MainForm.FindImage("100");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            // Set events you want to listen (combine as flags)
            EventManager.AddEventHandler(this, EventType.FileSwitch | EventType.Command);
            EventManager.AddEventHandler(this, EventType.Keys);


        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            this.settingObject = new Settings();
            if (!File.Exists(this.settingFilename)) this.SaveSettings();
            else
            {
                Object obj = ObjectSerializer.Deserialize(this.settingFilename, this.settingObject);
                this.settingObject = (Settings)obj;
            }
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(this.settingFilename, this.settingObject);
        }
        #endregion

        private bool shouldComplete(ScintillaControl sci)
        {
            string text = sci.GetLine(sci.CurrentLine);
            int startLine = sci.PositionFromLine(sci.CurrentLine);
            int currPos = sci.CurrentPos;
            int index = -1;

            if ((index = text.IndexOf(";")) > -1)
            {
                if (currPos > (startLine + index))
                    return false;
            }
            return true;
        }

        private void OnChar(ScintillaControl sci, int value)
        {
            if ((char)value != ';' && shouldComplete(sci))
            {
                if ((char)value == ' ') completion.choseCompletion(true, (char)value);
                else completion.getCompletion(false);
            }
            /*else if ((char)value == ';')
                completion.AddNewDeclaration();*/
        }
    }
}
