using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Toolbox.Core.IO
{
    public static class StreamExtension
    {
        public static byte[] ToArray(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);

                return memoryStream.ToArray();
            }
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            using (var reader = new FileReader(stream, true)) {
                reader.Position = 0;
                return reader.ReadBytes((int)stream.Length);
            }
        }

        public async static System.Threading.Tasks.Task SaveToFileAsync(this Stream stream, string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Create,
                FileAccess.ReadWrite, FileShare.Write))
            {
                stream.Position = 0;
                await stream.CopyToAsync(fileStream);
            }
        }

        public static void SaveToFile(this Stream stream, string fileName)
        {
            if (stream == null) return;

            string dir = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                stream.Position = 0;
                stream.CopyTo(fileStream);
            }
        }
    }
}
