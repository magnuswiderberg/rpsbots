namespace SharedKernel.ApiModels_V1
{
    public enum MatchMode
    {
        /// <summary>
        /// Every bot plays against all other bots
        /// </summary>
        AllAgainstAll,

        /// <summary>
        /// Bots are divided into groups with quarter, semi and finals as needed
        /// </summary>
        EliminationCompetition
    }
}