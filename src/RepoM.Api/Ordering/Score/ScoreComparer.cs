namespace RepoM.Api.Ordering.Score;

using System;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

public class ScoreComparer : IRepositoryComparer
{
    private readonly IRepositoryScoreCalculator _calculator;

    public ScoreComparer(IRepositoryScoreCalculator calculator)
    {
        _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
    }

    public int Compare(IRepository x, IRepository y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (ReferenceEquals(null, y))
        {
            return 1;
        }

        if (ReferenceEquals(null, x))
        {
            return -1;
        }

        var result = _calculator.Score(y) - _calculator.Score(x);
        return result;
    }
}