using DreamAquascape.Services.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DreamAquascape.Web.Controllers
{
    /// <summary>
    /// Handles voting functionality for contest entries. Ensures users can vote during the allowed period and tracks votes per entry.
    /// </summary>
    public class VotesController : Controller
    {
        private readonly IContestService _contestService;
        private readonly ILogger<VotesController> _logger;

        public VotesController(IContestService contestService, ILogger<VotesController> logger)
        {
            _contestService = contestService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CastVote(int contestId, int entryId, string? returnUrl = null)
        {
            try
            {
                // Get user information
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "You must be logged in to vote.";
                    return RedirectToAction("Login", "Account", new { returnUrl = returnUrl ?? Request.Headers["Referer"].ToString() });
                }

                // Get IP address for fraud prevention
                var ipAddress = GetClientIpAddress();

                // Cast the vote
                var vote = await _contestService.CastVoteAsync(contestId, entryId, userId, userName, ipAddress);

                // Set success message
                TempData["SuccessMessage"] = "Your vote has been cast successfully!";

                _logger.LogInformation("Vote cast by user {UserId} for entry {EntryId} in contest {ContestId}",
                    userId, entryId, contestId);
            }
            catch (InvalidOperationException ex)
            {
                // Business rule violations (already voted, contest not active, etc.)
                TempData["ErrorMessage"] = ex.Message;
                _logger.LogWarning("Vote failed for user {UserId}: {Message}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, ex.Message);
            }
            catch (Exception ex)
            {
                // Unexpected errors
                TempData["ErrorMessage"] = "An error occurred while casting your vote. Please try again.";
                _logger.LogError(ex, "Unexpected error casting vote for user {UserId} in contest {ContestId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, contestId);
            }

            // Redirect back to where the user came from
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Fallback to contest details
            return RedirectToAction("Details", "Contest", new { id = contestId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveVote(int contestId, string? returnUrl = null)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "You must be logged in to remove a vote.";
                    return RedirectToAction("Login", "Account");
                }

                await _contestService.RemoveVoteAsync(contestId, userId);
                TempData["SuccessMessage"] = "Your vote has been removed successfully!";

                _logger.LogInformation("Vote removed by user {UserId} from contest {ContestId}", userId, contestId);
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while removing your vote. Please try again.";
                _logger.LogError(ex, "Error removing vote for user {UserId} from contest {ContestId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, contestId);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "Contest", new { id = contestId });
        }

        private string? GetClientIpAddress()
        {
            // Check for forwarded IP first (load balancers, proxies)
            var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',').FirstOrDefault()?.Trim();
            }

            // Check for real IP
            var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Fall back to connection IP
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }
}
