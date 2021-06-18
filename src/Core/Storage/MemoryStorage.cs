using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using SharedKernel.ApiModels_V1;

namespace Core.Storage
{
    public class MemoryStorage : IStorage
    {
        private readonly ConcurrentDictionary<Guid, MatchRecord> _matchRecords = new ConcurrentDictionary<Guid, MatchRecord>();
        private readonly ConcurrentDictionary<Guid, GameRecord> _gameRecords = new ConcurrentDictionary<Guid, GameRecord>();

        public Task<MatchRecord> CreateMatchRecordAsync(List<Bot> competitors, Rules rules)
        {
            var record = new MatchRecord
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                Competitors = competitors,
                Scores = competitors.ToDictionary(x => x.Id, x => new BotMatchScore { Id = x.Id }),
                Rules = rules
            };
            _matchRecords.TryAdd(record.Id, record);
            return Task.FromResult(record);
        }

        public Task<MatchRecord> GetMatchRecordAsync(Guid id)
        {
            var match = _matchRecords.TryGetValue(id, out var record) ? record : null;
            return Task.FromResult(match);
        }

        public Task<GameRecord> CreateGameRecordAsync(MatchRecord match, Bot opponent1, Bot opponent2)
        {
            var record = new GameRecord
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                Match = match,
                Opponent1 = opponent1,
                Opponent2 = opponent2
            };
            _gameRecords.TryAdd(record.Id, record);
            return Task.FromResult(record);
        }

        public Task<ThrowRecord> CreateThrowAsync(MatchRecord matchRecord, GameRecord gameRecord, int bot1Points, int bot2Points)
        {
            var record = new ThrowRecord
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.Now,
                Scores = new List<BotGameScore>
                {
                    new BotGameScore
                    {
                        Id = gameRecord.Opponent1.Id,
                        Points = bot1Points
                    },
                    new BotGameScore
                    {
                        Id = gameRecord.Opponent2.Id,
                        Points = bot2Points
                    }
                }
            };
            return Task.FromResult(record);
        }

        public async Task UpdateMatch(MatchRecord match)
        {
            if (!_matchRecords.TryGetValue(match.Id, out var matchRecord)) return;
            _matchRecords.TryUpdate(match.Id, match, matchRecord);
            await Task.Yield();
        }
    }
}