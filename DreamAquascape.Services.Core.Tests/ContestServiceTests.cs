using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.Extensions.Logging;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestServiceTests : ServiceTestBase
    {
        private ContestService _service = null!;
        private Mock<ILogger<ContestService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<ContestService>();

            _service = new ContestService(
                MockUnitOfWork.Object,
                _mockLogger.Object,
                MockDateTimeProvider.Object);
        }

        [Test]
        public async Task SubmitContestAsyncShouldCreateContestSuccessfullyWhenValidDataProvided()
        {
            // Arrange
            var dto = new CreateContestViewModel
            {
                Title = "Test Contest",
                Description = "Test Description",
                ImageFileUrl = "test-image.jpg",
                SubmissionStartDate = TestDateTime.AddDays(1),
                SubmissionEndDate = TestDateTime.AddDays(10),
                VotingStartDate = TestDateTime.AddDays(11),
                VotingEndDate = TestDateTime.AddDays(20),
                ResultDate = TestDateTime.AddDays(21)
            };

            var prizeDto = new PrizeViewModel
            {
                Name = "Test Prize",
                Description = "Test Prize Description",
                ImageUrl = "prize-image.jpg"
            };

            var expectedContest = CreateTestContest(1, true);
            expectedContest.Title = dto.Title;
            expectedContest.Description = dto.Description;

            MockContestRepository.Setup(x => x.CreateContestWithPrizeAsync(It.IsAny<Contest>(), It.IsAny<Prize>()))
                .ReturnsAsync(expectedContest);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.SubmitContestAsync(dto, prizeDto, "test-user");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Title, Is.EqualTo("Test Contest"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));
            MockContestRepository.Verify(x => x.CreateContestWithPrizeAsync(It.IsAny<Contest>(), It.IsAny<Prize>()), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void SubmitContestAsyncShouldThrowExceptionWhenInvalidDatesProvided()
        {
            // Arrange
            var dto = new CreateContestViewModel
            {
                Title = "Test Contest",
                Description = "Test Description",
                SubmissionStartDate = TestDateTime.AddDays(10), // Start after end
                SubmissionEndDate = TestDateTime.AddDays(5),   // End before start
                VotingStartDate = TestDateTime.AddDays(11),
                VotingEndDate = TestDateTime.AddDays(20),
                ResultDate = TestDateTime.AddDays(21)
            };

            var prizeDto = new PrizeViewModel
            {
                Name = "Test Prize",
                Description = "Test Prize Description"
            };

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.SubmitContestAsync(dto, prizeDto, "test-user"));
        }

        [Test]
        public async Task DetermineAndSetWinnerAsyncShouldSetWinnerWhenValidContestProvided()
        {
            // Arrange
            var contestId = 1;
            var contest = CreateTestContest(contestId, true);
            contest.VotingEndDate = TestDateTime.AddDays(-1); // Voting ended
            contest.Winners = new List<ContestWinner>(); // No existing winners

            var entry1 = CreateTestEntry(1, contestId, "user1");
            entry1.Votes = new List<Vote> { new Vote(), new Vote(), new Vote() }; // 3 votes

            var entry2 = CreateTestEntry(2, contestId, "user2");
            entry2.Votes = new List<Vote> { new Vote(), new Vote() }; // 2 votes

            contest.Entries = new List<ContestEntry> { entry1, entry2 };

            MockContestRepository.Setup(x => x.GetContestForWinnerDeterminationAsync(contestId))
                .ReturnsAsync(contest);

            MockContestWinnerRepository.Setup(x => x.AddAsync(It.IsAny<ContestWinner>()))
                .Returns(Task.CompletedTask);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DetermineAndSetWinnerAsync(contestId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContestEntryId, Is.EqualTo(1)); // Entry with most votes
            Assert.That(result.Position, Is.EqualTo(1));
            MockContestWinnerRepository.Verify(x => x.AddAsync(It.IsAny<ContestWinner>()), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DetermineAndSetWinnerAsyncShouldReturnNullWhenNonExistentContestProvided()
        {
            // Arrange
            MockContestRepository.Setup(x => x.GetContestForWinnerDeterminationAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act
            var result = await _service.DetermineAndSetWinnerAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ToggleContestActiveStatusAsyncShouldToggleStatusWhenValidContestProvided()
        {
            // Arrange
            var contestId = 1;
            var contest = CreateTestContest(contestId, true); // Initially active

            MockContestRepository.Setup(x => x.GetContestForToggleAsync(contestId))
                .ReturnsAsync(contest);

            MockContestRepository.Setup(x => x.UpdateAsync(It.IsAny<Contest>()))
                .ReturnsAsync(true);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.ToggleContestActiveStatusAsync(contestId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(contest.IsActive, Is.False); // Should be toggled to false
            MockContestRepository.Verify(x => x.UpdateAsync(It.IsAny<Contest>()), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task ToggleContestActiveStatusAsyncShouldReturnFalseWhenNonExistentContestProvided()
        {
            // Arrange
            MockContestRepository.Setup(x => x.GetContestForToggleAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act
            var result = await _service.ToggleContestActiveStatusAsync(999);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteContestAsyncShouldDeleteSuccessfullyWhenValidContestWithNoEntriesProvided()
        {
            // Arrange
            var contestId = 1;
            var contest = CreateTestContest(contestId, true);
            contest.Entries = new List<ContestEntry>(); // No entries

            MockContestRepository.Setup(x => x.GetContestForDeleteAsync(contestId))
                .ReturnsAsync(contest);

            MockContestRepository.Setup(x => x.UpdateAsync(It.IsAny<Contest>()))
                .ReturnsAsync(true);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteContestAsync(contestId);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(contest.IsDeleted, Is.True);
            MockContestRepository.Verify(x => x.UpdateAsync(It.IsAny<Contest>()), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteContestAsyncShouldReturnFalseWhenContestHasExistingEntries()
        {
            // Arrange
            var contestId = 1;
            var contest = CreateTestContest(contestId, true);
            contest.Entries = new List<ContestEntry>
            {
                CreateTestEntry(1, contestId, "user1") // Has entries
            };

            MockContestRepository.Setup(x => x.GetContestForDeleteAsync(contestId))
                .ReturnsAsync(contest);

            // Act
            var result = await _service.DeleteContestAsync(contestId);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(contest.IsDeleted, Is.False); // Should not be deleted
        }

        [Test]
        public async Task UpdateContestAsyncShouldUpdateSuccessfullyWhenValidDataProvided()
        {
            // Arrange
            var contest = CreateTestContest(1, true);
            var model = new EditContestViewModel
            {
                Id = 1,
                Title = "Updated Title",
                Description = "Updated Description",
                SubmissionStartDate = TestDateTime.AddDays(1),
                SubmissionEndDate = TestDateTime.AddDays(10),
                VotingStartDate = TestDateTime.AddDays(11),
                VotingEndDate = TestDateTime.AddDays(20),
                ResultDate = TestDateTime.AddDays(21),
                IsActive = false
            };

            MockContestRepository.Setup(x => x.GetContestForEditAsync(1))
                .ReturnsAsync(contest);

            MockUnitOfWork.Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            MockContestRepository.Setup(x => x.UpdateAsync(It.IsAny<Contest>()))
                .ReturnsAsync(true);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            MockUnitOfWork.Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateContestAsync(model);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(contest.Title, Is.EqualTo("Updated Title"));
            Assert.That(contest.Description, Is.EqualTo("Updated Description"));
            Assert.That(contest.IsActive, Is.False);
        }

        [Test]
        public async Task UpdateContestAsyncShouldReturnFalseWhenNonExistentContestProvided()
        {
            // Arrange
            var model = new EditContestViewModel
            {
                Id = 999,
                Title = "Updated Title",
                Description = "Updated Description"
            };

            MockContestRepository.Setup(x => x.GetContestForEditAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act
            var result = await _service.UpdateContestAsync(model);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void SubmitContestAsyncShouldThrowExceptionWhenSubmissionStartDateAfterEndDate()
        {
            // Arrange
            var dto = new CreateContestViewModel
            {
                Title = "Test Contest",
                Description = "Test Description",
                SubmissionStartDate = TestDateTime.AddDays(10), // Start after end
                SubmissionEndDate = TestDateTime.AddDays(5),    // End before start
                VotingStartDate = TestDateTime.AddDays(11),
                VotingEndDate = TestDateTime.AddDays(20),
                ResultDate = TestDateTime.AddDays(21)
            };

            var prizeDto = new PrizeViewModel
            {
                Name = "Test Prize",
                Description = "Test Prize Description"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.SubmitContestAsync(dto, prizeDto, "test-user"));

            Assert.That(ex.Message, Contains.Substring("Submission start date must be before end date"));
        }

        [Test]
        public void SubmitContestAsyncShouldThrowExceptionWhenVotingDatesInvalid()
        {
            // Arrange
            var dto = new CreateContestViewModel
            {
                Title = "Test Contest",
                Description = "Test Description",
                SubmissionStartDate = TestDateTime.AddDays(1),
                SubmissionEndDate = TestDateTime.AddDays(10),
                VotingStartDate = TestDateTime.AddDays(1), // Voting starts at same time as submission
                VotingEndDate = TestDateTime.AddDays(8),   // This violates the rule: VotingStartDate <= SubmissionStartDate
                ResultDate = TestDateTime.AddDays(21)
            };

            var prizeDto = new PrizeViewModel
            {
                Name = "Test Prize",
                Description = "Test Prize Description"
            };

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.SubmitContestAsync(dto, prizeDto, "test-user"));

            Assert.That(ex.Message, Contains.Substring("Start voting date must be after submission start date"));
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldReturnNullWhenEntryNotFound()
        {
            // Arrange
            MockContestEntryRepository.Setup(x => x.GetEntryDetailsWithAllDataAsync(1, 999))
                .ReturnsAsync((ContestEntry?)null);

            // Act
            var result = await _service.GetContestEntryDetailsAsync(1, 999);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetContestEntryDetailsAsyncShouldCalculateCorrectRankingAndPercentage()
        {
            // Arrange
            var contest = CreateTestContest(1);
            contest.Winners = new HashSet<ContestWinner>();

            var entry1 = CreateTestEntry(1, 1, "user1");
            var entry2 = CreateTestEntry(2, 1, "user2");
            var entry3 = CreateTestEntry(3, 1, "user3");

            // Setup votes - entry2 has most votes (3), entry1 has 2, entry3 has 1
            var vote1 = CreateTestVote(1, 1, "voter1");
            vote1.User = CreateTestUser("voter1", "Voter1");
            var vote2 = CreateTestVote(2, 1, "voter2");
            vote2.User = CreateTestUser("voter2", "Voter2");
            var vote3 = CreateTestVote(3, 2, "voter1");
            vote3.User = CreateTestUser("voter1", "Voter1");
            var vote4 = CreateTestVote(4, 2, "voter3");
            vote4.User = CreateTestUser("voter3", "Voter3");
            var vote5 = CreateTestVote(5, 2, "voter4");
            vote5.User = CreateTestUser("voter4", "Voter4");
            var vote6 = CreateTestVote(6, 3, "voter5");
            vote6.User = CreateTestUser("voter5", "Voter5");

            entry1.Votes = new HashSet<Vote> { vote1, vote2 };
            entry2.Votes = new HashSet<Vote> { vote3, vote4, vote5 };
            entry3.Votes = new HashSet<Vote> { vote6 };

            entry1.Contest = contest;
            entry1.Participant = CreateTestUser("user1", "User1");
            entry1.EntryImages = new HashSet<EntryImage>();

            var allEntries = new List<ContestEntry> { entry1, entry2, entry3 };

            MockContestEntryRepository.Setup(x => x.GetEntryDetailsWithAllDataAsync(1, 1))
                .ReturnsAsync(entry1);
            MockContestEntryRepository.Setup(x => x.GetAllEntriesInContestAsync(1))
                .ReturnsAsync(allEntries);
            MockVoteRepository.Setup(x => x.GetUserVoteForEntryAsync("test-user", 1))
                .ReturnsAsync((Vote?)null);

            // Act
            var result = await _service.GetContestEntryDetailsAsync(1, 1, "test-user");

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.EntryRanking, Is.EqualTo(2)); // entry1 should be ranked 2nd (entry2 has more votes)
            Assert.That(Math.Round(result.VotePercentage, 2), Is.EqualTo(33.33)); // 2 votes out of 6 total = 33.33%
        }

        [Test]
        public async Task ProcessEndedContestsAsyncShouldReturnEmptyListWhenNoEndedContests()
        {
            // Arrange
            var endedContests = new List<Contest>();

            MockContestRepository.Setup(x => x.GetEndedContestsWithoutWinnersAsync())
                .ReturnsAsync(endedContests);

            // Act
            var result = await _service.ProcessEndedContestsAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Count, Is.EqualTo(0));
            MockContestRepository.Verify(x => x.GetEndedContestsWithoutWinnersAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteContestAsyncShouldReturnFalseWhenNonExistentContest()
        {
            // Arrange
            MockContestRepository.Setup(x => x.GetContestForDeleteAsync(999))
                .ReturnsAsync((Contest?)null);

            // Act
            var result = await _service.DeleteContestAsync(999);

            // Assert
            Assert.IsFalse(result);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        }
    }
}
