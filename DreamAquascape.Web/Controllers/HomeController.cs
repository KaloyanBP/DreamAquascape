using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.Home;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DreamAquascape.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAdminDashboardService _dashboardService;
        private readonly IContestQueryService _contestQueryService;

        public HomeController(
            IAdminDashboardService dashboardService,
            IContestQueryService contestQueryService,
            ILogger<HomeController> logger)
        {
            _dashboardService = dashboardService;
            _contestQueryService = contestQueryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get dashboard statistics
                var dashboardStats = await _dashboardService.GetDashboardStatsAsync();

                // Get contests for featured section
                var contestFilters = new ContestFilterViewModel
                {
                    Status = ContestStatus.All,
                    ExcludeArchived = true,
                    PageSize = 20, // Get more contests to filter from
                    Page = 1
                };

                var contestsData = await _contestQueryService.GetFilteredContestsAsync(contestFilters);
                var contests = contestsData.Contests.ToList();

                // Categorize contests by status
                var featuredContests = CategorizeContests(contests);

                var model = new HomeIndexViewModel
                {
                    // Statistics
                    ActiveContests = dashboardStats.ActiveContests,
                    TotalEntries = dashboardStats.TotalEntries,
                    TotalUsers = dashboardStats.TotalUsers,
                    TotalVotes = dashboardStats.TotalVotes,

                    // Featured contests
                    ActiveContest = featuredContests.ActiveContest,
                    VotingContest = featuredContests.VotingContest,
                    UpcomingContest = featuredContests.UpcomingContest,
                    RecentlyEndedContest = featuredContests.RecentlyEndedContest
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");

                // Return minimal model on error
                var fallbackModel = new HomeIndexViewModel
                {
                    ActiveContests = 0,
                    TotalEntries = 0,
                    TotalUsers = 0,
                    TotalVotes = 0
                };

                return View(fallbackModel);
            }
        }

        private static (ContestItemViewModel? ActiveContest,
                       ContestItemViewModel? VotingContest,
                       ContestItemViewModel? UpcomingContest,
                       ContestItemViewModel? RecentlyEndedContest) CategorizeContests(List<ContestItemViewModel> contests)
        {
            var now = DateTime.UtcNow;

            // Find one contest of each type for featured section
            var activeContest = contests
                .Where(c => c.IsActive && now >= c.SubmissionStartDate && now <= c.SubmissionEndDate)
                .OrderByDescending(c => c.EntryCount)
                .FirstOrDefault();

            var votingContest = contests
                .Where(c => c.IsActive && now > c.SubmissionEndDate && now <= c.VotingEndDate)
                .OrderByDescending(c => c.VoteCount)
                .FirstOrDefault();

            var upcomingContest = contests
                .Where(c => c.IsActive && now < c.SubmissionStartDate)
                .OrderBy(c => c.SubmissionStartDate)
                .FirstOrDefault();

            var recentlyEndedContest = contests
                .Where(c => now > c.VotingEndDate)
                .OrderByDescending(c => c.VotingEndDate)
                .FirstOrDefault();

            return (activeContest, votingContest, upcomingContest, recentlyEndedContest);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
