using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharedKernel.ApiModels_V1;

namespace Core.Restclients
{
    public class RestClient : IRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RestClient> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public RestClient(HttpClient httpClient, ILogger<RestClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<BotInformation> GetBotInformationAsync(Bot bot)
        {
            var url = $"{bot.Url}/v1/bot-information";
            try
            {
                var info = await _httpClient.GetFromJsonAsync(url, typeof(BotInformation));
                return (BotInformation)info;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to get bot information for bot '{bot.Name}' on '{url}': {e.Message}");
                throw;
            }
        }

        public async Task CreateMatchAsync(Bot bot, Match match)
        {
            var url = $"{bot.Url}/v1/matches";
            try
            {
                await _httpClient.PostAsJsonAsync(url, match);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to create match '{match.Id}' for '{bot.Name}' on '{url}': {e.Message}");
                throw;
            }
        }

        public async Task CreateGameAsync(Bot bot, Match match, Game game)
        {
            var url = $"{bot.Url}/v1/matches/{match.Id}/games";
            try
            {
                await _httpClient.PostAsJsonAsync(url, game);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to create game '{game.Id}' in match '{match.Id}' for '{bot.Name}' on '{url}': {e.Message}");
                throw;
            }
        }

        public async Task<HandShape> MakeMoveAsync(Bot bot, Match match, Game game, Throw move)
        {
            var url = $"{bot.Url}/v1/matches/{match.Id}/games/{game.Id}/throws";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, move);
                var responseString = await response.Content.ReadAsStringAsync();
                var hand = JsonSerializer.Deserialize<HandShape>(responseString, _jsonSerializerOptions);
                return hand;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to create throw '{move.Id}' in game '{game.Id}' in match '{match.Id}' for '{bot.Name}' on '{url}': {e.Message}", e);
                throw;
            }
        }

        public async Task ThrowFeedbackAsync(Bot bot, string matchId, string gameId, string throwId, ThrowFeedback feedback)
        {
            var url = $"{bot.Url}/v1/matches/{matchId}/games/{gameId}/throws/{throwId}/feedbacks";
            try
            {
                await _httpClient.PostAsJsonAsync(url, feedback);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to give feeback on throw '{throwId}' in game '{gameId}' in match '{matchId}' for '{bot.Name}' on '{url}': {e.Message}", e);
                throw;
            }
        }

        public async Task GameFeedbackAsync(Bot bot, string matchId, string gameId, GameFeedback feedback)
        {
            var url = $"{bot.Url}/v1/matches/{matchId}/games/{gameId}/feedbacks";
            try
            {
                await _httpClient.PostAsJsonAsync(url, feedback);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to give feeback on game '{gameId}' in match '{matchId}' for '{bot.Name}' on '{url}': {e.Message}", e);
                throw;
            }
        }

        public async Task MatchFeedbackAsync(Bot bot, string matchId, MatchFeedback feedback)
        {
            var url = $"{bot.Url}/v1/matches/{matchId}/feedbacks";
            try
            {
                await _httpClient.PostAsJsonAsync(url, feedback);
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to give feeback on match '{matchId}' for '{bot.Name}' on '{url}': {e.Message}", e);
                throw;
            }
        }
    }
}