using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SharedKernel.ApiModels_V1;
using Xunit;

namespace Tests.GameEngine
{
    public class MatchAdministrationTests : AbstractBaseTest
    {
        [Fact]
        public async Task Creating_A_Match_Preserves_The_Input_Values()
        {
            var match = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);
            
            Assert.NotNull(match?.Id);
            Assert.NotNull(match.Competitors);
            Assert.True(match.Competitors.Select(x => x.Id).All(id => DefaultCompetitors.Any(x => x.Id == id)));
            Assert.Equal(DefaultRules.Mode, match.Rules.Mode);
            Assert.Equal(DefaultRules.Games, match.Rules.Games);
            Assert.Equal(DefaultRules.BestOf, match.Rules.BestOf);
            Assert.Equal(DefaultRules.SameOutcomeLimit, match.Rules.SameOutcomeLimit);
        }

        [Fact]
        public async Task A_Match_Can_Be_Fetched_By_Its_Id()
        {
            var createdMatch = await GameLogic.CreateMatchAsync(DefaultCompetitors, DefaultRules);
            var fetchedMatch = await GameLogic.GetMatchAsync(createdMatch.Id.ToString());
            Assert.NotNull(fetchedMatch);
            Assert.Equal(createdMatch.Id, fetchedMatch.Id);
        }

        [Fact]
        public async Task Non_Existing_Match_Id_Returns_Null()
        {
            var fetchedMatch = await GameLogic.GetMatchAsync(Guid.NewGuid().ToString());
            Assert.Null(fetchedMatch);
        }

        #region Validation

        [Theory]
        [MemberData(nameof(InsufficientNumberOfCompetitors))]
        public async Task A_Match_Contains_At_Least_Two_Competitors(List<Bot> competitors)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(async () => await GameLogic.CreateMatchAsync(competitors, DefaultRules));
        }

        public static IEnumerable<object[]> InsufficientNumberOfCompetitors()
        {
            yield return new object[] { null };
            yield return new object[] { new List<Bot>() };
            yield return new object[] { new List<Bot> { Bot1 } };
            yield return new object[] { new List<Bot> { Bot2 } };
        }

        [Theory]
        [MemberData(nameof(NonUniqueCompetitors))]
        public async Task A_Match_Contains_Unique_Competitors(List<Bot> competitors)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(async () => await GameLogic.CreateMatchAsync(competitors, DefaultRules));
        }

        public static IEnumerable<object[]> NonUniqueCompetitors()
        {
            yield return new object[] { new List<Bot> { Bot1, Bot1 } };
            yield return new object[] { new List<Bot> { Bot1, Bot2, Bot3, Bot1 } };
            yield return new object[] { new List<Bot> { Bot1, Bot2, Bot3, Bot4, Bot4 } };
        }
        
        [Theory]
        [MemberData(nameof(IncompleteRules))]
        public async Task Create_Match_Requires_Rules(Rules rules)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(async () => await GameLogic.CreateMatchAsync(DefaultCompetitors, rules));
        }

        public static IEnumerable<object[]> IncompleteRules()
        {
            yield return new object[] { null };
            var rules = DefaultRules.Clone();
            rules.BestOf = -1;
            yield return new object[] { rules };
            rules = DefaultRules.Clone();
            rules.Games = -1;
            yield return new object[] { rules };
            rules = DefaultRules.Clone();
            rules.SameOutcomeLimit = -1;
            yield return new object[] { rules };
        }
        
        [Theory]
        [MemberData(nameof(NonGuidIds))]
        public async Task Get_Match_By_Id_Requires_Guid(string id)
        {
            await Assert.ThrowsAnyAsync<ArgumentException>(async () => await GameLogic.GetMatchAsync(id));
        }

        public static IEnumerable<object[]> NonGuidIds()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "foo-bar" };
            yield return new object[] { Guid.Empty.ToString() };
        }

        #endregion
    }
}
