using System;
using System.IO;
using PluginCore;
using PluginCore.Helpers;
using PluginCore.Localization;
using PluginCore.Managers;
using PluginCore.Utilities;
using WeifenLuo.WinFormsUI.Docking;
using ProjectManager.Projects;
using System.Windows.Forms;
using EditorConfig.Core;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ScintillaNet;
using PluginCore.Controls;

namespace EditorConfig
{
    public class PluginMain : IPlugin
    {
        private String pluginName = "EditorConfig";
        private String pluginGuid = "0158C50F-6FC7-45F3-BACA-F3C0F1242CEF";
        private String pluginHelp = "http://hexmachina.org/";
        private String pluginDesc = "Reads and applies .editorconfig files";
        private String pluginAuth = "Christoph Otter";

        private EditorConfigParser parser;
        private Project lastProject;
        private List<string> openingCache = new List<string>();

        private bool originalTrimWhitespace;
        private bool originalEnsureLastLine;

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

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        public Object Settings
        {
            get { return null; }
        }
        
        #endregion
        
        #region Required Methods
        
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            this.AddEventHandlers();
        }

        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose()
        {
        }
        
        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(Object sender, NotifyEvent e, HandlingPriority priority)
        {
            switch (e.Type)
            {
                case EventType.Command:
                    var data = (DataEvent)e;
                    if (data.Action == "ProjectManager.Project")
                    {
                        var project = (Project)data.Data;

                        if (lastProject != project)
                        {
                            parser = new EditorConfigParser();
                            ProcessCache();

                            lastProject = project;
                        }
                    }
                    break;
                case EventType.FileEncode: //have to use FileEncode, because otherwise some settings of SciControl are overwritten after FileSaving
                    DataEvent d = (DataEvent)e;
                    string file = d.Action;
                    var document = DocumentManager.FindDocument(file);

                    //Make sure indentation is not overwritten
                    ApplyIndentation(document.SciControl, GetConfig(document.FileName));

                    OnSaveFile(document);

                    //reset to old values
                    PluginBase.Settings.StripTrailingSpaces = originalTrimWhitespace;
                    PluginBase.Settings.EnsureLastLineEnd = originalEnsureLastLine;

                    break;
                case EventType.FileOpen:
                    TextEvent fileOpen = (TextEvent)e;
                    
                    if (PluginBase.CurrentProject == null) //not loaded yet
                    {
                        openingCache.Add(fileOpen.Value);
                    }
                    else
                    {
                        document = DocumentManager.FindDocument(fileOpen.Value);
                        OnOpenFile(document);
                    }

                    break;
            }
        }
        
        #endregion

        #region Custom Methods
        /// <summary>
        /// Processes the cached open files.
        /// In this case, cached means that these files were just opened, but OnOpenFile was not called yet.
        /// This is needed, because FileOpen event is fired before ProjectManager.Project event
        /// </summary>
        private void ProcessCache()
        {
            if (parser == null)
            {
                return;
            }

            //process cache first
            foreach (string cachedFile in openingCache)
            {
                var doc = DocumentManager.FindDocument(cachedFile);
                if (doc != null)
                {
                    OnOpenFile(doc);
                }
            }
            openingCache.Clear();
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            EventType eventMask = EventType.FileOpen | EventType.FileEncode | EventType.Command;
            EventManager.AddEventHandler(this, eventMask);
        }

        /// <summary>
        /// Gets the .editorconfig properties that apply to the given file
        /// </summary>
        private FileConfiguration GetConfig(string filename)
        {
            return parser.Parse(filename).First();
        }

        private void OnOpenFile(ITabbedDocument document)
        {
            TraceManager.Add(document.FileName);
            var sci = document.SciControl;
            var config = GetConfig(document.FileName);

            ApplyMaxLineLength(sci, config);

            ApplyCharset(sci, config);

            ApplyIndentation(sci, config);

            document.RefreshTexts();

        }

        private void OnSaveFile(ITabbedDocument document)
        {
            var sci = document.SciControl;
            var config = GetConfig(document.FileName);

            ApplyEOL(sci, config);

            ApplyTrimWhitespace(sci, config);
        }

        private void ApplyCharset(ScintillaControl sci, FileConfiguration config)
        {
            Encoding e = sci.Encoding;
            switch (config.Charset)
            {
                case Charset.Latin1:
                    e = Encoding.GetEncoding("ISO-8859-1");
                    break;
                case Charset.UTF16BE:
                    e = Encoding.BigEndianUnicode;
                    break;
                case Charset.UTF16LE:
                    e = Encoding.Unicode;
                    break;
                case Charset.UTF8:
                    e = new UTF8Encoding(false);
                    break;
                case Charset.UTF8BOM:
                    e = Encoding.UTF8;
                    break;
            }
            sci.Encoding = e;
        }

        private void ApplyIndentation(ScintillaControl sci, FileConfiguration config)
        {
            //indent style
            switch (config.IndentStyle)
            {
                case IndentStyle.Space:
                    sci.IsUseTabs = false;
                    break;
                case IndentStyle.Tab:
                    sci.IsUseTabs = true;
                    break;
            }

            //tab width
            sci.TabWidth = config.TabWidth ?? sci.TabWidth;

            //indent size
            sci.Indent = config.IndentSize.NumberOfColumns ?? sci.Indent;

        }

        private void ApplyMaxLineLength(ScintillaControl sci, FileConfiguration config)
        {
            if (config.MaxLineLength != null)
            {
                //This might be problematic if there already is an edge defined. In that case, it is overridden
                sci.EdgeMode = (int)ScintillaNet.Enums.EdgeVisualStyle.Line;
                sci.EdgeColumn = (int)config.MaxLineLength;
            }
        }

        private void ApplyFinalNewLine(ScintillaControl sci, FileConfiguration config)
        {
            if (config.InsertFinalNewline == true)
            {
                sci.AddLastLineEnd();
            }

            //we handle this ourself, however this is reset to the previous value later,
            //to not mess with the user's settings
            originalEnsureLastLine = PluginBase.Settings.EnsureLastLineEnd;
            PluginBase.Settings.EnsureLastLineEnd = false;
        }

        private void ApplyEOL(ScintillaControl sci, FileConfiguration config)
        {
            ScintillaNet.Enums.EndOfLine mode = sci.EndOfLineMode;
            switch (config.EndOfLine)
            {
                case EndOfLine.CR:
                    mode = ScintillaNet.Enums.EndOfLine.CR;
                    break;
                case EndOfLine.CRLF:
                    mode = ScintillaNet.Enums.EndOfLine.CRLF;
                    break;
                case EndOfLine.LF:
                    mode = ScintillaNet.Enums.EndOfLine.LF;
                    break;
            }
            sci.ConvertEOLs(mode);
        }

        private void ApplyTrimWhitespace(ScintillaControl sci, FileConfiguration config)
        {
            if (config.TrimTrailingWhitespace != null)
            {
                if ((bool)config.TrimTrailingWhitespace)
                {
                    sci.StripTrailingSpaces(false);
                }
            }

            //same as ApplyFinalNewLine
            originalTrimWhitespace = PluginBase.Settings.StripTrailingSpaces;
            PluginBase.Settings.StripTrailingSpaces = false;
        }

        #endregion

    }
    
}
