namespace Toolbox.Winforms
{
    partial class VideoPlayer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VideoPlayer));
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.stPanel1 = new STLibrary.Forms.STPanel();
            this.stPanel2 = new STLibrary.Forms.STPanel();
            this.stPanel3 = new STLibrary.Forms.STPanel();
            this.btnStop = new STLibrary.Forms.STButton();
            this.btnBackward = new STLibrary.Forms.STButton();
            this.btnForward = new STLibrary.Forms.STButton();
            this.btnPlay = new STLibrary.Forms.STButton();
            this.maxFrameCounterUD = new STLibrary.Forms.STNumbericUpDown();
            this.currentFrameCounterUD = new STLibrary.Forms.STNumbericUpDown();
            this.animationTrackBar = new ColorSlider.ColorSlider();
            this.currentTimeLabel = new STLibrary.Forms.STLabel();
            this.maxTimeLabel = new STLibrary.Forms.STLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.stPanel1.SuspendLayout();
            this.stPanel2.SuspendLayout();
            this.stPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxFrameCounterUD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentFrameCounterUD)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(778, 421);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // stPanel1
            // 
            this.stPanel1.Controls.Add(this.maxTimeLabel);
            this.stPanel1.Controls.Add(this.currentTimeLabel);
            this.stPanel1.Controls.Add(this.stPanel2);
            this.stPanel1.Controls.Add(this.animationTrackBar);
            this.stPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.stPanel1.Location = new System.Drawing.Point(0, 415);
            this.stPanel1.Name = "stPanel1";
            this.stPanel1.Size = new System.Drawing.Size(778, 63);
            this.stPanel1.TabIndex = 0;
            // 
            // stPanel2
            // 
            this.stPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stPanel2.Controls.Add(this.stPanel3);
            this.stPanel2.Controls.Add(this.maxFrameCounterUD);
            this.stPanel2.Controls.Add(this.currentFrameCounterUD);
            this.stPanel2.Location = new System.Drawing.Point(10, 6);
            this.stPanel2.Name = "stPanel2";
            this.stPanel2.Size = new System.Drawing.Size(765, 38);
            this.stPanel2.TabIndex = 25;
            // 
            // stPanel3
            // 
            this.stPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.stPanel3.Controls.Add(this.btnStop);
            this.stPanel3.Controls.Add(this.btnBackward);
            this.stPanel3.Controls.Add(this.btnForward);
            this.stPanel3.Controls.Add(this.btnPlay);
            this.stPanel3.Location = new System.Drawing.Point(102, 3);
            this.stPanel3.Name = "stPanel3";
            this.stPanel3.Size = new System.Drawing.Size(559, 32);
            this.stPanel3.TabIndex = 25;
            // 
            // btnStop
            // 
            this.btnStop.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnStop.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnStop.BackgroundImage")));
            this.btnStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStop.FlatAppearance.BorderSize = 0;
            this.btnStop.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStop.Location = new System.Drawing.Point(324, 2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(35, 28);
            this.btnStop.TabIndex = 21;
            this.btnStop.UseVisualStyleBackColor = false;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnBackward
            // 
            this.btnBackward.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnBackward.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnBackward.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnBackward.BackgroundImage")));
            this.btnBackward.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnBackward.FlatAppearance.BorderSize = 0;
            this.btnBackward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBackward.Location = new System.Drawing.Point(201, 2);
            this.btnBackward.Name = "btnBackward";
            this.btnBackward.Size = new System.Drawing.Size(35, 28);
            this.btnBackward.TabIndex = 19;
            this.btnBackward.UseVisualStyleBackColor = false;
            // 
            // btnForward
            // 
            this.btnForward.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnForward.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnForward.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnForward.BackgroundImage")));
            this.btnForward.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnForward.FlatAppearance.BorderSize = 0;
            this.btnForward.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForward.Location = new System.Drawing.Point(283, 2);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(35, 28);
            this.btnForward.TabIndex = 20;
            this.btnForward.UseVisualStyleBackColor = false;
            // 
            // btnPlay
            // 
            this.btnPlay.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnPlay.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.btnPlay.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPlay.BackgroundImage")));
            this.btnPlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPlay.FlatAppearance.BorderSize = 0;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Location = new System.Drawing.Point(242, 2);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(35, 28);
            this.btnPlay.TabIndex = 18;
            this.btnPlay.UseVisualStyleBackColor = false;
            this.btnPlay.Click += new System.EventHandler(this.btnPlay_Click);
            // 
            // maxFrameCounterUD
            // 
            this.maxFrameCounterUD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.maxFrameCounterUD.Location = new System.Drawing.Point(666, 11);
            this.maxFrameCounterUD.Name = "maxFrameCounterUD";
            this.maxFrameCounterUD.Size = new System.Drawing.Size(83, 20);
            this.maxFrameCounterUD.TabIndex = 24;
            // 
            // currentFrameCounterUD
            // 
            this.currentFrameCounterUD.Location = new System.Drawing.Point(0, 11);
            this.currentFrameCounterUD.Name = "currentFrameCounterUD";
            this.currentFrameCounterUD.Size = new System.Drawing.Size(83, 20);
            this.currentFrameCounterUD.TabIndex = 23;
            // 
            // animationTrackBar
            // 
            this.animationTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.animationTrackBar.BarInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.animationTrackBar.BarPenColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.animationTrackBar.BarPenColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.animationTrackBar.BorderRoundRectSize = new System.Drawing.Size(8, 8);
            this.animationTrackBar.ElapsedInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.animationTrackBar.ElapsedPenColorBottom = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.animationTrackBar.ElapsedPenColorTop = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.animationTrackBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F);
            this.animationTrackBar.ForeColor = System.Drawing.Color.White;
            this.animationTrackBar.LargeChange = ((uint)(5u));
            this.animationTrackBar.Location = new System.Drawing.Point(51, 41);
            this.animationTrackBar.Maximum = 1000;
            this.animationTrackBar.MouseEffects = false;
            this.animationTrackBar.Name = "animationTrackBar";
            this.animationTrackBar.ScaleDivisions = 10;
            this.animationTrackBar.ScaleSubDivisions = 5;
            this.animationTrackBar.ShowDivisionsText = true;
            this.animationTrackBar.ShowSmallScale = false;
            this.animationTrackBar.Size = new System.Drawing.Size(664, 19);
            this.animationTrackBar.SmallChange = ((uint)(1u));
            this.animationTrackBar.TabIndex = 17;
            this.animationTrackBar.Text = "colorSlider1";
            this.animationTrackBar.ThumbInnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.animationTrackBar.ThumbPenColor = System.Drawing.Color.Silver;
            this.animationTrackBar.ThumbRoundRectSize = new System.Drawing.Size(8, 8);
            this.animationTrackBar.ThumbSize = new System.Drawing.Size(8, 8);
            this.animationTrackBar.TickAdd = 0F;
            this.animationTrackBar.TickColor = System.Drawing.Color.White;
            this.animationTrackBar.TickDivide = 0F;
            this.animationTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.animationTrackBar.ValueChanged += new System.EventHandler(this.animationTrackBar_ValueChanged);
            // 
            // currentTimeLabel
            // 
            this.currentTimeLabel.AutoSize = true;
            this.currentTimeLabel.Location = new System.Drawing.Point(13, 42);
            this.currentTimeLabel.Name = "currentTimeLabel";
            this.currentTimeLabel.Size = new System.Drawing.Size(47, 13);
            this.currentTimeLabel.TabIndex = 26;
            this.currentTimeLabel.Text = "stLabel1";
            // 
            // maxTimeLabel
            // 
            this.maxTimeLabel.AutoSize = true;
            this.maxTimeLabel.Location = new System.Drawing.Point(718, 42);
            this.maxTimeLabel.Name = "maxTimeLabel";
            this.maxTimeLabel.Size = new System.Drawing.Size(47, 13);
            this.maxTimeLabel.TabIndex = 22;
            this.maxTimeLabel.Text = "stLabel2";
            // 
            // VideoPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.stPanel1);
            this.Name = "VideoPlayer";
            this.Size = new System.Drawing.Size(778, 478);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.stPanel1.ResumeLayout(false);
            this.stPanel1.PerformLayout();
            this.stPanel2.ResumeLayout(false);
            this.stPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.maxFrameCounterUD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.currentFrameCounterUD)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private STLibrary.Forms.STPanel stPanel1;
        private ColorSlider.ColorSlider animationTrackBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private STLibrary.Forms.STButton btnPlay;
        private STLibrary.Forms.STButton btnStop;
        private STLibrary.Forms.STButton btnForward;
        private STLibrary.Forms.STButton btnBackward;
        private System.Windows.Forms.Timer timer1;
        private STLibrary.Forms.STNumbericUpDown maxFrameCounterUD;
        private STLibrary.Forms.STNumbericUpDown currentFrameCounterUD;
        private STLibrary.Forms.STPanel stPanel2;
        private STLibrary.Forms.STPanel stPanel3;
        private STLibrary.Forms.STLabel maxTimeLabel;
        private STLibrary.Forms.STLabel currentTimeLabel;
    }
}
