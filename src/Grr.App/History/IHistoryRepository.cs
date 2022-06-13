namespace Grr.App.History;

public interface IHistoryRepository
{
    State Load();

    void Save(State state);
}