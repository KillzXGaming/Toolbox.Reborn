using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.ModelView;

namespace Toolbox.Core
{
    public class CompressionMenu
    {
        public static ToolMenuItem[] GetMenuItems()
        {
            List<ToolMenuItem> items = new List<ToolMenuItem>();
            foreach (var file in FileManager.GetCompressionFormats())
                items.Add(CreateMenu(file));
            return items.ToArray();
        }

        public static ToolMenuItem CreateMenu(ICompressionFormat format) {
            var item = new CompressionMenuItem(format);
            return item;
        }

        public class CompressionMenuItem : ToolMenuItem
        {
            public ICompressionFormat Format;

            public CompressionMenuItem(ICompressionFormat format) : base(format.ToString())
            {
                Format = format;
                Children.Add(new ToolMenuItem("Decompress", DecompressMenu)) ;
                Children.Add(new ToolMenuItem("Compress", CompressMenu)
                {
                    Enabled = Format.CanCompress,
                });
            }

            private void DecompressMenu(object sender, EventArgs e)
            {

            }

            private void CompressMenu(object sender, EventArgs e)
            {

            }
        }
    }
}
