namespace Toolbox.Winforms
{
    partial class ProgressWindow
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.progressLabel = new STLibrary.Forms.STLabel();
            this.stPanel1 = new STLibrary.Forms.STPanel();
            this.contentContainer.SuspendLayout();
            this.stPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentContainer
            // 
            this.contentContainer.Controls.Add(this.stPanel1);
            this.contentContainer.Controls.Add(this.progressBar1);
            this.contentContainer.Size = new System.Drawing.Size(275, 97);
            this.contentContainer.Controls.SetChildIndex(this.progressBar1, 0);
            this.contentContainer.Controls.SetChildIndex(this.stPanel1, 0);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar1.Location = new System.Drawing.Point(4, 27);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(271, 23);
            this.progressBar1.TabIndex = 11;
            // 
            // progressLabel
            // 
            this.progressLabel.Location = new System.Drawing.Point(1, 5);
            this.progressLabel.Name = "progressLabel";
            this.progressLabel.Size = new System.Drawing.Size(261, 32);
            this.progressLabel.TabIndex = 12;
            this.progressLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // stPanel1
            // 
            this.stPanel1.Controls.Add(this.progressLabel);
            this.stPanel1.Location = new System.Drawing.Point(4, 54);
            this.stPanel1.Name = "stPanel1";
            this.stPanel1.Size = new System.Drawing.Size(268, 40);
            this.stPanel1.TabIndex = 13;
            this.stPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.stPanel1_Paint);
            // 
            // ProgressWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(281, 102);
            this.Name = "ProgressWindow";
            this.Text = "ProgressWindow";
            this.contentContainer.ResumeLayout(false);
            this.stPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private STLibrary.Forms.STLabel progressLabel;
        private System.Windows.Forms.ProgressBar progressBar1;
        private STLibrary.Forms.STPanel stPanel1;
    }
}