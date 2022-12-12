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

    public UsageScorerFactory(StatisticsService service, IClock clock)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public IRepositoryScoreCalculator Create(UsageScorerConfigurationV1 config)
    {
        var scoreCalculatorConfig = new ScoreCalculatorConfig
            {
                MaxScore = config.MaxScore ?? int.MaxValue,
                Ranges = config.Windows
                               .Select(x => new ScoreCalculatorRangeConfig
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
            scoreCalculatorConfig);
    }
}