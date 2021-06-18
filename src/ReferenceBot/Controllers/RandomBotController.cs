using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.ApiModels_V1;

namespace ReferenceBot.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("bots/random/{count:int}/v{version:apiVersion}")]
    public class RandomBotController : ControllerBase
    {
        /// <summary>
        /// Provides basic information about this bot.
        /// </summary>
        [Description("Information")]
        [HttpGet("bot-information")]
        [MapToApiVersion("1.0")]
        public BotInformation BotInformation(int count)
        {
            return new BotInformation
            {
                Name =  $"Random bot {count}",
                Version = "1.0"
            };
        }

        [Description("Matches & Games")]
        [HttpPost("matches")]
        [MapToApiVersion("1.0")]
        public void CreateMatch(Match match)
        {
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
