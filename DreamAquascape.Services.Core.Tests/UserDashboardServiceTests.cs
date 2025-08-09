using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.GCommon;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class UserDashboardServiceTests : ServiceTestBase
    {
        private UserDashboardService _service = null!;
        private Mock<ILogger<UserDashboardService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<UserDashboardService>();

            _service = new UserDashboardService(
                MockUnitOfWork.Object,
                _mockLogger.Object,
                MockDateTimeProvider.Object);
        }

        [Test]
        public async Task GetUserQuickStatsAsyncShouldReturnStatsWhenValidUserIdProvided()
        {
            // Arrange
            var userId = "test-user";
            var contestIdsFromEntries = new List<int> { 1, 2 };
            var contestIdsFromVotes = new List<int> { 2, 3 };

            MockContestEntryRepository.Setup(x => x.GetContestIdsUserEnteredAsync(userId))
                .ReturnsAsync(contestIdsFromEntries);

            MockVoteRepository.Setup(x => x.GetContestIdsUserVotedInAsync(userId))
                .ReturnsAsync(contestIdsFromVotes);

            MockContestEntryRepository.Setup(x => x.GetTotalEntriesSubmittedByUserAsync(userId))
                .ReturnsAsync(5);

            MockVoteRepository.Setup(x => x.GetVotesCastByUserAsync(userId))
                .ReturnsAsync(10);

            MockVoteRepository.Setup(x => x.GetVotesReceivedByUserAsync(userId))
                .ReturnsAsync(15);

            MockContestWinnerRepository.Setup(x => x.GetContestsWonByUserAsync(userId))
                .ReturnsAsync(2);

            MockContestRepository.Setup(x => x.GetActiveContestsCountForUserAsync(userId, TestDateTime))
                .ReturnsAsync(3);

            MockContestRepository.Setup(x => x.GetSubmissionsInProgressCountForUserAsync(userId, TestDateTime))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GetUserQuickStatsAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalContestsParticipated, Is.EqualTo(3)); // Union of {1,2} and {2,3} = {1,2,3}
            Assert.That(result.TotalEntriesSubmitted, Is.EqualTo(5));
            Assert.That(result.TotalVotesCast, Is.EqualTo(10));
            Assert.That(result.TotalVotesReceived, Is.EqualTo(15));
            Assert.That(result.ContestsWon, Is.EqualTo(2));
            Assert.That(result.ActiveContests, Is.EqualTo(3));
            Assert.That(result.SubmissionsInProgress, Is.EqualTo(1));
            Assert.That(result.AverageVotesPerEntry, Is.EqualTo(3.0)); // 15 votes / 5 entries
        }

        [Test]
        public async Task GetUserQuickStatsAsyncShouldHandleZeroEntriesWhenCalculatingAverages()
        {
            // Arrange
            var userId = "test-user";

            MockContestEntryRepository.Setup(x => x.GetContestIdsUserEnteredAsync(userId))
                .ReturnsAsync(new List<int>());

            MockVoteRepository.Setup(x => x.GetContestIdsUserVotedInAsync(userId))
                .ReturnsAsync(new List<int>());

            MockContestEntryRepository.Setup(x => x.GetTotalEntriesSubmittedByUserAsync(userId))
                .ReturnsAsync(0);

            MockVoteRepository.Setup(x => x.GetVotesCastByUserAsync(userId))
                .ReturnsAsync(0);

            MockVoteRepository.Setup(x => x.GetVotesReceivedByUserAsync(userId))
                .ReturnsAsync(0);

            MockContestWinnerRepository.Setup(x => x.GetContestsWonByUserAsync(userId))
                .ReturnsAsync(0);

            MockContestRepository.Setup(x => x.GetActiveContestsCountForUserAsync(userId, TestDateTime))
                .ReturnsAsync(0);

            MockContestRepository.Setup(x => x.GetSubmissionsInProgressCountForUserAsync(userId, TestDateTime))
                .ReturnsAsync(0);

            // Act
            var result = await _service.GetUserQuickStatsAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalContestsParticipated, Is.EqualTo(0));
            Assert.That(result.AverageVotesPerEntry, Is.EqualTo(0)); // Should handle division by zero
            Assert.That(result.WinRate, Is.EqualTo(0)); // Should handle division by zero
        }

        [Test]
        public async Task GetUserActiveContestsAsyncShouldReturnActiveContestsWhenValidUserIdProvided()
        {
            // Arrange
            var userId = "test-user";
            var contest1 = CreateTestContest(1, true);
            contest1.SubmissionStartDate = TestDateTime.AddDays(-5);
            contest1.SubmissionEndDate = TestDateTime.AddDays(5);
            contest1.VotingStartDate = TestDateTime.AddDays(6);
            contest1.VotingEndDate = TestDateTime.AddDays(15);

            var contest2 = CreateTestContest(2, true);
            contest2.SubmissionStartDate = TestDateTime.AddDays(-10);
            contest2.SubmissionEndDate = TestDateTime.AddDays(-1);
            contest2.VotingStartDate = TestDateTime.AddDays(-1);
            contest2.VotingEndDate = TestDateTime.AddDays(5);

            var activeContests = new List<Contest> { contest1, contest2 };

            MockContestRepository.Setup(x => x.GetActiveContestsWithFullDataAsync(TestDateTime))
                .ReturnsAsync(activeContests);

            MockVoteRepository.Setup(x => x.GetUserVoteForContestAsync(userId, It.IsAny<int>()))
                .ReturnsAsync((Vote)null!);

            MockVoteRepository.Setup(x => x.GetTotalVotesForContestAsync(It.IsAny<int>()))
                .ReturnsAsync(10);

            // Act
            var result = await _service.GetUserActiveContestsAsync(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            var firstContest = result.FirstOrDefault(c => c.ContestId == 1);
            Assert.That(firstContest, Is.Not.Null);
            Assert.That(firstContest.Phase, Is.EqualTo(ContestPhases.Submission));

            var secondContest = result.FirstOrDefault(c => c.ContestId == 2);
            Assert.That(secondContest, Is.Not.Null);
            Assert.That(secondContest.Phase, Is.EqualTo(ContestPhases.Voting));
        }

        [Test]
        public async Task GetUserSubmissionsAsyncShouldReturnUserSubmissionsWhenValidUserIdProvided()
        {
            // Arrange
            var userId = "test-user";
            var page = 1;
            var pageSize = 10;

            var entry1 = CreateTestEntry(1, 1, userId);
            entry1.Title = "Test Entry 1";
            entry1.Contest = CreateTestContest(1, true);

            var entry2 = CreateTestEntry(2, 2, userId);
            entry2.Title = "Test Entry 2";
            entry2.Contest = CreateTestContest(2, true);

            var userSubmissions = new List<ContestEntry> { entry1, entry2 };

            MockContestEntryRepository.Setup(x => x.GetUserSubmissionsWithFullDataAsync(userId, page, pageSize))
                .ReturnsAsync(userSubmissions);

            MockVoteRepository.Setup(x => x.GetTotalVotesForContestAsync(It.IsAny<int>()))
                .ReturnsAsync(20);

            MockContestEntryRepository.Setup(x => x.GetEntryRankingInContestAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GetUserSubmissionsAsync(userId, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].EntryTitle, Is.EqualTo("Test Entry 1"));
            Assert.That(result[1].EntryTitle, Is.EqualTo("Test Entry 2"));
        }

        [Test]
        public async Task GetUserSubmissionsAsyncShouldReturnEmptyListWhenUserHasNoSubmissions()
        {
            // Arrange
            var userId = "test-user";
            var page = 1;
            var pageSize = 10;

            MockContestEntryRepository.Setup(x => x.GetUserSubmissionsWithFullDataAsync(userId, page, pageSize))
                .ReturnsAsync(new List<ContestEntry>());

            // Act
            var result = await _service.GetUserSubmissionsAsync(userId, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserVotingHistoryAsyncShouldReturnVotingHistoryWhenValidUserIdProvided()
        {
            // Arrange
            var userId = "test-user";
            var page = 1;
            var pageSize = 10;

            var vote1 = CreateTestVote(1, 1, userId);
            vote1.ContestEntry = CreateTestEntry(1, 1, "other-user");
            vote1.ContestEntry.Contest = CreateTestContest(1, true);

            var vote2 = CreateTestVote(2, 2, userId);
            vote2.ContestEntry = CreateTestEntry(2, 2, "other-user");
            vote2.ContestEntry.Contest = CreateTestContest(2, true);

            var votingHistory = new List<Vote> { vote1, vote2 };

            MockVoteRepository.Setup(x => x.GetUserVotingHistoryAsync(userId, page, pageSize))
                .ReturnsAsync(votingHistory);

            MockContestEntryRepository.Setup(x => x.GetEntryRankingInContestAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.GetUserVotingHistoryAsync(userId, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].VoteId, Is.EqualTo(1));
            Assert.That(result[1].VoteId, Is.EqualTo(2));
        }

        [Test]
        public async Task GetUserVotingHistoryAsyncShouldReturnEmptyListWhenUserHasNoVotingHistory()
        {
            // Arrange
            var userId = "test-user";
            var page = 1;
            var pageSize = 10;

            MockVoteRepository.Setup(x => x.GetUserVotingHistoryAsync(userId, page, pageSize))
                .ReturnsAsync(new List<Vote>());

            // Act
            var result = await _service.GetUserVotingHistoryAsync(userId, page, pageSize);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetUserDetailsShouldReturnUserWhenValidUserIdProvided()
        {
            // Arrange
            var userId = "test-user";
            var expectedUser = new ApplicationUser
            {
                Id = userId,
                UserName = "testuser",
                Email = "test@example.com"
            };

            MockUnitOfWork.Setup(x => x.UserRepository.GetUserByIdAsync(userId))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _service.GetUserDetails(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(userId));
            Assert.That(result.UserName, Is.EqualTo("testuser"));
            Assert.That(result.Email, Is.EqualTo("test@example.com"));
        }

        [Test]
        public async Task GetUserDetailsShouldReturnNullWhenUserNotFound()
        {
            // Arrange
            var userId = "non-existent-user";

            MockUnitOfWork.Setup(x => x.UserRepository.GetUserByIdAsync(userId))
                .ReturnsAsync((ApplicationUser)null!);

            // Act
            var result = await _service.GetUserDetails(userId);

            // Assert
            Assert.That(result, Is.Null);
        }
    }
}
