using System;
using System.Windows.Forms;
using System.Collections.Generic;
using FlashDebugger;
using ProjectManager;
using System.IO;
using PluginCore.Managers;

namespace ExpressionExplorer
{
    public class PluginUI : UserControl
    {
        private Parser parser = new Parser();

        private RichTextBox richTextBox;
        private TextBox addjson;
        private Button GenTree;
        private TreeView treeViewJson;
        private PluginMain pluginMain;

        public PluginUI(PluginMain pluginMain)
        {
            this.InitializeComponent();
            this.pluginMain = pluginMain;
        }

        /// <summary>
        /// Accessor to the RichTextBox
        /// </summary>
        public RichTextBox Output
        {
            get { return this.richTextBox; }
        }

        #region Windows Forms Designer Generated Code

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.addjson = new System.Windows.Forms.TextBox();
            this.GenTree = new System.Windows.Forms.Button();
            this.treeViewJson = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // richTextBox
            // 
            this.richTextBox.DetectUrls = false;
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(256, 352);
            this.richTextBox.TabIndex = 0;
            this.richTextBox.Text = "";
            // 
            // addjson
            // 
            this.addjson.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.addjson.Location = new System.Drawing.Point(13, 324);
            this.addjson.Name = "addjson";
            this.addjson.Size = new System.Drawing.Size(239, 20);
            this.addjson.TabIndex = 1;
            // 
            // GenTree
            // 
            this.GenTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.GenTree.Location = new System.Drawing.Point(258, 317);
            this.GenTree.Name = "GenTree";
            this.GenTree.Size = new System.Drawing.Size(118, 32);
            this.GenTree.TabIndex = 2;
            this.GenTree.Text = "Generate Tree";
            this.GenTree.UseVisualStyleBackColor = true;
            this.GenTree.Click += new System.EventHandler(this.GenTree_Click);
            // 
            // treeViewJson
            // 
            this.treeViewJson.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeViewJson.HotTracking = true;
            this.treeViewJson.Location = new System.Drawing.Point(3, 3);
            this.treeViewJson.Name = "treeViewJson";
            this.treeViewJson.ShowLines = false;
            this.treeViewJson.Size = new System.Drawing.Size(373, 308);
            this.treeViewJson.TabIndex = 3;
            this.treeViewJson.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewJson_AfterSelect);
            // 
            // PluginUI
            // 
            this.Controls.Add(this.treeViewJson);
            this.Controls.Add(this.GenTree);
            this.Controls.Add(this.addjson);
            this.Name = "PluginUI";
            this.Size = new System.Drawing.Size(379, 352);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void treeViewJson_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeViewHitTestInfo info = treeViewJson.HitTest(treeViewJson.PointToClient(Cursor.Position));
            if (info != null)
            {
                try
                {
                    MyTuple itm = parser.result.Find(x => x.expression == info.Node.Text);

                    if (itm != null)
                    {
                        string tmp;
                        int line;

                        if (itm.position == "")
                        {
                            MessageBox.Show("This expression has no position");
                            return;
                        }
                        string postmp = itm.position;
                        try
                        {
                            postmp = itm.position.Replace("(", "");
                            postmp = postmp.Replace(")", "");
                            postmp = postmp.Substring(0, postmp.IndexOf(": characters"));
                            line = Int32.Parse(postmp.Substring(postmp.LastIndexOf(":") + 1));
                            postmp = postmp.Substring(0, postmp.LastIndexOf(":"));

                            if (ProjectManager.Projects.ProjectPaths.GetAbsolutePath(Directory.GetCurrentDirectory().ToString(), postmp) != postmp)
                                tmp = ProjectManager.Projects.ProjectPaths.GetAbsolutePath(Directory.GetCurrentDirectory().ToString(), "../" + postmp);
                            else
                                tmp = postmp;
                            ScintillaHelper.ActivateDocument(tmp, line, true);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(itm.position);
                            MessageBox.Show("This expression has no position.");
                        }
                    }
                }
                catch(Exception fail)
                {
                    return;
                }

                
                  
            }
        }

        public void treeGeneration(string txt)
        {
            if (txt != "")
                if (txt.IndexOf("expr =") > -1 && txt.IndexOf("pos =") > -1)
                    parseExpressions(txt);
        }

        private void parseExpressions(string txt)
        {
            parser.parseExpressions(txt);
            if (parser.result.Count > 0)
            {
                treeViewJson.Nodes.Clear();
                TreeNode root = new TreeNode();
                root.Text = "Root";
                treeViewJson.Nodes.Add(root);
                FromTxtToTree(parser.result, parser.result[0].parentPosition);
            }
        }

        private void Recursive(TreeNode treeNode, MyTuple itm, int cpt)
        {
            if (treeNode.Nodes.Count == 0)
            {
                if (cpt == itm.parentPosition)
                {
                    treeNode.Nodes.Add(new TreeNode(itm.expression));
                    cpt++;
                }
            }
            else
            {
                foreach (TreeNode tn in treeNode.Nodes)
                {
                    cpt++;
                    if (itm.parentPosition == cpt)
                    {
                        tn.Nodes.Add(new TreeNode(itm.expression));
                        return;
                    }
                    Recursive(tn, itm, cpt);
                }
            }
        }

        private void FindParentNode(MyTuple itm, int Ppos)
        {
            int cpt = 0;
            TreeNodeCollection nodes = treeViewJson.Nodes;

            foreach (TreeNode n in nodes)
            {
                Recursive(n, itm, cpt);
                cpt++;
            }
        }

        private void FromTxtToTree(List<MyTuple> list, int pPos)
        {
            int i = 0;

            while (i < list.Count)
            { 
                FindParentNode(list[i], list[i].parentPosition);
                i++;
            }
        }

        private void GenTree_Click(object sender, EventArgs e)
        {
            parseExpressions(addjson.Text);
        }
   
    }
        #endregion
}
