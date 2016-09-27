using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace YeomanTemplates.yeoman
{
    class GeneratorListParser
    {
        private TreeView view;
        private TreeNode lastMainGen;

        public GeneratorListParser(TreeView view)
        {
            this.view = view;
        }

        public void ParseInput(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                ParseLine(line);
            }

            EndParse();
        }

        /// <summary>
        /// Parses the given line into a TreeNode and adds it to the given TreeView.
        /// The output has to be parsed from top to bottom
        /// </summary>
        /// <param name="line"></param>
        public void ParseLine(string line)
        {
            if (line.StartsWith("    ")) //sub-generator
            {
                if (lastMainGen == null)
                {
                    PluginCore.Managers.ErrorManager.ShowError(new Exception("Error reading yo output: " + line));
                    return;
                }

                var node = new TreeNode(line.Substring(4));
                lastMainGen.Nodes.Add(node);
            }
            else if (line.StartsWith("  ")) //generator
            {
                var node = new TreeNode(line.Substring(2));
                lastMainGen = node;
                view.Nodes.Add(node);
            }
            //else ignore
        }

        public void EndParse()
        {
            view.ExpandAll();
        }
    }
}
