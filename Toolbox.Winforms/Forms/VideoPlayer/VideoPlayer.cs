using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;
using Toolbox.Core.Imaging;

namespace Toolbox.Winforms
{
    public partial class VideoPlayer : UserControl
    {
        public bool IsLooping = true;

        public int FrameRate { get; set; } = 60;

        public PlayerState AnimationPlayerState = PlayerState.Stop;

        public enum PlayerState
        {
            Playing,
            Pause,
            Stop,
        }

        public bool IsPlaying
        {
            get { return AnimationPlayerState == PlayerState.Playing; }
        }

        private IVideoFormat videoFormat;
        public IVideoFormat VideoFormat
        {
            get
            {
                return videoFormat;
            }
            set
            {
                videoFormat = value;
                FrameRate = (int)value.FrameRate;
                maxFrameCounterUD.Maximum = value.FrameCount;
                maxFrameCounterUD.Value = value.FrameCount;
                currentFrameCounterUD.Maximum = value.FrameCount;
                animationTrackBar.TickDivide = 1;
                animationTrackBar.Maximum = (int)value.FrameCount;
                animationTrackBar.Minimum = 0;
                animationTrackBar.Value = 0;
                currentFrameCounterUD.Value = 0;
                currentTimeLabel.Text = "0.0";
                maxTimeLabel.Text = "0.0";

                timer1.Interval = (int)(1000.0f / (float)FrameRate);
            }
        }

        public VideoPlayer()
        {
            InitializeComponent();

            pictureBox1.BackColor = Color.Black;
            btnPlay.BackgroundImage = STLibraryForms.Properties.Resources.PlayArrowR;
            btnStop.BackgroundImage = STLibraryForms.Properties.Resources.StopBtn;
            btnBackward.BackgroundImage = STLibraryForms.Properties.Resources.RewindArrows1L;
            btnForward.BackgroundImage = STLibraryForms.Properties.Resources.RewindArrows1R;
            btnPlay.BackColor = STLibrary.Forms.FormThemes.BaseTheme.FormBackColor;
            btnStop.BackColor = STLibrary.Forms.FormThemes.BaseTheme.FormBackColor;
            btnBackward.BackColor = STLibrary.Forms.FormThemes.BaseTheme.FormBackColor;
            btnForward.BackColor = STLibrary.Forms.FormThemes.BaseTheme.FormBackColor;

            currentFrameCounterUD.Value = 0;
            maxFrameCounterUD.Value = 1;

            currentTimeLabel.Text = "0.0";
            maxTimeLabel.Text = "0.0";

            timer1.Interval = 100 / FrameRate;
        }

        public void LoadVideoFile(IVideoFormat fileFormat) {
            VideoFormat = fileFormat;
        }

        private void Play()
        {
            timer1.Start();
            AnimationPlayerState = PlayerState.Playing;
            UpdateAnimationUI();
        }

        private void Pause()
        {
            timer1.Stop();
            AnimationPlayerState = PlayerState.Stop;
            UpdateAnimationUI();
        }

        private void Stop()
        {
            timer1.Stop();
            animationTrackBar.Value = 0;
            AnimationPlayerState = PlayerState.Stop;
            UpdateAnimationUI();
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            if (AnimationPlayerState == PlayerState.Playing)
                Pause();
            else
                Play();
        }

        private void btnStop_Click(object sender, EventArgs e) {
            Stop();
        }

        private void UpdateAnimationUI() {
            btnPlay.BackgroundImage = IsPlaying ? STLibraryForms.Properties.Resources.PauseBtn
                : STLibraryForms.Properties.Resources.PlayArrowR;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (AnimationPlayerState == PlayerState.Playing)
            {
                if (animationTrackBar.Value == animationTrackBar.Maximum)
                {
                    if (IsLooping)
                        animationTrackBar.Value = 0;
                    else
                        Stop();
                }
                else
                {
                    animationTrackBar.Value++;
                }
            }
        }

        private void maxFrameCounterUD_ValueChanged(object sender, EventArgs e)
        {
            if (VideoFormat == null) return;
            if (maxFrameCounterUD.Value < 1)
            {
                maxFrameCounterUD.Maximum = 1;
                maxFrameCounterUD.Value = 1;
            }
            else
            {
                animationTrackBar.Value = 0;
                animationTrackBar.Maximum = VideoFormat.FrameCount;
                animationTrackBar.Minimum = 0;
            }
        }

        private void animationTrackBar_ValueChanged(object sender, EventArgs e)
        {
            currentFrameCounterUD.Value = animationTrackBar.Value;
            SetAnimationsToFrame(animationTrackBar.Value);
        }

        private void currentFrameCounterUD_ValueChanged(object sender, EventArgs e)
        {
            if (currentFrameCounterUD.Value > maxFrameCounterUD.Value)
                currentFrameCounterUD.Value = maxFrameCounterUD.Value;

            animationTrackBar.Value = (int)currentFrameCounterUD.Value;
        }

        private void SetAnimationsToFrame(int Frame)
        {
            var video = VideoFormat.VideoData;
            if (video == null) return;

            var frame = video.GetFrame(Frame);
            if (frame != null)
            {
                var bitmap = BitmapExtension.CreateBitmap(frame.GetImageData(), (int)video.Width, (int)video.Height);
                if (frame.FlipImage)
                    bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                pictureBox1.Image = bitmap;
            }
        }
    }
}
