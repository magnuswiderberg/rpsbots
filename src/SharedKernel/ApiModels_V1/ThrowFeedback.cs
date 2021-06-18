using System.Collections.Generic;

namespace SharedKernel.ApiModels_V1
{
    public class ThrowFeedback
    {
        public List<BotThrow> Result { get; set; }
        public string Winner { get; set; }
        public int TotalThrows { get; set; }
    }
}
