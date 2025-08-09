using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.Services.Core;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using DreamAquascape.Services.Core.Infrastructure;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestEntryQueryServiceTests : ServiceTestBase
    {
        private ContestEntryQueryService _service = null!;
        private Mock<ILogger<ContestEntryQueryService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<ContestEntryQueryService>();
            _service = new ContestEntryQueryService(MockUnitOfWork.Object, MockDateTimeProvider.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldReturnDetailsViewModelWhenEntryExists()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = CreateTestContest(contestId);
            var entry = CreateTestEntry(entryId, contestId, userId);
            entry.Contest = contest;
            entry.Participant = new ApplicationUser { Id = userId, UserName = "TestUser" };

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync(entry);
            MockContestEntryRepository.Setup(x => x.GetEntryRankingInContestAsync(contestId, entryId))
                .ReturnsAsync(1);
            MockContestEntryRepository.Setup(x => x.GetVoteCountsByContestAsync(contestId))
                .ReturnsAsync(new Dictionary<int, int> { { entryId, 5 } });
            MockContestEntryRepository.Setup(x => x.GetByContestIdWithImagesAsync(contestId))
                .ReturnsAsync(new List<ContestEntry> { entry });
            MockVoteRepository.Setup(x => x.GetUserVoteForEntryAsync(userId, entryId))
                .ReturnsAsync((Vote?)null);

            // Act
            var result = await _service.GetContestEntryDetailsAsync(contestId, entryId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(entryId));
            Assert.That(result.ContestId, Is.EqualTo(contestId));
            Assert.That(result.Title, Is.EqualTo(entry.Title));
            Assert.That(result.ParticipantName, Is.EqualTo("TestUser"));
            Assert.That(result.IsOwnEntry, Is.True);
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldReturnNullWhenEntryNotFound()
        {
            // Arrange
            var contestId = 1;
            var entryId = 999;

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync((ContestEntry?)null);

            // Act
            var result = await _service.GetContestEntryDetailsAsync(contestId, entryId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldNotShowUserSpecificDataWhenAnonymousUser()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;

            var contest = CreateTestContest(contestId);
            var entry = CreateTestEntry(entryId, contestId, "owner123");
            entry.Contest = contest;
            entry.Participant = new ApplicationUser { Id = "owner123", UserName = "Owner" };

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync(entry);
            MockContestEntryRepository.Setup(x => x.GetEntryRankingInContestAsync(contestId, entryId))
                .ReturnsAsync(1);
            MockContestEntryRepository.Setup(x => x.GetVoteCountsByContestAsync(contestId))
                .ReturnsAsync(new Dictionary<int, int> { { entryId, 3 } });
            MockContestEntryRepository.Setup(x => x.GetByContestIdWithImagesAsync(contestId))
                .ReturnsAsync(new List<ContestEntry> { entry });

            // Act
            var result = await _service.GetContestEntryDetailsAsync(contestId, entryId, null);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsOwnEntry, Is.False);
            Assert.That(result.CanUserVote, Is.False);
            Assert.That(result.HasUserVoted, Is.False);
            Assert.That(result.CanEdit, Is.False);
        }

        [Test]
        public async Task GetContestEntryForEditAsyncShouldReturnEditViewModelWhenUserOwnsEntry()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = CreateTestContest(contestId, isActive: true);
            contest.SubmissionEndDate = TestDateTime.AddDays(1); // Still in submission period

            var entry = CreateTestEntry(entryId, contestId, userId);
            entry.Contest = contest;

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.GetContestEntryForEditAsync(contestId, entryId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(entryId));
            Assert.That(result.ContestId, Is.EqualTo(contestId));
            Assert.That(result.Title, Is.EqualTo(entry.Title));
        }

        [Test]
        public async Task GetContestEntryForEditAsyncShouldReturnNullWhenUserDoesNotOwnEntry()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";
            var ownerId = "owner456";

            var contest = CreateTestContest(contestId);
            var entry = CreateTestEntry(entryId, contestId, ownerId);
            entry.Contest = contest;

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.GetContestEntryForEditAsync(contestId, entryId, userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetContestEntryForEditAsyncShouldReturnNullWhenSubmissionPeriodEnded()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = CreateTestContest(contestId, isActive: true);
            contest.SubmissionEndDate = TestDateTime.AddDays(-1); // Submission period ended

            var entry = CreateTestEntry(entryId, contestId, userId);
            entry.Contest = contest;

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.GetContestEntryForEditAsync(contestId, entryId, userId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetEntryCountByContestAsyncShouldReturnCorrectCount()
        {
            // Arrange
            var contestId = 1;
            var expectedCount = 5;

            MockContestEntryRepository.Setup(x => x.GetEntryCountByContestAsync(contestId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetEntryCountByContestAsync(contestId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task GetVoteCountByEntryAsyncShouldReturnCorrectCount()
        {
            // Arrange
            var entryId = 1;
            var expectedCount = 3;

            MockContestEntryRepository.Setup(x => x.GetVoteCountByEntryAsync(entryId))
                .ReturnsAsync(expectedCount);

            // Act
            var result = await _service.GetVoteCountByEntryAsync(entryId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCount));
        }

        [Test]
        public async Task GetVoteCountsByContestAsyncShouldReturnDictionary()
        {
            // Arrange
            var contestId = 1;
            var expectedCounts = new Dictionary<int, int>
            {
                { 1, 5 },
                { 2, 3 },
                { 3, 7 }
            };

            MockContestEntryRepository.Setup(x => x.GetVoteCountsByContestAsync(contestId))
                .ReturnsAsync(expectedCounts);

            // Act
            var result = await _service.GetVoteCountsByContestAsync(contestId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[1], Is.EqualTo(5));
            Assert.That(result[2], Is.EqualTo(3));
            Assert.That(result[3], Is.EqualTo(7));
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldRethrowWhenExceptionThrown()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;

            MockContestEntryRepository.Setup(x => x.GetEntryWithAllDataAsync(contestId, entryId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(
                () => _service.GetContestEntryDetailsAsync(contestId, entryId));

            Assert.That(ex.Message, Is.EqualTo("Database error"));
        }
    }
}
