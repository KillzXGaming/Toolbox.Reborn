using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Drawing;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.GUI;

namespace Toolbox.Winforms
{
    public class TextureSettingsBase
    {
        public IPlatformSwizzle Platform { get; set; } = new DefaultSwizzle();

        public TexFormat Format { get; set; }

        public virtual string FormatDisplay => Format.ToString();

        public virtual ITextureSettingsGUI GetGUIHandler()
        {
            return new GUISettings();
        }

        STGenericTexture BaseTexture = null;

        public string Name { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }

        public List<Surface> Surfaces = new List<Surface>();

        public void ImportTexture(string fileName)
        {
            Name = Path.GetFileNameWithoutExtension(fileName);

            STGenericTexture texture = null;
            foreach (var importable in FileManager.GetImportableTextures()) {
                Console.WriteLine($"importable {importable}");

                if (importable.IdentifyImport(Utils.GetExtension(fileName)))
                    texture = importable.Import(fileName);
            }
            ImportTexture(texture);
        }

        private void ImportTexture(STGenericTexture texture) {
            if (texture == null) return;

            Width = texture.Width;
            Height = texture.Height;
            BaseTexture = texture;
        }

        public Bitmap GetDecodedImage()
        {
            if (BaseTexture != null)
                return BaseTexture.GetBitmap();

            return null;
        }

        public List<byte[]> EncodeSurface()
        {
            List<byte[]> encoded = new List<byte[]>();
            foreach (var surface in BaseTexture.GetSurfaces())
                encoded.Add(Platform.EncodeImage(BaseTexture, surface.mipmaps[0], Width, Height, 0, 0));
            return encoded;
        }

        public class Surface
        {
            public Stream ImageData { get; set; }
        }

        public static readonly TexFormat[] FormatList =
        {
                TexFormat.RGBA8_SNORM,
                TexFormat.RGBA8_SRGB,

                TexFormat.BC1_UNORM,
                TexFormat.BC1_SRGB,
                TexFormat.BC2_UNORM,
                TexFormat.BC2_SRGB,
                TexFormat.BC3_UNORM,
                TexFormat.BC3_SRGB,
                TexFormat.BC4_UNORM,
                TexFormat.BC4_SNORM,
                TexFormat.BC5_UNORM,
                TexFormat.BC5_SNORM,
                TexFormat.BC6H_UF16,
                TexFormat.BC6H_SF16,
                TexFormat.BC7_UNORM,
                TexFormat.BC7_SRGB,
        };

        public class GUISettings : ITextureSettingsGUI
        {
            [BindGUI("Format")]
            [BindCategory("Surface")]
            public Controls.ComboBox FormatCombobox { get; set; }

            //Skipped in GUI
            public TexFormat Format { get; set; }

            [BindGUI("Surface Type")]
            [BindCategory("Surface")]
            public STSurfaceType Surface { get; set; }

            [BindGUI("BC4 Alpha")]
            [BindCategory("Surface")]
            public bool BC4Alpha { get; set; }

            [BindGUI("Mip Count")]
            [BindCategory("Surface")]
            public Controls.NumericUpDown MipCount { get; set; }

            [BindGUI("Red Channel")]
            [BindCategory("Channels")]
            public STChannelType RedChannel { get; set; }

            [BindGUI("Green Channel")]
            [BindCategory("Channels")]
            public STChannelType GreenChannel { get; set; }

            [BindGUI("Blue Channel")]
            [BindCategory("Channels")]
            public STChannelType BlueChannel { get; set; }

            [BindGUI("Alpha Channel")]
            [BindCategory("Channels")]
            public STChannelType AlphaChannel { get; set; }

            public void UpdateSettings(TextureSettingsBase settings)
            {

            }

            public GUISettings()
            {
                FormatCombobox = new Controls.ComboBox();
                MipCount = new Controls.NumericUpDown(0, 13, 0);

                FormatCombobox.Bind(this, Format, FormatList);

                RedChannel = STChannelType.Red;
                GreenChannel = STChannelType.Green;
                BlueChannel = STChannelType.Blue;
                AlphaChannel = STChannelType.Alpha;
                Surface = STSurfaceType.Texture2D;
            }
        }
    }
}
