using System.Collections.Generic;

namespace SharedKernel.ApiModels_V1
{
    public class GameFeedback
    {
        public List<BotGameScore> Scores { get; set; }
        public string Winner { get; set; }
    }
}
