using System;
using System.Collections.Generic;
using System.Text;
using Toolbox.Core.IO;

namespace GCNLibrary.WW
{
    public static class Extension
    {
        public static void WriteOffset(this FileWriter writer, int target)
        {
            long position = writer.Position;
            using (writer.TemporarySeek(target, System.IO.SeekOrigin.Begin)) {
                writer.Write((uint)(position / 4));
            }
        }
    }
}
