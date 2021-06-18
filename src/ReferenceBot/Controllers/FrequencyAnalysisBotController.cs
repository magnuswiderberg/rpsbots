using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.ApiModels_V1;

namespace ReferenceBot.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("bots/fa/v{version:apiVersion}")]
    public class FrequencyAnalysisBotController : ControllerBase
    {
        [Description("Information")]
        [HttpGet("bot-information")]
        [MapToApiVersion("1.0")]
        public BotInformation BotInformation()
        {
            return new BotInformation
            {
                Name = "Frequency Analysis Bot",
                Version = "1.0"
            };
        }

        [Description("Matches & Games")]
        [HttpPost("matches")]
        [MapToApiVersion("1.0")]
        public void CreateMatch(Match match)
        {
            if (string.IsNullOrWhiteSpace(match?.Id)) throw new ArgumentNullException(nameof(Match.Id));
            if (match.Competitors == null || match.Competitors.Count == 0) throw new ArgumentNullException(nameof(Match.Competitors));
            var rules = match.Rules;
            if (rules == null || rules.BestOf < 1 || rules.Games < 1 || rules.SameOutcomeLimit < 0) throw new ArgumentException(nameof(Match.Competitors));
        }

        [Description("Matches & Games")]
        [HttpPost("matches/{matchId}/games")]
        [MapToApiVersion("1.0")]
        public void CreateGame(string matchId, Game game)
        {
        }

        [Description("Matches & Games")]
        [HttpPost("matches/{matchId}/games/{gameId}/throws")]
        [MapToApiVersion("1.0")]
        public HandShape MakeThrow(string matchId, string gameId, Throw @throw)
        {
            return new HandShape
            {
                Shape = (Shape) new Random().Next(3)
            };
        }

        [Description("Feedback")]
        [HttpPost("matches/{matchId}/games/{gameId}/throws/{throwId}/feedbacks")]
        [MapToApiVersion("1.0")]
        public void ThrowFeedback(string matchId, string gameId, string throwId, ThrowFeedback feedback)
        {
        }

        [Description("Feedback")]
        [HttpPost("matches/{matchId}/games/{gameId}/feedbacks")]
        [MapToApiVersion("1.0")]
        public void GameFeedback(string matchId, string gameId, GameFeedback feedback)
        {
        }

        [Description("Feedback")]
        [HttpPost("matches/{matchId}/feedbacks")]
        [MapToApiVersion("1.0")]
        public void MatchFeedback(string matchId, MatchFeedback feedback)
        {
        }
    }
}
