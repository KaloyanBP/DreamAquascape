using DreamAquascape.Data.Models;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Tests.Infrastructure;
using Microsoft.Extensions.Logging;
using Moq;
using static DreamAquascape.GCommon.ExceptionMessages;

namespace DreamAquascape.Services.Core.Tests
{
    [TestFixture]
    public class VotingServiceTests : ServiceTestBase
    {
        private VotingService _votingService;
        private Mock<ILogger<VotingService>> _mockLogger;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
            _mockLogger = CreateMockLogger<VotingService>();

            _votingService = new VotingService(
                MockUnitOfWork.Object,
                MockBusinessRules.Object,
                MockPermissionService.Object,
                MockDateTimeProvider.Object,
                _mockLogger.Object);
        }

        #region CastVoteAsync Tests

        [Test]
        public async Task CastVoteAsyncShouldReturnVoteOnValidRequest()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";
            var ipAddress = "192.168.1.1";

            var contest = CreateTestContest(contestId);
            var entry = CreateTestEntry(entryId, contestId, "otheruser");

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);
            MockPermissionService.Setup(x => x.CanUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync(true);

            // Act
            var result = await _votingService.CastVoteAsync(contestId, entryId, userId, ipAddress);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ContestEntryId, Is.EqualTo(entryId));
            Assert.That(result.UserId, Is.EqualTo(userId));
            Assert.That(result.CreatedAt, Is.EqualTo(TestDateTime));
            Assert.That(result.IpAddress, Is.EqualTo(ipAddress));

            MockVoteRepository.Verify(x => x.AddAsync(It.IsAny<Vote>()), Times.Once);
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowNotFoundExceptionWhenContestIsNotFound()
        {
            // Arrange
            var contestId = 999;
            var entryId = 1;
            var userId = "user123";

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync((Contest?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Does.Contain(contestId.ToString()));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowNotFoundExceptionWhenContestIsInactive()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = false, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Does.Contain(contestId.ToString()));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowInvalidOperationExceptionWhenTheVotingPeriodIsNotActive()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(ContestVotingPeriodNotActiveMessage));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowNotFoundExceptionWhenEntryNotFound()
        {
            // Arrange
            var contestId = 1;
            var entryId = 999;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync((ContestEntry?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(ContestEntryNotFoundMessage));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowNotFoundExceptionWhenEntryFromDifferentContest()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };
            var entry = new ContestEntry { Id = entryId, ContestId = 2, ParticipantId = "otheruser", IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(ContestEntryNotFoundMessage));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowInvalidOperationExceptionWhenUserAlreadyVoted()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };
            var entry = new ContestEntry { Id = entryId, ContestId = contestId, ParticipantId = "otheruser", IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);
            MockPermissionService.Setup(x => x.CanUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(UserAlreadyVotedInContestMessage));
        }

        [Test]
        public async Task CastVoteAsyncShouldThrowInvalidOperationExceptionWhenUserVotingForOwnEntry()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };
            var entry = new ContestEntry { Id = entryId, ContestId = contestId, ParticipantId = userId, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockContestEntryRepository.Setup(x => x.GetByIdAsync(entryId))
                .ReturnsAsync(entry);
            MockPermissionService.Setup(x => x.CanUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync(true);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(UserCannotVoteForOwnEntryMessage));
        }

        #endregion

        #region RemoveVoteAsync Tests

        [Test]
        public async Task RemoveVoteAsync_ValidRequest_SoftDeletesVote()
        {
            // Arrange
            var contestId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };
            var existingVote = new Vote { Id = 1, ContestEntryId = 1, UserId = userId };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockVoteRepository.Setup(x => x.GetUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync(existingVote);

            // Setup the mock to actually modify the vote when DeleteAsync is called
            MockVoteRepository.Setup(x => x.DeleteAsync(existingVote, TestDateTime, userId))
                .Callback<Vote, DateTime?, string?>((vote, deletedAt, deletedBy) =>
                {
                    vote.IsDeleted = true;
                    vote.DeletedAt = deletedAt;
                    vote.DeletedBy = deletedBy;
                })
                .ReturnsAsync(true);

            // Act
            await _votingService.RemoveVoteAsync(contestId, userId);

            // Assert
            Assert.That(existingVote.IsDeleted, Is.True);
            Assert.That(existingVote.DeletedAt, Is.EqualTo(TestDateTime));
            Assert.That(existingVote.DeletedBy, Is.EqualTo(userId));
            MockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RemoveVoteAsync_ContestNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var contestId = 999;
            var userId = "user123";

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync((Contest?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.RemoveVoteAsync(contestId, userId));

            Assert.That(ex.Message, Is.EqualTo(ContestNotFoundErrorMessage));
        }

        [Test]
        public async Task RemoveVoteAsyncShouldThrowInvalidOperationExceptionWhenVotingPeriodIsNotActive()
        {
            // Arrange
            var contestId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(false);

            // Act & Assert
            var ex = Assert.ThrowsAsync<InvalidOperationException>(
                () => _votingService.RemoveVoteAsync(contestId, userId));

            Assert.That(ex.Message, Is.EqualTo(ContestVotingPeriodNotActiveMessage));
        }

        [Test]
        public async Task RemoveVoteAsyncShouldThrowNotFoundExceptionWhenNoExistingVote()
        {
            // Arrange
            var contestId = 1;
            var userId = "user123";

            var contest = new Contest { Id = contestId, IsActive = true, IsDeleted = false };

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ReturnsAsync(contest);
            MockBusinessRules.Setup(x => x.IsVotingPeriodActive(contest, TestDateTime))
                .Returns(true);
            MockVoteRepository.Setup(x => x.GetUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync((Vote?)null);

            // Act & Assert
            var ex = Assert.ThrowsAsync<NotFoundException>(
                () => _votingService.RemoveVoteAsync(contestId, userId));

            Assert.That(ex.Message, Is.EqualTo(NoExistingVoteFoundMessage));
        }

        #endregion

        #region GetUserVoteInContestAsync Tests

        [Test]
        public async Task GetUserVoteInContestAsyncShouldReturnVoteWhenVoteExists()
        {
            // Arrange
            var userId = "user123";
            var contestId = 1;
            var expectedVote = new Vote { Id = 1, UserId = userId, ContestEntryId = 1 };

            MockVoteRepository.Setup(x => x.GetUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync(expectedVote);

            // Act
            var result = await _votingService.GetUserVoteInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.EqualTo(expectedVote));
        }

        [Test]
        public async Task GetUserVoteInContestAsyncShouldReturnNullWhenVoteDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            var contestId = 1;

            MockVoteRepository.Setup(x => x.GetUserVoteInContestAsync(userId, contestId))
                .ReturnsAsync((Vote?)null);

            // Act
            var result = await _votingService.GetUserVoteInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.Null);
        }

        #endregion

        #region HasUserVotedInContestAsync Tests

        [Test]
        public async Task HasUserVotedInContestAsyncShouldReturnTrueWhenUserHasVoted()
        {
            // Arrange
            var userId = "user123";
            var contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(true);

            // Act
            var result = await _votingService.HasUserVotedInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task HasUserVotedInContestAsyncShouldReturnFalseWhenUserHasNotVoted()
        {
            // Arrange
            var userId = "user123";
            var contestId = 1;

            MockVoteRepository.Setup(x => x.HasUserVotedInContestAsync(userId, contestId))
                .ReturnsAsync(false);

            // Act
            var result = await _votingService.HasUserVotedInContestAsync(userId, contestId);

            // Assert
            Assert.That(result, Is.False);
        }

        #endregion

        #region Exception Handling Tests

        [Test]
        public async Task CastVoteAsyncShouldRethrowWhenRepositoryThrowsException()
        {
            // Arrange
            var contestId = 1;
            var entryId = 1;
            var userId = "user123";

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ThrowsAsync(new Exception(DatabaseConnectionFailedMessage));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(
                () => _votingService.CastVoteAsync(contestId, entryId, userId));

            Assert.That(ex.Message, Is.EqualTo(DatabaseConnectionFailedMessage));
        }

        [Test]
        public async Task RemoveVoteAsyncShowlRethrowWhenRepositoryThrowsException()
        {
            // Arrange
            var contestId = 1;
            var userId = "user123";

            MockContestRepository.Setup(x => x.GetByIdAsync(contestId))
                .ThrowsAsync(new Exception(DatabaseConnectionFailedMessage));

            // Act & Assert
            var ex = Assert.ThrowsAsync<Exception>(
                () => _votingService.RemoveVoteAsync(contestId, userId));

            Assert.That(ex.Message, Is.EqualTo(DatabaseConnectionFailedMessage));
        }

        #endregion
    }
}
