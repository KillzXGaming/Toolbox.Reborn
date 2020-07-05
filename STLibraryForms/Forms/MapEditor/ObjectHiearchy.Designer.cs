namespace STLibrary.Forms.MapEditor
{
    partial class ObjectHiearchy
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
            this.components = new System.ComponentModel.Container();
            this.stListView1 = new BrightIdeasSoftware.TreeListView();
            this.Label = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.searchTB = new STLibrary.Forms.STTextBox();
            this.stContextMenuStrip1 = new STLibrary.Forms.STContextMenuStrip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.stListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // stListView1
            // 
            this.stListView1.AllColumns.Add(this.Label);
            this.stListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stListView1.CellEditUseWholeCell = false;
            this.stListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Label});
            this.stListView1.Cursor = System.Windows.Forms.Cursors.Default;
            this.stListView1.HideSelection = false;
            this.stListView1.Location = new System.Drawing.Point(3, 29);
            this.stListView1.Name = "stListView1";
            this.stListView1.ShowGroups = false;
            this.stListView1.Size = new System.Drawing.Size(386, 402);
            this.stListView1.TabIndex = 0;
            this.stListView1.UseCompatibleStateImageBehavior = false;
            this.stListView1.View = System.Windows.Forms.View.Details;
            this.stListView1.VirtualMode = true;
            this.stListView1.SelectionChanged += new System.EventHandler(this.stListView1_SelectionChanged);
            this.stListView1.SelectedIndexChanged += new System.EventHandler(this.stListView1_SelectedIndexChanged);
            this.stListView1.DoubleClick += new System.EventHandler(this.stListView1_DoubleClick);
            this.stListView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.stListView1_MouseClick);
            // 
            // Label
            // 
            this.Label.AspectName = "Label";
            this.Label.Text = "Label";
            this.Label.Width = 294;
            // 
            // searchTB
            // 
            this.searchTB.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTB.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.searchTB.Location = new System.Drawing.Point(3, 3);
            this.searchTB.Name = "searchTB";
            this.searchTB.Size = new System.Drawing.Size(386, 20);
            this.searchTB.TabIndex = 1;
            this.searchTB.TextChanged += new System.EventHandler(this.searchTB_TextChanged);
            // 
            // stContextMenuStrip1
            // 
            this.stContextMenuStrip1.Name = "stContextMenuStrip1";
            this.stContextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // ObjectHiearchy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.searchTB);
            this.Controls.Add(this.stListView1);
            this.Name = "ObjectHiearchy";
            this.Size = new System.Drawing.Size(392, 434);
            ((System.ComponentModel.ISupportInitialize)(this.stListView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BrightIdeasSoftware.TreeListView stListView1;
        private STLibrary.Forms.STTextBox searchTB;
        private BrightIdeasSoftware.OLVColumn Label;
        private STLibrary.Forms.STContextMenuStrip stContextMenuStrip1;
    }
}
