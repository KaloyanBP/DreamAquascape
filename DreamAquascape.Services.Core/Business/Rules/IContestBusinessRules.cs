using DreamAquascape.Data.Models;

namespace DreamAquascape.Services.Core.Business.Rules
{
    /// <summary>
    /// Contains business rules related to contest operations
    /// </summary>
    public interface IContestBusinessRules
    {
        /// <summary>
        /// Checks if a user can vote in a specific contest
        /// </summary>
        bool CanUserVote(Contest contest, string userId, DateTime currentTime);

        /// <summary>
        /// Checks if the voting period is currently active for a contest
        /// </summary>
        bool IsVotingPeriodActive(Contest contest, DateTime currentTime);

        /// <summary>
        /// Checks if the submission period is currently active for a contest
        /// </summary>
        bool IsSubmissionPeriodActive(Contest contest, DateTime currentTime);

        /// <summary>
        /// Checks if contest is in announcement period
        /// </summary>
        bool IsAnnouncementPeriod(Contest contest, DateTime currentTime);
    }
}
