namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Represents the state of a game between two players
    /// </summary>
    [JsonConverter(typeof(SinglePlayerGameJsonConverter))]
    public record SinglePlayerGame(Guid GameId, int PlayerIndex, IReadOnlyBoard Board, BoardContent ShootingBoard)
        : ISinglePlayerGame
    {
        internal IList<SinglePlayerGameLogRecord> log { get; init; } = new List<SinglePlayerGameLogRecord>();

        /// <inheritdoc/>
        public IReadOnlyList<SinglePlayerGameLogRecord> Log => log.ToArray();

        /// <inheritdoc/>
        public int NumberOfShots => log.Count;

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
            if (NumberOfShots > 100) return SinglePlayerGameState.TooManyShots;
            if (ShootingBoard.HasLost(ships)) return SinglePlayerGameState.AllShipsSunken;
            return SinglePlayerGameState.InProgress;
        }
    }

    /// <summary>
    /// Factory for <see cref="SinglePlayerGame"/> instances.
    /// </summary>
    public interface ISinglePlayerGameFactory
    {
        /// <summary>
        /// Create a <see cref="SinglePlayerGame"/> instance.
        /// </summary>
        /// <param name="playerIndex">Index of player</param>
        /// <returns>
        /// New game.
        /// </returns>
        ISinglePlayerGame Create(int playerIndex);
    }

    /// <summary>
    /// Factory for <see cref="SinglePlayerGame"/> instances.
    /// </summary>
    /// <remarks>
    /// The reason for this factory is that it needs a <see cref="IBoardFiller"/>
    /// from dependency injection.
    /// </remarks>
    public class SinglePlayerGameFactory : ISinglePlayerGameFactory
    {
        private readonly IBoardFiller filler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SinglePlayerGameFactory"/> type.
        /// </summary>
        /// <param name="filler">Filler used to fill the game board.</param>
        public SinglePlayerGameFactory(IBoardFiller filler)
        {
            this.filler = filler;
        }

        /// <inheritdoc/>
        public ISinglePlayerGame Create(int playerIndex)
        {
            var board = new BattleshipBoard();
            filler.Fill(BattleshipBoard.Ships, board);

            return new SinglePlayerGame(Guid.NewGuid(), playerIndex, board, new BoardContent(SquareContent.Unknown));
        }
    }

}
