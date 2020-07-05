namespace Toolbox.Winforms
{
    partial class MaterialEditor
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
            this.stTabControl1 = new STLibrary.Forms.STTabControl();
            this.SuspendLayout();
            // 
            // stTabControl1
            // 
            this.stTabControl1.CausesValidation = false;
            this.stTabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stTabControl1.Location = new System.Drawing.Point(0, 0);
            this.stTabControl1.Name = "stTabControl1";
            this.stTabControl1.SelectedIndex = 0;
            this.stTabControl1.Size = new System.Drawing.Size(506, 449);
            this.stTabControl1.TabIndex = 0;
            // 
            // MaterialEditor
            // 
            this.Controls.Add(this.stTabControl1);
            this.Name = "MaterialEditor";
            this.Size = new System.Drawing.Size(506, 449);
            this.ResumeLayout(false);

        }

        #endregion

        private STLibrary.Forms.STTabControl stTabControl1;
    }
}
