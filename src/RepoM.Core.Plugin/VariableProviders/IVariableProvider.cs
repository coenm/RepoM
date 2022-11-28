namespace RepoM.Core.Plugin.VariableProviders;

/// <summary>
/// IVariableProvider.
/// </summary>
public interface IVariableProvider
{
    /// <summary>
    /// CanProvide.
    /// </summary>
    /// <param name="key">key.</param>
    /// <returns>bool.</returns>
    bool CanProvide(string key);

    /// <summary>
    /// Provide.
    /// </summary>
    /// <param name="key">key.</param>
    /// <param name="arg">arguments.</param>
    /// <returns>variable value.</returns>
    object? Provide(string key, string? arg);
}