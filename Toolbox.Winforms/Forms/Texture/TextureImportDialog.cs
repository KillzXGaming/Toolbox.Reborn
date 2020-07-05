using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core.Nitro;
using STLibrary.Forms;
using Toolbox.Core.Imaging;
using Toolbox.Core.GUI;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    public partial class TextureImportDialog : STForm
    {
        public TextureSettingsBase ActiveSetting { get; set; }
        public List<TextureSettingsBase> Settings = new List<TextureSettingsBase>();

        private ITextureSettingsGUI GUIHandle;

        Dictionary<string, IPlatformSwizzle> Platforms = new Dictionary<string, IPlatformSwizzle>()
        {
            { "Normal",   new DefaultSwizzle() },
            { "Gamecube", new GamecubeSwizzle() },
            { "Wii",      new GamecubeSwizzle() },
            { "Wii U",    new WiiUSwizzle(TexFormat.RGB8_SRGB) },
            { "Switch",   new SwitchSwizzle(TexFormat.RGB8_SRGB) },
            { "3DS",      new CTRSwizzle(CTR_3DS.PICASurfaceFormat.RGBA8) },
            { "DS",       new NitroSwizzle(NitroTex.NitroTexFormat.CMPR_4x4) },
        };

        public TextureImportDialog()
        {
            InitializeComponent();
            ToggleArrayList(false);

            foreach (var platform in Platforms)
                platformCB.Items.Add(platform.Key);
            platformCB.SelectedIndex = 0;
        }

        public void LoadTextures(string[] filePaths, STGenericTexture textureBase) {
            foreach (var file in filePaths)
                LoadTextures(file, textureBase);

            listViewCustom1.Items[0].Selected = true;
            listViewCustom1.Select();
        }

        public void LoadTextures(string filePath, STGenericTexture textureBase) {
            var settings = UpdateSettingsBase(textureBase.Platform);
            settings.ImportTexture(filePath);

            ListViewItem item = new ListViewItem(settings.Name);
            item.SubItems.Add(settings.FormatDisplay);
            listViewCustom1.Items.Add(item);

            Settings.Add(settings);
        }

        private void UpdateSettingsDisplay()
        {
            if (GUIHandle == null || GUIHandle.GetType() != ActiveSetting.GetGUIHandler().GetType())
            {
                GUIHandle = ActiveSetting.GetGUIHandler();
                GUIHandle.UpdateSettings(ActiveSetting);
                var control = GUIGenerator.Generate(GUIHandle);
                control.Dock = DockStyle.Fill;  
                stPanel1.Controls.Clear();
                stPanel1.Controls.Add(control);
            }
            else
                GUIHandle.UpdateSettings(ActiveSetting);
        }

        private TextureSettingsBase UpdateSettingsBase(IPlatformSwizzle platform)
        {
            Console.WriteLine($"platform {platform}");

            if (platform is GamecubeSwizzle)
                return new GCNTextureSettings();
            else
                return new TextureSettingsBase();
        }

        public void ToggleArrayList(bool toggle)
        {
            if (toggle)
            {
                splitContainer2.Panel2Collapsed = false;
            }
            else
            {
                splitContainer2.Panel2Collapsed = true;
            }
        }

        private void propertyChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedItems.Count == 0)
                return;

            var item = listViewCustom1.SelectedItems[0];
            item.SubItems[0].Text = ActiveSetting.FormatDisplay;
        }

        private void platformCB_SelectedIndexChanged(object sender, EventArgs e) {
            if (platformCB.SelectedIndex == -1 || ActiveSetting == null) return;

            foreach (var setting in Settings)
                setting.Platform = Platforms[platformCB.GetSelectedText()];
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedIndices.Count == 0) return;

            int index = listViewCustom1.SelectedIndices[0];
            ActiveSetting = Settings[index];

            UpdateSettingsDisplay();
            UpdateImage(ActiveSetting);
        }

        private void UpdateImage(TextureSettingsBase settings)
        {
            var image = settings.GetDecodedImage();
            pictureBoxCustom1.Image = image;
        }
    }
}
