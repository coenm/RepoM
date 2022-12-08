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
    using System.Linq;
    using System.Reflection;
    using SimpleInjector.Packaging;

    /// <summary>
    /// Extension methods for working with packages.
    /// </summary>
    public static class PackageExtensions
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
        public static void RegisterPackages(this Container container, IEnumerable<Assembly> assemblies)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            foreach (var package in container.GetPackagesToRegister(assemblies))
            {
                package.RegisterServices(container);
            }
        }

        /// <summary>
        /// Loads all <see cref="IPackage"/> implementations from the given set of
        /// <paramref name="assemblies"/> and returns a list of created package instances.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="assemblies">The assemblies that will be searched for packages.</param>
        /// <returns>Returns a list of created packages.</returns>
        public static IPackage[] GetPackagesToRegister(
            this Container container, IEnumerable<Assembly> assemblies)
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            assemblies = assemblies.ToArray();

            if (assemblies.Any(a => a is null))
            {
                throw new ArgumentNullException(
                    "The elements of the supplied collection should not be null.",
                    nameof(assemblies));
            }

            var packageTypes = (
                from assembly in assemblies
                from type in GetExportedTypesFrom(assembly)
                where typeof(IPackage).Info().IsAssignableFrom(type.Info())
                where !type.Info().IsAbstract
                where !type.Info().IsGenericTypeDefinition
                select type)
                .ToArray();

            RequiresPackageTypesHaveDefaultConstructor(packageTypes);

            return packageTypes.Select(CreatePackage).ToArray();
        }

        private static IEnumerable<Type> GetExportedTypesFrom(Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes.Select(info => info.AsType());
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
                // Assembly and it would be safe to skip this exception.
                return Enumerable.Empty<Type>();
            }
        }

        private static void RequiresPackageTypesHaveDefaultConstructor(Type[] packageTypes)
        {
            var invalidPackageType =
                packageTypes.FirstOrDefault(type => !type.HasDefaultConstructor());

            if (invalidPackageType != null)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The type {0} does not contain a default (public parameterless) constructor. " +
                        "Packages must have a default constructor.",
                        invalidPackageType.FullName));
            }
        }

        private static IPackage CreatePackage(Type packageType)
        {
            try
            {
                return (IPackage)Activator.CreateInstance(packageType);
            }
            catch (Exception ex)
            {
                string message = string.Format(
                    CultureInfo.InvariantCulture,
                    "The creation of package type {0} failed. {1}",
                    packageType.FullName,
                    ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }

        private static bool HasDefaultConstructor(this Type type) =>
            type.GetConstructors().Any(ctor => !ctor.GetParameters().Any());

        private static ConstructorInfo[] GetConstructors(this Type type) =>
            type.GetTypeInfo().DeclaredConstructors.ToArray();

        private static TypeInfo Info(this Type type) => type.GetTypeInfo();
    }
}