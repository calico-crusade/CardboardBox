using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;

namespace CardboardBox.Setup
{
    public static class Settings
    {
        private static Dictionary<string, Func<IConfigurationBuilder, string, bool, bool, IConfigurationBuilder>> Loaders 
            = new Dictionary<string, Func<IConfigurationBuilder, string, bool, bool, IConfigurationBuilder>>
            {
                ["xml"] = (b, f, o, r) => b.AddXmlFile(f, o, r),
                ["json"] = (b, f, o, r) => b.AddJsonFile(f, o, r),
                ["ini"] = (b, f, o, r) => b.AddIniFile(f, o, r)
            };

        public static string DefaultStartDirectory { get; set; } = Directory.GetCurrentDirectory();

        public static T Get<T>(params string[] files) where T: new()
        {
            var builder = Config().AddEnvironmentVariables();

            foreach(var file in files)
            {
                builder.LoadFile(file);
            }

            return builder.Get<T>();
        }

        public static IConfigurationBuilder Config(string basedir = null)
        {
            if (basedir == null)
                basedir = DefaultStartDirectory;

            var pf = new PhysicalFileProvider(basedir);
            return new ConfigurationBuilder()
                    .SetFileProvider(pf);
        }

        public static T Get<T>(this IConfigurationBuilder builder) where T: new()
        {
            var config = builder.Build();
            var settings = new T();
            config.Bind(settings);
            return settings;
        }

        public static IConfigurationBuilder LoadFile(this IConfigurationBuilder builder, string file, bool optional = false, bool reloadOnChange = false)
        {
            var ext = Path.GetExtension(file).ToLower().Trim('.');

            if (Loaders.ContainsKey(ext))
                return Loaders[ext](builder, file, optional, reloadOnChange);

            throw new NotSupportedException($"Files with \"{ext}\" extensions are not yet supported by automatic resolution. Please resolve manually.");
        }

        public static void AddLoader(string extension, Func<IConfigurationBuilder, string, bool, bool, IConfigurationBuilder> loader)
        {
            var ext = extension.ToLower().Trim('.');
            if (Loaders.ContainsKey(ext))
                Loaders[ext] = loader;
            else
                Loaders.Add(ext, loader);
        }
    }
}
