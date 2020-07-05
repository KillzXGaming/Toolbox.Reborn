using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Toolbox.Core
{
    public class FileManager
    {
        public static IFileIconLoader[] GetFileIconLoaders()
        {
            List<IFileIconLoader> types = new List<IFileIconLoader>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.FileIconLoaders);

            return types.ToArray();
        }

        public static ICompressionFormat[] GetCompressionFormats()
        {
            List<ICompressionFormat> types = new List<ICompressionFormat>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.CompressionFormats);

            return types.ToArray();
        }

        public static IFileFormat[] GetFileFormats()
        {
            List<IFileFormat> types = new List<IFileFormat>();
            foreach (var plugin in PluginManager.LoadPlugins(true))
                types.AddRange(plugin.FileFormats);

            return types.ToArray();
        }

        public static IImportableTexture[] GetImportableTextures()
        {
            List<IImportableTexture> types = new List<IImportableTexture>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.ImportableTextures);

            return types.ToArray();
        }

        public static IExportableTexture[] GetExportableTextures()
        {
            List<IExportableTexture> types = new List<IExportableTexture>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.ExportableTextures);

            return types.ToArray();
        }

        public static IExportableModel[] GetExportableModels()
        {
            List<IExportableModel> types = new List<IExportableModel>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.ExportableModels);

            return types.ToArray();
        }

        public static IExportableAnimation[] GetExportableAnimations()
        {
            List< IExportableAnimation> types = new List<IExportableAnimation> ();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.ExportableAnimations);

            return types.ToArray();
        }

        public static ITextureDecoder[] GetTextureDecoders()
        {
            List<ITextureDecoder> types = new List<ITextureDecoder>();
            foreach (var plugin in PluginManager.LoadPlugins())
                types.AddRange(plugin.TextureDecoders);

            return types.ToArray();
        }
    }
}
