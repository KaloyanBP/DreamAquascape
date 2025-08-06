using DreamAquascape.Data.Repository.Interfaces;

namespace DreamAquascape.Services.Core.Business.Permissions
{
    public class ContestPermissionService : IContestPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ContestPermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CanUserVoteInContestAsync(string userId, int contestId)
        {
            // Check if user already voted - if they have, they cannot vote again
            var hasVoted = await HasUserVotedInContestAsync(userId, contestId);
            if (hasVoted)
                return false;

            // User can vote if they haven't voted yet (even if they own an entry)
            // The check for not voting on their own entry will be done at the entry level
            return true;
        }

        public async Task<bool> HasUserVotedInContestAsync(string userId, int contestId)
        {
            return await _unitOfWork.VoteRepository.HasUserVotedInContestAsync(userId, contestId);
        }

        public async Task<bool> DoesUserOwnEntryInContestAsync(string userId, int contestId)
        {
            var userEntrie = await _unitOfWork.ContestEntryRepository.GetUserEntryInContestAsync(contestId, userId);
            return userEntrie != null && !userEntrie.IsDeleted;
        }

        public async Task<bool> CanUserEditEntryAsync(string userId, int entryId)
        {
            var entry = await _unitOfWork.ContestEntryRepository.GetByIdAsync(entryId);
            return entry != null &&
                   entry.ParticipantId == userId &&
                   !entry.IsDeleted;
        }
    }
}
