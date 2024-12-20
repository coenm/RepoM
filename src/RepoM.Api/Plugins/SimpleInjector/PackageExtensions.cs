// Copyright (c) Simple Injector Contributors. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for license information.

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace RepoM.Api.Plugins.SimpleInjector;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RepoM.Core.Plugin;
using Container = global::SimpleInjector.Container;

/// <summary>
/// Extension methods for working with packages.
/// </summary>
public static class PackageExtensions
{
    public static Task RegisterPackagesAsync(this Container container, IEnumerable<Assembly> assemblies, Func<string, IPackageConfiguration> packageConfigurationFactoryMethod)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentNullException.ThrowIfNull(packageConfigurationFactoryMethod);

        return RegisterPackagesInnerAsync(container, assemblies, packageConfigurationFactoryMethod);
    }

    /// <summary>
    /// Loads all <see cref="IPackage"/> implementations from the given set of
    /// <paramref name="assemblies"/> and returns a list of created package instances.
    /// </summary>
    /// <param name="assemblies">The assemblies that will be searched for packages.</param>
    /// <returns>Returns a list of created packages.</returns>
    private static IPackage[] GetPackagesToRegister(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        assemblies = assemblies.ToArray();

        if (assemblies.Any(a => a is null))
        {
            throw new ArgumentNullException(nameof(assemblies), "The elements of the supplied collection should not be null.");
        }

        Type[] packageTypes = (
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

    private static async Task RegisterPackagesInnerAsync(Container container, IEnumerable<Assembly> assemblies, Func<string, IPackageConfiguration> packageConfigurationFactoryMethod)
    {
        foreach (Assembly assembly in assemblies)
        {
            var assemblyName = assembly.GetName().Name ?? string.Empty;

            foreach (IPackage packageWithConfiguration in GetPackagesToRegister([assembly,]))
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
        }
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
            return [];
        }
    }

    private static void RequiresPackageTypesHaveDefaultConstructor(Type[] packageTypes)
    {
        Type? invalidPackageType = Array.Find(packageTypes, type => !type.HasDefaultConstructor());

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
            return (IPackage)Activator.CreateInstance(packageType)!;
        }
        catch (Exception ex)
        {
            var message = string.Format(
                CultureInfo.InvariantCulture,
                "The creation of package type {0} failed. {1}",
                packageType.FullName,
                ex.Message);

            throw new InvalidOperationException(message, ex);
        }
    }

    private static bool HasDefaultConstructor(this Type type)
    {
        return Array.Exists(type.GetConstructors(), ctor => ctor.GetParameters().Length == 0);
    }

    private static TypeInfo Info(this Type type)
    {
        return type.GetTypeInfo();
    }
}