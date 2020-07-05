﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using STLibrary.Forms;
using Toolbox.Core;
using Toolbox.Core.Imaging;

namespace Toolbox.Winforms
{
    public partial class CubeMapFaceViewer : STForm
    {
        private bool DisplayAlpha = true;

        public CubeMapFaceViewer()
        {
            InitializeComponent();

            BackColor = FormThemes.BaseTheme.FormBackColor;
            ForeColor = FormThemes.BaseTheme.FormForeColor;

            contentContainer.BackColor = FormThemes.BaseTheme.FormBackColor;
            contentContainer.ForeColor = FormThemes.BaseTheme.FormForeColor;

            pbFrontFace.Paint += CreatePictureBoxText("Front");
            pbBackFace.Paint += CreatePictureBoxText("Back");
            pbRightFace.Paint += CreatePictureBoxText("Right");
            pbLeftFace.Paint += CreatePictureBoxText("Left");
            pbTopFace.Paint += CreatePictureBoxText("Top");
            pbBottomFace.Paint += CreatePictureBoxText("Bottom");
            chkDisplayAlpha.Checked = DisplayAlpha;
        }

        private PaintEventHandler CreatePictureBoxText(string Text)
        {
            return new PaintEventHandler((sender, e) =>
            {
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                e.Graphics.DrawString(Text, Font, Brushes.Black, 0, 0);
            });
        }

        private STGenericTexture ActiveTexture;
        private int ArrayCounter = 1;

        public void LoadTexture(STGenericTexture Texture)
        {
            ActiveTexture = Texture;

            ArrayCounter = 0;
            for (int i = 0; i < Texture.ArrayCount; i++)
            {
                if ((i + 1) % 6 == 0)
                    ArrayCounter += 1;
            }

            UpdateArrayLevel();
        }

        private const int FRONT_FACE = 0;
        private const int BACK_FACE = 1;
        private const int TOP_FACE = 2;
        private const int BOTTOM_FACE = 3;
        private const int LEFT_FACE = 4;
        private const int RIGHT_FACE = 5;

        private int CurArrayDisplayLevel = 0;
        private int TotalArrayCount = 0;

        private void UpdateArrayLevel()
        {
            if (ActiveTexture == null) return;

            TotalArrayCount = (int)(ActiveTexture.ArrayCount / 6) - 1;

            arrayLevelCounterLabel.Text = $"Array Level: {CurArrayDisplayLevel} / {TotalArrayCount}";

            for (int i = 0; i < 6; i++)
            {
                var CubeFaceBitmap = ActiveTexture.GetBitmap(i + (CurArrayDisplayLevel * 6));
                if (!DisplayAlpha)
                    BitmapExtension.SetChannel(CubeFaceBitmap, ActiveTexture.RedChannel, ActiveTexture.GreenChannel, ActiveTexture.BlueChannel, STChannelType.One);

                if (i == FRONT_FACE)
                    pbFrontFace.Image = CubeFaceBitmap;
                else if (i == BACK_FACE)
                    pbBackFace.Image = CubeFaceBitmap;
                else if (i == BOTTOM_FACE)
                    pbBottomFace.Image = CubeFaceBitmap;
                else if (i == TOP_FACE)
                    pbTopFace.Image = CubeFaceBitmap;
                else if (i == LEFT_FACE)
                    pbLeftFace.Image = CubeFaceBitmap;
                else if (i == RIGHT_FACE)
                    pbRightFace.Image = CubeFaceBitmap;
            }

            if (CurArrayDisplayLevel != TotalArrayCount)
                btnRightArray.Enabled = true;
            else
                btnRightArray.Enabled = false;

            if (CurArrayDisplayLevel != 0)
                btnLeftArray.Enabled = true;
            else
                btnLeftArray.Enabled = false;
        }

        private void chkDisplayAlpha_CheckedChanged(object sender, EventArgs e)
        {
            DisplayAlpha = chkDisplayAlpha.Checked;
            UpdateArrayLevel();
        }

        private void btnRightArray_Click(object sender, EventArgs e)
        {
            if (CurArrayDisplayLevel != TotalArrayCount)
                CurArrayDisplayLevel += 1;

            UpdateArrayLevel();
        }

        private void btnLeftArray_Click(object sender, EventArgs e)
        {
            if (CurArrayDisplayLevel != 0)
                CurArrayDisplayLevel -= 1;

            UpdateArrayLevel();
        }
    }
}