namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the state of a game between two players
    /// </summary>
    public record SinglePlayerGame(Guid GameId, int PlayerIndex, IReadOnlyBoard Board, BoardContent ShootingBoard)
        : ISinglePlayerGame
    {
        private readonly IList<SinglePlayerGameLogRecord> log = new List<SinglePlayerGameLogRecord>();

        /// <inheritdoc/>
        public IEnumerable<SinglePlayerGameLogRecord> Log => log.ToArray();

        /// <inheritdoc/>
        IReadOnlyBoard ISinglePlayerGame.ShootingBoard => ShootingBoard;

        /// <inheritdoc/>
        public BoardIndex? LastShot => log.LastOrDefault()?.Location;

        /// <inheritdoc/>
        public SquareContent Shoot(BoardIndex ix)
        {
            var content = Board[ix];
            ShootingBoard[ix] = content;
            if (content == SquareContent.Ship)
            {
                // We have a hit
                content = ShootingBoard[ix] = SquareContent.HitShip;

                // Check whether the hit sank the ship
                var shipResult = Board.TryFindShip(ix, out var shipRange);
                if (shipResult == ShipFindingResult.CompleteShip
                    && shipRange.All(ix => ShootingBoard[ix] == SquareContent.HitShip))
                {
                    // The hit sank the ship -> change all ship quares to SunkenShip
                    content = SquareContent.SunkenShip;
                    foreach(var shipIx in shipRange)
                    {
                        ShootingBoard[shipIx] = SquareContent.SunkenShip;
                    }
                }
            }

            log.Add(new(ix, content));
            return content;
        }

        /// <inheritdoc/>
        public SinglePlayerGameState GetGameState(params int[] ships)
        {
            if (Log.Count() >= 200) return SinglePlayerGameState.TooManyShots;
            if (ShootingBoard.HasLost(ships)) return SinglePlayerGameState.AllShipsSunken;
            return SinglePlayerGameState.InProgress;
        }
    }
}
