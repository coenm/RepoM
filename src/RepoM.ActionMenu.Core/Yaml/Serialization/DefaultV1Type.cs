namespace RepoM.ActionMenu.Core.Yaml.Serialization;

using System.Diagnostics.CodeAnalysis;
using RepoM.ActionMenu.Core.Yaml.Model.ActionContext.SetVariable;

[SuppressMessage("Code Smell", "S2094:Classes should not be empty", Justification = "Intentionally empty. Used as fallback for deserialization.")]
internal class DefaultV1Type : ContextActionSetVariableV1
{
    // intentionally empty.
}