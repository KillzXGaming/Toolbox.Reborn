using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using STLibrary.Forms;
using Toolbox.Core;
using Toolbox.Core.Imaging;

namespace Toolbox.Winforms
{
    internal partial class TextureMapEditor : UserControl
    {
        ImageList imgListSmall = new ImageList();
        ImageList imgListBig = new ImageList();

        private STGenericMaterial ActiveMaterial;
        private List<STGenericMesh> ActiveMeshes;

        private UVViewport uvViewport1;

        private bool isLoaded = false;
        private Thread Thread;

        public TextureMapEditor()
        {
            InitializeComponent();

            uvViewport1 = new UVViewport();
            uvViewport1.Dock = DockStyle.Fill;
            viewportPanel.Controls.Add(uvViewport1);

            stDropDownPanel1.ResetColors();
            stDropDownPanel2.ResetColors();

            imgListSmall = new ImageList()
            {
                ImageSize = new Size(30, 30),
                ColorDepth = ColorDepth.Depth32Bit,
            };
            imgListBig = new ImageList()
            {
                ColorDepth = ColorDepth.Depth32Bit,
                ImageSize = new Size(40, 40),
            };
        }

        public void LoadMaterial(STGenericMaterial material, List<STGenericMesh> meshes)
        {
            Reset();

            ActiveMaterial = material;
            ActiveMeshes = meshes;

            listViewCustom1.LargeImageList = imgListBig;
            listViewCustom1.SmallImageList = imgListSmall;
            listViewCustom1.View = View.LargeIcon;

            foreach (var texture in material.TextureMaps)
            {
                ListViewItem item = new ListViewItem();
                item.Text = texture.Name;
                item.ImageIndex = 0;
                listViewCustom1.Items.Add(item);
            }

            if (Thread != null && Thread.IsAlive)
                Thread.Abort();

            Thread = new Thread((ThreadStart)(() =>
            {
                int index = 0;
                foreach (var texture in material.TextureMaps)
                {
                    var tex = texture.GetTexture();
                    if (tex != null)
                    {
                        LoadTextureIcon(index, tex);
                    }
                    index++;
                }
            }));
            Thread.Start();

            isLoaded = true;

            if (listViewCustom1.Items.Count > 0)
            {
                listViewCustom1.Items[0].Selected = true;
                listViewCustom1.Select();
            }
        }

        void Reset()
        {
            if (Thread != null && Thread.IsAlive)
                Thread.Abort();

            for (int i = 0; i < imgListBig.Images.Count; i++)
                imgListBig.Images[i].Dispose();

            for (int i = 0; i < imgListSmall.Images.Count; i++)
                imgListSmall.Images[i].Dispose();

            imgListBig.Images.Clear();
            imgListSmall.Images.Clear();
            listViewCustom1.Items.Clear();

            isLoaded = false;
        }

        private void LoadTextureIcon(int index, STGenericTexture texture)
        {
            Bitmap temp = texture.GetBitmap();
            if (temp == null)
                return;

            //temp = texture.GetComponentBitmap(temp, true);
            temp = BitmapExtension.CreateImageThumbnail(temp, 40, 40);

            if (listViewCustom1.InvokeRequired)
            {
                listViewCustom1.Invoke((MethodInvoker)delegate {
                    var item = listViewCustom1.Items[index];
                    item.ImageIndex = imgListBig.Images.Count;

                    // Running on the UI thread
                    imgListBig.Images.Add(temp);
                    imgListSmall.Images.Add(temp);

                    var dummy = imgListBig.Handle;
                    var dummy2 = imgListSmall.Handle;
                });
            }

            temp.Dispose();
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isLoaded)
                return;

            if (listViewCustom1.SelectedItems.Count > 0)
            {
                int index = listViewCustom1.SelectedIndices[0];
                var textureMap = ActiveMaterial.TextureMaps[index];
                uvViewport1.ActiveTextureMap = textureMap;

                uvViewport1.ActiveObjects.Clear();
                foreach (var mesh in ActiveMeshes)
                    uvViewport1.ActiveObjects.Add(mesh);

                uvViewport1.UpdateViewport();
            }
            else
            {
                uvViewport1.ActiveTextureMap = null;
            }
        }
    }
}
