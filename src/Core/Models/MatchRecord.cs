using System;
using System.Collections.Generic;
using SharedKernel.ApiModels_V1;

namespace Core.Models
{
    public class MatchRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset CompletedAt { get; set; }

        public List<Bot> Competitors { get; set; }
        public Bot Winner { get; set; }
        public Dictionary<string, BotMatchScore> Scores { get; set; }
        public Rules Rules { get; set; }
    }
}