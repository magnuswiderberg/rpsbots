using System;
using System.Collections.Generic;
using System.Linq;
using ReferenceBot.Controllers;
using SharedKernel.ApiModels_V1;
using Xunit;

namespace Tests.InputValidation
{
    public class ControllerInputValidation
    {
        private readonly BotVersion1Controller _controller;

        private static readonly Bot Bot1 = new Bot { Id = "identity-01", Name = "B1" };
        private static readonly Bot Bot2 = new Bot { Id = "identity-02", Name = "B2" };
        private static readonly Bot Bot3 = new Bot { Id = "identity-03", Name = "B3" };
        private static readonly Bot Bot4 = new Bot { Id = "identity-04", Name = "B4" };

        private static readonly List<Bot> DefaultCompetitors = new List<Bot> { Bot1, Bot2, Bot3, Bot4 };

        private static readonly Rules DefaultRules = new Rules { Games = 10, BestOf = 3, Mode = MatchMode.AllAgainstAll, SameOutcomeLimit = 10 };

        public ControllerInputValidation()
        {
            _controller = new BotVersion1Controller();
        }

        [Theory]
        [MemberData(nameof(EmptyMatchIds))]
        public void Create_Match_Requires_Id(string id)
        {
            Assert.ThrowsAny<ArgumentNullException>(() => _controller.CreateMatch(new Match
            {
                Id = id,
                Competitors = DefaultCompetitors,
                Rules = DefaultRules
            }));
        }
        public static IEnumerable<object[]> EmptyMatchIds()
        {
            yield return new object[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { "  " };
            yield return new object[] { "\t" };
        }

        [Theory]
        [MemberData(nameof(EmptyCompetitors))]
        public void Create_Match_Requires_Competitors(IEnumerable<Bot> competitors)
        {
            Assert.ThrowsAny<ArgumentNullException>(() => _controller.CreateMatch(new Match
            {
                Id = "m01",
                Competitors = competitors?.ToList(),
                Rules = DefaultRules
            }));
        }

        public static IEnumerable<object[]> EmptyCompetitors()
        {
            yield return new object[] { null };
            yield return new object[] { new Bot[] { } };
            yield return new object[] { new List<Bot>() };
        }

        [Theory]
        [MemberData(nameof(IncompleteRules))]
        public void Create_Match_Requires_Rules(Rules rules)
        {
            Assert.ThrowsAny<ArgumentException>(() => _controller.CreateMatch(new Match
            {
                Id = "m01",
                Competitors = DefaultCompetitors,
                Rules = rules
            }));
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

    }
}
