using System.Collections.Generic;

namespace SharedKernel.ApiModels_V1
{
    public class Match
    {
        public string Id { get; set; }
        public List<Bot> Competitors { get; set; }
        public Rules Rules { get; set; }
    }
}
