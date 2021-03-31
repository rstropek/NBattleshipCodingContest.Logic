using System;
using System.Linq;
using Xunit;

namespace NBattleshipCodingContest.Logic.Tests
{
    public class SinglePlayerGameTests
    {
        private static SinglePlayerGame CreateGame() =>
            new(Guid.Empty, 4711, new BoardContent(SquareContent.Water), new BoardContent(SquareContent.Unknown));

        [Fact]
        public void Shoot_Into_Water()
        {
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame() with { ShootingBoard = shooterBoard };

            Assert.Equal(SquareContent.Water, game.Shoot("A1"));
            Assert.Equal(SquareContent.Water, shooterBoard[new BoardIndex(0, 0)]);
        }
        
        [Fact]
        public void Shoot_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame() with
            {
                Board = board,
                ShootingBoard = shooterBoard
            };

            Assert.Equal(SquareContent.HitShip, game.Shoot("A1"));
            Assert.Equal(SquareContent.HitShip, shooterBoard[new BoardIndex(0, 0)]);
        }
        
        [Fact]
        public void Sink_Ship()
        {
            var board = new BoardContent(SquareContent.Water);
            board[0] = board[1] = SquareContent.Ship;
            var shooterBoard = new BoardContent(SquareContent.Unknown);

            var game = CreateGame() with
            {
                Board = board,
                ShootingBoard = shooterBoard
            };

            Assert.Equal(SquareContent.HitShip, game.Shoot("A1"));
            Assert.Equal(SquareContent.SunkenShip, game.Shoot("B1"));
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(0)]);
            Assert.Equal(SquareContent.SunkenShip, shooterBoard[new BoardIndex(1)]);
        }
        
        [Fact]
        public void GetWinner_InProgress()
        {
            var shootingBoard = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoard = shootingBoard };
            shootingBoard[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(SinglePlayerGameState.InProgress, game.GetGameState(1, 1));
        }

        [Fact]
        public void GetWinner_Draw_Too_Many_moves()
        {
            var shootingBoard = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoard = shootingBoard };
            for (var i = 0; i < 200; i++)
            {
                game.Shoot(new BoardIndex(0));
            }

            Assert.Equal(SinglePlayerGameState.TooManyShots, game.GetGameState(1));
        }

        [Fact]
        public void GetWinner()
        {
            var shooterBoard = new BoardContent(SquareContent.Unknown);
            var game = CreateGame() with { ShootingBoard = shooterBoard };
            shooterBoard[new BoardIndex(0, 0)] = SquareContent.HitShip;
            Assert.Equal(SinglePlayerGameState.AllShipsSunken, game.GetGameState(1));
        }

        [Fact]
        public void Log()
        {
            var game = CreateGame();
            game.Shoot("A1");
            game.Shoot("A1");

            Assert.Equal(2, game.Log.Count());
            Assert.Equal(new BoardIndex(), game.Log.First().Location);
        }

        [Fact]
        public void LastShot()
        {
            var game = CreateGame();

            game.Shoot("A1");
            Assert.Equal("A1", game.LastShot);

            game.Shoot("B1");
            Assert.Equal("B1", game.LastShot);
        }
    }
}
