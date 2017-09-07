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
        
        bool? originalTrimWhitespace;
        bool? originalEnsureLastLine;
        bool? originalUseTabs;
        int? originalTabWidth;
        int? originalIndentSize;
        ScintillaNet.Enums.EndOfLine? originalEOLMode;
        int? originalPrintMargin;
        

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
                        ApplyConfig(document);
                    }

                    break;
                case EventType.ApplySettings:
                    if (PluginBase.CurrentProject != null && !PluginBase.MainForm.ClosingEntirely)
                    {
                        //reset settings to editorconfig if the user changed them
                        ApplyConfig(PluginBase.MainForm.CurrentDocument);
                    }

                    break;
                case EventType.FileSwitch:
                    if (PluginBase.CurrentProject != null && !PluginBase.MainForm.ClosingEntirely) //not loaded yet / unloading again
                        ApplyConfig(PluginBase.MainForm.CurrentDocument);

                    break;
                case EventType.UIClosing:
                    RestoreSettings();

                    break;
            }
        }
        
        #endregion

        #region Custom Methods

        /// <summary>
        /// Restores the settings previously backuped.
        /// </summary>
        void RestoreSettings()
        {
            if (originalUseTabs != null) PluginBase.Settings.UseTabs = (bool) originalUseTabs;
            if (originalEnsureLastLine != null) PluginBase.Settings.EnsureLastLineEnd = (bool) originalEnsureLastLine;
            if (originalTrimWhitespace != null) PluginBase.Settings.StripTrailingSpaces = (bool) originalTrimWhitespace;
            if (originalEOLMode != null) PluginBase.Settings.EOLMode = (ScintillaNet.Enums.EndOfLine) originalEOLMode;
            if (originalTabWidth != null) PluginBase.Settings.TabWidth = (int) originalTabWidth;
            if (originalIndentSize != null) PluginBase.Settings.IndentSize = (int) originalIndentSize;
            if (originalPrintMargin != null) PluginBase.Settings.PrintMarginColumn = (int) originalPrintMargin;
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
                    ApplyConfig(doc);
            }
            openingCache.Clear();
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        public void AddEventHandlers()
        {
            EventManager.AddEventHandler(this, EventType.FileOpen | EventType.Command | EventType.ApplySettings | EventType.FileSwitch | EventType.UIClosing);
        }

        /// <summary>
        /// Gets the .editorconfig properties that apply to the given file
        /// </summary>
        FileConfiguration GetConfig(string filename)
        {
            return parser.Parse(filename).First();
        }

        void ApplyConfig(ITabbedDocument document)
        {
            var sci1 = document.SplitSci1;
            var sci2 = document.SplitSci2;
            var config = GetConfig(document.FileName);
            
            ApplyEOL(sci1, config);
            ApplyEOL(sci2, config);

            ApplyCharset(sci1, config);
            ApplyCharset(sci2, config);

            ApplyIndentation(sci1, config);
            ApplyIndentation(sci2, config);

            ApplyMaxLineLength(config);
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

        void ApplyIndentation(ScintillaControl sci, FileConfiguration config)
        {
            Apply(() => config.IndentStyle, ref originalUseTabs, () => PluginBase.Settings.UseTabs, s => PluginBase.Settings.UseTabs = s,
                () =>
                {
                    //indent style
                    switch (config.IndentStyle)
                    {
                        case IndentStyle.Space:
                            PluginBase.Settings.UseTabs = false;
                            sci.IsUseTabs = false;
                            break;
                        case IndentStyle.Tab:
                            PluginBase.Settings.UseTabs = true;
                            sci.IsUseTabs = true;
                            break;
                    }
                });

            Apply(() => config.TabWidth, ref originalTabWidth, () => PluginBase.Settings.TabWidth, s => PluginBase.Settings.TabWidth = s,
                () =>
                {
                    //tab width
                    PluginBase.Settings.TabWidth = (int) config.TabWidth;
                    sci.TabWidth = (int) config.TabWidth;
                });

            Apply(() => config.IndentSize?.NumberOfColumns, ref originalIndentSize, () => PluginBase.Settings.IndentSize, s => PluginBase.Settings.IndentSize = s,
                () =>
                {
                    //indent size
                    PluginBase.Settings.IndentSize = (int) config.IndentSize?.NumberOfColumns;
                    sci.Indent = (int) config.IndentSize?.NumberOfColumns;
                });
        }

        void ApplyMaxLineLength(FileConfiguration config)
        {
            Apply(() => config.MaxLineLength, ref originalPrintMargin, () => PluginBase.Settings.PrintMarginColumn, s => PluginBase.Settings.PrintMarginColumn = s,
                () => PluginBase.Settings.PrintMarginColumn = (int)config.MaxLineLength);
        }

        void ApplyFinalNewLine(FileConfiguration config)
        {
            Apply(() => config.InsertFinalNewline, ref originalEnsureLastLine, () => PluginBase.Settings.EnsureLastLineEnd, s => PluginBase.Settings.EnsureLastLineEnd = s,
                () => PluginBase.Settings.EnsureLastLineEnd = (bool)config.InsertFinalNewline);
        }

        void ApplyEOL(ScintillaControl sci, FileConfiguration config)
        {
            if (sci == null || sci.IsReadOnly) return;

            Apply(() => config.EndOfLine, ref originalEOLMode, () => PluginBase.Settings.EOLMode, s => PluginBase.Settings.EOLMode = s,
                () =>
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
                        case null:
                            return;
                    }

                    PluginBase.Settings.EOLMode = sci.EndOfLineMode = mode;
                    sci.ConvertEOLs(mode);
                });
        }

        void ApplyTrimWhitespace(FileConfiguration config)
        {
            Apply(() => config.TrimTrailingWhitespace, ref originalTrimWhitespace, () => PluginBase.Settings.StripTrailingSpaces,
                s => PluginBase.Settings.StripTrailingSpaces = s,
                () => PluginBase.Settings.StripTrailingSpaces = (bool) config.TrimTrailingWhitespace);

            PluginBase.Settings.StripTrailingSpaces = config.TrimTrailingWhitespace ?? PluginBase.Settings.StripTrailingSpaces;
        }

        /// <summary>
        /// Helper method to make sure original settings are handled correctly
        /// </summary>
        /// <param name="getConfigOption">Getter function for the config option that should be processed</param>
        /// <param name="original">The original value of the setting, should be null if no setting was backuped yet</param>
        /// <param name="getSetting">Getter function for PluginCore.Setting.YourSetting</param>
        /// <param name="applyOriginal">Applies the original settings if the document is not affected by .editorconfig</param>
        /// <param name="applierFunc">Applies the config option. Is only called if <code>getConfigOption() != null</code></param>
        void Apply<S, T>(Func<S> getConfigOption, ref T? original, Func<T> getSetting, Action<T> applyOriginal, Action applierFunc) where T : struct
        {
            if (getConfigOption() != null) //document affected by .editorconfig
            {
                if (original == null) //we have no backup yet
                    original = getSetting(); //backup

                applierFunc();
            }
            else if (original != null) //document not affected and we have saved settings from before
            {
                applyOriginal((T) original); //restore
            }
        }

        #endregion

    }
    
}
