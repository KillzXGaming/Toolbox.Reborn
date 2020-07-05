namespace Toolbox.Winforms
{
    partial class ImageEditorForm
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
            this.imageEditorBase1 = new Toolbox.Winforms.ImageEditorBase();
            this.contentContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // contentContainer
            // 
            this.contentContainer.Controls.Add(this.imageEditorBase1);
            this.contentContainer.Controls.SetChildIndex(this.imageEditorBase1, 0);
            // 
            // imageEditorBase1
            // 
            this.imageEditorBase1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageEditorBase1.Location = new System.Drawing.Point(0, 25);
            this.imageEditorBase1.Name = "imageEditorBase1";
            this.imageEditorBase1.Size = new System.Drawing.Size(543, 368);
            this.imageEditorBase1.TabIndex = 11;
            // 
            // ImageEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 398);
            this.Name = "ImageEditorForm";
            this.Text = "ImageEditorForm";
            this.contentContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ImageEditorBase imageEditorBase1;
    }
}