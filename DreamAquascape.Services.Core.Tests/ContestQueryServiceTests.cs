using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestQueryServiceTests : ServiceTestBase
    {
        private ContestQueryService _contestQueryService;
        private Mock<ILogger<ContestQueryService>> _mockLogger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<ContestQueryService>();
            _contestQueryService = new ContestQueryService(MockUnitOfWork.Object, _mockLogger.Object, MockDateTimeProvider.Object);
        }

        [Test]
        public async Task GetContestDetailsAsyncShouldReturnContestDetailsWhenValidContestIdIsProvided()
        {
            // Arrange
            var contestId = 1;
            var userId = "test-user";
            var contest = CreateTestContest(contestId);
            var entry = CreateTestEntry(1, contestId, "participant-1");
            var participant = CreateTestUser("participant-1", "TestParticipant");
            var vote = CreateTestVote(1, entry.Id, userId);

            // Set up navigation properties properly
            contest.Entries = new List<ContestEntry> { entry };
            entry.Votes = new List<Vote> { vote };
            entry.Contest = contest;
            entry.Participant = participant;
            vote.ContestEntry = entry; // This was missing!

            MockContestRepository
                .Setup(x => x.GetContestDetailsAsync(contestId))
                .ReturnsAsync(contest);

            // Act
            var result = await _contestQueryService.GetContestDetailsAsync(contestId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(contestId));
            Assert.That(result.Title, Is.EqualTo(contest.Title));
            Assert.That(result.Description, Is.EqualTo(contest.Description));
            Assert.That(result.Entries, Has.Count.EqualTo(1));
            Assert.That(result.Entries.First().Id, Is.EqualTo(entry.Id));
            Assert.That(result.UserHasVoted, Is.True);
            Assert.That(result.UserVotedForEntryId, Is.EqualTo(entry.Id));
        }

        [Test]
        public async Task GetContestDetailsAsyncShouldReturnNullWhenInvalidContestIdIsProvided()
        {
            // Arrange
            var contestId = 999;

            MockContestRepository
                .Setup(x => x.GetContestDetailsAsync(contestId))
                .ReturnsAsync((Contest?)null);

            // Act
            var result = await _contestQueryService.GetContestDetailsAsync(contestId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetFilteredContestsAsyncShouldReturnFilteredResultsWhenFiltersAreValid()
        {
            // Arrange
            var filters = new ContestFilterViewModel
            {
                Page = 1,
                PageSize = 10,
                Status = ContestStatus.Active
            };

            var contests = new List<Contest>
            {
                CreateTestContest(1),
                CreateTestContest(2)
            };

            var totalCount = 2;
            var stats = new ContestStatsViewModel
            {
                TotalContests = 2,
                ActiveContests = 2,
                InactiveContests = 0,
                SubmissionPhase = 0,
                VotingPhase = 2,
                EndedContests = 0,
                ArchivedContests = 0
            };

            MockContestRepository
                .Setup(x => x.GetFilteredContestsAsync(filters))
                .ReturnsAsync((contests, totalCount));

            MockContestRepository
                .Setup(x => x.GetContestStatsAsync())
                .ReturnsAsync(stats);

            // Act
            var result = await _contestQueryService.GetFilteredContestsAsync(filters);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Contests.Count(), Is.EqualTo(2));
            Assert.That(result.Pagination.TotalItems, Is.EqualTo(totalCount));
            Assert.That(result.Pagination.CurrentPage, Is.EqualTo(filters.Page));
            Assert.That(result.Stats, Is.Not.Null);
        }

        [Test]
        public async Task GetContestForEditAsyncShouldReturnEditViewModelWhenValidContestIdIsProvided()
        {
            // Arrange
            var contestId = 1;
            var contest = CreateTestContest(contestId);
            var prize = new Prize
            {
                Id = 1,
                ContestId = contestId,
                Name = "Test Prize",
                Description = "Test Prize Description",
                MonetaryValue = 100.50m
            };
            contest.Prizes = new List<Prize> { prize };

            MockContestRepository
                .Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);

            // Act
            var result = await _contestQueryService.GetContestForEditAsync(contestId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(contestId));
            Assert.That(result.Title, Is.EqualTo(contest.Title));
            Assert.That(result.PrizeName, Is.EqualTo(prize.Name));
            Assert.That(result.PrizeDescription, Is.EqualTo(prize.Description));
            Assert.That(result.PrizeMonetaryValue, Is.EqualTo(prize.MonetaryValue));
        }

        [Test]
        public async Task GetContestStatsAsyncShouldReturnValidStatistics()
        {
            // Arrange
            var expectedStats = new ContestStatsViewModel
            {
                TotalContests = 10,
                ActiveContests = 5,
                InactiveContests = 3,
                SubmissionPhase = 2,
                VotingPhase = 2,
                EndedContests = 1,
                ArchivedContests = 2
            };

            MockContestRepository
                .Setup(x => x.GetContestStatsAsync())
                .ReturnsAsync(expectedStats);

            // Act
            var result = await _contestQueryService.GetContestStatsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalContests, Is.EqualTo(expectedStats.TotalContests));
            Assert.That(result.ActiveContests, Is.EqualTo(expectedStats.ActiveContests));
            Assert.That(result.InactiveContests, Is.EqualTo(expectedStats.InactiveContests));
            Assert.That(result.SubmissionPhase, Is.EqualTo(expectedStats.SubmissionPhase));
        }

        [Test]
        public async Task GetContestDetailsAsyncShouldCalculateUserPermissionsCorrectly()
        {
            // Arrange
            var contestId = 1;
            var userId = "test-user";
            var contest = CreateTestContest(contestId);

            // Set contest to be in voting phase
            contest.SubmissionStartDate = TestDateTime.AddDays(-10);
            contest.SubmissionEndDate = TestDateTime.AddDays(-5);
            contest.VotingStartDate = TestDateTime.AddDays(-5);
            contest.VotingEndDate = TestDateTime.AddDays(5);

            contest.Entries = new List<ContestEntry>();

            MockContestRepository
                .Setup(x => x.GetContestDetailsAsync(contestId))
                .ReturnsAsync(contest);

            // Act
            var result = await _contestQueryService.GetContestDetailsAsync(contestId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.CanSubmitEntry, Is.False); // Submission period ended
            Assert.That(result.CanVote, Is.True); // Voting period us active and user hasn't voted
            Assert.That(result.UserHasSubmittedEntry, Is.False);
            Assert.That(result.UserHasVoted, Is.False);
        }
    }
}
