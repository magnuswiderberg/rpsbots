using Core.Restclients;
using Core.Storage;
using Microsoft.Extensions.Logging;

namespace Core.Logic
{
    public partial class GameLogic : IGameLogic
    {
        private readonly ILogger<GameLogic> _logger;
        private readonly IStorage _storage;
        private readonly IRestClient _restClient;

        public GameLogic(IStorage storage, IRestClient restClient, ILogger<GameLogic> logger)
        {
            _storage = storage;
            _restClient = restClient;
            _logger = logger;
        }
    }
}
