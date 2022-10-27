namespace RepoM.Api.IO.Variables;

using System.Dynamic;
using ExpressionStringEvaluator.Methods;

public class CombinedTypeContainerExtended
{
    public CombinedTypeContainerExtended(CombinedTypeContainer combinedTypeContainer)
    {
        CombinedTypeContainer = combinedTypeContainer;
    }

    public CombinedTypeContainerExtended(ExpandoObject expandoObject)
    {
        ExpandoObject = expandoObject;
    }

    public CombinedTypeContainer? CombinedTypeContainer { get; private set; }


    /// <summary>
    /// Gets NullInstance.
    /// </summary>
    public static CombinedTypeContainerExtended NullInstance { get; } = new CombinedTypeContainerExtended(CombinedTypeContainer.NullInstance);

    /// <summary>
    /// Gets TrueInstance.
    /// </summary>
    public static CombinedTypeContainerExtended TrueInstance { get; } = new CombinedTypeContainerExtended(CombinedTypeContainer.TrueInstance);

    /// <summary>
    /// Gets FalseInstance.
    /// </summary>
    public static CombinedTypeContainerExtended FalseInstance { get; } = new CombinedTypeContainerExtended(CombinedTypeContainer.FalseInstance);


    public ExpandoObject? ExpandoObject { get; private set; }


}