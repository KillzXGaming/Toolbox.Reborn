using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Toolbox.Core;

namespace Toolbox.Winforms
{
    internal partial class MaterialEditor : UserControl
    {
        TextureMapEditor TextureMapEditor;

        public MaterialEditor() {
            InitializeComponent();

            TextureMapEditor = new TextureMapEditor()
            {
                Dock = DockStyle.Fill,
            };

            var tabPage = new TabPage();
            tabPage.Text = "Texture Maps";
            tabPage.Controls.Add(TextureMapEditor);
            stTabControl1.Controls.Add(tabPage);
        }

        public void LoadMaterial(STGenericMaterial material) {
            TextureMapEditor.LoadMaterial(material, material.GetMappedMeshes());
        }
    }
}
