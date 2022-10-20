namespace RepoM.Api.IO.ModuleBasedRepositoryActionProvider.Data.Actions;

public class RepositoryActionPinRepositoryV1 : RepositoryAction
{
    public PinMode Mode { get; set; }

    public enum PinMode
    {
        // when deserialization fails, this is the value.
        Unknown,

        Toggle,

        Pin,

        UnPin,
    }
}