using DreamAquascape.Data.Models;

namespace DreamAquascape.Services.Core.Business.Rules
{
    public class ContestBusinessRules : IContestBusinessRules
    {
        public bool CanUserVote(Contest contest, string userId, DateTime currentTime)
        {
            return contest.IsActive &&
                   !contest.IsDeleted &&
                   IsVotingPeriodActive(contest, currentTime);
        }

        public bool IsVotingPeriodActive(Contest contest, DateTime currentTime)
        {
            return currentTime >= contest.VotingStartDate &&
                   currentTime <= contest.VotingEndDate;
        }

        public bool IsSubmissionPeriodActive(Contest contest, DateTime currentTime)
        {
            return currentTime >= contest.SubmissionStartDate &&
                   currentTime <= contest.SubmissionEndDate;
        }

        public bool IsAnnouncementPeriod(Contest contest, DateTime currentTime)
        {
            return currentTime > contest.VotingEndDate;
        }
    }
}
