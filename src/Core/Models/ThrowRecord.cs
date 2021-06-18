using System;
using System.Collections.Generic;
using SharedKernel.ApiModels_V1;

namespace Core.Models
{
    public class ThrowRecord
    {
        public Guid Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<BotGameScore> Scores { get; set; }
    }
}
