namespace RepoM.Plugin.Statistics.Ordering;

using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using RepoM.Core.Plugin.Common;
using RepoM.Core.Plugin.RepositoryOrdering;

public sealed class UsageScorerFactory : IRepositoryScoreCalculatorFactory<UsageScorerConfigurationV1>
{
    private readonly StatisticsService _service;
    private readonly IClock _clock;
    private readonly ILoggerFactory _loggerFactory;

    public UsageScorerFactory(StatisticsService service, IClock clock, ILoggerFactory loggerFactory)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public IRepositoryScoreCalculator Create(UsageScorerConfigurationV1 config)
    {
        var scoreCalculatorConfig = new ScoreCalculatorConfig
            {
                MaxScore = config.MaxScore ?? int.MaxValue,
                Ranges = config.Windows
                               .Select(x => new RangeConfig
                                   {
                                       Score = x.Weight,
                                       MaxItems = x.MaxItems,
                                       MaxAge = x.Until,
                                   })
                               .ToList(),
            };


        return new UsageScoreCalculator(
            _service,
            _clock,
            scoreCalculatorConfig,
            _loggerFactory.CreateLogger(typeof(UsageScoreCalculator)));
    }
}