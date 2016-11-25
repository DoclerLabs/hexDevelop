using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YeomanTemplates.Helper;

namespace YeomanTemplates.yeoman
{
    class GeneratorListParser
    {
        public readonly NestedStringList Result;
        private NestedStringList lastMainGen;

        public GeneratorListParser()
        {
            Result = new NestedStringList(null);
        }

        public void ParseInput(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                ParseLine(line);
            }
        }

        /// <summary>
        /// Parses the given line into a TreeNode and adds it to the given TreeView.
        /// The output has to be parsed from top to bottom
        /// </summary>
        /// <param name="line"></param>
        private void ParseLine(string line)
        {
            if (line.StartsWith("    ")) //sub-generator
            {
                if (lastMainGen == null)
                {
                    PluginCore.Managers.ErrorManager.ShowError(new Exception("Error reading yo output: " + line));
                    return;
                }

                var node = new NestedStringList(line.Substring(4));
                lastMainGen.Add(node);
            }
            else if (line.StartsWith("  ")) //generator
            {
                var node = new NestedStringList(line.Substring(2));
                lastMainGen = node;
                Result.Add(node);
            }
            //else ignore
        }
    }
}
