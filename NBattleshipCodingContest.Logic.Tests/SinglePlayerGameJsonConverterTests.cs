using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace NBattleshipCodingContest.Logic.Tests
{
    public class SinglePlayerGameJsonConverterTests
    {
        [Fact]
        public void SerializeDeserialize()
        {
            var board = new BoardContent(SquareContent.Water);
            var shootingBoard = new BoardContent(SquareContent.Unknown);
            var spg = new SinglePlayerGame(Guid.Empty, 42, board, shootingBoard)
            {
                log = new List<SinglePlayerGameLogRecord>() { new("A1", SquareContent.HitShip), new("B1", SquareContent.HitShip) }
            };

            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new BoardContentJsonConverter());
            serializeOptions.Converters.Add(new SinglePlayerGameJsonConverter());
            var jsonString = JsonSerializer.Serialize(spg, serializeOptions);

            var spgResult = JsonSerializer.Deserialize<SinglePlayerGame>(jsonString);

            Assert.NotNull(spgResult);
            Assert.Equal(spg.GameId, spgResult!.GameId);
            Assert.Equal(spg.PlayerIndex, spgResult.PlayerIndex);
            Assert.Equal(spg.NumberOfShots, spgResult.NumberOfShots);
            Assert.Equal(spg.Board.ToShortString(), spgResult.Board.ToShortString());
            Assert.Equal(spg.ShootingBoard.ToShortString(), spgResult.ShootingBoard.ToShortString());
            Assert.Equal(spg.Log, spgResult.Log);
        }

        [Fact]
        public void DeserializeWrongType()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SinglePlayerGame>("42"));
        }

        [Fact]
        public void DeserializeInvalidPlayerIndex()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SinglePlayerGame>("{\"playerIndex\": true}"));
        }

        [Fact]
        public void DeserializeInvalidGameId()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SinglePlayerGame>("{\"gameId\": \"asdf\"}"));
        }

        [Fact]
        public void DeserializeMissingProperties()
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SinglePlayerGame>("{}"));
        }

        [Fact]
        public void DeserializeLogInvalidLength()
        {
            Assert.Throws<JsonException>(() => SinglePlayerGameJsonConverter.DeserializeLog("00"));
        }

        [Fact]
        public void DeserializeLogEmpty()
        {
            var result = SinglePlayerGameJsonConverter.DeserializeLog(string.Empty);
            Assert.Empty(result);
        }

        [Fact]
        public void DeserializeLog()
        {
            var result = SinglePlayerGameJsonConverter.DeserializeLog("00 ");
            Assert.Single(result);
            Assert.Equal(new BoardIndex("A1"), result[0].Location);
            Assert.Equal(SquareContent.Unknown, result[0].ShotResult);
        }

        [Fact]
        public void SerializeLog()
        {
            var result = SinglePlayerGameJsonConverter.SerializeLog(new List<SinglePlayerGameLogRecord>()
            {
                new("A1", SquareContent.HitShip),
                new("B1", SquareContent.Water),
            });
            Assert.Equal("00H10W", result);
        }
    }
}
