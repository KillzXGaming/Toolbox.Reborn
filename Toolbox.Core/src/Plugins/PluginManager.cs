using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

namespace Toolbox.Core
{
    public class PluginManager
    {
        private static ICollection<PluginInstance> Plugins;

        public class PluginInstance
        {
            //File handling
            public List<IFileFormat> FileFormats = new List<IFileFormat>();
            public List<ICompressionFormat> CompressionFormats = new List<ICompressionFormat>();

            //Exportables
            public List<IExportableModel> ExportableModels = new List<IExportableModel>();
            public List<IExportableAnimation> ExportableAnimations = new List<IExportableAnimation>();
            public List<IExportableTexture> ExportableTextures = new List<IExportableTexture>();

            //importables
            public List<IImportableTexture> ImportableTextures = new List<IImportableTexture>();
            public List<IImportableModel> ImportableModels = new List<IImportableModel>();

            //GUI based
            public List<IFileEditor> FileEditors = new List<IFileEditor>();
            public List<IFileIconLoader> FileIconLoaders = new List<IFileIconLoader>();

            //Texture decoding
            public List<ITextureDecoder> TextureDecoders = new List<ITextureDecoder>();

            public IPlugin PluginHandler;
        }

        public static ICollection<PluginInstance> LoadPlugins(bool force = false)
        {
            if (Plugins != null & !force)
                return Plugins;

            List<PluginInstance> PluginList = new List<PluginInstance>();
            //var mainLibrary = SearchMainAssembly();
           // PluginList.Add(mainLibrary);

            string path = Path.Combine(Runtime.ExecutableDir, "Plugins");

            List<string> dllFileNames = new List<string>();
            dllFileNames.Add(Path.Combine(Runtime.ExecutableDir, "Toolbox.Core.dll"));
            if (Directory.Exists(path))
            {
                foreach (var dir in Directory.GetDirectories(path))
                    dllFileNames.AddRange(Directory.GetFiles($"{dir}", "*.dll"));

                dllFileNames.AddRange(Directory.GetFiles(path, "*.dll"));
            }
            if (dllFileNames == null) {
                return PluginList;
            }

            List<string> loadedLibs = new List<string>();
            
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Count);
            foreach (string dllFile in dllFileNames)
            {
                string name = Path.GetFileName(dllFile);
                if (loadedLibs.Contains(name))
                    continue;

                loadedLibs.Add(name);

                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }

            foreach (Assembly assembly in assemblies)
            {
                //Create a plugin instance for each assembly
                PluginInstance plugin = SearchAssembly(assembly);

                if (plugin.PluginHandler != null)
                    PluginList.Add(plugin);
            }

            assemblies.Clear();

            Plugins = PluginList;
            return PluginList;
        }

        private static PluginInstance SearchMainAssembly()
        {
            string dllFile = Path.Combine(Runtime.ExecutableDir, "Toolbox.Core.dll");

            AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
            Assembly assembly = Assembly.Load(an);

            return SearchAssembly(assembly); 
        }

        private static PluginInstance SearchAssembly(Assembly assembly)
        {
            Type pluginType = typeof(IPlugin);
            PluginInstance plugin = new PluginInstance();

            if (assembly != null)
            {
                try
                {
                    Type[] types = assembly.GetTypes();
                    foreach (Type type in types)
                    {
                        if (type.IsInterface || type.IsAbstract)
                        {
                            continue;
                        }
                        else
                        {
                            if (type.GetInterface(pluginType.FullName) != null) {
                                plugin.PluginHandler = (IPlugin)Activator.CreateInstance(type);
                            }

                            AddTypeList(plugin.FileFormats, type);
                            AddTypeList(plugin.CompressionFormats, type);
                            AddTypeList(plugin.FileEditors, type);
                            AddTypeList(plugin.TextureDecoders, type);
                            AddTypeList(plugin.FileIconLoaders, type);
                            AddTypeList(plugin.ExportableTextures, type);
                            AddTypeList(plugin.ImportableTextures, type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Exception exSub in ex.LoaderExceptions)
                    {
                        sb.AppendLine(exSub.Message);
                        FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                        if (exFileNotFound != null)
                        {
                            if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                            {
                                sb.AppendLine("Fusion Log:");
                                sb.AppendLine(exFileNotFound.FusionLog);
                            }
                        }
                        sb.AppendLine();
                    }
                    string errorMessage = sb.ToString();
                    throw new Exception(errorMessage);
                }
            }
            return plugin;
        }

        private static void AddTypeList<T>(List<T> list, Type type)
        {
            if (type.GetInterface(typeof(T).FullName) != null) {
                list.Add((T)Activator.CreateInstance(type));
            }
        }
    }
}
