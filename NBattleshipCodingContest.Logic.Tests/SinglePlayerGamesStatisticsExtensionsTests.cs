using Moq;
using System.Linq;
using Xunit;

namespace NBattleshipCodingContest.Logic.Tests
{
    public class SinglePlayerGamesStatisticsExtensionsTests
    {
        [Fact]
        public void Analyze()
        {
            var game1 = new Mock<ISinglePlayerGame>();
            game1.SetupGet(m => m.Log).Returns(Enumerable.Range(0, 2).Select(_ =>
                new SinglePlayerGameLogRecord("A1", SquareContent.Water)).ToArray());
            var game2 = new Mock<ISinglePlayerGame>();
            game2.SetupGet(m => m.Log).Returns(Enumerable.Range(0, 4).Select(_ =>
                new SinglePlayerGameLogRecord("A1", SquareContent.Water)).ToArray());

            var games = new[] { game1.Object, game2.Object };
            var (avg, stdDev) = games.Analyze();

            Assert.Equal(3d, avg);
            Assert.Equal(1d, stdDev);
        }
    }
}
