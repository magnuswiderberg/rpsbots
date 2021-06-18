using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using SharedKernel.ApiModels_V1;

namespace Core.Logic
{
    public partial class GameLogic : IGameLogic
    {
        public async Task PlayMatchAsync(MatchRecord match, Action<Dictionary<string, BotMatchScore>> scoreCallback, Action completedAction)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (match.Rules.Mode != MatchMode.AllAgainstAll) throw new NotImplementedException($"Only {MatchMode.AllAgainstAll} is available");
            if (match.Scores == null) throw new ArgumentNullException(nameof(match.Scores));

            if (match.CompletedAt != default) return;

            await CreateMatchesInBotsAsync(match);

            var tasks = new List<Task>();
            for (var i = 0; i < match.Competitors.Count; i++)
            {
                for (var j = i + 1; j < match.Competitors.Count; j++)
                {
                    tasks.Add(PlayGamesAsync(match, match.Competitors[i], match.Competitors[j], scoreCallback));
                }
            }

            await Task.WhenAll(tasks);
            match.CompletedAt = DateTimeOffset.Now;
            await _storage.UpdateMatch(match);
            await MatchFeedbackAsync(match);
            completedAction();
        }

        private async Task PlayGamesAsync(MatchRecord match, Bot competitor1, Bot competitor2, Action<Dictionary<string, BotMatchScore>> scoreCallback)
        {
            var tasks = new List<Task>();
            for (var i = 0; i < match.Rules.Games; i++)
            {
                var gameRecord = await _storage.CreateGameRecordAsync(match, competitor1, competitor2);
                var createGameTask1 = _restClient.CreateGameAsync(competitor1, ToMatch(match), ToGame(gameRecord, competitor2));
                var createGameTask2 = _restClient.CreateGameAsync(competitor2, ToMatch(match), ToGame(gameRecord, competitor1));
                await Task.WhenAll(createGameTask1, createGameTask2);

                tasks.Add(PlayGameAsync(match, gameRecord, competitor1, competitor2));
                scoreCallback(match.Scores);
            }

            await Task.WhenAll(tasks);
        }

        public async Task PlayGameAsync(MatchRecord match, GameRecord game, Bot competitor1, Bot competitor2)
        {
            var gameScores = new Dictionary<string, int> { { competitor1.Id, 0 }, { competitor2.Id, 0 } };
            var throwsToWin = match.Rules.BestOf / 2 + 1;
            var throwCount = 0;
            Bot winner = null;
            while (++throwCount <= match.Rules.SameOutcomeLimit)
            {
                var throwRecord = await _storage.CreateThrowAsync(match, game, gameScores[competitor1.Id], gameScores[competitor2.Id]);
                var (shape1, shape2) = await MakeMovesAsync(match, game, throwRecord, competitor1, competitor2);
                var throwWinner = CalculateThrowWinner(competitor1, shape1, competitor2, shape2);

                await ThrowFeedback(throwCount, throwWinner, competitor1, shape1, competitor2, shape2, match, game, throwRecord);

                if (throwWinner == null) continue;
                if (++gameScores[throwWinner.Id] >= throwsToWin)
                {
                    winner = throwWinner;
                    break;
                }
            }

            UpdateMatchScores(match, competitor1, competitor2, gameScores, throwsToWin);
            await GameFeedbackAsync(winner, gameScores, competitor1, competitor2, match, game);
        }

        private static void UpdateMatchScores(MatchRecord match, Bot competitor1, Bot competitor2, Dictionary<string, int> gameScores, int throwsToWin)
        {
            if (gameScores[competitor1.Id] >= throwsToWin)
            {
                match.Scores[competitor1.Id].Wins++;
                match.Scores[competitor2.Id].Losses++;
            }
            else if (gameScores[competitor2.Id] >= throwsToWin)
            {
                match.Scores[competitor2.Id].Wins++;
                match.Scores[competitor1.Id].Losses++;
            }
            else
            {
                match.Scores[competitor1.Id].Draws++;
                match.Scores[competitor2.Id].Draws++;
            }
        }

        private async Task<(Shape, Shape)> MakeMovesAsync(MatchRecord match, GameRecord game, ThrowRecord @throw, Bot competitor1, Bot competitor2)
        {
            var hand1Task = _restClient.MakeMoveAsync(competitor1, ToMatch(match), ToGame(game, competitor2), ToThrow(@throw));
            var hand2Task = _restClient.MakeMoveAsync(competitor2, ToMatch(match), ToGame(game, competitor1), ToThrow(@throw));
            await Task.WhenAll(hand1Task, hand2Task);

            return ((await hand1Task).Shape, (await hand2Task).Shape);
        }

