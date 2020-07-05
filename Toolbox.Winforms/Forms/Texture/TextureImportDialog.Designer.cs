namespace Toolbox.Winforms
{
    partial class TextureImportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureImportDialog));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.stPanel3 = new STLibrary.Forms.STPanel();
            this.listViewCustom1 = new STLibrary.Forms.ListViewCustom();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnOk = new STLibrary.Forms.STPanel();
            this.stPanel2 = new STLibrary.Forms.STPanel();
            this.platformCB = new STLibrary.Forms.STComboBox();
            this.stLabel1 = new STLibrary.Forms.STLabel();
            this.stPanel1 = new STLibrary.Forms.STPanel();
            this.stButton2 = new STLibrary.Forms.STButton();
            this.btnCancel = new STLibrary.Forms.STButton();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.pictureBoxCustom1 = new STLibrary.Forms.PictureBoxCustom();
            this.listViewCustom2 = new STLibrary.Forms.ListViewCustom();
            this.contentContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.stPanel3.SuspendLayout();
            this.btnOk.SuspendLayout();
            this.stPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom1)).BeginInit();
            this.SuspendLayout();
            // 
            // contentContainer
            // 
            this.contentContainer.Controls.Add(this.splitContainer1);
            this.contentContainer.Size = new System.Drawing.Size(1081, 479);
            this.contentContainer.Controls.SetChildIndex(this.splitContainer1, 0);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 27);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.stPanel3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnOk);
            this.splitContainer1.Size = new System.Drawing.Size(1081, 449);
            this.splitContainer1.SplitterDistance = 222;
            this.splitContainer1.TabIndex = 1;
            // 
            // stPanel3
            // 
            this.stPanel3.Controls.Add(this.listViewCustom1);
            this.stPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stPanel3.Location = new System.Drawing.Point(0, 0);
            this.stPanel3.Name = "stPanel3";
            this.stPanel3.Size = new System.Drawing.Size(222, 449);
            this.stPanel3.TabIndex = 0;
            // 
            // listViewCustom1
            // 
            this.listViewCustom1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewCustom1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewCustom1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCustom1.FullRowSelect = true;
            this.listViewCustom1.HideSelection = false;
            this.listViewCustom1.Location = new System.Drawing.Point(0, 0);
            this.listViewCustom1.Name = "listViewCustom1";
            this.listViewCustom1.OwnerDraw = true;
            this.listViewCustom1.Size = new System.Drawing.Size(222, 449);
            this.listViewCustom1.TabIndex = 0;
            this.listViewCustom1.UseCompatibleStateImageBehavior = false;
            this.listViewCustom1.View = System.Windows.Forms.View.Details;
            this.listViewCustom1.SelectedIndexChanged += new System.EventHandler(this.listViewCustom1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Name";
            this.columnHeader1.Width = 98;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Format";
            this.columnHeader2.Width = 124;
            // 
            // btnOk
            // 
            this.btnOk.Controls.Add(this.stPanel2);
            this.btnOk.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnOk.Location = new System.Drawing.Point(0, 0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(855, 449);
            this.btnOk.TabIndex = 0;
            // 
            // stPanel2
            // 
            this.stPanel2.Controls.Add(this.platformCB);
            this.stPanel2.Controls.Add(this.stLabel1);
            this.stPanel2.Controls.Add(this.stPanel1);
            this.stPanel2.Controls.Add(this.stButton2);
            this.stPanel2.Controls.Add(this.btnCancel);
            this.stPanel2.Controls.Add(this.splitContainer2);
            this.stPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stPanel2.Location = new System.Drawing.Point(0, 0);
            this.stPanel2.Name = "stPanel2";
            this.stPanel2.Size = new System.Drawing.Size(855, 449);
            this.stPanel2.TabIndex = 0;
            // 
            // platformCB
            // 
            this.platformCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.platformCB.BorderColor = System.Drawing.Color.Empty;
            this.platformCB.BorderStyle = System.Windows.Forms.ButtonBorderStyle.Solid;
            this.platformCB.ButtonColor = System.Drawing.Color.Empty;
            this.platformCB.FormattingEnabled = true;
            this.platformCB.IsReadOnly = false;
            this.platformCB.Location = new System.Drawing.Point(669, 1);
            this.platformCB.Name = "platformCB";
            this.platformCB.Size = new System.Drawing.Size(177, 21);
            this.platformCB.TabIndex = 7;
            this.platformCB.SelectedIndexChanged += new System.EventHandler(this.platformCB_SelectedIndexChanged);
            // 
            // stLabel1
            // 
            this.stLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.stLabel1.AutoSize = true;
            this.stLabel1.Location = new System.Drawing.Point(573, 8);
            this.stLabel1.Name = "stLabel1";
            this.stLabel1.Size = new System.Drawing.Size(48, 13);
            this.stLabel1.TabIndex = 6;
            this.stLabel1.Text = "Platform:";
            // 
            // stPanel1
            // 
            this.stPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stPanel1.Location = new System.Drawing.Point(563, 24);
            this.stPanel1.Name = "stPanel1";
            this.stPanel1.Size = new System.Drawing.Size(289, 383);
            this.stPanel1.TabIndex = 5;
            // 
            // stButton2
            // 
            this.stButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.stButton2.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.stButton2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.stButton2.Location = new System.Drawing.Point(682, 413);
            this.stButton2.Name = "stButton2";
            this.stButton2.Size = new System.Drawing.Size(79, 33);
            this.stButton2.TabIndex = 4;
            this.stButton2.Text = "Ok";
            this.stButton2.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Location = new System.Drawing.Point(767, 413);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(79, 33);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(3, -3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.pictureBoxCustom1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.listViewCustom2);
            this.splitContainer2.Size = new System.Drawing.Size(554, 455);
            this.splitContainer2.SplitterDistance = 355;
            this.splitContainer2.TabIndex = 2;
            // 
            // pictureBoxCustom1
            // 
            this.pictureBoxCustom1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxCustom1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBoxCustom1.BackgroundImage")));
            this.pictureBoxCustom1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxCustom1.Location = new System.Drawing.Point(0, 0);
            this.pictureBoxCustom1.Name = "pictureBoxCustom1";
            this.pictureBoxCustom1.Size = new System.Drawing.Size(554, 355);
            this.pictureBoxCustom1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxCustom1.TabIndex = 0;
            this.pictureBoxCustom1.TabStop = false;
            // 
            // listViewCustom2
            // 
            this.listViewCustom2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewCustom2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewCustom2.HideSelection = false;
            this.listViewCustom2.Location = new System.Drawing.Point(0, 0);
            this.listViewCustom2.Name = "listViewCustom2";
            this.listViewCustom2.OwnerDraw = true;
            this.listViewCustom2.Size = new System.Drawing.Size(554, 96);
            this.listViewCustom2.TabIndex = 0;
            this.listViewCustom2.UseCompatibleStateImageBehavior = false;
            // 
            // TextureImportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1087, 484);
            this.Name = "TextureImportDialog";
            this.Text = "Texture Import";
            this.contentContainer.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.stPanel3.ResumeLayout(false);
            this.btnOk.ResumeLayout(false);
            this.stPanel2.ResumeLayout(false);
            this.stPanel2.PerformLayout();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxCustom1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private STLibrary.Forms.ListViewCustom listViewCustom1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private STLibrary.Forms.STPanel btnOk;
        private STLibrary.Forms.STPanel stPanel2;
        private STLibrary.Forms.PictureBoxCustom pictureBoxCustom1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private STLibrary.Forms.ListViewCustom listViewCustom2;
        private STLibrary.Forms.STPanel stPanel3;
        private STLibrary.Forms.STButton stButton2;
        private STLibrary.Forms.STButton btnCancel;
        private STLibrary.Forms.STPanel stPanel1;
        private STLibrary.Forms.STComboBox platformCB;
        private STLibrary.Forms.STLabel stLabel1;
    }
}