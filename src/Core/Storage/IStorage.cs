using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using SharedKernel.ApiModels_V1;

namespace Core.Storage
{
    public interface IStorage
    {
        Task<MatchRecord> CreateMatchRecordAsync(List<Bot> competitors, Rules rules);
        Task<MatchRecord> GetMatchRecordAsync(Guid id);
        Task<GameRecord> CreateGameRecordAsync(MatchRecord match, Bot opponent1, Bot opponent2);
        Task<ThrowRecord> CreateThrowAsync(MatchRecord matchRecord, GameRecord gameRecord, int bot1Points, int bot2Points);
        Task UpdateMatch(MatchRecord match);
    }
}
