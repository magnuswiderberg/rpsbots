namespace SharedKernel.ApiModels_V1
{
    public class Rules
    {
        public MatchMode Mode { get; set; }

        /// <summary>
        /// The number of games to play against each opponent
        /// </summary>
        public int Games { get; set; }

        /// <summary>
        /// The maximum number non-draw throws in a game.
        /// First to Math.Floor(<see cref="BestOf"/>/2) + 1 wins.
        /// </summary>
        public int BestOf { get; set; }

        /// <summary>
        /// The maximum number of times in a row that bots can show the same throw.
        /// After that, the game is considered a draw.
        /// </summary>
        public int SameOutcomeLimit { get; set; }
    }
}