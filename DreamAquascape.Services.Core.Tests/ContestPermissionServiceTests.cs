using DreamAquascape.Data.Models;
using DreamAquascape.Services.Core.Business.Permissions;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using Moq;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class ContestPermissionServiceBasicTests : ServiceTestBase
    {
        private ContestPermissionService _service = null!;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _service = new ContestPermissionService(MockUnitOfWork.Object);
        }

        [Test]
        public async Task CanUserVoteInContestAsyncShouldReturnTrueWhenUserHasNotVoted()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanUserVoteInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.True);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }

        [Test]
        public async Task CanUserVoteInContestAsyncShouldReturnFalseWhenUserHasAlreadyVoted()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanUserVoteInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }

        [Test]
        public async Task HasUserVotedInContestAsyncShouldReturnTrueWhenUserHasVoted()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.HasUserVotedInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.True);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }

        [Test]
        public async Task HasUserVotedInContestAsyncShouldReturnFalseWhenUserHasNotVoted()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.HasUserVotedInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }

        [Test]
        public async Task DoesUserOwnEntryInContestAsyncShouldReturnTrueWhenUserOwnsActiveEntry()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;
            var userEntry = CreateTestEntry(1, contestId, userId);
            userEntry.IsDeleted = false;

            MockContestEntryRepository.Setup(x => x.GetUserEntryInContestAsync(contestId, userId))
                .ReturnsAsync(userEntry);

            // Act
            var result = await _service.DoesUserOwnEntryInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.True);
            MockContestEntryRepository.Verify(x => x.GetUserEntryInContestAsync(contestId, userId), Times.Once);
        }

        [Test]
        public async Task DoesUserOwnEntryInContestAsyncShouldReturnFalseWhenUserOwnsDeletedEntry()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;
            var userEntry = CreateTestEntry(1, contestId, userId);
            userEntry.IsDeleted = true; // Entry is deleted

            MockContestEntryRepository.Setup(x => x.GetUserEntryInContestAsync(contestId, userId))
                .ReturnsAsync(userEntry);

            // Act
            var result = await _service.DoesUserOwnEntryInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetUserEntryInContestAsync(contestId, userId), Times.Once);
        }

        [Test]
        public async Task DoesUserOwnEntryInContestAsyncShouldReturnFalseWhenUserHasNoEntry()
        {
            // Arrange
            string userId = "test-user";
            int contestId = 1;

            MockContestEntryRepository.Setup(x => x.GetUserEntryInContestAsync(contestId, userId))
                .ReturnsAsync((ContestEntry?)null);

            // Act
            var result = await _service.DoesUserOwnEntryInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetUserEntryInContestAsync(contestId, userId), Times.Once);
        }

        [Test]
        public async Task CanUserEditEntryAsyncShouldReturnTrueWhenUserOwnsActiveEntry()
        {
            // Arrange
            string userId = "test-user";
            int entryId = 1;
            var entry = CreateTestEntry(entryId, 1, userId);
            entry.IsDeleted = false;

            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.CanUserEditEntryAsync(userId, entryId);

            // Assert
            Assert.That(result, Is.True);
            MockContestEntryRepository.Verify(x => x.GetByIdAsync(entryId), Times.Once);
        }

        [Test]
        public async Task CanUserEditEntryAsyncShouldReturnFalseWhenUserOwnsDeletedEntry()
        {
            // Arrange
            string userId = "test-user";
            int entryId = 1;
            var entry = CreateTestEntry(entryId, 1, userId);
            entry.IsDeleted = true; // Entry is deleted

            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.CanUserEditEntryAsync(userId, entryId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetByIdAsync(entryId), Times.Once);
        }

        [Test]
        public async Task CanUserEditEntryAsyncShouldReturnFalseWhenUserDoesNotOwnEntry()
        {
            // Arrange
            string userId = "test-user";
            string otherUserId = "other-user";
            int entryId = 1;
            var entry = CreateTestEntry(entryId, 1, otherUserId); // Entry belongs to another user
            entry.IsDeleted = false;

            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.CanUserEditEntryAsync(userId, entryId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetByIdAsync(entryId), Times.Once);
        }

        [Test]
        public async Task CanUserEditEntryAsyncShouldReturnFalseWhenEntryDoesNotExist()
        {
            // Arrange
            string userId = "test-user";
            int entryId = 999;

            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync((ContestEntry?)null);

            // Act
            var result = await _service.CanUserEditEntryAsync(userId, entryId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetByIdAsync(entryId), Times.Once);
        }

        [Test]
        public async Task CanUserVoteInContestAsyncShouldReturnTrueWhenUserOwnsEntryButHasNotVoted()
        {
            // Arrange - User owns an entry but hasn't voted yet
            // Users can vote even if they own an entry in the contest
            // The check for not voting on their own entry is done at the entry level
            string userId = "test-user";
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanUserVoteInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.True);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }

        [Test]
        public async Task DoesUserOwnEntryInContestAsyncShouldHandleNullUserId()
        {
            // Arrange
            string? userId = null;
            int contestId = 1;

            MockContestEntryRepository.Setup(x => x.GetUserEntryInContestAsync(contestId, userId))
                .ReturnsAsync((ContestEntry?)null);

            // Act
            var result = await _service.DoesUserOwnEntryInContestAsync(userId!, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetUserEntryInContestAsync(contestId, userId), Times.Once);
        }

        [Test]
        public async Task CanUserEditEntryAsyncShouldHandleNullUserId()
        {
            // Arrange
            string? userId = null;
            int entryId = 1;
            var entry = CreateTestEntry(entryId, 1, "other-user");

            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);

            // Act
            var result = await _service.CanUserEditEntryAsync(userId!, entryId);

            // Assert
            Assert.That(result, Is.False);
            MockContestEntryRepository.Verify(x => x.GetByIdAsync(entryId), Times.Once);
        }

        [Test]
        public async Task HasUserVotedInContestAsyncShouldHandleNullUserId()
        {
            // Arrange
            string? userId = null;
            int contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.HasUserVotedInContestAsync(userId!, contestId);

            // Assert
            Assert.That(result, Is.False);
            MockVoteRepository.Verify(x => x.HasUserVotedInContestAsync(userId, contestId), Times.Once);
        }
    }
}
