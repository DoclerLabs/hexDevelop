using System;
using PluginCore;
using PluginCore.Managers;
using ProjectManager.Projects;
using EditorConfig.Core;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ScintillaNet;

namespace EditorConfig
{
    public class PluginMain : IPlugin
    {
        EditorConfigParser parser;
        Project lastProject;
        readonly List<string> openingCache = new List<string>();

        bool originalTrimWhitespace;
        bool originalEnsureLastLine;
        bool originalUseTabs;
        int originalTabWidth;
        int originalIndentSize;
        ScintillaNet.Enums.EndOfLine originalEOLMode;
        

        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api => 1;

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public string Name { get; } = "EditorConfig";

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid { get; } = "0158C50F-6FC7-45F3-BACA-F3C0F1242CEF";

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author { get; } = "Christoph Otter";

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public string Description { get; } = "Reads and applies .editorconfig files";

        /// <summary>
        /// Web address for help
        /// </summary> 
        public string Help { get; } = "http://hexmachina.org/";

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        public object Settings => null;

        #endregion
        
        #region Required Methods
        
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            this.AddEventHandlers();

            BackupSettings();
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
        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
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
                case EventType.FileOpen:
                    TextEvent fileOpen = (TextEvent)e;
                    
                    if (PluginBase.CurrentProject == null) //not loaded yet
                    {
                        openingCache.Add(fileOpen.Value);
                    }
                    else
                    {
                        var document = DocumentManager.FindDocument(fileOpen.Value);
                        OnOpenFile(document);
                    }

                    break;
                case EventType.ApplySettings:
                    if (PluginBase.CurrentProject != null && !PluginBase.MainForm.ClosingEntirely)
                    {
                        //reset settings to editorconfig if the user changed them
                        foreach (var doc in PluginBase.MainForm.Documents)
                            OnOpenFile(doc);
                    }

                    break;
                case EventType.FileSwitch:
                    if (PluginBase.CurrentProject != null && !PluginBase.MainForm.ClosingEntirely) //not loaded yet / unloading again
                        OnOpenFile(PluginBase.MainForm.CurrentDocument);

                    break;
                case EventType.UIClosing:
                    RestoreSettings();

                    break;
            }
        }
        
        #endregion

        #region Custom Methods

        /// <summary>
        /// Backups the current user settings, so they can be restored later.
        /// </summary>
        void BackupSettings()
        {
            originalUseTabs = PluginBase.Settings.UseTabs;
            originalEnsureLastLine = PluginBase.Settings.EnsureLastLineEnd;
            originalTrimWhitespace = PluginBase.Settings.StripTrailingSpaces;
            originalEOLMode = PluginBase.Settings.EOLMode;
            originalTabWidth = PluginBase.Settings.TabWidth;
            originalIndentSize = PluginBase.Settings.IndentSize;
        }

        /// <summary>
        /// Restores the settings previously backuped by <see cref="BackupSettings"/>
        /// </summary>
        void RestoreSettings()
        {
            PluginBase.Settings.UseTabs = originalUseTabs;
            PluginBase.Settings.EnsureLastLineEnd = originalEnsureLastLine;
            PluginBase.Settings.StripTrailingSpaces = originalTrimWhitespace;
            PluginBase.Settings.EOLMode = originalEOLMode;
            PluginBase.Settings.TabWidth = originalTabWidth;
            PluginBase.Settings.IndentSize = originalIndentSize;
        }

        /// <summary>
        /// Processes the cached open files.
        /// In this case, cached means that these files were just opened, but OnOpenFile was not called yet.
        /// This is needed, because FileOpen event is fired before ProjectManager.Project event
        /// </summary>
        void ProcessCache()
        {
            if (parser == null) return;

            //process cache first
            foreach (var cachedFile in openingCache)
            {
                var doc = DocumentManager.FindDocument(cachedFile);
                if (doc != null)
                    OnOpenFile(doc);
            }
            openingCache.Clear();
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileOpen | EventType.FileEncode | EventType.Command | EventType.ApplySettings | EventType.FileSwitch | EventType.UIClosing);
        }

        /// <summary>
        /// Gets the .editorconfig properties that apply to the given file
        /// </summary>
        FileConfiguration GetConfig(string filename)
        {
            return parser.Parse(filename).First();
        }

        void OnOpenFile(ITabbedDocument document)
        {
            RestoreSettings();

            var sci1 = document.SplitSci1;
            var sci2 = document.SplitSci2;
            var config = GetConfig(document.FileName);

            ApplyEOL(sci1, config);
            ApplyEOL(sci2, config);

            ApplyMaxLineLength(sci1, config);
            ApplyMaxLineLength(sci2, config);

            ApplyCharset(sci1, config);
            ApplyCharset(sci2, config);

            ApplyIndentation(config);
            ApplyTrimWhitespace(config);
            ApplyFinalNewLine(config);

            document.RefreshTexts();

        }

        void ApplyCharset(ScintillaControl sci, FileConfiguration config)
        {
            if (sci == null || sci.IsReadOnly) return;

            var e = sci.Encoding;
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

        void ApplyIndentation(FileConfiguration config)
        {
            //indent style
            switch (config.IndentStyle)
            {
                case IndentStyle.Space:
                    PluginBase.Settings.UseTabs = false;
                    break;
                case IndentStyle.Tab:
                    PluginBase.Settings.UseTabs = true;
                    break;
                case null:
                    return;
            }

            //tab width
            PluginBase.Settings.TabWidth = config.TabWidth ?? PluginBase.Settings.TabWidth;

            //indent size
            if (config.IndentSize?.NumberOfColumns != null) PluginBase.Settings.IndentSize = (int)config.IndentSize.NumberOfColumns;
        }

        void ApplyMaxLineLength(ScintillaControl sci, FileConfiguration config)
        {
            if (sci == null || sci.IsReadOnly || config.MaxLineLength == null) return;

            //This might be problematic if there already is an edge defined. In that case, it is overridden
            sci.EdgeMode = (int)ScintillaNet.Enums.EdgeVisualStyle.Line;
            sci.EdgeColumn = (int)config.MaxLineLength;
        }

        void ApplyFinalNewLine(FileConfiguration config)
        {
            PluginBase.Settings.EnsureLastLineEnd = config.InsertFinalNewline ?? PluginBase.Settings.EnsureLastLineEnd;
        }

        void ApplyEOL(ScintillaControl sci, FileConfiguration config)
        {
            if (sci == null || sci.IsReadOnly) return;

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
                case null:
                    return;
            }
            PluginBase.Settings.EOLMode = sci.EndOfLineMode = mode;
            sci.ConvertEOLs(mode);
        }

        void ApplyTrimWhitespace(FileConfiguration config)
        {
            PluginBase.Settings.StripTrailingSpaces = config.TrimTrailingWhitespace ?? PluginBase.Settings.StripTrailingSpaces;
        }

        #endregion

    }
    
}
