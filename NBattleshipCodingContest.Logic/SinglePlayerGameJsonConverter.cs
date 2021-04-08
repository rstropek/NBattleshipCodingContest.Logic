namespace NBattleshipCodingContest.Logic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Custom JSON converter for <see cref="SinglePlayerGame"/>
    /// </summary>
    public class SinglePlayerGameJsonConverter : JsonConverter<SinglePlayerGame>
    {
        private static BoardContent DeserializeBoardContent(JsonConverter<BoardContent> converter, ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            var board = converter.Read(ref reader, typeof(BoardContent), options);
            if (board == null) throw new JsonException($"Could not deserialize board content.");
            return board;
        }

        internal static string SerializeLog(IEnumerable<SinglePlayerGameLogRecord> log)
        {
            return string.Create(log.Count() * 3, log, (buf, log) =>
            {
                foreach (var item in log)
                {
                    item.Location.Column.TryFormat(buf, out var _);
                    item.Location.Row.TryFormat(buf[1..], out var _);
                    buf[2] = BoardContentJsonConverter.SquareContentToChar(item.ShotResult);
                    buf = buf[3..];
                }
            });
        }

        internal static List<SinglePlayerGameLogRecord> DeserializeLog(ReadOnlySpan<char> logString)
        {
            if (logString.Length % 3 != 0) throw new JsonException("Log string has invalid length.");

            var result = new List<SinglePlayerGameLogRecord>(logString.Length / 3);
            while (logString.Length != 0)
            {
                var column = int.Parse(logString[0..1]);
                var row = int.Parse(logString[1..2]);
                var shotResult = BoardContentJsonConverter.CharToSquareContent(logString[2]);
                result.Add(new(new BoardIndex(column, row), shotResult));
                logString = logString[3..];
            }

            return result;
        }

        /// <inheritdoc/>
        public override SinglePlayerGame Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var boardContentConverter = options.GetConverter(typeof(BoardContent)) as JsonConverter<BoardContent>;
            if (boardContentConverter == null)
            {
                throw new JsonException($"Missing converter for type {nameof(BoardContent)}. Did you forget to register {nameof(BoardContentJsonConverter)}?");
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"Expected start of object, got {reader.TokenType}");
            }

            int? playerIndex = null;
            BoardContent? board = null;
            BoardContent? shootingBoard = null;
            List<SinglePlayerGameLogRecord>? log = null;
            Guid? gameId = null;

            string? currentProperty = null;
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                if (reader.TokenType == JsonTokenType.Comment) continue;
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    currentProperty = reader.GetString();
                }
                else if (reader.TokenType is JsonTokenType.String or JsonTokenType.Number && currentProperty != null)
                {
                    switch (currentProperty)
                    {
                        case "playerIndex":
                            if (!reader.TryGetInt32(out var pi)) throw new JsonException("Invalid value in player index");
                            playerIndex = pi;
                            break;
                        case "gameId":
                            if (!reader.TryGetGuid(out var gi)) throw new JsonException("Invalid value in game ID");
                            gameId = gi;
                            break;
                        case "board":
                            board = DeserializeBoardContent(boardContentConverter, ref reader, options);
                            break;
                        case "shootingBoard":
                            shootingBoard = DeserializeBoardContent(boardContentConverter, ref reader, options);
                            break;
                        case "log":
                            log = DeserializeLog(reader.GetString());
                            break;
                        default:
                            break;
                    }

                    currentProperty = null;
                }
            }

            if (!playerIndex.HasValue || !gameId.HasValue || board == null || shootingBoard == null || log == null)
            {
                throw new JsonException("Incomplete JSON for SinglePlayerGame");
            }

            return new SinglePlayerGame(gameId.Value, playerIndex.Value, board, shootingBoard)
            {
                log = log
            };
        }

        /// <inheritdoc/>
        public override void Write(Utf8JsonWriter writer, SinglePlayerGame value, JsonSerializerOptions _)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("playerIndex");
            writer.WriteNumberValue(value.PlayerIndex);

            writer.WritePropertyName("gameId");
            writer.WriteStringValue(value.GameId);

            writer.WritePropertyName("board");
            writer.WriteStringValue(value.Board.ToShortString());

            writer.WritePropertyName("shootingBoard");
            writer.WriteStringValue(value.ShootingBoard.ToShortString());

            writer.WritePropertyName("log");
            string logString = SerializeLog(value.Log);
            writer.WriteStringValue(logString);

            writer.WriteEndObject();
        }
    }
}
