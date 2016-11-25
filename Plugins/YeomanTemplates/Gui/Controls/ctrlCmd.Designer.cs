namespace YeomanTemplates.Gui.Controls
{
    partial class ctrlCmd
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
            this.lblRunning = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblRunning
            // 
            this.lblRunning.AutoSize = true;
            this.lblRunning.Location = new System.Drawing.Point(-3, 0);
            this.lblRunning.Name = "lblRunning";
            this.lblRunning.Size = new System.Drawing.Size(98, 13);
            this.lblRunning.TabIndex = 1;
            this.lblRunning.Text = "Running Yeoman...";
            // 
            // ctrlCmd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblRunning);
            this.Name = "ctrlCmd";
            this.Size = new System.Drawing.Size(677, 250);
            this.ResumeLayout(false);
            this.PerformLayout();
            this.Load += ctrlCmd_Load;
        }

        #endregion
        private System.Windows.Forms.Label lblRunning;
        private ConsoleControl.ConsoleControl cmdConsole;
    }
}
