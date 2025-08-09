using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Business.Permissions;
using DreamAquascape.Services.Core.Business.Rules;
using DreamAquascape.GCommon.Infrastructure;
using DreamAquascape.Services.Core.Interfaces;
using Microsoft.Extensions.Logging;
using DreamAquascape.GCommon;

namespace DreamAquascape.Services.Core
{
    public class VotingService : IVotingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IContestBusinessRules _businessRules;
        private readonly IContestPermissionService _permissionService;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<VotingService> _logger;

        public VotingService(
            IUnitOfWork unitOfWork,
            IContestBusinessRules businessRules,
            IContestPermissionService permissionService,
            IDateTimeProvider dateTimeProvider,
            ILogger<VotingService> logger)
        {
            _unitOfWork = unitOfWork;
            _businessRules = businessRules;
            _permissionService = permissionService;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
        }

        public async Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string? ipAddress = null)
        {
            try
            {
                // 1. Get contest and validate existence
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);
                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(String.Format(ExceptionMessages.ContestNotFoundMessage, contestId));

                // 2. Validate voting period using business rules
                if (!_businessRules.IsVotingPeriodActive(contest, _dateTimeProvider.UtcNow))
                    throw new InvalidOperationException(ExceptionMessages.ContestVotingPeriodNotActiveMessage);

                // 3. Get entry and validate
                var entry = await _unitOfWork.ContestEntryRepository.GetByIdAsync(entryId);
                if (entry == null || entry.ContestId != contestId || entry.IsDeleted)
                    throw new NotFoundException(ExceptionMessages.ContestEntryNotFoundMessage);

                // 4. Check if user has already voted in this contest
                if (!await _permissionService.CanUserVoteInContestAsync(userId, contestId))
                {
                    throw new InvalidOperationException(ExceptionMessages.UserAlreadyVotedInContestMessage);
                }

                // 5. Check if user is trying to vote for their own entry
                if (entry.ParticipantId == userId)
                {
                    throw new InvalidOperationException(ExceptionMessages.UserCannotVoteForOwnEntryMessage);
                }

                // 6. Create and save vote
                var vote = new Vote
                {
                    ContestEntryId = entryId,
                    UserId = userId,
                    VotedAt = _dateTimeProvider.UtcNow,
                    IpAddress = ipAddress
                };

                await _unitOfWork.VoteRepository.AddAsync(vote);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vote cast by user {UserId} for entry {EntryId} in contest {ContestId}",
                    userId, entryId, contestId);

                return vote;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cast vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task RemoveVoteAsync(int contestId, string userId)
        {
            try
            {
                // 1. Validate voting period
                var contest = await _unitOfWork.ContestRepository.GetByIdAsync(contestId);
                if (contest == null || !contest.IsActive || contest.IsDeleted)
                    throw new NotFoundException(ExceptionMessages.ContestNotFoundErrorMessage);

                if (!_businessRules.IsVotingPeriodActive(contest, _dateTimeProvider.UtcNow))
                    throw new InvalidOperationException(ExceptionMessages.ContestVotingPeriodNotActiveMessage);

                // 2. Get existing vote
                var existingVote = await _unitOfWork.VoteRepository.GetUserVoteInContestAsync(userId, contestId);
                if (existingVote == null)
                    throw new NotFoundException(ExceptionMessages.NoExistingVoteFoundMessage);

                // 3. Remove the vote
                await _unitOfWork.VoteRepository.DeleteAsync(existingVote, _dateTimeProvider.UtcNow, userId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Vote removed by user {UserId} in contest {ContestId}",
                    userId, contestId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task<Vote?> GetUserVoteInContestAsync(string userId, int contestId)
        {
            return await _unitOfWork.VoteRepository.GetUserVoteInContestAsync(userId, contestId);
        }

        public async Task<bool> HasUserVotedInContestAsync(string userId, int contestId)
        {
            return await _unitOfWork.VoteRepository.HasUserVotedInContestAsync(userId, contestId);
        }
    }
}
