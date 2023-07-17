// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace SimpleInjector
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using RepoM.Core.Plugin;
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
        /// <param name="packageConfigurationFactoryMethod">The factory method to create an <see cref="IPackageConfiguration"/> instance based.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static Task RegisterPackagesAsync(this Container container, IEnumerable<Assembly> assemblies, Func<string, IPackageConfiguration> packageConfigurationFactoryMethod)
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

            return RegisterPackagesInnerAsync(container, assemblies, packageConfigurationFactoryMethod);
        }

        private static async Task RegisterPackagesInnerAsync(Container container, IEnumerable<Assembly> assemblies, Func<string, IPackageConfiguration> packageConfigurationFactoryMethod)
        {
            foreach (Assembly assembly in assemblies)
            {
                var assemblyName = assembly.GetName().Name ?? string.Empty;

                foreach (IPackage package in container.GetPackagesToRegister(new[] { assembly, }))
                {
                    if (package is IPackageWithConfiguration packageWithConfiguration)
                    {
                        var fileName = assemblyName;
                        if (fileName.StartsWith("RepoM.Plugin."))
                        {
                            fileName = fileName["RepoM.Plugin.".Length..];
                        }

                        if (!string.IsNullOrWhiteSpace(packageWithConfiguration.Name))
                        {
                            fileName += "." + packageWithConfiguration.Name;
                        }

                        await packageWithConfiguration
                              .RegisterServicesAsync(container, packageConfigurationFactoryMethod.Invoke(fileName))
                              .ConfigureAwait(false);
                    }
                    else
                    {
                        package.RegisterServices(container);
                    }
                }
            }
        }
    }
}