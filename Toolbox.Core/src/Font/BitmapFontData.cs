using System;
using System.Collections.Generic;
using System.Text;

namespace Toolbox.Core.Font
{
    public class BitmapFontData
    {
        public uint SheetWidth { get; set; }
        public uint SheetHeight { get; set; }

        public uint CellWidth { get; set; }
        public uint CellHeight { get; set; }

        public uint Baseline { get; set; }

        public uint MaxCharacterWidth { get; set; }

        public uint SheetCount { get; set; }

        public byte[] GetImage(int sheetIndex) {
            return null;
        }
    }
}
