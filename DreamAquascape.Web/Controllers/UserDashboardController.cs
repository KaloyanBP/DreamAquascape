using Microsoft.AspNetCore.Mvc;
using DreamAquascape.Web.ViewModels.UserDashboard;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Personal contest dashboard
    /// </summary>
    [Route("user")]
    public class UserDashboardController : Controller
    {
        [Route("dashboard")]
        public IActionResult Index()
        {
            var model = new UserDashboardViewModel
            {
                UserName = "AquaFan",
                QuickStats = new UserQuickStatsViewModel
                {
                    TotalContestsParticipated = 5,
                    TotalEntriesSubmitted = 12,
                    TotalVotesCast = 34,
                    ContestsWon = 2
                },
                ActiveContests = new List<UserActiveContestViewModel>
            {
                new UserActiveContestViewModel
                {
                    ContestId = 101,
                    Title = "July Aquascape Challenge",
                    Phase = "Submission",
                    TotalEntries = 18,
                    DaysRemaining = 5,
                    HasSubmitted = false,
                    HasVoted = false,
                    IsFollowing = false,
                    ImageFileUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=400&q=80"
                },
                new UserActiveContestViewModel
                {
                    ContestId = 102,
                    Title = "Summer Nano Tanks",
                    Phase = "Voting",
                    TotalEntries = 25,
                    DaysRemaining = 2,
                    HasSubmitted = true,
                    HasVoted = false,
                    IsFollowing = true,
                    UserEntryId = 501,
                    ImageFileUrl = "https://images.unsplash.com/photo-1464983953574-0892a716854b?auto=format&fit=crop&w=400&q=80"
                }
            },
                MySubmissions = new List<UserSubmissionViewModel>
            {
                new UserSubmissionViewModel
                {
                    EntryId = 501,
                    ContestTitle = "Summer Nano Tanks",
                    SubmittedAt = DateTime.Now.AddDays(-3),
                    Images = new List<EntryImageViewModel>()
                    {
                        new EntryImageViewModel
                        {
                            ImageUrl = "https://images.unsplash.com/photo-1506744038136-46273834b3fb?auto=format&fit=crop&w=400&q=80"
                        }
                    },
                    Status = "Active"
                },
                new UserSubmissionViewModel
                {
                    EntryId = 502,
                    ContestTitle = "Spring Aquascape",
                    SubmittedAt = DateTime.Now.AddMonths(-1),
                    Images = new List<EntryImageViewModel>()
                    {
                        new EntryImageViewModel
                        {
                            ImageUrl = "https://images.unsplash.com/photo-1464983953574-0892a716854b?auto=format&fit=crop&w=400&q=80"
                        }
                    },
                    Status = "Winner"
                }
            },
            VotingHistory = new List<UserVotingHistoryViewModel>
            {
                new UserVotingHistoryViewModel
                {
                    EntryTitle = "Mountain Stream",
                    EntryOwner = "GreenScaper",
                    VotedAt = DateTime.Now.AddDays(-2),
                    EntryWon = true,
                    EntryFinalRanking = 1
                },
                new UserVotingHistoryViewModel
                {
                    EntryTitle = "Blue Lagoon",
                    EntryOwner = "AquaArtist",
                    VotedAt = DateTime.Now.AddDays(-7),
                    EntryWon = false,
                    EntryFinalRanking = 5
                },
                new UserVotingHistoryViewModel
                {
                    EntryTitle = "Forest Floor",
                    EntryOwner = "NatureTank",
                    VotedAt = DateTime.Now.AddDays(-10),
                    EntryWon = null,
                    EntryFinalRanking = null
                }
            }
            };

            return View(model);
        }

    }
}
