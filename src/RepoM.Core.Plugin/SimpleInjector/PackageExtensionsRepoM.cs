// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using RepoM.Core.Plugin;
    using RepoM.Core.Plugin.Common;
    using SimpleInjector.Packaging;

    /// <summary>
    /// Extension methods for working with packages.
    /// </summary>
    public static class PackageExtensionsRepoM
    {
        /// <summary>
        /// Loads all <see cref="IPackage"/> implementations from the given set of
        /// <paramref name="assemblies"/> and calls their <see cref="IPackage.RegisterServices">Register</see> method.
        /// Note that only publicly exposed classes that contain a public default constructor will be loaded.
        /// </summary>
        /// <param name="container">The container to which the packages will be applied to.</param>
        /// <param name="assemblies">The assemblies that will be searched for packages.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static async Task RegisterPackagesAsync(this Container container, IEnumerable<Assembly> assemblies, Func<string, IPackageConfiguration> packageConfigurationFactoryMethod)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            if (packageConfigurationFactoryMethod == null)
            {
                throw new ArgumentNullException(nameof(packageConfigurationFactoryMethod));
            }

            foreach (Assembly assembly in assemblies)
            {
                var n = assembly.GetName().Name;

                foreach (IPackage package in container.GetPackagesToRegister(new [] { assembly, }))
                {
                    if (package is IPackageWithConfiguration packageWithConfiguration)
                    {
                        var x = n + "." + packageWithConfiguration.Name;

                        await packageWithConfiguration.RegisterServicesAsync(container, packageConfigurationFactoryMethod.Invoke(x)).ConfigureAwait(false);
                    }
                    else
                    {
                        package.RegisterServices(container);
                    }
                }
            }

            // foreach (IPackage package in container.GetPackagesToRegister(assemblies))
            // {
            //     if (package is IPackageWithConfiguration packageWithConfiguration)
            //     {
            //         await packageWithConfiguration.RegisterServicesAsync(container, packageConfigurationFactoryMethod.Invoke("")).ConfigureAwait(false);
            //     }
            //     else
            //     {
            //         package.RegisterServices(container);
            //     }
            // }
        }
    }

    public class FileBasedPackageConfiguration : IPackageConfiguration
    {
        private readonly IAppDataPathProvider _appDataPathProvider;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger _logger;
        private readonly string _filename;

        public FileBasedPackageConfiguration(IAppDataPathProvider appDataPathProvider, IFileSystem fileSystem, ILogger logger, string filename)
        {
            _appDataPathProvider = appDataPathProvider ?? throw new ArgumentNullException(nameof(appDataPathProvider));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _filename = filename ?? throw new ArgumentNullException(nameof(filename));
        }

        public async Task<int?> GetConfigurationVersionAsync()
        {
            var result = await LoadAsync<object>().ConfigureAwait(false);
            return result?.Version;
        }

        public async Task<T?> LoadConfigurationAsync<T>() where T : class, new()
        {
            var result = await LoadAsync<T>().ConfigureAwait(false);
            return result?.Payload;
        }

        public async Task PersistConfigurationAsync<T>(T configuration, int version)
        {
            if (configuration == null)
            {
                return;
            }

            var filename = GetFilename();

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new ConfigEnveloppe<T> { Version = version, Payload = configuration, }, Formatting.Indented);
            await _fileSystem.File.WriteAllTextAsync(filename, json).ConfigureAwait(false);
        }

        private string GetFilename()
        {
            return Path.Combine(_appDataPathProvider.GetAppDataPath(), "Module", _filename + ".json");
        }

        private async Task<ConfigEnveloppe<T>?> LoadAsync<T>()
        {
            var filename = GetFilename();
            if (!_fileSystem.File.Exists(filename))
            {
                return null;
            }

            var json = await _fileSystem.File.ReadAllTextAsync(filename).ConfigureAwait(false);
            // Newtonsoft.Json.JsonConvert.DefaultSettings = () => new Newtonsoft.Json.JsonSerializerSettings
            //     {
            //     Converters = new List<Newtonsoft.Json.JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() },
            //     NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            //     Formatting = Newtonsoft.Json.Formatting.Indented,
            // };

            var result = JsonConvert.DeserializeObject<ConfigEnveloppe<T>>(json);

            return result;

        }

    }

    public class ConfigEnveloppe<T>
    {
        public int Version { get; set; }

        public T Payload { get; set; }
    }
}