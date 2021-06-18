using System.Collections.Generic;
using Core.Logic;
using Core.Restclients;
using Core.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SharedKernel.ApiModels_V1;

namespace Tests.GameEngine
{
    public abstract class AbstractBaseTest
    {
        protected readonly GameLogic GameLogic;
        protected readonly Mock<IRestClient> RestClientMock;

        protected static readonly Bot Bot1 = new Bot { Id = "identity-01", Name = "B1" };
        protected static readonly Bot Bot2 = new Bot { Id = "identity-02", Name = "B2" };
        protected static readonly Bot Bot3 = new Bot { Id = "identity-03", Name = "B3" };
        protected static readonly Bot Bot4 = new Bot { Id = "identity-04", Name = "B4" };

        protected static readonly List<Bot> DefaultCompetitors = new List<Bot> { Bot1, Bot2, Bot3, Bot4 };

        protected static readonly Rules DefaultRules = new Rules { Games = 10, BestOf = 3, Mode = MatchMode.AllAgainstAll, SameOutcomeLimit = 10 };


        protected AbstractBaseTest()
        {
            IStorage storage = new MemoryStorage();
            RestClientMock = new Mock<IRestClient>();
            GameLogic = new GameLogic(storage, RestClientMock.Object, new NullLogger<GameLogic>());

            RestClientMock
                .Setup(x => x.GetBotInformationAsync(It.IsAny<Bot>()))
                .ReturnsAsync((Bot bot) => new BotInformation { Name = bot.Name, Version = "1.0" });
        }
    }
}
