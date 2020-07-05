namespace Toolbox.Winforms
{
    partial class Viewport
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
            this.stPanel1 = new STLibrary.Forms.STPanel();
            this.stPanel2 = new STLibrary.Forms.STPanel();
            this.stPanel4 = new STLibrary.Forms.STPanel();
            this.stLabel2 = new STLibrary.Forms.STLabel();
            this.pickingModeCB = new STLibrary.Forms.STComboBox();
            this.stCheckBox1 = new STLibrary.Forms.STCheckBox();
            this.activeModelCB = new STLibrary.Forms.STComboBox();
            this.stLabel1 = new STLibrary.Forms.STLabel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.stPanel3 = new STLibrary.Forms.STPanel();
            this.stMenuStrip1 = new STLibrary.Forms.STMenuStrip();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.shadingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPoseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toOriginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stPanel2.SuspendLayout();
            this.stPanel4.SuspendLayout();
            this.stMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // stPanel1
            // 
            this.stPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stPanel1.Location = new System.Drawing.Point(3, 47);
            this.stPanel1.Name = "stPanel1";
            this.stPanel1.Size = new System.Drawing.Size(555, 203);
            this.stPanel1.TabIndex = 0;
            // 
            // stPanel2
            // 
            this.stPanel2.Controls.Add(this.stPanel4);
            this.stPanel2.Controls.Add(this.splitter1);
            this.stPanel2.Controls.Add(this.stPanel3);
            this.stPanel2.Controls.Add(this.stPanel1);
            this.stPanel2.Controls.Add(this.stMenuStrip1);
            this.stPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stPanel2.Location = new System.Drawing.Point(0, 0);
            this.stPanel2.Name = "stPanel2";
            this.stPanel2.Size = new System.Drawing.Size(561, 354);
            this.stPanel2.TabIndex = 1;
            // 
            // stPanel4
            // 
            this.stPanel4.Controls.Add(this.stLabel2);
            this.stPanel4.Controls.Add(this.pickingModeCB);
            this.stPanel4.Controls.Add(this.stCheckBox1);
            this.stPanel4.Controls.Add(this.activeModelCB);
            this.stPanel4.Controls.Add(this.stLabel1);
            this.stPanel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.stPanel4.Location = new System.Drawing.Point(0, 24);
            this.stPanel4.Name = "stPanel4";
            this.stPanel4.Size = new System.Drawing.Size(561, 24);
            this.stPanel4.TabIndex = 0;
            // 
            // stLabel2
            // 
            this.stLabel2.AutoSize = true;
            this.stLabel2.Location = new System.Drawing.Point(372, 3);
            this.stLabel2.Name = "stLabel2";
            this.stLabel2.Size = new System.Drawing.Size(37, 13);
            this.stLabel2.TabIndex = 6;
            this.stLabel2.Text = "Mode:";
            // 
            // pickingModeCB
            // 
            this.pickingModeCB.BorderColor = System.Drawing.Color.Empty;
            this.pickingModeCB.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.pickingModeCB.ButtonColor = System.Drawing.Color.Empty;
            this.pickingModeCB.FormattingEnabled = true;
            this.pickingModeCB.IsReadOnly = false;
            this.pickingModeCB.Location = new System.Drawing.Point(415, 0);
            this.pickingModeCB.Name = "pickingModeCB";
            this.pickingModeCB.Size = new System.Drawing.Size(146, 21);
            this.pickingModeCB.TabIndex = 5;
            this.pickingModeCB.SelectedIndexChanged += new System.EventHandler(this.pickingModeCB_SelectedIndexChanged);
            // 
            // stCheckBox1
            // 
            this.stCheckBox1.AutoSize = true;
            this.stCheckBox1.Location = new System.Drawing.Point(282, 2);
            this.stCheckBox1.Name = "stCheckBox1";
            this.stCheckBox1.Size = new System.Drawing.Size(74, 17);
            this.stCheckBox1.TabIndex = 4;
            this.stCheckBox1.Text = "Display All";
            this.stCheckBox1.UseVisualStyleBackColor = true;
            // 
            // activeModelCB
            // 
            this.activeModelCB.BorderColor = System.Drawing.Color.Empty;
            this.activeModelCB.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.activeModelCB.ButtonColor = System.Drawing.Color.Empty;
            this.activeModelCB.FormattingEnabled = true;
            this.activeModelCB.IsReadOnly = false;
            this.activeModelCB.Location = new System.Drawing.Point(93, 0);
            this.activeModelCB.Name = "activeModelCB";
            this.activeModelCB.Size = new System.Drawing.Size(183, 21);
            this.activeModelCB.TabIndex = 2;
            this.activeModelCB.SelectedIndexChanged += new System.EventHandler(this.activeModelCB_SelectedIndexChanged);
            // 
            // stLabel1
            // 
            this.stLabel1.AutoSize = true;
            this.stLabel1.Location = new System.Drawing.Point(3, 3);
            this.stLabel1.Name = "stLabel1";
            this.stLabel1.Size = new System.Drawing.Size(69, 13);
            this.stLabel1.TabIndex = 3;
            this.stLabel1.Text = "Active Model";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Location = new System.Drawing.Point(0, 247);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(561, 3);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // stPanel3
            // 
            this.stPanel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.stPanel3.Location = new System.Drawing.Point(0, 250);
            this.stPanel3.Name = "stPanel3";
            this.stPanel3.Size = new System.Drawing.Size(561, 104);
            this.stPanel3.TabIndex = 4;
            // 
            // stMenuStrip1
            // 
            this.stMenuStrip1.HighlightSelectedTab = false;
            this.stMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewToolStripMenuItem,
            this.shadingToolStripMenuItem,
            this.cameraToolStripMenuItem,
            this.resetPoseToolStripMenuItem});
            this.stMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.stMenuStrip1.Name = "stMenuStrip1";
            this.stMenuStrip1.Size = new System.Drawing.Size(561, 24);
            this.stMenuStrip1.TabIndex = 1;
            this.stMenuStrip1.Text = "stMenuStrip1";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // shadingToolStripMenuItem
            // 
            this.shadingToolStripMenuItem.Name = "shadingToolStripMenuItem";
            this.shadingToolStripMenuItem.Size = new System.Drawing.Size(111, 20);
            this.shadingToolStripMenuItem.Text = "Shading [Default]";
            // 
            // cameraToolStripMenuItem
            // 
            this.cameraToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToolStripMenuItem});
            this.cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
            this.cameraToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.cameraToolStripMenuItem.Text = "Camera";
            // 
            // resetPoseToolStripMenuItem
            // 
            this.resetPoseToolStripMenuItem.Name = "resetPoseToolStripMenuItem";
            this.resetPoseToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.resetPoseToolStripMenuItem.Text = "Reset Pose";
            this.resetPoseToolStripMenuItem.Click += new System.EventHandler(this.resetPoseToolStripMenuItem_Click);
            // 
            // resetToolStripMenuItem
            // 
            this.resetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toOriginToolStripMenuItem,
            this.toModelToolStripMenuItem});
            this.resetToolStripMenuItem.Name = "resetToolStripMenuItem";
            this.resetToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resetToolStripMenuItem.Text = "Reset";
            // 
            // toOriginToolStripMenuItem
            // 
            this.toOriginToolStripMenuItem.Name = "toOriginToolStripMenuItem";
            this.toOriginToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.toOriginToolStripMenuItem.Text = "To Origin";
            this.toOriginToolStripMenuItem.Click += new System.EventHandler(this.toOriginToolStripMenuItem_Click);
            // 
            // toModelToolStripMenuItem
            // 
            this.toModelToolStripMenuItem.Name = "toModelToolStripMenuItem";
            this.toModelToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.toModelToolStripMenuItem.Text = "To Model";
            // 
            // Viewport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stPanel2);
            this.Name = "Viewport";
            this.Size = new System.Drawing.Size(561, 354);
            this.Load += new System.EventHandler(this.Viewport_Load);
            this.stPanel2.ResumeLayout(false);
            this.stPanel2.PerformLayout();
            this.stPanel4.ResumeLayout(false);
            this.stPanel4.PerformLayout();
            this.stMenuStrip1.ResumeLayout(false);
            this.stMenuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private STLibrary.Forms.STPanel stPanel1;
        private STLibrary.Forms.STPanel stPanel2;
        private STLibrary.Forms.STLabel stLabel1;
        private STLibrary.Forms.STComboBox activeModelCB;
        private STLibrary.Forms.STMenuStrip stMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem shadingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetPoseToolStripMenuItem;
        private System.Windows.Forms.Splitter splitter1;
        private STLibrary.Forms.STPanel stPanel3;
        private STLibrary.Forms.STPanel stPanel4;
        private STLibrary.Forms.STCheckBox stCheckBox1;
        private STLibrary.Forms.STLabel stLabel2;
        private STLibrary.Forms.STComboBox pickingModeCB;
        private System.Windows.Forms.ToolStripMenuItem resetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toOriginToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toModelToolStripMenuItem;
    }
}
