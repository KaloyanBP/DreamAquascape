using DreamAquascape.Data.Models;
using DreamAquascape.GCommon.Infrastructure;

namespace DreamAquascape.Services.Common.Extensions
{
    public static class ContestExtensions
    {
        /// <summary>
        /// Checks if the contest is currently open for voting based on the current date and time.
        /// </summary>
        /// <param name="contest"></param>
        /// <param name="dateTimeProvider"></param>
        /// <returns></returns>
        public static bool IsVotingOpen(this Contest contest, IDateTimeProvider dateTimeProvider)
        {
            var now = dateTimeProvider.UtcNow;
            return !contest.IsDeleted 
                && contest.IsActive 
                && now >= contest.VotingStartDate 
                && now <= contest.VotingEndDate;
        }

        /// <summary>
        /// Checks if the contest is currently open for submissions based on the current date and time.
        /// </summary>
        /// <param name="contest"></param>
        /// <param name="dateTimeProvider"></param>
        /// <returns></returns>
        public static bool IsSubmissionOpen(this Contest contest, IDateTimeProvider dateTimeProvider)
        {
            var now = dateTimeProvider.UtcNow;
            return !contest.IsDeleted 
                && contest.IsActive 
                && now >= contest.SubmissionStartDate 
                && now <= contest.SubmissionEndDate;
        }

        /// <summary>
        /// Checks if the contest is currently in progress based on the current date and time.
        /// </summary>
        /// <param name="contest"></param>
        /// <param name="dateTimeProvider"></param>
        /// <returns></returns>
        public static bool InProgress(this Contest contest, IDateTimeProvider dateTimeProvider)
        {
            return contest.IsVotingOpen(dateTimeProvider) 
                || contest.IsSubmissionOpen(dateTimeProvider);
        }
    }
}
