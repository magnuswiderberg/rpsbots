using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using SharedKernel.ApiModels_V1;

namespace Core.Logic
{
    public interface IGameLogic
    {
        Task<MatchRecord> CreateMatchAsync(List<Bot> competitors, Rules defaultRules);
        Task<MatchRecord> GetMatchAsync(string id);
        Task<GameRecord> CreateGameAsync(MatchRecord match, Bot opponent1, Bot opponent2); // TODO: Remove
        Task PlayMatchAsync(MatchRecord matchRecord, Action<Dictionary<string, BotMatchScore>> scoreCallback, Action completedAction);
    }
}