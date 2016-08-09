namespace UnitTestSessionsPanel {

    partial class TestsExplorerPanel {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing ) {
            if ( disposing && ( components != null ) ) {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestsExplorerPanel));
            this.optionsToolStrip = new PluginCore.Controls.ToolStripEx();
            this.refreshToolButton = new System.Windows.Forms.ToolStripButton();
            this.runToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.expandAllToolButton = new System.Windows.Forms.ToolStripButton();
            this.collapseAllToolButton = new System.Windows.Forms.ToolStripButton();
            this.groupByToolDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.noGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packageGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.categoryGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.categoryAndPackageGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suiteGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.packageAndSuiteGroupToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortToolButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.testsTreeView = new Aga.Controls.Tree.TreeViewAdv();
            this.testColumn = new Aga.Controls.Tree.TreeColumn();
            this.iconNodeField = new Aga.Controls.Tree.NodeControls.NodeIcon();
            this.nameNodeField = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.childCountNodeField = new Aga.Controls.Tree.NodeControls.NodeTextBox();
            this.optionsToolStrip.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // optionsToolStrip
            // 
            this.optionsToolStrip.CanOverflow = false;
            this.optionsToolStrip.ClickThrough = true;
            this.optionsToolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.optionsToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.optionsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.refreshToolButton,
            this.runToolButton,
            this.toolStripButton7,
            this.toolStripButton8,
            this.expandAllToolButton,
            this.collapseAllToolButton,
            this.groupByToolDropDownButton,
            this.sortToolButton});
            this.optionsToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.optionsToolStrip.Location = new System.Drawing.Point(0, 0);
            this.optionsToolStrip.Name = "optionsToolStrip";
            this.optionsToolStrip.Size = new System.Drawing.Size(400, 23);
            this.optionsToolStrip.Stretch = true;
            this.optionsToolStrip.TabIndex = 10;
            this.optionsToolStrip.Text = "toolStrip1";
            // 
            // refreshToolButton
            // 
            this.refreshToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.refreshToolButton.Image = ((System.Drawing.Image)(resources.GetObject("refreshToolButton.Image")));
            this.refreshToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.refreshToolButton.Name = "refreshToolButton";
            this.refreshToolButton.Size = new System.Drawing.Size(23, 20);
            this.refreshToolButton.Text = "toolStripButton5";
            this.refreshToolButton.Click += new System.EventHandler(this.RefreshToolButton_Click);
            // 
            // runToolButton
            // 
            this.runToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runToolButton.Image = ((System.Drawing.Image)(resources.GetObject("runToolButton.Image")));
            this.runToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runToolButton.Name = "runToolButton";
            this.runToolButton.Size = new System.Drawing.Size(23, 20);
            this.runToolButton.Text = "toolStripButton6";
            this.runToolButton.Click += new System.EventHandler(this.RunToolButton_Click);
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton7.Text = "toolStripButton7";
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton8.Image")));
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(23, 20);
            this.toolStripButton8.Text = "toolStripButton8";
            // 
            // expandAllToolButton
            // 
            this.expandAllToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.expandAllToolButton.Image = ((System.Drawing.Image)(resources.GetObject("expandAllToolButton.Image")));
            this.expandAllToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.expandAllToolButton.Name = "expandAllToolButton";
            this.expandAllToolButton.Size = new System.Drawing.Size(23, 20);
            this.expandAllToolButton.Text = "Expand All";
            this.expandAllToolButton.Click += new System.EventHandler(this.ExpandAllToolButton_Click);
            // 
            // collapseAllToolButton
            // 
            this.collapseAllToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.collapseAllToolButton.Image = ((System.Drawing.Image)(resources.GetObject("collapseAllToolButton.Image")));
            this.collapseAllToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.collapseAllToolButton.Name = "collapseAllToolButton";
            this.collapseAllToolButton.Size = new System.Drawing.Size(23, 20);
            this.collapseAllToolButton.Text = "Collapse All";
            this.collapseAllToolButton.Click += new System.EventHandler(this.CollapseAllToolButton_Click);
            // 
            // groupByToolDropDownButton
            // 
            this.groupByToolDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.groupByToolDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.noGroupToolMenuItem,
            this.packageGroupToolMenuItem,
            this.categoryGroupToolMenuItem,
            this.categoryAndPackageGroupToolMenuItem,
            this.suiteGroupToolMenuItem,
            this.packageAndSuiteGroupToolMenuItem});
            this.groupByToolDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("groupByToolDropDownButton.Image")));
            this.groupByToolDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.groupByToolDropDownButton.Name = "groupByToolDropDownButton";
            this.groupByToolDropDownButton.Size = new System.Drawing.Size(29, 20);
            this.groupByToolDropDownButton.Text = "Group By";
            // 
            // noGroupToolMenuItem
            // 
            this.noGroupToolMenuItem.Name = "noGroupToolMenuItem";
            this.noGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.noGroupToolMenuItem.Text = "None";
            this.noGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // packageGroupToolMenuItem
            // 
            this.packageGroupToolMenuItem.Checked = true;
            this.packageGroupToolMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.packageGroupToolMenuItem.Name = "packageGroupToolMenuItem";
            this.packageGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.packageGroupToolMenuItem.Text = "Package";
            this.packageGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // categoryGroupToolMenuItem
            // 
            this.categoryGroupToolMenuItem.Name = "categoryGroupToolMenuItem";
            this.categoryGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.categoryGroupToolMenuItem.Text = "Category";
            this.categoryGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // categoryAndPackageGroupToolMenuItem
            // 
            this.categoryAndPackageGroupToolMenuItem.Name = "categoryAndPackageGroupToolMenuItem";
            this.categoryAndPackageGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.categoryAndPackageGroupToolMenuItem.Text = "Category and Package";
            this.categoryAndPackageGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // suiteGroupToolMenuItem
            // 
            this.suiteGroupToolMenuItem.Name = "suiteGroupToolMenuItem";
            this.suiteGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.suiteGroupToolMenuItem.Text = "Suite";
            this.suiteGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // packageAndSuiteGroupToolMenuItem
            // 
            this.packageAndSuiteGroupToolMenuItem.Name = "packageAndSuiteGroupToolMenuItem";
            this.packageAndSuiteGroupToolMenuItem.Size = new System.Drawing.Size(192, 22);
            this.packageAndSuiteGroupToolMenuItem.Text = "Package and Suite";
            this.packageAndSuiteGroupToolMenuItem.Click += new System.EventHandler(this.GroupByToolMenuItem_Click);
            // 
            // sortToolButton
            // 
            this.sortToolButton.CheckOnClick = true;
            this.sortToolButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.sortToolButton.Image = ((System.Drawing.Image)(resources.GetObject("sortToolButton.Image")));
            this.sortToolButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.sortToolButton.Name = "sortToolButton";
            this.sortToolButton.Size = new System.Drawing.Size(23, 20);
            this.sortToolButton.Text = "Sort";
            this.sortToolButton.CheckedChanged += new System.EventHandler(this.SortToolButton_CheckedChanged);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.testsTreeView);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(400, 248);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(10, 10);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(400, 271);
            this.toolStripContainer1.TabIndex = 12;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.optionsToolStrip);
            // 
            // testsTreeView
            // 
            this.testsTreeView.BackColor = System.Drawing.SystemColors.Window;
            this.testsTreeView.BackColor2 = System.Drawing.SystemColors.Window;
            this.testsTreeView.BackgroundPaintMode = Aga.Controls.Tree.BackgroundPaintMode.Default;
            this.testsTreeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.testsTreeView.ColumnHeaderBackColor = System.Drawing.SystemColors.Control;
            this.testsTreeView.ColumnHeaderBorderColor = System.Drawing.SystemColors.ActiveBorder;
            this.testsTreeView.ColumnHeaderTextColor = System.Drawing.SystemColors.ControlText;
            this.testsTreeView.Columns.Add(this.testColumn);
            this.testsTreeView.DefaultToolTipProvider = null;
            this.testsTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testsTreeView.DragDropMarkColor = System.Drawing.Color.Black;
            this.testsTreeView.FullRowSelect = true;
            this.testsTreeView.HighlightColorActive = System.Drawing.SystemColors.Highlight;
            this.testsTreeView.HighlightColorInactive = System.Drawing.SystemColors.InactiveBorder;
            this.testsTreeView.LineColor = System.Drawing.SystemColors.ControlDark;
            this.testsTreeView.LineColor2 = System.Drawing.SystemColors.ControlDark;
            this.testsTreeView.Location = new System.Drawing.Point(0, 0);
            this.testsTreeView.Model = null;
            this.testsTreeView.Name = "testsTreeView";
            this.testsTreeView.NodeControls.Add(this.iconNodeField);
            this.testsTreeView.NodeControls.Add(this.nameNodeField);
            this.testsTreeView.NodeControls.Add(this.childCountNodeField);
            this.testsTreeView.OnVisibleOverride = null;
            this.testsTreeView.SelectedNode = null;
            this.testsTreeView.ShowNodeToolTips = true;
            this.testsTreeView.Size = new System.Drawing.Size(400, 248);
            this.testsTreeView.TabIndex = 10;
            this.testsTreeView.NodeMouseDoubleClick += new System.EventHandler<Aga.Controls.Tree.TreeNodeAdvMouseEventArgs>(this.TestsTreeView_NodeMouseDoubleClick);
            // 
            // testColumn
            // 
            this.testColumn.Header = "";
            this.testColumn.SortOrder = System.Windows.Forms.SortOrder.None;
            this.testColumn.TooltipText = null;
            this.testColumn.Width = 250;
            // 
            // iconNodeField
            // 
            this.iconNodeField.DataPropertyName = "Image";
            this.iconNodeField.LeftMargin = 1;
            this.iconNodeField.ParentColumn = this.testColumn;
            this.iconNodeField.ScaleMode = Aga.Controls.Tree.ImageScaleMode.Clip;
            this.iconNodeField.VirtualMode = true;
            // 
            // nameNodeField
            // 
            this.nameNodeField.DataPropertyName = "Text";
            this.nameNodeField.IncrementalSearchEnabled = true;
            this.nameNodeField.LeftMargin = 3;
            this.nameNodeField.ParentColumn = this.testColumn;
            this.nameNodeField.VirtualMode = true;
            // 
            // childCountNodeField
            // 
            this.childCountNodeField.IncrementalSearchEnabled = true;
            this.childCountNodeField.LeftMargin = 3;
            this.childCountNodeField.ParentColumn = this.testColumn;
            this.childCountNodeField.VirtualMode = true;
            // 
            // TestsExplorerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.toolStripContainer1);
            this.Margin = new System.Windows.Forms.Padding(5);
            this.Name = "TestsExplorerPanel";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Size = new System.Drawing.Size(420, 291);
            this.optionsToolStrip.ResumeLayout(false);
            this.optionsToolStrip.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private PluginCore.Controls.ToolStripEx optionsToolStrip;
        private System.Windows.Forms.ToolStripButton refreshToolButton;
        private System.Windows.Forms.ToolStripButton runToolButton;
        private System.Windows.Forms.ToolStripButton toolStripButton7;
        private System.Windows.Forms.ToolStripButton toolStripButton8;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private Aga.Controls.Tree.NodeControls.NodeIcon iconNodeField;
        private Aga.Controls.Tree.NodeControls.NodeTextBox nameNodeField;
        private Aga.Controls.Tree.NodeControls.NodeTextBox childCountNodeField;
        private Aga.Controls.Tree.TreeColumn testColumn;
        private System.Windows.Forms.ToolStripButton expandAllToolButton;
        private System.Windows.Forms.ToolStripButton collapseAllToolButton;
        private System.Windows.Forms.ToolStripDropDownButton groupByToolDropDownButton;
        private System.Windows.Forms.ToolStripMenuItem noGroupToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem packageGroupToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem categoryGroupToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suiteGroupToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem categoryAndPackageGroupToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem packageAndSuiteGroupToolMenuItem;
        private System.Windows.Forms.ToolStripButton sortToolButton;
        private Aga.Controls.Tree.TreeViewAdv testsTreeView;
    }
}