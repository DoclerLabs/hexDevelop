using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ASCompletion.Context;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Localization;
using PluginCore.Managers;
using PluginCore.Utilities;
using UnitTestSessionsPanel.Handlers;
using UnitTestSessionsPanel.Handlers.MessageHandlers;
using UnitTestSessionsPanel.Localization;
using WeifenLuo.WinFormsUI.Docking;

namespace UnitTestSessionsPanel
{
    public class PluginMain : IPlugin
    {
        private const string ExplorerPanelGuid = "3598862e-85e7-4998-96b8-7db0c315fb68";

        private string settingsFilename;
        private TestsSessionsPanel sessionsPanel;
        private TestsExplorerPanel explorerPanel;
        private Image image;
        private DockContent sessionsDockContent;
        private DockContent explorerDockContent;
        private IEventHandler processHandler;
        private IEventHandler traceHandler;
        private Handlers.MessageHandlers.HexUnit.HexUnitWebSocketHandler hexUnitHandler;
        private Handlers.MessageHandlers.HexUnit.HexUnitSocketHandler hexUnitHandler2;

        private string pluginDesc = "FlashDevelop Plugin for Unit Testing for Haxe and AS3";

        #region IPlugin Getters

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api
        {
            get { return 1; }
        }

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public string Name
        {
            get { return "UnitTestSessionsPanel"; }
        }

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid
        {
            get { return "89D3CD09-9AF5-4372-9067-2A5419595EE8"; }
        }

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author
        {
            get { return "FlashDevelop"; }
        }

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public string Description
        {
            get { return pluginDesc; }
        }

        /// <summary>
        /// Web address for help
        /// </summary> 
        public string Help
        {
            get { return "http://www.flashdevelop.org/community/"; }
        }

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        public object Settings { get; private set; }

        #endregion

        #region IPlugin Implementation

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            InitBasics();
            LoadSettings();
            InitLocalization();
            CreatePluginPanel();
            CreateMenuItem();
            AddEventHandlers();
        }

        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose()
        {
            SaveSettings();
        }

        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            if (ASContext.Context.IsFileValid && ASContext.Context.CurrentModel.haXe)
            {
                foreach (var classModel in ASContext.Context.CurrentModel.Classes)
                foreach (var member in classModel.Members.Items)
                {
                    if (member.MetaDatas != null)
                    {
                        foreach (var meta in member.MetaDatas)
                        {
                            if (meta.Name == "Test" || meta.Name == "Suite" || meta.Name == "Ignore")
                            {
                                explorerPanel.AddTest(new TestInformation
                                {
                                    ClassName = classModel.QualifiedName,
                                    Name = member.Name,
                                    FunctionName = member.Name,
                                    Path = ASContext.Context.CurrentFile
                                });

                                break;
                            }
                        }
                    }
                }
            }

            switch (e.Type)
            {
                case EventType.Command:
                    var de = e as DataEvent;
                    if (de != null && de.Action == ProjectManager.ProjectManagerEvents.BuildComplete)
                    {
                        MessageBox.Show("test");
                    }
                    break;
            }
            //runner.HandleEvent()
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes important variables
        /// </summary>
        private void InitBasics()
        {
            string dataPath = Path.Combine(PathHelper.DataDir, "UnitTestSessionsPanel");
            if (!Directory.Exists(dataPath)) Directory.CreateDirectory(dataPath);
            settingsFilename = Path.Combine(dataPath, "Settings.fdb");
            image = PluginBase.MainForm.FindImage("340");
        }

        /// <summary>
        /// Initializes the localization of the plugin
        /// </summary>
        private void InitLocalization()
        {
            LocaleVersion language = PluginBase.MainForm.Settings.LocaleVersion;
            switch (language)
            {
                case LocaleVersion.de_DE:
                case LocaleVersion.eu_ES:
                case LocaleVersion.ja_JP:
                case LocaleVersion.zh_CN:
                case LocaleVersion.en_US:
                default:
                    LocalizationHelper.Initialize(LocaleVersion.en_US);
                    break;
            }
            pluginDesc = LocalizationHelper.GetString("Description");
        }
        
        /// <summary>
        /// Creates a plugin panel for the plugin
        /// </summary>
        private void CreatePluginPanel()
        {
            sessionsPanel = new TestsSessionsPanel {Text = LocalizationHelper.GetString("PluginPanel"), Settings = (Settings) Settings};
            explorerPanel = new TestsExplorerPanel {Text = "Unit Test Explorer"/*, Settings = (Settings)Settings*/};
            sessionsDockContent = PluginBase.MainForm.CreateDockablePanel(sessionsPanel, Guid, image, DockState.DockBottom);
            explorerDockContent = PluginBase.MainForm.CreateDockablePanel(explorerPanel, ExplorerPanelGuid, image, DockState.DockBottom);
            processHandler = new ProcessEventHandler(sessionsPanel);
            //traceHandler = new TraceHandler(ui);
            hexUnitHandler = new Handlers.MessageHandlers.HexUnit.HexUnitWebSocketHandler(sessionsPanel);
            hexUnitHandler2 = new Handlers.MessageHandlers.HexUnit.HexUnitSocketHandler(sessionsPanel);
        }

        /// <summary>
        /// Creates a menu item for the plugin and adds a ignored key
        /// </summary>
        private void CreateMenuItem()
        {
            string label = LocalizationHelper.GetString("ViewMenuItem");
            ToolStripMenuItem viewMenu = (ToolStripMenuItem) PluginBase.MainForm.FindMenuItem("ViewMenu");
            ToolStripMenuItem unitTestsItem = new ToolStripMenuItem("Unit Tests");
            ToolStripMenuItem sessionsItem = new ToolStripMenuItem(label, image, OpenSessionsPanel);
            ToolStripMenuItem explorerItem = new ToolStripMenuItem("Unit Test Explorer", image, OpenExplorerPanel);

            unitTestsItem.DropDownItems.AddRange(new[]{ explorerItem, sessionsItem });
            viewMenu.DropDownItems.Add(unitTestsItem);
        }
        
        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        private void AddEventHandlers()
        {
            EventManager.AddEventHandler(processHandler, EventType.ProcessStart | EventType.ProcessEnd);
            EventManager.AddEventHandler(this, EventType.FileSwitch);
            //EventManager.AddEventHandler(traceHandler, EventType.Trace);
        }

        private void OpenSessionsPanel(object sender, EventArgs e)
        {
            sessionsDockContent.Show();
        }

        private void OpenExplorerPanel(object sender, EventArgs e)
        {
            explorerDockContent.Show();
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        public void LoadSettings()
        {
            Settings = new Settings();
            if (!File.Exists(settingsFilename)) this.SaveSettings();
            else Settings = (Settings)ObjectSerializer.Deserialize(settingsFilename, Settings);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        public void SaveSettings()
        {
            ObjectSerializer.Serialize(settingsFilename, Settings);
        }

        #endregion
    }
}