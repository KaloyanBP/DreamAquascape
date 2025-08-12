using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Core.Business.Permissions;
using DreamAquascape.Services.Core.Business.Rules;
using DreamAquascape.GCommon.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests.Infrastructure
{
    /// <summary>
    /// Base test class with common setup for service tests
    /// </summary>
    public abstract class ServiceTestBase
    {
        protected Mock<IUnitOfWork> MockUnitOfWork { get; private set; } = null!;
        protected Mock<IContestRepository> MockContestRepository { get; private set; } = null!;
        protected Mock<IContestEntryRepository> MockContestEntryRepository { get; private set; } = null!;
        protected Mock<IContestWinnerRepository> MockContestWinnerRepository { get; private set; } = null!;
        protected Mock<IVoteRepository> MockVoteRepository { get; private set; } = null!;
        protected Mock<IUserRepository> MockUserRepository { get; private set; } = null!;
        protected Mock<IPrizeRepository> MockPrizeRepository { get; private set; } = null!;
        protected Mock<IContestBusinessRules> MockBusinessRules { get; private set; } = null!;
        protected Mock<IContestPermissionService> MockPermissionService { get; private set; } = null!;
        protected Mock<IDateTimeProvider> MockDateTimeProvider { get; private set; } = null!;
        protected DateTime TestDateTime { get; } = new(2025, 8, 6, 12, 0, 0, DateTimeKind.Utc);

        [SetUp]
        public virtual void SetUp()
        {
            MockUnitOfWork = new Mock<IUnitOfWork>();
            MockContestRepository = new Mock<IContestRepository>();
            MockContestEntryRepository = new Mock<IContestEntryRepository>();
            MockContestWinnerRepository = new Mock<IContestWinnerRepository>();
            MockVoteRepository = new Mock<IVoteRepository>();
            MockUserRepository = new Mock<IUserRepository>();
            MockPrizeRepository = new Mock<IPrizeRepository>();
            MockBusinessRules = new Mock<IContestBusinessRules>();
            MockPermissionService = new Mock<IContestPermissionService>();
            MockDateTimeProvider = new Mock<IDateTimeProvider>();

            MockUnitOfWork.Setup(x => x.ContestRepository).Returns(MockContestRepository.Object);
            MockUnitOfWork.Setup(x => x.ContestEntryRepository).Returns(MockContestEntryRepository.Object);
            MockUnitOfWork.Setup(x => x.ContestWinnerRepository).Returns(MockContestWinnerRepository.Object);
            MockUnitOfWork.Setup(x => x.VoteRepository).Returns(MockVoteRepository.Object);
            MockUnitOfWork.Setup(x => x.UserRepository).Returns(MockUserRepository.Object);
            MockUnitOfWork.Setup(x => x.PrizeRepository).Returns(MockPrizeRepository.Object);
            MockDateTimeProvider.Setup(x => x.UtcNow).Returns(TestDateTime);
        }

        protected Mock<ILogger<T>> CreateMockLogger<T>() => new Mock<ILogger<T>>();

        protected Contest CreateTestContest(int id = 1, bool isActive = true, bool isDeleted = false)
        {
            return new Contest
            {
                Id = id,
                Title = $"Test Contest {id}",
                Description = $"Test Description {id}",
                SubmissionStartDate = TestDateTime.AddDays(-10),
                SubmissionEndDate = TestDateTime.AddDays(-5),
                VotingStartDate = TestDateTime.AddDays(-5),
                VotingEndDate = TestDateTime.AddDays(5),
                ResultDate = TestDateTime.AddDays(10),
                IsActive = isActive,
                IsDeleted = isDeleted,
                CreatedAt = TestDateTime.AddDays(-15),
                CreatedBy = "test-user"
            };
        }

        protected ContestEntry CreateTestEntry(int id = 1, int contestId = 1, string participantId = "test-user")
        {
            return new ContestEntry
            {
                Id = id,
                ContestId = contestId,
                ParticipantId = participantId,
                Title = $"Test Entry {id}",
                Description = $"Test Entry Description {id}",
                SubmittedAt = TestDateTime.AddDays(-3),
                IsDeleted = false,
                EntryImages = new HashSet<EntryImage>()
            };
        }

        protected Vote CreateTestVote(int id = 1, int entryId = 1, string userId = "test-voter")
        {
            return new Vote
            {
                Id = id,
                ContestEntryId = entryId,
                UserId = userId,
                CreatedAt = TestDateTime.AddDays(-1),
                IpAddress = "127.0.0.1"
            };
        }

        protected ContestWinner CreateTestWinner(int contestId = 1, int entryId = 1, int position = 1)
        {
            return new ContestWinner
            {
                ContestId = contestId,
                ContestEntryId = entryId,
                Position = position,
                WonAt = TestDateTime,
                AwardTitle = $"Prize {position}"
            };
        }

        protected Prize CreateTestPrize(int id = 1, int contestId = 1)
        {
            return new Prize
            {
                Id = id,
                ContestId = contestId,
                Name = $"Test Prize {id}",
                Description = $"Test Prize Description {id}",
                ImageUrl = $"prize-{id}.jpg",
                MonetaryValue = 100m,
                Place = 1
            };
        }

        protected ApplicationUser CreateTestUser(string id = "test-user", string userName = "TestUser")
        {
            return new ApplicationUser
            {
                Id = id,
                UserName = userName,
                Email = $"{userName.ToLower()}@test.com",
                EmailConfirmed = true
            };
        }
    }
}
