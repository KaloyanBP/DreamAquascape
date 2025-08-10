using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.GCommon.Infrastructure;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestRepository : BaseRepository<Contest, int>, IContestRepository
    {
        public ContestRepository(ApplicationDbContext dbContext, IDateTimeProvider dateTimeProvider) : base(dbContext, dateTimeProvider)
        {
        }

        public async Task<Contest?> GetContestDetailsAsync(int contestId)
        {
            var contest = await GetAllAttached()
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Participant)
                .Include(c => c.Entries)
                    .ThenInclude(e => e.EntryImages)
                .Include(c => c.Prizes)
                .Include(c => c.Winners)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == contestId && c.IsActive && !c.IsDeleted);
            return contest;
        }

        public async Task<Contest?> GetContestForToggleAsync(int contestId)
        {
            return await FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForDeleteAsync(int contestId)
        {
            return await GetAllAttached()
                .Include(c => c.Entries)
                .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForEditAsync(int contestId)
        {
            return await GetAllAttached()
                .Include(c => c.Prizes)
                .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForWinnerDeterminationAsync(int contestId)
        {
            return await GetAllAttached()
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Winners)
                .FirstOrDefaultAsync(c => c.Id == contestId);
        }

        public async Task<IEnumerable<Contest>> GetEndedContestsWithoutWinnersAsync()
        {
            var now = DateTimeProvider.UtcNow;
            return await GetAllAttached()
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Winners)
                .Where(c => c.VotingEndDate <= now &&
                           !c.Winners.Any(w => w.Position == 1) &&
                           c.Entries.Any())
                .ToListAsync();
        }

        public async Task<(IEnumerable<Contest> contests, int totalCount)> GetFilteredContestsAsync(ContestFilterViewModel filters)
        {
            var now = DateTimeProvider.UtcNow;

            // Start with all contests
            var query = GetAllAttached()
                .Include(c => c.Entries.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Prizes)
                .Include(c => c.Categories)
                    .ThenInclude(cc => cc.Category)
                .Where(c => !c.IsDeleted);

            if (filters.ExcludeArchived)
            {
                // Do not include archived contests (IsActive == false)
                query = query.Where(c => c.IsActive == true);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var searchTerm = filters.Search.Trim().ToLower();
                query = query.Where(c => c.Title.ToLower().Contains(searchTerm) ||
                                       c.Description.ToLower().Contains(searchTerm));
            }

            // Apply status filter
            switch (filters.Status)
            {
                case ContestStatus.Active:
                    query = query.Where(c => c.IsActive && now <= c.VotingEndDate);
                    break;
                case ContestStatus.Submission:
                    query = query.Where(c => c.IsActive && now >= c.SubmissionStartDate &&
                                           now <= c.SubmissionEndDate);
                    break;
                case ContestStatus.Voting:
                    query = query.Where(c => c.IsActive && now > c.SubmissionEndDate &&
                                           now <= c.VotingEndDate);
                    break;
                case ContestStatus.Ended:
                    query = query.Where(c => (c.IsActive && now > c.VotingEndDate) || !c.IsActive);
                    break;
                case ContestStatus.Archived:
                    query = query.Where(c => !c.IsActive);
                    break;
                case ContestStatus.All:
                default:
                    // No additional filter
                    break;
            }

            // Apply sorting
            query = filters.SortBy switch
            {
                ContestSortBy.Oldest => query.OrderBy(c => c.SubmissionStartDate),
                ContestSortBy.EndingSoon => query.OrderBy(c => c.VotingEndDate),
                ContestSortBy.MostEntries => query.OrderByDescending(c => c.Entries.Count(e => !e.IsDeleted)),
                ContestSortBy.Title => query.OrderBy(c => c.Title),
                ContestSortBy.Newest or _ => query.OrderByDescending(c => c.SubmissionStartDate)
            };

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            var contests = await query
                .Skip((filters.Page - 1) * filters.PageSize)
                .Take(filters.PageSize)
                .ToListAsync();

            return (contests, totalCount);
        }

        public async Task<ContestStatsViewModel> GetContestStatsAsync()
        {
            var now = DateTimeProvider.UtcNow;
            var allContests = await GetAllAttached().Where(c => !c.IsDeleted).ToListAsync();

            return new ContestStatsViewModel
            {
                TotalContests = allContests.Count,
                ActiveContests = allContests.Count(c => c.IsActive && now <= c.VotingEndDate),
                InactiveContests = allContests.Count(c => !c.IsActive),
                SubmissionPhase = allContests.Count(c => c.IsActive && now >= c.SubmissionStartDate && now <= c.SubmissionEndDate),
                VotingPhase = allContests.Count(c => c.IsActive && now > c.SubmissionEndDate && now <= c.VotingEndDate),
                EndedContests = allContests.Count(c => (c.IsActive && now > c.VotingEndDate) || !c.IsActive),
                ArchivedContests = allContests.Count(c => !c.IsActive)
            };
        }

        public async Task<Contest> CreateContestWithPrizeAsync(Contest contest, Prize prize)
        {
            // Add the prize to the contest
            contest.Prizes = new List<Prize> { prize };

            // Add the contest
            await DbSet.AddAsync(contest);
            return contest;
        }

        public async Task<int> GetActiveContestCountAsync()
        {
            var now = DateTimeProvider.UtcNow;
            return await GetAllAttached().CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.SubmissionStartDate <= now && c.VotingEndDate >= now);
        }

        public async Task<int> GetContestsEndingSoonCountAsync(DateTime now, DateTime endDate)
        {
            return await GetAllAttached().CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.VotingEndDate <= endDate && c.VotingEndDate >= now);
        }

        public async Task<double> GetAverageEntriesPerContestAsync()
        {
            var contestsWithEntries = await GetAllAttached()
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Id, EntryCount = c.Entries.Count(e => !e.IsDeleted) })
                .ToListAsync();

            return contestsWithEntries.Any()
                ? contestsWithEntries.Average(c => c.EntryCount)
                : 0;
        }

        public async Task<IEnumerable<Contest>> GetActiveContestsWithFullDataAsync(DateTime now)
        {
            return await GetAllAttached()
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= now && c.VotingEndDate >= now)
                .Include(c => c.Categories)
                    .ThenInclude(cc => cc.Category)
                .Include(c => c.Prizes)
                .Include(c => c.Entries.Where(e => !e.IsDeleted))
                .ToListAsync();
        }

        public async Task<int> GetActiveContestsCountForUserAsync(string userId, DateTime currentDate)
        {
            // Get contest IDs where user has participated (entries or votes)
            var userEntryContestIds = await DbContext.ContestEntries
                .Where(e => e.ParticipantId == userId && !e.IsDeleted)
                .Select(e => e.ContestId)
                .ToListAsync();

            var userVoteContestIds = await DbContext.Votes
                .Where(v => v.UserId == userId)
                .Select(v => v.ContestEntry.ContestId)
                .ToListAsync();

            var participatedContestIds = userEntryContestIds
                .Union(userVoteContestIds)
                .Distinct()
                .ToList();

            return await GetAllAttached()
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= currentDate && c.VotingEndDate >= currentDate &&
                           participatedContestIds.Contains(c.Id))
                .CountAsync();
        }

        public async Task<int> GetSubmissionsInProgressCountForUserAsync(string userId, DateTime currentDate)
        {
            return await GetAllAttached()
                .Where(c => c.IsActive && !c.IsDeleted &&
                           c.SubmissionStartDate <= currentDate && c.SubmissionEndDate >= currentDate &&
                           !c.Entries.Any(e => e.ParticipantId == userId && !e.IsDeleted))
                .CountAsync();
        }

        public async Task<List<ContestsCategories>> GetContestCategoriesAsync(int contestId)
        {
            return await DbContext.ContestsCategories
                .Include(cc => cc.Category)
                .Where(cc => cc.ContestId == contestId)
                .ToListAsync();
        }

        public async Task AddContestCategoryAsync(int contestId, int categoryId)
        {
            var contestCategory = new ContestsCategories
            {
                ContestId = contestId,
                CategoryId = categoryId
            };

            await DbContext.ContestsCategories.AddAsync(contestCategory);
        }

        public async Task RemoveContestCategoryAsync(int contestId, int categoryId)
        {
            var contestCategory = await DbContext.ContestsCategories
                .FirstOrDefaultAsync(cc => cc.ContestId == contestId && cc.CategoryId == categoryId);

            if (contestCategory != null)
            {
                DbContext.ContestsCategories.Remove(contestCategory);
            }
        }
    }
}
