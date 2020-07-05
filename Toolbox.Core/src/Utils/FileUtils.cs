using System;
using System.Collections.Generic;
using System.IO;

namespace Toolbox.Core
{
    public class FileUtils
    {
        public static void DeleteIfExists(string FilePath)
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
    }
}
