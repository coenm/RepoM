namespace RepoM.Core.Plugin.RepositoryActions.Commands;

public sealed class PinRepositoryCommand : IRepositoryCommand
{
    private PinRepositoryCommand(PinRepositoryType type)
    {
        Type = type;
    }

    public PinRepositoryType Type { get; }

    public static PinRepositoryCommand Pin { get; } = new(PinRepositoryType.Pin);

    public static PinRepositoryCommand Toggle { get; } = new(PinRepositoryType.Toggle);

    public static PinRepositoryCommand UnPin { get; } = new(PinRepositoryType.UnPin);

    public enum PinRepositoryType
    {
        Pin,

        Toggle,

        UnPin,
    }
}