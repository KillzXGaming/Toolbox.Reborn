using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Toolbox.Core;
using Toolbox.Core.Imaging;
using Toolbox.Core.GUI;

namespace Toolbox.Winforms
{
    public class GCNTextureSettings : TextureSettingsBase
    {
        public Decode_Gamecube.TextureFormats GCNFormat { get; set; }
        public Decode_Gamecube.PaletteFormats GCNPaletteFormat { get; set; }

        public int MipCount { get; set; } = 1;
        public int Colors { get; set; } = 256;

        public override string FormatDisplay => GCNFormat.ToString();

        public override ITextureSettingsGUI GetGUIHandler()
        {
            return new GCNGUISettings();
        }

        public GCNTextureSettings()
        {
            GCNFormat = Decode_Gamecube.TextureFormats.CMPR;
            GCNPaletteFormat = Decode_Gamecube.PaletteFormats.RGB565;
        }

        public class GCNGUISettings : ITextureSettingsGUI
        {
            [BindGUI("Format")]
            [BindCategory("Image Settings")]
            public Controls.ComboBox FormatCombobox { get; set; }

            [BindGUI("Mip Count")]
            [BindCategory("Image Settings")]
            public Controls.NumericUpDown MipCount { get; set; }

            [BindGUI("Palette Format")]
            [BindCategory("Palette")]
            public Controls.ComboBox PaletteFormatCombobox { get; set; }

            [BindGUI("Palette Colors")]
            [BindCategory("Palette")]
            [ControlAttributes.MinMax(16, 256)]
            public int Colors
            {
                get { return _settings.Colors; }
                set { _settings.Colors = value; }
            }

            private GCNTextureSettings _settings = new GCNTextureSettings();

            public void UpdateSettings(TextureSettingsBase settings)
            {
                _settings = (GCNTextureSettings)settings;

                FormatCombobox.Bind(typeof(Decode_Gamecube.TextureFormats), settings, _settings.GCNFormat);
                PaletteFormatCombobox.Bind(typeof(Decode_Gamecube.PaletteFormats), settings, _settings.GCNPaletteFormat);
                MipCount.Value = _settings.MipCount;

                FormatCombobox.SelectedValue = _settings.GCNFormat;
                PaletteFormatCombobox.SelectedValue = _settings.GCNPaletteFormat;
            }

            public GCNGUISettings()
            {
                FormatCombobox = new Controls.ComboBox();
                PaletteFormatCombobox = new Controls.ComboBox();
                MipCount = new Controls.NumericUpDown(0, 13, 0);
            }
        }
    }
}
