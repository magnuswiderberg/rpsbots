using System.Threading.Tasks;
using Xunit;

namespace Tests.GameEngine
{
    public class GameAdministrationTests : AbstractBaseTest
    {
        [Fact]
        public async Task Creating_A_Game_Preserves_The_Input_Values()
        {
            var match = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);
            var game = await GameLogic.CreateGameAsync(match, Bot1, Bot2);
            
            Assert.NotNull(game?.Id);
            Assert.NotNull(game.Match?.Id);
            Assert.Equal(match.Id, game.Match.Id);
            Assert.Equal(Bot1.Id, game.Opponent1.Id);
            Assert.Equal(Bot2.Id, game.Opponent2.Id);
        }
    }
}
