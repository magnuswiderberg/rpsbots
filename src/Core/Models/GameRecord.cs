using System;
using SharedKernel.ApiModels_V1;

namespace Core.Models
{
    public class GameRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public MatchRecord Match { get; set; }
        public Bot Opponent1 { get; set; }
        public Bot Opponent2 { get; set; }
    }
}
