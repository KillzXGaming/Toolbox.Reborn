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

namespace Toolbox.Winforms
{
    public partial class MeshEditor : UserControl
    {
        private STGenericMesh ActiveMesh { get; set; }
        private STGenericMaterial ActiveMaterial { get; set; }

        //Mesh Editors

        //Material Editors
        private TextureMapEditor TextureMapEditor;

        public MeshEditor()
        {
            InitializeComponent();

            TextureMapEditor = new TextureMapEditor();
            AddTab(TextureMapEditor, "Texture Maps");
        }

        public void LoadMesh(STGenericMesh mesh) {
            ActiveMesh = mesh;
            for (int i = 0; i < mesh.PolygonGroups.Count; i++) {
                if (mesh.PolygonGroups[i].Material != null)
                {
                    LoadMaterial(mesh.PolygonGroups[i].Material);
                    break;
                }
            }
        }

        public void LoadMaterial(STGenericMaterial mat)
        {
            TextureMapEditor.LoadMaterial(mat, new List<STGenericMesh>() { ActiveMesh });
        }

        private void LoadEditor(int index)
        {
            switch (index)
            {

            }
        }

        private void AddTab(Control control, string text)
        {
            TabPage tabPage = new TabPage();
            tabPage.Text = text;
            control.Dock = DockStyle.Fill;
            tabPage.Controls.Add(control);
            stTabControl1.TabPages.Add(tabPage);
        }
    }
}
