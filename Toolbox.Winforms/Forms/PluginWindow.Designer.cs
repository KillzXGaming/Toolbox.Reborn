namespace Toolbox.Winforms
{
    partial class PluginWindow
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
            this.stLabel2 = new STLibrary.Forms.STLabel();
            this.listViewCustom2 = new STLibrary.Forms.ListViewCustom();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.stLabel1 = new STLibrary.Forms.STLabel();
            this.listViewCustom1 = new STLibrary.Forms.ListViewCustom();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contentContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentContainer
            // 
            this.contentContainer.Controls.Add(this.stLabel2);
            this.contentContainer.Controls.Add(this.listViewCustom2);
            this.contentContainer.Controls.Add(this.stLabel1);
            this.contentContainer.Controls.Add(this.listViewCustom1);
            this.contentContainer.Controls.SetChildIndex(this.listViewCustom1, 0);
            this.contentContainer.Controls.SetChildIndex(this.stLabel1, 0);
            this.contentContainer.Controls.SetChildIndex(this.listViewCustom2, 0);
            this.contentContainer.Controls.SetChildIndex(this.stLabel2, 0);
            // 
            // stLabel2
            // 
            this.stLabel2.AutoSize = true;
            this.stLabel2.Location = new System.Drawing.Point(175, 42);
            this.stLabel2.Name = "stLabel2";
            this.stLabel2.Size = new System.Drawing.Size(66, 13);
            this.stLabel2.TabIndex = 3;
            this.stLabel2.Text = "File Formats:";
            // 
            // listViewCustom2
            // 
            this.listViewCustom2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewCustom2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewCustom2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listViewCustom2.FullRowSelect = true;
            this.listViewCustom2.HideSelection = false;
            this.listViewCustom2.Location = new System.Drawing.Point(178, 58);
            this.listViewCustom2.Name = "listViewCustom2";
            this.listViewCustom2.OwnerDraw = true;
            this.listViewCustom2.Size = new System.Drawing.Size(367, 332);
            this.listViewCustom2.TabIndex = 2;
            this.listViewCustom2.UseCompatibleStateImageBehavior = false;
            this.listViewCustom2.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Plugin";
            this.columnHeader5.Width = 111;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 162;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Can Save";
            this.columnHeader2.Width = 105;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Extensions";
            this.columnHeader3.Width = 63;
            // 
            // stLabel1
            // 
            this.stLabel1.AutoSize = true;
            this.stLabel1.Location = new System.Drawing.Point(13, 42);
            this.stLabel1.Name = "stLabel1";
            this.stLabel1.Size = new System.Drawing.Size(44, 13);
            this.stLabel1.TabIndex = 1;
            this.stLabel1.Text = "Plugins:";
            // 
            // listViewCustom1
            // 
            this.listViewCustom1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listViewCustom1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listViewCustom1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4});
            this.listViewCustom1.FullRowSelect = true;
            this.listViewCustom1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewCustom1.HideSelection = false;
            this.listViewCustom1.Location = new System.Drawing.Point(12, 58);
            this.listViewCustom1.Name = "listViewCustom1";
            this.listViewCustom1.OwnerDraw = true;
            this.listViewCustom1.Size = new System.Drawing.Size(160, 335);
            this.listViewCustom1.TabIndex = 0;
            this.listViewCustom1.UseCompatibleStateImageBehavior = false;
            this.listViewCustom1.View = System.Windows.Forms.View.Details;
            this.listViewCustom1.SelectedIndexChanged += new System.EventHandler(this.listViewCustom1_SelectedIndexChanged);
            this.listViewCustom1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.listViewCustom1_MouseDown);
            this.listViewCustom1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listViewCustom1_MouseUp);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 145;
            // 
            // PluginWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 398);
            this.Name = "PluginWindow";
            this.Text = "Plugin Window";
            this.Load += new System.EventHandler(this.PluginWindow_Load);
            this.contentContainer.ResumeLayout(false);
            this.contentContainer.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private STLibrary.Forms.ListViewCustom listViewCustom1;
        private STLibrary.Forms.STLabel stLabel1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private STLibrary.Forms.ListViewCustom listViewCustom2;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private STLibrary.Forms.STLabel stLabel2;
        private System.Windows.Forms.ColumnHeader columnHeader5;
    }
}