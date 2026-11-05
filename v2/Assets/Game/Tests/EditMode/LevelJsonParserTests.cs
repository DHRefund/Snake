using NUnit.Framework;
using SnakeApple.Domain.Board;
using SnakeApple.Infrastructure.Levels;

namespace SnakeApple.Tests.EditMode
{
    public sealed class LevelJsonParserTests
    {
        [Test]
        public void Parse_ConvertsTopDownRowsAndTracesSnakeHeadToTail()
        {
            const string json = @"{
                ""schemaVersion"": 1,
                ""id"": ""test-001"",
                ""size"": { ""width"": 5, ""height"": 4 },
                ""tiles"": [
                    ""#####"",
                    ""#a.E#"",
                    ""#toH#"",
                    ""#####""
                ]
            }";

            var level = new LevelJsonParser().Parse(json);

            Assert.That(level.SnakeSegments[0], Is.EqualTo(new GridPos(3, 1)));
            Assert.That(level.SnakeSegments[2], Is.EqualTo(new GridPos(1, 1)));
            Assert.That(level.ExitPosition, Is.EqualTo(new GridPos(3, 2)));
            Assert.That(level.Collectibles.ContainsKey(new GridPos(1, 2)), Is.True);
        }

        [Test]
        public void Parse_RejectsUnknownSymbolWithCellContext()
        {
            const string json = @"{
                ""schemaVersion"": 1,
                ""id"": ""bad"",
                ""size"": { ""width"": 3, ""height"": 2 },
                ""tiles"": [""HE?"", ""...""]
            }";

            var exception = Assert.Throws<System.FormatException>(() => new LevelJsonParser().Parse(json));
            Assert.That(exception.Message, Does.Contain("Unknown symbol '?'"));
        }
    }
}

