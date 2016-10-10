namespace YeomanTemplates.gui.controls
{
    partial class ctrlChoose
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tvGenerator = new System.Windows.Forms.TreeView();
            this.lblChoose = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tvGenerator
            // 
            this.tvGenerator.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tvGenerator.Location = new System.Drawing.Point(0, 16);
            this.tvGenerator.Name = "tvGenerator";
            this.tvGenerator.ShowPlusMinus = false;
            this.tvGenerator.Size = new System.Drawing.Size(236, 112);
            this.tvGenerator.TabIndex = 0;
            this.tvGenerator.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvGenerator_AfterSelect);
            this.tvGenerator.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvGenerator_NodeMouseDoubleClick);
            // 
            // lblChoose
            // 
            this.lblChoose.AutoSize = true;
            this.lblChoose.Location = new System.Drawing.Point(-3, 0);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(185, 13);
            this.lblChoose.TabIndex = 2;
            this.lblChoose.Text = "Please choose your generator, below:";
            // 
            // ctrlChoose
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblChoose);
            this.Controls.Add(this.tvGenerator);
            this.Name = "ctrlChoose";
            this.Size = new System.Drawing.Size(236, 128);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView tvGenerator;
        private System.Windows.Forms.Label lblChoose;
    }
}
