using System;
using System.Collections.Generic;
using System.Linq;

namespace NBattleshipCodingContest.Logic
{
    public record GameStatistics(double Average, double StdDev);

    public static class SinglePlayerGamesStatisticsExtensions
    {
        public static GameStatistics Analyze(this IReadOnlyList<ISinglePlayerGame> games)
        {
            var numberOfShots = games.Select(g => (double)g.Log.Count).ToArray();
            if (numberOfShots.Length == 0) return new GameStatistics(0d, 0d);

            double stdDev = 0d;
            double average = numberOfShots.Average();
            if (numberOfShots.Length > 1)
            {
                var sum = numberOfShots.Sum(d => (d - average) * (d - average));
                stdDev = Math.Sqrt(sum / numberOfShots.Length);
            }

            return new GameStatistics(average, stdDev);
        }
    }
}
