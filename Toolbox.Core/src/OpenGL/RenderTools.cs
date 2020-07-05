using System;
using System.Collections.Generic;
using System.Drawing;
using Toolbox.Core.Imaging;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace Toolbox.Core
{
    public class RenderTools
    {
        static STGenericTexture defaulttex;
        public static STGenericTexture defaultTex
        {
            get
            {
                if (defaulttex == null)
                {
                    defaulttex = new GenericBitmapTexture(Properties.Resources.DefaultTexture);
                    defaulttex.LoadOpenGLTexture();
                    defaulttex.RenderableTex.Bind();
                }
                return defaulttex;
            }
            set
            {
                defaulttex = value;
            }
        }

        static STGenericTexture boneWeightGradient;
        public static STGenericTexture BoneWeightGradient
        {
            get
            {
                if (boneWeightGradient == null)
                {
                    boneWeightGradient = new GenericBitmapTexture(Properties.Resources.boneWeightGradient);
                    boneWeightGradient.LoadOpenGLTexture();
                    boneWeightGradient.RenderableTex.Bind();
                }
                return boneWeightGradient;
            }
            set
            {
                boneWeightGradient = value;
            }
        }

        static STGenericTexture boneWeightGradient2;
        public static STGenericTexture BoneWeightGradient2
        {
            get
            {
                if (boneWeightGradient2 == null)
                {
                    boneWeightGradient2 = new GenericBitmapTexture(Properties.Resources.boneWeightGradient2);
                    boneWeightGradient2.LoadOpenGLTexture();
                    boneWeightGradient2.RenderableTex.Bind();
                }
                return boneWeightGradient2;
            }
            set
            {
                boneWeightGradient2 = value;
            }
        }

        static STGenericTexture uvtestPattern;
        public static STGenericTexture uvTestPattern
        {
            get
            {
                if (uvtestPattern == null)
                {
                    uvtestPattern = new GenericBitmapTexture(Properties.Resources.UVPattern);
                    uvtestPattern.LoadOpenGLTexture();
                    uvtestPattern.RenderableTex.Bind();
                }
                return uvtestPattern;
            }
            set
            {
                uvtestPattern = value;
            }
        }

        public static void LoadTextures()
        {

        }

        public static void DisposeTextures()
        {
            defaultTex = null;
            boneWeightGradient = null;
            boneWeightGradient2 = null;
            uvTestPattern = null;

            //  brdfPbr = null;
            //  diffusePbr = null;
            // specularPbr = null;
        }
    }
}