        private async Task ThrowFeedback(int throwCount, Bot throwWinner, Bot competitor1, Shape shape1, Bot competitor2, Shape shape2, MatchRecord match, GameRecord game, ThrowRecord @throw)
        {
            var feedback = new ThrowFeedback
            {
                TotalThrows = throwCount,
                Winner = throwWinner?.Id,
                Result = new List<BotThrow>
                {
                    new BotThrow { Id = competitor1.Id, Shape = shape1 },
                    new BotThrow { Id = competitor2.Id, Shape = shape2 }
                }
            };
            var feedbackTask1 = _restClient.ThrowFeedbackAsync(competitor1, match.Id.ToString(), game.Id.ToString(), @throw.Id.ToString(), feedback);
            var feedbackTask2 = _restClient.ThrowFeedbackAsync(competitor2, match.Id.ToString(), game.Id.ToString(), @throw.Id.ToString(), feedback);

            await Task.WhenAll(feedbackTask1, feedbackTask2);
        }

        private async Task GameFeedbackAsync(Bot winner, Dictionary<string, int> scores, Bot competitor1, Bot competitor2, MatchRecord match, GameRecord game)
        {
            var feedback = new GameFeedback
            {
                Winner = winner?.Id,
                Scores = scores.Select(x => new BotGameScore { Id = x.Key, Points = x.Value }).ToList()
            };
            var feedbackTask1 = _restClient.GameFeedbackAsync(competitor1, match.Id.ToString(), game.Id.ToString(), feedback);
            var feedbackTask2 = _restClient.GameFeedbackAsync(competitor2, match.Id.ToString(), game.Id.ToString(), feedback);

            await Task.WhenAll(feedbackTask1, feedbackTask2);
        }

        private async Task MatchFeedbackAsync(MatchRecord match)
        {
            string winnerId;
            var maxWins = match.Scores.Max(x => x.Value.Wins);
            var botsWithMaxWins = match.Scores.Values.Where(x => x.Wins == maxWins).ToList();
            if (botsWithMaxWins.Count == 1)
            {
                // Single bot with highest score wins
                winnerId = botsWithMaxWins.First().Id;
            }
            else
            {
                var minLosses = match.Scores.Min(x => x.Value.Losses);
                var botsWithMinlosses = match.Scores.Values.Where(x => x.Losses== minLosses).ToList();
                if (botsWithMinlosses.Count == 1)
                {
                    // If multiple with highest score, bot with fewest losses wins
                    winnerId = botsWithMinlosses.First().Id;
                }
                else
                {
                    // Can't determine winner
                    winnerId = null;
                }
            }

            var feedback = new MatchFeedback
            {
                Winner = winnerId,
                Scores = match.Scores.Values.ToList()
            };
            var tasks = match.Competitors.Select(competitor => _restClient.MatchFeedbackAsync(competitor, match.Id.ToString(), feedback));
            await Task.WhenAll(tasks);
        }

        private Bot CalculateThrowWinner(Bot competitor1, Shape shape1, Bot competitor2, Shape shape2)
        {
            switch (shape1)
            {
                case Shape.paper:
                    {
                        switch (shape2)
                        {
                            case Shape.paper: return null;
                            case Shape.rock: return competitor1;
                            case Shape.scissors: return competitor2;
                            default: throw new InvalidOperationException();
                        }
                    }
                case Shape.rock:
                    {
                        switch (shape2)
                        {
                            case Shape.paper: return competitor2;
                            case Shape.rock: return null;
                            case Shape.scissors: return competitor1;
                            default: throw new InvalidOperationException();
                        }
                    }
                case Shape.scissors:
                    {
                        switch (shape2)
                        {
                            case Shape.paper: return competitor1;
                            case Shape.rock: return competitor2;
                            case Shape.scissors: return null;
                            default: throw new InvalidOperationException();
                        }
                    }
                default: throw new InvalidOperationException();
            }
        }

        private async Task CreateMatchesInBotsAsync(MatchRecord match)
        {
            foreach (var bot in match.Competitors)
            {
                await _restClient.CreateMatchAsync(bot, ToMatch(match));
            }
        }

        private static Match ToMatch(MatchRecord matchRecord)
        {
            return new Match
            {
                Id = matchRecord.Id.ToString(),
                Rules = matchRecord.Rules,
                Competitors = matchRecord.Competitors
            };
        }

        private static Game ToGame(GameRecord gameRecord, Bot opponent)
        {
            return new Game
            {
                Id = gameRecord.Id.ToString(),
                Opponent = opponent
            };
        }

        private static Throw ToThrow(ThrowRecord throwRecord)
        {
            return new Throw
            {
                Id = throwRecord.Id.ToString(),
                Scores = throwRecord.Scores
            };
        }
    }
}