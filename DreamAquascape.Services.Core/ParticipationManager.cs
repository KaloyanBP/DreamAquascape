using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Services.Core
{
    public class ParticipationManager
    {
        private readonly ApplicationDbContext _context;

        public ParticipationManager(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserContestParticipation> EnsureParticipationAsync(
            int contestId, string userId, string userName)
        {
            var participation = await _context.UserContestParticipations
                .FirstOrDefaultAsync(p => p.ContestId == contestId && p.UserId == userId);

            if (participation == null)
            {
                participation = new UserContestParticipation
                {
                    ContestId = contestId,
                    UserId = userId,
                    ParticipationDate = DateTime.UtcNow,
                    HasSubmittedEntry = false,
                    HasVoted = false
                };
                await _context.UserContestParticipations.AddAsync(participation);
            }

            return participation;
        }

        public async Task UpdateEntryParticipationAsync(
            UserContestParticipation participation, int entryId)
        {
            participation.HasSubmittedEntry = true;
            participation.SubmittedEntryId = entryId;
            participation.EntrySubmittedAt = DateTime.UtcNow;
        }

        public async Task UpdateVoteParticipationAsync(
            UserContestParticipation participation, int entryId)
        {
            participation.HasVoted = true;
            participation.VotedForEntryId = entryId;
            participation.VotedAt = DateTime.UtcNow;
        }

        public async Task ClearEntryParticipationAsync(UserContestParticipation participation)
        {
            participation.HasSubmittedEntry = false;
            participation.SubmittedEntryId = null;
            participation.EntrySubmittedAt = null;

            // Remove participation if no other engagement
            if (!participation.HasVoted)
            {
                _context.UserContestParticipations.Remove(participation);
            }
        }

        public async Task ClearVoteParticipationAsync(UserContestParticipation participation)
        {
            participation.HasVoted = false;
            participation.VotedForEntryId = null;
            participation.VotedAt = null;

            // Remove participation if no other engagement
            if (!participation.HasSubmittedEntry)
            {
                _context.UserContestParticipations.Remove(participation);
            }
        }
    }
}
