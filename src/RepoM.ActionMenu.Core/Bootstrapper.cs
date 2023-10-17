namespace RepoM.ActionMenu.Core;

using System;
using System.Reflection;
using RepoM.ActionMenu.Core.Misc;
using RepoM.ActionMenu.Core.Model;
using RepoM.ActionMenu.Core.PublicApi;
using RepoM.ActionMenu.Core.Services;
using RepoM.ActionMenu.Core.Yaml.Serialization;
using SimpleInjector;

public sealed class Bootstrapper
{
    private static readonly Assembly _thisAssembly = typeof(Bootstrapper).Assembly;

    public static void RegisterServices(Container container)
    {
        if (container == null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        RegisterPrivateTypes(container);
        RegisterPublicTypes(container);
    }

    private static void RegisterPublicTypes(Container container)
    {
        container.Register<IUserInterfaceActionMenuFactory, UserInterfaceActionMenuFactory>();
    }

    private static void RegisterPrivateTypes(Container container)
    {
        container.RegisterSingleton<ITemplateParser, FixedTemplateParser>();
        container.RegisterSingleton<IActionMenuDeserializer, ActionMenuDeserializer>();
    }
}