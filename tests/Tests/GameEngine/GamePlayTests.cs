using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Models;
using Moq;
using SharedKernel.ApiModels_V1;
using Xunit;
using Xunit.Abstractions;
using Match = SharedKernel.ApiModels_V1.Match;

namespace Tests.GameEngine
{
    public class GamePlayTests : AbstractBaseTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GamePlayTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            RestClientMock
                .Setup(x => x.GetBotInformationAsync(It.IsAny<Bot>()))
                .ReturnsAsync((Bot bot) => new BotInformation { Name = $"test-{bot.Name}", Version = "1.0" });
            RestClientMock
                .Setup(x => x.MakeMoveAsync(It.IsAny<Bot>(), It.IsAny<Match>(), It.IsAny<Game>(), It.IsAny<Throw>()))
                .ReturnsAsync(() =>
                {
                    return new HandShape
                    {
                        Shape = new Random().Next(3) switch
                        {
                            0 => Shape.rock,
                            1 => Shape.paper,
                            2 => Shape.scissors,
                            _ => throw new InvalidOperationException()
                        }
                    };
                });
        }

        [Fact]
        public async Task Random_Bots_Can_Play_A_Match_Sucessfully()
        {
            var matchRecord = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);
            await GameLogic.PlayMatchAsync(matchRecord, scores => { }, () => { });

            foreach (var competitor in DefaultCompetitors)
            {
                var score = matchRecord.Scores[competitor.Id];
                Assert.Equal(DefaultRules.Games * (DefaultCompetitors.Count - 1), score.Wins + score.Losses + score.Draws);
            }
        }

        private async Task<(MatchRecord, GameRecord)> CreateGame()
        {
            var matchRecord = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);
            var gameRecord = await GameLogic.CreateGameAsync(matchRecord, Bot1, Bot2);
            return (matchRecord, gameRecord);
        }

        [Theory]
        [InlineData(Shape.rock, Shape.scissors)]
        [InlineData(Shape.scissors, Shape.paper)]
        [InlineData(Shape.paper, Shape.rock)]
        public async Task Rock_Beats_Scissors_Beats_Paper_Beats_Rock(Shape winningShape, Shape losingShape)
        {
            RestClientMock.Setup(x => x.MakeMoveAsync(Bot1, It.IsAny<Match>(), It.IsAny<Game>(), It.IsAny<Throw>())).ReturnsAsync(new HandShape { Shape = winningShape });
            RestClientMock.Setup(x => x.MakeMoveAsync(Bot2, It.IsAny<Match>(), It.IsAny<Game>(), It.IsAny<Throw>())).ReturnsAsync(new HandShape { Shape = losingShape });

            var (matchRecord, gameRecord) = await CreateGame();
            await GameLogic.PlayGameAsync(matchRecord, gameRecord, Bot1, Bot2);

            Assert.Equal(1, matchRecord.Scores[Bot1.Id].Wins);
            Assert.Equal(0, matchRecord.Scores[Bot1.Id].Losses);
            Assert.Equal(0, matchRecord.Scores[Bot2.Id].Wins);
            Assert.Equal(1, matchRecord.Scores[Bot2.Id].Losses);
        }

        [Theory]
        [InlineData(Shape.rock)]
        [InlineData(Shape.scissors)]
        [InlineData(Shape.paper)]
        public async Task Draw_Is_Handled(Shape shape)
        {
            RestClientMock.Setup(x => x.MakeMoveAsync(Bot1, It.IsAny<Match>(), It.IsAny<Game>(), It.IsAny<Throw>())).ReturnsAsync(new HandShape { Shape = shape });
            RestClientMock.Setup(x => x.MakeMoveAsync(Bot2, It.IsAny<Match>(), It.IsAny<Game>(), It.IsAny<Throw>())).ReturnsAsync(new HandShape { Shape = shape });

            var (matchRecord, gameRecord) = await CreateGame();
            await GameLogic.PlayGameAsync(matchRecord, gameRecord, Bot1, Bot2);

            Assert.Equal(1, matchRecord.Scores[Bot1.Id].Draws);
            Assert.Equal(0, matchRecord.Scores[Bot1.Id].Wins);
            Assert.Equal(0, matchRecord.Scores[Bot1.Id].Losses);
            Assert.Equal(1, matchRecord.Scores[Bot2.Id].Draws);
            Assert.Equal(0, matchRecord.Scores[Bot2.Id].Wins);
            Assert.Equal(0, matchRecord.Scores[Bot2.Id].Losses);
        }

        [Fact]
        public async Task All_Bots_Get_Feedback_On_A_Throw()
        {
            var (matchRecord, gameRecord) = await CreateGame();

            var resetEvent = new ManualResetEvent(false);
            var botsWithFeedback = new HashSet<Bot>();
            RestClientMock
                .Setup(x => x.ThrowFeedbackAsync(It.IsAny<Bot>(), matchRecord.Id.ToString(), gameRecord.Id.ToString(), It.IsAny<string>(), It.IsAny<ThrowFeedback>()))
                .Callback((Bot bot, string matchId, string gameId, string throwId, ThrowFeedback feedback) =>
                {
                    botsWithFeedback.Add(bot);
                    if (botsWithFeedback.Count == 2) resetEvent.Set();
                });

            await GameLogic.PlayGameAsync(matchRecord, gameRecord, Bot1, Bot2);

            Assert.True(resetEvent.WaitOne(100));
            Assert.Contains(Bot1, botsWithFeedback);
            Assert.Contains(Bot2, botsWithFeedback);
        }

        [Fact]
        public async Task All_Bots_Get_Feedback_On_A_Game()
        {
            var (matchRecord, gameRecord) = await CreateGame();

            var resetEvent = new ManualResetEvent(false);
            var botsWithFeedback = new HashSet<Bot>();
            RestClientMock
                .Setup(x => x.GameFeedbackAsync(It.IsAny<Bot>(), matchRecord.Id.ToString(), gameRecord.Id.ToString(), It.IsAny<GameFeedback>()))
                .Callback((Bot bot, string matchId, string gameId, GameFeedback feedback) =>
                {
                    botsWithFeedback.Add(bot);
                    if (botsWithFeedback.Count == 2) resetEvent.Set();
                });

            await GameLogic.PlayGameAsync(matchRecord, gameRecord, Bot1, Bot2);

            Assert.True(resetEvent.WaitOne(100));
            Assert.Contains(Bot1, botsWithFeedback);
            Assert.Contains(Bot2, botsWithFeedback);
        }

        [Fact]
        public async Task All_Bots_Get_Feedback_On_A_Match()
        {
            var matchRecord = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);

            var resetEvent = new ManualResetEvent(false);
            var botsWithFeedback = new HashSet<Bot>();
            RestClientMock
                .Setup(x => x.MatchFeedbackAsync(It.IsAny<Bot>(), matchRecord.Id.ToString(), It.IsAny<MatchFeedback>()))
                .Callback((Bot bot, string matchId, MatchFeedback feedback) =>
                {
                    botsWithFeedback.Add(bot);
                    if (botsWithFeedback.Count == DefaultCompetitors.Count) resetEvent.Set();
                });

            await GameLogic.PlayMatchAsync(matchRecord, scores => { }, () => { });

            Assert.True(resetEvent.WaitOne(100));
            Assert.True(botsWithFeedback.All(x => DefaultCompetitors.Contains(x)));
        }

        // TODO: Test that throw feedback is correct
        // TODO: Test that game feedback is correct
        // TODO: Test that match feedback is correct

        // TODO: Test that bot with most Wins wins
        // TODO: Test that bot with fewest Losses wins if equals Wins
    }
}
