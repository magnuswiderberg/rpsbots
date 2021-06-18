using System.Threading.Tasks;
using SharedKernel.ApiModels_V1;

namespace Core.Restclients
{
    public interface IRestClient
    {
        Task<BotInformation> GetBotInformationAsync(Bot bot);
        Task CreateMatchAsync(Bot bot, Match match);
        Task CreateGameAsync(Bot bot, Match match, Game game);
        Task<HandShape> MakeMoveAsync(Bot bot, Match match, Game game, Throw move);

        Task ThrowFeedbackAsync(Bot bot, string matchId, string gameId, string throwId, ThrowFeedback feedback);
        Task GameFeedbackAsync(Bot bot, string matchId, string gameId, GameFeedback feedback);
        Task MatchFeedbackAsync(Bot bot, string matchId, MatchFeedback feedback);
    }
}