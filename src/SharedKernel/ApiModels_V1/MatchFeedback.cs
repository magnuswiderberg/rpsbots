using System.Collections.Generic;

namespace SharedKernel.ApiModels_V1
{
    public class MatchFeedback
    {
        public List<BotMatchScore> Scores { get; set; }
        public string Winner { get; set; }
    }
}
