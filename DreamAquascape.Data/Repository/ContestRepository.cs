using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using Microsoft.EntityFrameworkCore;

namespace DreamAquascape.Data.Repository
{
    public class ContestRepository : BaseRepository<Contest, int>, IContestRepository
    {
        public ContestRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Contest>> GetActiveContestsAsync()
        {
            var now = DateTime.UtcNow;
            return await DbSet
                .Where(c => c.IsActive && !c.IsDeleted && c.SubmissionStartDate <= now && c.SubmissionEndDate >= now)
                .OrderByDescending(c => c.SubmissionStartDate)
                .ToListAsync();
        }

        public async Task<Contest?> GetContestDetailsAsync(int contestId)
        {
            var contest = await DbSet
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
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
            return await DbSet
                .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForDeleteAsync(int contestId)
        {
            return await DbSet
                .Include(c => c.Entries)
                .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForEditAsync(int contestId)
        {
            return await DbSet
                .Include(c => c.Prizes)
                .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);
        }

        public async Task<Contest?> GetContestForWinnerDeterminationAsync(int contestId)
        {
            return await DbSet
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Winners)
                .FirstOrDefaultAsync(c => c.Id == contestId);
        }

        public async Task<IEnumerable<Contest>> GetEndedContestsWithoutWinnersAsync()
        {
            var now = DateTime.UtcNow;
            return await DbSet
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
            var now = DateTime.UtcNow;

            // Start with all contests
            var query = DbSet
                .Include(c => c.Entries.Where(e => !e.IsDeleted))
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Prizes)
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
            var now = DateTime.UtcNow;
            var allContests = await DbSet.Where(c => !c.IsDeleted).ToListAsync();

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
            using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // Add the prize to the contest
                contest.Prizes = new List<Prize> { prize };

                // Add the contest
                await DbSet.AddAsync(contest);
                await DbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return contest;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int> GetTotalContestCountAsync()
        {
            return await DbSet.CountAsync(c => !c.IsDeleted);
        }

        public async Task<int> GetActiveContestCountAsync()
        {
            var now = DateTime.UtcNow;
            return await DbSet.CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.SubmissionStartDate <= now && c.VotingEndDate >= now);
        }

        public async Task<int> GetContestsEndingSoonCountAsync(DateTime now, DateTime endDate)
        {
            return await DbSet.CountAsync(c =>
                c.IsActive && !c.IsDeleted &&
                c.VotingEndDate <= endDate && c.VotingEndDate >= now);
        }

        public async Task<double> GetAverageEntriesPerContestAsync()
        {
            var contestsWithEntries = await DbSet
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Id, EntryCount = c.Entries.Count(e => !e.IsDeleted) })
                .ToListAsync();

            return contestsWithEntries.Any()
                ? contestsWithEntries.Average(c => c.EntryCount)
                : 0;
        }
    }
}
