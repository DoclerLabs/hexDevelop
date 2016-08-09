using System.Collections.Generic;
using System.IO;
using PluginCore;
using PluginCore.FRService;
using ScintillaNet;

namespace UnitTestSessionsPanel.Helpers
{
    internal static class ScintillaHelper
    {
        public static void SelectTextOnFileLine(string path, string text)
        {
            if (!File.Exists(path)) return;

            var doc = PluginBase.MainForm.OpenEditableDocument(path, false) as ITabbedDocument;
            if (doc == null || !doc.FileName.Equals(path, System.StringComparison.OrdinalIgnoreCase)) return; //TODO: Case sensitive filesystems
            ScintillaControl sci = doc.SciControl;
            sci.RemoveHighlights();
            List<SearchMatch> matches = GetResults(sci, text);
            if (matches.Count > 0)
            {
                sci.AddHighlights(matches, 0xff0000);
                sci.EnsureVisible(matches[0].Line);
                int pos = sci.MBSafePosition(matches[0].Index);
                sci.SetSel(pos, pos);
            }
        }

        private static List<SearchMatch> GetResults(ScintillaControl sci, string text)
        {
            FRSearch search = new FRSearch(text)
            {
                Filter = SearchFilter.OutsideCodeComments,
                NoCase = true,
                WholeWord = true
            };
            return search.Matches(sci.Text);
        }
    }
}
