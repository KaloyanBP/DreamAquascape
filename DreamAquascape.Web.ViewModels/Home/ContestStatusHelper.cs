using DreamAquascape.Web.ViewModels.Contest;

namespace DreamAquascape.Web.ViewModels.Home
{
    public static class ContestStatusHelper
    {
        public enum ContestStatus
        {
            Upcoming,
            SubmissionOpen,
            Voting,
            Ended
        }

        public class ContestStatusInfo
        {
            public ContestStatus Status { get; set; }
            public string StatusText { get; set; } = string.Empty;
            public string StatusClass { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
        }

        public static ContestStatusInfo GetContestStatus(ContestItemViewModel contest)
        {
            var now = DateTime.UtcNow;

            if (now < contest.SubmissionStartDate)
            {
                return new ContestStatusInfo
                {
                    Status = ContestStatus.Upcoming,
                    StatusText = "Starting Soon",
                    StatusClass = "status-upcoming",
                    Icon = "bi-calendar-plus"
                };
            }
            else if (now >= contest.SubmissionStartDate && now <= contest.SubmissionEndDate)
            {
                return new ContestStatusInfo
                {
                    Status = ContestStatus.SubmissionOpen,
                    StatusText = "Submissions Open",
                    StatusClass = "status-active",
                    Icon = "bi-upload"
                };
            }
            else if (now > contest.SubmissionEndDate && now <= contest.VotingEndDate)
            {
                return new ContestStatusInfo
                {
                    Status = ContestStatus.Voting,
                    StatusText = "Voting Phase",
                    StatusClass = "status-voting",
                    Icon = "bi-heart"
                };
            }
            else
            {
                return new ContestStatusInfo
                {
                    Status = ContestStatus.Ended,
                    StatusText = "Recently Finished",
                    StatusClass = "status-ended",
                    Icon = "bi-trophy"
                };
            }
        }

        public static string GetPrimaryActionText(ContestStatus status)
        {
            return status switch
            {
                ContestStatus.Upcoming => "Get Notified",
                ContestStatus.SubmissionOpen => "Submit Entry",
                ContestStatus.Voting => "Vote Now",
                ContestStatus.Ended => "View Results",
                _ => "View Details"
            };
        }

        public static string GetPrimaryActionIcon(ContestStatus status)
        {
            return status switch
            {
                ContestStatus.Upcoming => "bi-bell",
                ContestStatus.SubmissionOpen => "bi-upload",
                ContestStatus.Voting => "bi-heart",
                ContestStatus.Ended => "bi-trophy",
                _ => "bi-eye"
            };
        }

        public static string GetPrimaryActionClass(ContestStatus status)
        {
            return status switch
            {
                ContestStatus.SubmissionOpen => "btn-success",
                ContestStatus.Voting => "btn-warning",
                ContestStatus.Ended => "btn-outline-success",
                _ => "btn-outline-primary"
            };
        }
    }
}
