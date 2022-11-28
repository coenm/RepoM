namespace RepoM.Core.Plugin.VariableProviders;

/// <summary>
/// Typed IVariableProvider.
/// </summary>
/// <typeparam name="T">Context type.</typeparam>
public interface IVariableProvider<in T> : IVariableProvider
{
    /// <summary>
    /// Provide.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="key">key.</param>
    /// <param name="arg">arguments.</param>
    /// <returns>variable value.</returns>
    object? Provide(T context, string key, string? arg);
}