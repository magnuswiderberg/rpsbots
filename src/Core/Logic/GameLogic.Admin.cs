using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Models;
using SharedKernel.ApiModels_V1;

namespace Core.Logic
{
    public partial class GameLogic
    {
        public async Task<MatchRecord> CreateMatchAsync(List<Bot> competitors, Rules rules)
        {
            if (competitors == null) throw new ArgumentNullException(nameof(competitors));
            if (competitors.Count < 2) throw new ArgumentException("Provide at least 2 competitors", nameof(competitors));
            if (competitors.Count != competitors.Select(x => x.Id).Distinct().Count()) throw new ArgumentException("All competitors must be unique");
            if (rules == null) throw new ArgumentNullException(nameof(rules));
            if (rules.BestOf < 1) throw new ArgumentException("Requires > 0", nameof(rules.BestOf));
            if (rules.Games < 1) throw new ArgumentException("Requires > 0", nameof(rules.Games));
            if (rules.SameOutcomeLimit < 1) throw new ArgumentException("Requires > 0", nameof(rules.SameOutcomeLimit));

            foreach (var competitor in competitors)
            {
                try
                {
                    var info = await _restClient.GetBotInformationAsync(competitor);
                    if (string.IsNullOrWhiteSpace(info?.Name)) throw new InvalidOperationException($"There was no bot information for '{competitor.Name}'");
                    competitor.Name = info.Name;
                }
                catch (Exception e)
                {
                    throw new ArgumentException($"Could not get bot information. {e.Message}'", e);
                }
            }

            var matchRecord = await _storage.CreateMatchRecordAsync(competitors, rules);
            return matchRecord;
        }

        public async Task<MatchRecord> GetMatchAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            if (!Guid.TryParse(id, out var guidId)) throw new ArgumentException($"{id} is not a Guid");
            if (guidId == default) throw new ArgumentException("The empty Guid is not a valid id");

            var matchRecord = await _storage.GetMatchRecordAsync(guidId);
            return matchRecord;
        }

        public async Task<GameRecord> CreateGameAsync(MatchRecord match, Bot opponent1, Bot opponent2)
        {
            if (match == null) throw new ArgumentNullException(nameof(match));
            if (opponent1?.Id == null) throw new ArgumentNullException(nameof(opponent1));
            if (opponent2?.Id == null) throw new ArgumentNullException(nameof(opponent2));

            var gameRecord = await _storage.CreateGameRecordAsync(match, opponent1, opponent2);
            return gameRecord;
        }
    }
}