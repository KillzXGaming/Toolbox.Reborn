using System;
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
using Toolbox.Core.GUI;

namespace Toolbox.Winforms
{
    public partial class TextureExportDialog : STForm
    {
        public string DefaultExt = ".png";

        static List<ExtensionItem> Extensions = new List<ExtensionItem>();

        List<TextureExportSettings> Settings = new List<TextureExportSettings>();

        public TextureExportDialog()
        {
            InitializeComponent();

            splitContainer1.Panel1Collapsed = true;

            foreach (var format in FileManager.GetExportableTextures()) {
                if (format is IFileFormat) {
                    string[] extensions =  ((IFileFormat)format).Extension;
                    string[] descs = ((IFileFormat)format).Description;
                    string desc = "";
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        if (descs.Length > i) desc = descs[i];

                        ExtensionItem ext = new ExtensionItem();
                        ext.Extension = extensions[i];
                        ext.Description = desc;
                        Extensions.Add(ext);
                    }
                }
            }
        }

        class ExtensionItem
        {
            public string Extension;
            public string Description;

            public override string ToString()
            {
                return $"{Description}({Extension})";
            }
        }

        public void LoadTextures(IEnumerable<STGenericTexture> textures)
        {
            listViewCustom1.Items.Clear();
            listViewCustom1.Items.Add("All Textures");
            Settings.Clear();

            Settings.Add(new TextureExportSettings());
            foreach (var tex in textures)
            {
                Settings.Add(new TextureExportSettings());

                ListViewItem item = new ListViewItem(tex.Name);
                item.SubItems.Add(DefaultExt);
                listViewCustom1.Items.Add(item);
            }
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedItems.Count == 0) return;

            int index = listViewCustom1.SelectedIndices[0];
            if (index == 0)
                splitContainer1.Panel1Collapsed = true;
            else
                splitContainer1.Panel1Collapsed = false;

            var settings = Settings[index];
            var control = GUIGenerator.Generate(settings);
            stPanel2.Controls.Clear();
            stPanel2.Controls.Add(control);
        }

        class TextureExportSettings
        {
            public ExtensionItem Extension { get; set; }

            [BindGUI("Format")]
            public Controls.ComboBox FormatBox { get; set; }

            [BindGUI("Channel Swaps")]
            public bool UseChannelSwaps { get; set; }

            public TextureExportSettings()
            {
                FormatBox = new Controls.ComboBox();
                FormatBox.Bind(this, Extension, Extensions);
            }
        }
    }
}
