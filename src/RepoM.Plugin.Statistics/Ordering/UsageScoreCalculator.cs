namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using System.Linq;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class UsageScoreCalculator : IRepositoryScoreCalculator
{
    private readonly IStatisticsService _service;
    private readonly IClock _clock;
    private readonly ScoreCalculatorConfig _config;
    private readonly List<ScoreCalculatorRangeConfig> _ranges;

    public UsageScoreCalculator(IStatisticsService service, IClock clock, ScoreCalculatorConfig config)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _ranges = [.. config.Ranges.OrderBy(x => x.MaxAge), ];
    }

    public int Score(IRepository repository)
    {
        if (_ranges.Count == 0)
        {
            return 0;
        }

        if (_config.MaxScore == 0)
        {
            return 0;
        }

        DateTime now = _clock.Now;

        IReadOnlyRepositoryStatistics? repositoryRecording = _service.GetRepositoryRecording(repository);
        if (repositoryRecording == null)
        {
            return 0;
        }
        
        var score = CalculateScore(now, repositoryRecording);

        if (score < 0)
        {
            return 0;
        }

        if (score > _config.MaxScore)
        {
            return _config.MaxScore;
        }

        return score;
    }

    private int CalculateScore(DateTime now, IReadOnlyRepositoryStatistics repositoryRecording)
    {
        var score = 0;
        var unused = 0;
        var currentCount = 0;

        ScoreCalculatorRangeConfig previousRange = _ranges[0];
        ScoreCalculatorRangeConfig currentRange = _ranges[0];

        DateTime dateTime = now.Subtract(currentRange.MaxAge);

        currentCount = repositoryRecording.GetRecordingCountFrom(dateTime);
        if (currentCount <= currentRange.MaxItems)
        {
            score += currentCount * currentRange.Score;
            unused = 0;
        }
        else
        {
            score += currentRange.MaxItems * currentRange.Score;
            unused = currentCount - currentRange.MaxItems;
        }

        for (var i = 1; i < _ranges.Count; i++)
        {
            currentRange = _ranges[i];
            currentCount = repositoryRecording.GetRecordingCount(
                now.Subtract(currentRange.MaxAge),
                now.Subtract(previousRange.MaxAge)) + unused;

            if (currentCount <= currentRange.MaxItems)
            {
                score += currentCount * currentRange.Score;
                unused = 0;
            }
            else
            {
                score += currentRange.MaxItems * currentRange.Score;
                unused = currentCount - currentRange.MaxItems;
            }

            previousRange = currentRange;
        }

        return score;
    }
}