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
    }
}
