using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.Extensions.Logging;
using Moq;
using DreamAquascape.Services.Common.Exceptions;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestEntryServiceTests : ServiceTestBase
    {
        private ContestEntryService _service = null!;
        private Mock<ILogger<ContestEntryService>> _mockLogger = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<ContestEntryService>();

            _service = new ContestEntryService(
                _mockLogger.Object,
                MockUnitOfWork.Object,
                MockDateTimeProvider.Object);
        }

        [Test]
        public async Task SubmitEntryAsyncShouldCreateEntrySuccessfullyWithValidData()
        {
            // Arrange
            var contest = CreateTestContest(1, true);
            contest.SubmissionStartDate = TestDateTime.AddDays(-5);
            contest.SubmissionEndDate = TestDateTime.AddDays(5);

            var dto = new CreateContestEntryViewModel
            {
                ContestId = 1,
                Title = "Test Entry",
                Description = "Test Description",
                EntryImages = new List<string> { "image1.jpg", "image2.jpg" }
            };

            MockContestRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(contest);

            MockContestEntryRepository.Setup(x => x.UserHasEntryInContestAsync(1, "test-user"))
                .ReturnsAsync(false);

            MockContestEntryRepository.Setup(x => x.AddAsync(It.IsAny<ContestEntry>()))
                .Returns(Task.CompletedTask);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            MockUnitOfWork.Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            MockUnitOfWork.Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.SubmitEntryAsync(dto, "test-user", "TestUser");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContestId, Is.EqualTo(1));
            Assert.That(result.ParticipantId, Is.EqualTo("test-user"));
            Assert.That(result.Title, Is.EqualTo("Test Entry"));
            Assert.That(result.Description, Is.EqualTo("Test Description"));
            Assert.That(result.SubmittedAt, Is.EqualTo(TestDateTime));
        }

        [Test]
        public void SubmitEntryAsyncShouldThrowExceptionWhenContestIsNotExisting()
        {
            // Arrange
            var dto = new CreateContestEntryViewModel
            {
                ContestId = 999,
                Title = "Test Entry",
                Description = "Test Description",
                EntryImages = new List<string> { "image1.jpg" }
            };

            MockContestRepository.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.SubmitEntryAsync(dto, "test-user", "TestUser"));
        }

        [Test]
        public async Task UpdateContestEntryAsyncShouldReturnTrueWhenValidData()
        {
            // Arrange
            var contest = CreateTestContest(1, true);
            contest.SubmissionStartDate = TestDateTime.AddDays(-5);
            contest.SubmissionEndDate = TestDateTime.AddDays(5);

            var entry = CreateTestEntry(1, 1, "test-user");

            var model = new EditContestEntryViewModel
            {
                Id = 1,
                ContestId = 1,
                Title = "Updated Title",
                Description = "Updated Description"
            };

            MockContestRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(contest);

            MockContestEntryRepository.Setup(x => x.GetEntryForEditAsync(1, 1, "test-user"))
                .ReturnsAsync(entry);

            MockUnitOfWork.Setup(x => x.BeginTransactionAsync())
                .Returns(Task.CompletedTask);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            MockUnitOfWork.Setup(x => x.CommitTransactionAsync())
                .Returns(Task.CompletedTask);

            MockContestEntryRepository.Setup(x => x.UpdateAsync(It.IsAny<ContestEntry>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateContestEntryAsync(model, "test-user");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(entry.Title, Is.EqualTo("Updated Title"));
            Assert.That(entry.Description, Is.EqualTo("Updated Description"));
        }

        [Test]
        public async Task UpdateContestEntryAsyncShouldReturnFalseWhenTheContestDonotExists()
        {
            // Arrange
            var model = new EditContestEntryViewModel
            {
                Id = 1,
                ContestId = 999,
                Title = "Updated Title",
                Description = "Updated Description"
            };

            MockContestRepository.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act
            var result = await _service.UpdateContestEntryAsync(model, "test-user");

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteEntryAsyncShouldReturnTrueWithValidData()
        {
            // Arrange
            var contest = CreateTestContest(1, true);
            contest.SubmissionStartDate = TestDateTime.AddDays(-5);
            contest.SubmissionEndDate = TestDateTime.AddDays(5);

            var entry = CreateTestEntry(1, 1, "test-user");

            MockContestRepository.Setup(x => x.GetByIdAsync(1))
                .ReturnsAsync(contest);

            MockContestEntryRepository.Setup(x => x.GetEntryForEditAsync(1, 1, "test-user"))
                .ReturnsAsync(entry);

            MockContestEntryRepository.Setup(x => x.UpdateAsync(It.IsAny<ContestEntry>()))
                .ReturnsAsync(true);

            MockUnitOfWork.Setup(x => x.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _service.DeleteEntryAsync(1, 1, "test-user");

            // Assert
            Assert.That(result, Is.True);
            Assert.That(entry.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteEntryAsyncShouldReturnFalseWithNonExistentContest()
        {
            // Arrange
            MockContestRepository.Setup(x => x.GetByIdAsync(999))
                .ReturnsAsync((Contest)null!);

            // Act
            var result = await _service.DeleteEntryAsync(999, 1, "test-user");

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
