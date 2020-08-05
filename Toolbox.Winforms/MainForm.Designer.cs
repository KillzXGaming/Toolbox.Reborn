namespace Toolbox.Winforms
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.stMenuStrip1 = new STLibrary.Forms.STMenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.recentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compressionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.customEditorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cascadeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maximizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.consoleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabForms = new STLibrary.Forms.STTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabControlContextMenuStrip = new STLibrary.Forms.STContextMenuStrip(this.components);
            this.BtnMdiMinimize = new System.Windows.Forms.PictureBox();
            this.BtnMdiMinMax = new System.Windows.Forms.PictureBox();
            this.BtnMdiClose = new System.Windows.Forms.PictureBox();
            this.stToolStrip1 = new STLibrary.Forms.STToolStrip();
            this.stPanel1 = new STLibrary.Forms.STPanel();
            this.stMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiMinimize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiMinMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiClose)).BeginInit();
            this.stPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // stMenuStrip1
            // 
            this.stMenuStrip1.AllowMerge = false;
            this.stMenuStrip1.HighlightSelectedTab = false;
            this.stMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.customEditorsToolStripMenuItem,
            this.windowsToolStripMenuItem,
            this.pluginsToolStripMenuItem,
            this.consoleToolStripMenuItem});
            this.stMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.stMenuStrip1.Name = "stMenuStrip1";
            this.stMenuStrip1.Size = new System.Drawing.Size(988, 24);
            this.stMenuStrip1.TabIndex = 0;
            this.stMenuStrip1.Text = "stMenuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.newFromFileToolStripMenuItem,
            this.openToolStripMenuItem,
            this.recentToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newToolStripMenuItem.Text = "New";
            // 
            // newFromFileToolStripMenuItem
            // 
            this.newFromFileToolStripMenuItem.Name = "newFromFileToolStripMenuItem";
            this.newFromFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.newFromFileToolStripMenuItem.Text = "New From File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // recentToolStripMenuItem
            // 
            this.recentToolStripMenuItem.Name = "recentToolStripMenuItem";
            this.recentToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.recentToolStripMenuItem.Text = "Recent";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Save";
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveAsToolStripMenuItem.Text = "Save As";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.compressionToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(46, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // compressionToolStripMenuItem
            // 
            this.compressionToolStripMenuItem.Name = "compressionToolStripMenuItem";
            this.compressionToolStripMenuItem.Size = new System.Drawing.Size(144, 22);
            this.compressionToolStripMenuItem.Text = "Compression";
            // 
            // customEditorsToolStripMenuItem
            // 
            this.customEditorsToolStripMenuItem.Name = "customEditorsToolStripMenuItem";
            this.customEditorsToolStripMenuItem.Size = new System.Drawing.Size(100, 20);
            this.customEditorsToolStripMenuItem.Text = "Custom Editors";
            // 
            // windowsToolStripMenuItem
            // 
            this.windowsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cascadeToolStripMenuItem,
            this.minimizeToolStripMenuItem,
            this.maximizeToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.closeAllToolStripMenuItem});
            this.windowsToolStripMenuItem.Name = "windowsToolStripMenuItem";
            this.windowsToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.windowsToolStripMenuItem.Text = "Windows";
            // 
            // cascadeToolStripMenuItem
            // 
            this.cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
            this.cascadeToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.cascadeToolStripMenuItem.Text = "Cascade";
            this.cascadeToolStripMenuItem.Click += new System.EventHandler(this.cascadeToolStripMenuItem_Click);
            // 
            // minimizeToolStripMenuItem
            // 
            this.minimizeToolStripMenuItem.Name = "minimizeToolStripMenuItem";
            this.minimizeToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.minimizeToolStripMenuItem.Text = "Minimize";
            this.minimizeToolStripMenuItem.Click += new System.EventHandler(this.minimizeToolStripMenuItem_Click);
            // 
            // maximizeToolStripMenuItem
            // 
            this.maximizeToolStripMenuItem.Name = "maximizeToolStripMenuItem";
            this.maximizeToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.maximizeToolStripMenuItem.Text = "Maximize";
            this.maximizeToolStripMenuItem.Click += new System.EventHandler(this.maximizeToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // closeAllToolStripMenuItem
            // 
            this.closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
            this.closeAllToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.closeAllToolStripMenuItem.Text = "Close All";
            this.closeAllToolStripMenuItem.Click += new System.EventHandler(this.closeAllToolStripMenuItem_Click);
            // 
            // pluginsToolStripMenuItem
            // 
            this.pluginsToolStripMenuItem.Name = "pluginsToolStripMenuItem";
            this.pluginsToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginsToolStripMenuItem.Text = "Plugins";
            this.pluginsToolStripMenuItem.Click += new System.EventHandler(this.pluginsToolStripMenuItem_Click);
            // 
            // consoleToolStripMenuItem
            // 
            this.consoleToolStripMenuItem.Name = "consoleToolStripMenuItem";
            this.consoleToolStripMenuItem.Size = new System.Drawing.Size(62, 20);
            this.consoleToolStripMenuItem.Text = "Console";
            this.consoleToolStripMenuItem.Click += new System.EventHandler(this.consoleToolStripMenuItem_Click);
            // 
            // tabForms
            // 
            this.tabForms.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabForms.Location = new System.Drawing.Point(0, 49);
            this.tabForms.Name = "tabForms";
            this.tabForms.SelectedIndex = 0;
            this.tabForms.Size = new System.Drawing.Size(988, 24);
            this.tabForms.TabIndex = 9;
            this.tabForms.Visible = false;
            this.tabForms.SelectedIndexChanged += new System.EventHandler(this.tabForms_SelectedIndexChanged);
            this.tabForms.MouseClick += new System.Windows.Forms.MouseEventHandler(this.tabForms_MouseClick);
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(573, 71);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(192, 71);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabControlContextMenuStrip
            // 
            this.tabControlContextMenuStrip.Name = "tabControlContextMenuStrip";
            this.tabControlContextMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // BtnMdiMinimize
            // 
            this.BtnMdiMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMdiMinimize.BackColor = System.Drawing.Color.Transparent;
            this.BtnMdiMinimize.Image = ((System.Drawing.Image)(resources.GetObject("BtnMdiMinimize.Image")));
            this.BtnMdiMinimize.Location = new System.Drawing.Point(1, 3);
            this.BtnMdiMinimize.Name = "BtnMdiMinimize";
            this.BtnMdiMinimize.Size = new System.Drawing.Size(38, 22);
            this.BtnMdiMinimize.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.BtnMdiMinimize.TabIndex = 14;
            this.BtnMdiMinimize.TabStop = false;
            this.BtnMdiMinimize.Click += new System.EventHandler(this.BtnMdiMinimize_Click);
            this.BtnMdiMinimize.MouseEnter += new System.EventHandler(this.BtnMdiMinimize_MouseEnter);
            this.BtnMdiMinimize.MouseLeave += new System.EventHandler(this.BtnMdiMinimize_MouseLeave);
            // 
            // BtnMdiMinMax
            // 
            this.BtnMdiMinMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMdiMinMax.BackColor = System.Drawing.Color.Transparent;
            this.BtnMdiMinMax.Image = ((System.Drawing.Image)(resources.GetObject("BtnMdiMinMax.Image")));
            this.BtnMdiMinMax.Location = new System.Drawing.Point(39, 3);
            this.BtnMdiMinMax.Name = "BtnMdiMinMax";
            this.BtnMdiMinMax.Size = new System.Drawing.Size(38, 22);
            this.BtnMdiMinMax.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.BtnMdiMinMax.TabIndex = 13;
            this.BtnMdiMinMax.TabStop = false;
            this.BtnMdiMinMax.Click += new System.EventHandler(this.BtnMdiMinMax_Click);
            this.BtnMdiMinMax.MouseEnter += new System.EventHandler(this.BtnMdiMinMax_MouseEnter);
            this.BtnMdiMinMax.MouseLeave += new System.EventHandler(this.BtnMdiMinMax_MouseLeave);
            // 
            // BtnMdiClose
            // 
            this.BtnMdiClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMdiClose.BackColor = System.Drawing.Color.Transparent;
            this.BtnMdiClose.Image = ((System.Drawing.Image)(resources.GetObject("BtnMdiClose.Image")));
            this.BtnMdiClose.Location = new System.Drawing.Point(77, 3);
            this.BtnMdiClose.Name = "BtnMdiClose";
            this.BtnMdiClose.Size = new System.Drawing.Size(38, 22);
            this.BtnMdiClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.BtnMdiClose.TabIndex = 12;
            this.BtnMdiClose.TabStop = false;
            this.BtnMdiClose.Click += new System.EventHandler(this.BtnMdiClose_Click);
            this.BtnMdiClose.MouseEnter += new System.EventHandler(this.BtnMdiClose_MouseEnter);
            this.BtnMdiClose.MouseLeave += new System.EventHandler(this.BtnMdiClose_MouseLeave);
            // 
            // stToolStrip1
            // 
            this.stToolStrip1.HighlightSelectedTab = false;
            this.stToolStrip1.Location = new System.Drawing.Point(0, 24);
            this.stToolStrip1.Name = "stToolStrip1";
            this.stToolStrip1.Size = new System.Drawing.Size(988, 25);
            this.stToolStrip1.TabIndex = 16;
            this.stToolStrip1.Text = "stToolStrip1";
            // 
            // stPanel1
            // 
            this.stPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stPanel1.Controls.Add(this.BtnMdiMinimize);
            this.stPanel1.Controls.Add(this.BtnMdiClose);
            this.stPanel1.Controls.Add(this.BtnMdiMinMax);
            this.stPanel1.Location = new System.Drawing.Point(873, 24);
            this.stPanel1.Name = "stPanel1";
            this.stPanel1.Size = new System.Drawing.Size(115, 25);
            this.stPanel1.TabIndex = 18;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(988, 621);
            this.Controls.Add(this.stPanel1);
            this.Controls.Add(this.tabForms);
            this.Controls.Add(this.stToolStrip1);
            this.Controls.Add(this.stMenuStrip1);
            this.IsMdiContainer = true;
            this.KeyPreview = true;
            this.MainMenuStrip = this.stMenuStrip1;
            this.Name = "MainForm";
            this.Text = "Toolbox Reborn";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MdiChildActivate += new System.EventHandler(this.MainForm_MdiChildActivate);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.stMenuStrip1.ResumeLayout(false);
            this.stMenuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiMinimize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiMinMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BtnMdiClose)).EndInit();
            this.stPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private STLibrary.Forms.STMenuStrip stMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem recentToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem customEditorsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pluginsToolStripMenuItem;
        private STLibrary.Forms.STTabControl tabForms;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private STLibrary.Forms.STContextMenuStrip tabControlContextMenuStrip;
        private System.Windows.Forms.PictureBox BtnMdiMinimize;
        private System.Windows.Forms.PictureBox BtnMdiMinMax;
        private System.Windows.Forms.PictureBox BtnMdiClose;
        private STLibrary.Forms.STToolStrip stToolStrip1;
        private STLibrary.Forms.STPanel stPanel1;
        private System.Windows.Forms.ToolStripMenuItem windowsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cascadeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem minimizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem maximizeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem consoleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem compressionToolStripMenuItem;
    }
}

