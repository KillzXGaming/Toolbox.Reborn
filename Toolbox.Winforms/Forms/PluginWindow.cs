using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Toolbox.Core;
using STLibrary.Forms;

namespace Toolbox.Winforms
{
    public partial class PluginWindow : STForm
    {
        public PluginWindow()
        {
            InitializeComponent();
        }

        private void PluginWindow_Load(object sender, EventArgs e)
        {
            var plugins = PluginManager.LoadPlugins();
            Console.WriteLine($"plugins {plugins.Count}");
            foreach (var plugin in plugins)
                listViewCustom1.Items.Add(plugin.PluginHandler.Name);
        }

        private void listViewCustom1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewCustom1.SelectedIndices.Count == 0 || mouseDown)
                return;

            SelectionChanged();
        }

        private void SelectionChanged()
        {
            listViewCustom2.Items.Clear();
            foreach (int index in listViewCustom1.SelectedIndices)
            {
                var plugins = PluginManager.LoadPlugins().ToList();
                var plugin = plugins[index];
                if (plugin.FileFormats.Count == 0)
                    continue;

                //listViewCustom2.Items.Add($"---{plugin.PluginHandler.Name}----");
                foreach (var fileFormat in plugin.FileFormats)
                {
                    string extsions = fileFormat.Extension != null ? string.Join(",", fileFormat.Extension) : "";

                    ListViewItem item = new ListViewItem();
                    item.Text = plugin.PluginHandler.Name;
                    item.SubItems.Add(fileFormat.Description[0]);
                    item.SubItems.Add(fileFormat.CanSave.ToString());
                    item.SubItems.Add(extsions);
                    listViewCustom2.Items.Add(item);
                }

                foreach (var fileFormat in plugin.CompressionFormats)
                {
                    string extsions = fileFormat.Extension != null ? string.Join(",", fileFormat.Extension) : "";

                    ListViewItem item = new ListViewItem();
                    item.Text = plugin.PluginHandler.Name;
                    item.SubItems.Add(fileFormat.Description[0]);
                    item.SubItems.Add(fileFormat.CanCompress.ToString());
                    item.SubItems.Add(extsions);
                    listViewCustom2.Items.Add(item);
                }
            }
        }

        private bool mouseDown = false;
        private void listViewCustom1_MouseDown(object sender, MouseEventArgs e) {
            mouseDown = true;
        }

        private void listViewCustom1_MouseUp(object sender, MouseEventArgs e) {
            mouseDown = false;
            if (listViewCustom1.SelectedIndices.Count > 0)
                SelectionChanged();
        }
    }
}
