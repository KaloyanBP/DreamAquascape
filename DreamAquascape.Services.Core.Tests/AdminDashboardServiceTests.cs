using DreamAquascape.Services.Core.AdminDashboard;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class AdminDashboardServiceTests : ServiceTestBase
    {
        private AdminDashboardService _service = null!;
        private Mock<ILogger<AdminDashboardService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<AdminDashboardService>();

            _service = new AdminDashboardService(
                MockUnitOfWork.Object,
                _mockLogger.Object,
                MockDateTimeProvider.Object);
        }

        [Test]
        public async Task GetDashboardStatsAsyncShouldReturnCompleteStatsWhenAllDataAvailable()
        {
            // Arrange
            var thirtyDaysAgo = TestDateTime.AddDays(-30);
            var sevenDaysFromNow = TestDateTime.AddDays(7);

            // Basic counts
            MockContestRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(10);

            MockContestRepository.Setup(x => x.GetActiveContestCountAsync())
                .ReturnsAsync(3);

            MockContestEntryRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(50);

            MockVoteRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(200);

            MockContestEntryRepository.Setup(x => x.GetPendingEntriesCountAsync(TestDateTime))
                .ReturnsAsync(5);

            MockContestRepository.Setup(x => x.GetContestsEndingSoonCountAsync(TestDateTime, sevenDaysFromNow))
                .ReturnsAsync(2);

            MockPrizeRepository.Setup(x => x.GetTotalActivePrizeValueAsync())
                .ReturnsAsync(1500.00m);

            // Performance metrics
            MockContestRepository.Setup(x => x.GetAverageEntriesPerContestAsync())
                .ReturnsAsync(5.0);

            MockContestEntryRepository.Setup(x => x.GetAverageVotesPerEntryAsync())
                .ReturnsAsync(4.0);

            // User engagement data
            var entryParticipants = new List<string> { "user1", "user2", "user3" };
            var voteParticipants = new List<string> { "user2", "user3", "user4", "user5" };
            var activeEntryParticipants = new List<string> { "user1", "user2" };
            var activeVoteParticipants = new List<string> { "user2", "user4" };

            MockContestEntryRepository.Setup(x => x.GetAllParticipantIdsAsync())
                .ReturnsAsync(entryParticipants);

            MockVoteRepository.Setup(x => x.GetAllVoterIdsAsync())
                .ReturnsAsync(voteParticipants);

            MockContestEntryRepository.Setup(x => x.GetParticipantIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(activeEntryParticipants);

            MockVoteRepository.Setup(x => x.GetVoterIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(activeVoteParticipants);

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalContests, Is.EqualTo(10));
            Assert.That(result.ActiveContests, Is.EqualTo(3));
            Assert.That(result.TotalEntries, Is.EqualTo(50));
            Assert.That(result.TotalVotes, Is.EqualTo(200));
            Assert.That(result.TotalUsers, Is.EqualTo(5)); // 5 participants: user1, user2, user3, user4, user5
            Assert.That(result.PendingEntries, Is.EqualTo(5));
            Assert.That(result.ContestsEndingSoon, Is.EqualTo(2));
            Assert.That(result.TotalPrizeValue, Is.EqualTo(1500.00m));
            Assert.That(result.AverageEntriesPerContest, Is.EqualTo(5.0));
            Assert.That(result.AverageVotesPerEntry, Is.EqualTo(4.0));
            Assert.That(result.UserEngagementRate, Is.EqualTo(60.0)); // 3 active users, 5 total = 60%
        }

        [Test]
        public async Task GetDashboardStatsAsyncShouldHandleZeroDataGracefully()
        {
            // Arrange
            var thirtyDaysAgo = TestDateTime.AddDays(-30);
            var sevenDaysFromNow = TestDateTime.AddDays(7);

            // All counts return zero
            MockContestRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            MockContestRepository.Setup(x => x.GetActiveContestCountAsync())
                .ReturnsAsync(0);

            MockContestEntryRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            MockVoteRepository.Setup(x => x.CountAsync())
                .ReturnsAsync(0);

            MockContestEntryRepository.Setup(x => x.GetPendingEntriesCountAsync(TestDateTime))
                .ReturnsAsync(0);

            MockContestRepository.Setup(x => x.GetContestsEndingSoonCountAsync(TestDateTime, sevenDaysFromNow))
                .ReturnsAsync(0);

            MockPrizeRepository.Setup(x => x.GetTotalActivePrizeValueAsync())
                .ReturnsAsync(0.00m);

            MockContestRepository.Setup(x => x.GetAverageEntriesPerContestAsync())
                .ReturnsAsync(0.0);

            MockContestEntryRepository.Setup(x => x.GetAverageVotesPerEntryAsync())
                .ReturnsAsync(0.0);

            // Empty user lists
            MockContestEntryRepository.Setup(x => x.GetAllParticipantIdsAsync())
                .ReturnsAsync(new List<string>());

            MockVoteRepository.Setup(x => x.GetAllVoterIdsAsync())
                .ReturnsAsync(new List<string>());

            MockContestEntryRepository.Setup(x => x.GetParticipantIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(new List<string>());

            MockVoteRepository.Setup(x => x.GetVoterIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(new List<string>());

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalContests, Is.EqualTo(0));
            Assert.That(result.ActiveContests, Is.EqualTo(0));
            Assert.That(result.TotalEntries, Is.EqualTo(0));
            Assert.That(result.TotalVotes, Is.EqualTo(0));
            Assert.That(result.TotalUsers, Is.EqualTo(0));
            Assert.That(result.PendingEntries, Is.EqualTo(0));
            Assert.That(result.ContestsEndingSoon, Is.EqualTo(0));
            Assert.That(result.TotalPrizeValue, Is.EqualTo(0.00m));
            Assert.That(result.AverageEntriesPerContest, Is.EqualTo(0.0));
            Assert.That(result.AverageVotesPerEntry, Is.EqualTo(0.0));
            Assert.That(result.UserEngagementRate, Is.EqualTo(0.0)); // Should handle division by zero
        }

        [Test]
        public async Task GetDashboardStatsAsyncShouldCalculateUserEngagementCorrectly()
        {
            // Arrange
            var thirtyDaysAgo = TestDateTime.AddDays(-30);
            var sevenDaysFromNow = TestDateTime.AddDays(7);

            // Setup basic repository calls with minimal data
            MockContestRepository.Setup(x => x.CountAsync()).ReturnsAsync(1);
            MockContestRepository.Setup(x => x.GetActiveContestCountAsync()).ReturnsAsync(1);
            MockContestEntryRepository.Setup(x => x.CountAsync()).ReturnsAsync(1);
            MockVoteRepository.Setup(x => x.CountAsync()).ReturnsAsync(1);
            MockContestEntryRepository.Setup(x => x.GetPendingEntriesCountAsync(TestDateTime)).ReturnsAsync(1);
            MockContestRepository.Setup(x => x.GetContestsEndingSoonCountAsync(TestDateTime, sevenDaysFromNow)).ReturnsAsync(1);
            MockPrizeRepository.Setup(x => x.GetTotalActivePrizeValueAsync()).ReturnsAsync(100m);
            MockContestRepository.Setup(x => x.GetAverageEntriesPerContestAsync()).ReturnsAsync(1.0);
            MockContestEntryRepository.Setup(x => x.GetAverageVotesPerEntryAsync()).ReturnsAsync(1.0);

            // Focus on user engagement calculation
            var allEntryParticipants = new List<string> { "user1", "user2", "user3", "user4" };
            var allVoteParticipants = new List<string> { "user3", "user4", "user5", "user6" };
            var activeEntryParticipants = new List<string> { "user1", "user3" };
            var activeVoteParticipants = new List<string> { "user5" };

            MockContestEntryRepository.Setup(x => x.GetAllParticipantIdsAsync())
                .ReturnsAsync(allEntryParticipants);

            MockVoteRepository.Setup(x => x.GetAllVoterIdsAsync())
                .ReturnsAsync(allVoteParticipants);

            MockContestEntryRepository.Setup(x => x.GetParticipantIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(activeEntryParticipants);

            MockVoteRepository.Setup(x => x.GetVoterIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(activeVoteParticipants);

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            // Total unique users: user1, user2, user3, user4, user5, user6 = 6 users
            Assert.That(result.TotalUsers, Is.EqualTo(6));
            // Active users: user1, user3, user5 = 3 users
            // Engagement rate: 3/6 * 100 = 50%
            Assert.That(result.UserEngagementRate, Is.EqualTo(50.0));
        }

        [Test]
        public async Task GetDashboardStatsAsyncShouldHandlePartialEngagementData()
        {
            // Arrange
            var thirtyDaysAgo = TestDateTime.AddDays(-30);
            var sevenDaysFromNow = TestDateTime.AddDays(7);

            // Setup basic repository calls
            MockContestRepository.Setup(x => x.CountAsync()).ReturnsAsync(5);
            MockContestRepository.Setup(x => x.GetActiveContestCountAsync()).ReturnsAsync(2);
            MockContestEntryRepository.Setup(x => x.CountAsync()).ReturnsAsync(15);
            MockVoteRepository.Setup(x => x.CountAsync()).ReturnsAsync(45);
            MockContestEntryRepository.Setup(x => x.GetPendingEntriesCountAsync(TestDateTime)).ReturnsAsync(3);
            MockContestRepository.Setup(x => x.GetContestsEndingSoonCountAsync(TestDateTime, sevenDaysFromNow)).ReturnsAsync(1);
            MockPrizeRepository.Setup(x => x.GetTotalActivePrizeValueAsync()).ReturnsAsync(750m);
            MockContestRepository.Setup(x => x.GetAverageEntriesPerContestAsync()).ReturnsAsync(3.0);
            MockContestEntryRepository.Setup(x => x.GetAverageVotesPerEntryAsync()).ReturnsAsync(3.0);

            // Test case: users who only submitted entries, no votes
            var entryOnlyParticipants = new List<string> { "entryUser1", "entryUser2" };
            var voteOnlyParticipants = new List<string> { "voteUser1", "voteUser2", "voteUser3" };
            var noActiveParticipants = new List<string>(); // No recent activity

            MockContestEntryRepository.Setup(x => x.GetAllParticipantIdsAsync())
                .ReturnsAsync(entryOnlyParticipants);

            MockVoteRepository.Setup(x => x.GetAllVoterIdsAsync())
                .ReturnsAsync(voteOnlyParticipants);

            MockContestEntryRepository.Setup(x => x.GetParticipantIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(noActiveParticipants);

            MockVoteRepository.Setup(x => x.GetVoterIdsSinceAsync(thirtyDaysAgo))
                .ReturnsAsync(noActiveParticipants);

            // Act
            var result = await _service.GetDashboardStatsAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalUsers, Is.EqualTo(5)); // 2 entry users + 3 vote users = 5 unique
            Assert.That(result.UserEngagementRate, Is.EqualTo(0.0)); // No recent activity = 0%
            Assert.That(result.TotalContests, Is.EqualTo(5));
            Assert.That(result.ActiveContests, Is.EqualTo(2));
        }
    }
}
