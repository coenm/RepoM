namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.Repository;
using RepoM.Core.Plugin.RepositoryOrdering;

internal class RangeConfig
{
    public TimeSpan MaxAge { get; set; }

    public int Score { get; set;}

    public int MaxItems { get; set; } = int.MaxValue;
}

internal class ScoreCalculatorConfig
{
    public List<RangeConfig> Ranges { get; set; } = new List<RangeConfig>();

    public int MaxScore { get; set; } = int.MaxValue;
}

internal class UsageScoreCalculator : IRepositoryScoreCalculator
{
    private readonly StatisticsService _service;
    private readonly IClock _clock;
    private readonly ScoreCalculatorConfig _config;
    private readonly ILogger _logger;
    private readonly List<RangeConfig> _ranges;

    public UsageScoreCalculator(StatisticsService service, IClock clock, ScoreCalculatorConfig config, ILogger logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ranges = config.Ranges.OrderBy(x => x.MaxAge).ToList();
    }

    public int Score(IRepository repository)
    {
        try
        {
            return Score1(repository);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "------------------- ERROR {m}", e.Message);
            return 0;
        }
    }

    public int Score1(IRepository repository)
    {
        DateTime now = _clock.Now;
        _logger.LogDebug("-- {Repository}", repository.SafePath);

        IReadOnlyRepositoryStatistics? repositoryRecording = _service.GetRepositoryRecording(repository);
        if (repositoryRecording == null)
        {
            return 0;
        }
        
        var score = 0;

        // first one
        var unused = 0;

        var previousRange = _ranges[0];
        var currentRange = _ranges[0];
        DateTime dateTime = now.Subtract(currentRange.MaxAge);
        _logger.LogDebug("Age: {age}", currentRange.MaxAge);
        _logger.LogDebug("Date: {dt}", dateTime);
        var count1 = repositoryRecording.GetRecordingCountFrom(dateTime);
        if (count1 <= currentRange.MaxItems)
        {
            score += count1 * currentRange.Score;
        }
        else
        {
            score += currentRange.MaxItems * currentRange.Score;
            unused = count1 - currentRange.MaxItems;
        }

        _logger.LogDebug("-- {count} - {score} - {unused}", count1, score, unused);

        for (int i = 1; i < _ranges.Count; i++)
        {
            currentRange = _ranges[i];
            var c = repositoryRecording.GetRecordingCount(
                now.Subtract(previousRange.MaxAge),
                now.Subtract(currentRange.MaxAge)) + unused;

            if (c <= currentRange.MaxItems)
            {
                score += c * currentRange.Score;
            }
            else
            {
                score += currentRange.MaxItems * currentRange.Score;
                unused = c - currentRange.MaxItems;
            }
            _logger.LogDebug("-- {count} - {score} - {unused}", c, score, unused);
            previousRange = currentRange;
        }

        currentRange = _ranges.Last();
        var countLast = repositoryRecording.GetRecordingCountBefore(now.Add(-1 * currentRange.MaxAge)) + unused;
        if (countLast <= currentRange.MaxItems)
        {
            score += countLast * currentRange.Score;
        }
        else
        {
            score += currentRange.MaxItems * currentRange.Score;
            unused = countLast - currentRange.MaxItems;
        }

        _logger.LogDebug("-- {count} - {score} - {unused} [max {max}]", countLast, score, unused, _config.MaxScore);

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
}