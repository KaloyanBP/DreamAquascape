using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Data.Repository.Interfaces;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.ContestEntry;
using DreamAquascape.Web.ViewModels.UserDashboard;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class ContestService : IContestService
    {
        private readonly ILogger<ContestService> _logger;
        private readonly IContestRepository _contestRepository;
        private readonly ApplicationDbContext _context;

        public ContestService(ApplicationDbContext context, IContestRepository contestRepository, ILogger<ContestService> logger)
        {
            _logger = logger;
            _contestRepository = contestRepository;
            _context = context;
        }

        public async Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync()
        {
            var activeContests = await _contestRepository.GetActiveContestsAsync();

            return activeContests
                .Select(c => new ContestItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ImageUrl = c.ImageFileUrl ?? "",
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    IsActive = c.IsActive,
                })
                .ToList();
        }

        public async Task<ContestListViewModel> GetFilteredContestsAsync(ContestFilterViewModel filters)
        {
            var now = DateTime.UtcNow;

            // Start with all contests
            var query = _context.Contests
                .Include(c => c.Entries)
                .Where(c => !c.IsDeleted);

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
                .Select(c => new ContestItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ImageUrl = c.ImageFileUrl ?? "",
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    IsActive = c.IsActive,
                    EntryCount = c.Entries.Count(e => !e.IsDeleted),
                    VoteCount = c.Entries.SelectMany(e => e.Votes).Count()
                })
                .ToListAsync();

            // Calculate statistics
            var allContests = await _context.Contests.Where(c => !c.IsDeleted).ToListAsync();
            var stats = new ContestStatsViewModel
            {
                TotalContests = allContests.Count,
                ActiveContests = allContests.Count(c => c.IsActive && now <= c.VotingEndDate),
                SubmissionPhase = allContests.Count(c => c.IsActive && now >= c.SubmissionStartDate && now <= c.SubmissionEndDate),
                VotingPhase = allContests.Count(c => c.IsActive && now > c.SubmissionEndDate && now <= c.VotingEndDate),
                EndedContests = allContests.Count(c => (c.IsActive && now > c.VotingEndDate) || !c.IsActive),
                ArchivedContests = allContests.Count(c => !c.IsActive)
            };

            return new ContestListViewModel
            {
                Contests = contests,
                Filters = filters,
                Stats = stats,
                Pagination = new PaginationViewModel
                {
                    CurrentPage = filters.Page,
                    PageSize = filters.PageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / filters.PageSize)
                }
            };
        }

        public async Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId, string? currentUserId = null)
        {
            // First, get the contest with all related data using Include
            Contest? contest = await _contestRepository.GetContestDetailsAsync(contestId);

            if (contest == null)
                return null;

            // Check if contest has just ended and needs winner determination
            var now = DateTime.UtcNow;
            if (now > contest.VotingEndDate && contest.PrimaryWinner == null && contest.Entries.Any())
            {
                try
                {
                    // Automatically determine winner when contest is viewed after voting ends
                    await DetermineAndSetWinnerAsync(contestId);
                    // Refresh contest data to get the new winner
                    contest = await _contestRepository.GetContestDetailsAsync(contestId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error auto-determining winner for contest {ContestId} during view", contestId);
                    // Continue with existing data even if winner determination fails
                }
            }

            // Get user participation data if user is authenticated
            ContestEntry? userEntry = null;
            Vote? userVote = null;

            if (!string.IsNullOrEmpty(currentUserId))
            {
                userEntry = contest.Entries
                    .FirstOrDefault(e => e.ContestId == contestId && e.ParticipantId == currentUserId && !e.IsDeleted);

                userVote = contest.Entries?.SelectMany(e => e.Votes)
                    .FirstOrDefault(v => v.UserId == currentUserId && v.ContestEntry.ContestId == contestId);
            }

            return new ContestDetailsViewModel
            {
                Id = contest.Id,
                Title = contest.Title,
                Description = contest.Description,
                StartDate = contest.SubmissionStartDate,
                EndDate = contest.VotingEndDate,
                IsActive = contest.IsActive,

                CanSubmitEntry = !string.IsNullOrEmpty(currentUserId) &&
                               now >= contest.SubmissionStartDate &&
                               now <= contest.SubmissionEndDate &&
                               userEntry == null,

                CanVote = !string.IsNullOrEmpty(currentUserId) &&
                         now >= contest.VotingStartDate &&
                         now <= contest.VotingEndDate &&
                         userVote == null,

                Prize = contest.PrimaryPrize != null ? new PrizeViewModel
                {
                    Name = contest.PrimaryPrize.Name,
                    Description = contest.PrimaryPrize.Description,
                } : null,

                Entries = contest.Entries
                    .Where(e => !e.IsDeleted)
                    .Select(e => new ContestEntryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        EntryImages = e.EntryImages
                            .OrderBy(img => img.DisplayOrder)
                            .Select(img => img.ImageUrl)
                            .ToList(),

                        CanUserVote = !string.IsNullOrEmpty(currentUserId) &&
                                     now >= contest.VotingStartDate &&
                                     now <= contest.VotingEndDate &&
                                     e.ParticipantId != currentUserId &&
                                     userVote == null,

                        HasUserVoted = userVote?.ContestEntryId == e.Id,
                        IsWinner = contest.PrimaryWinner?.ContestEntryId == e.Id,
                        IsOwnEntry = e.ParticipantId == currentUserId,
                        VoteCount = e.Votes.Count(),
                        SubmittedAt = e.SubmittedAt
                    })
                    .OrderByDescending(e => e.VoteCount)
                    .ThenBy(e => e.SubmittedAt)
                    .ToList(),

                WinnerEntryId = contest.PrimaryWinner?.ContestEntryId,
                UserHasSubmittedEntry = userEntry != null,
                UserHasVoted = userVote != null,
                UserVotedForEntryId = userVote?.ContestEntryId,
                UserSubmittedEntryId = userEntry?.Id
            };
        }

        //public async Task<EditContestEntryViewModel?> GetContestEntryForEditAsync(int contestId, int entryId, string currentUserId)
        //{
        //    var contest = await _context.Contests
        //        .FirstOrDefaultAsync(c => c.Id == contestId && !c.IsDeleted);

        //    if (contest == null) return null;

        //    var entry = await _context.ContestEntries
        //        .Include(e => e.EntryImages)
        //        .FirstOrDefaultAsync(e => e.Id == entryId &&
        //                                e.ContestId == contestId &&
        //                                e.ParticipantId == currentUserId &&
        //                                !e.IsDeleted);

        //    if (entry == null) return null;

        //    var now = DateTime.UtcNow;
        //    var canEdit = contest.IsActive &&
        //                 now >= contest.SubmissionStartDate &&
        //                 now <= contest.SubmissionEndDate;

        //    return new EditContestEntryViewModel
        //    {
        //        Id = entry.Id,
        //        ContestId = contestId,
        //        Title = entry.Title,
        //        Description = entry.Description,
        //        ContestTitle = contest.Title,
        //        SubmissionEndDate = contest.SubmissionEndDate,
        //        CanEdit = canEdit,
        //        ExistingImages = entry.EntryImages
        //            .Where(img => !img.IsDeleted)
        //            .Select(img => new ExistingImageViewModel
        //            {
        //                Id = img.Id,
        //                ImageUrl = img.ImageUrl
        //            })
        //            .ToList()
        //    };
        //}

        //public async Task<bool> UpdateContestEntryAsync(EditContestEntryViewModel model, string currentUserId)
        //{
        //    var contest = await _context.Contests
        //        .FirstOrDefaultAsync(c => c.Id == model.ContestId && !c.IsDeleted);

        //    if (contest == null) return false;

        //    var entry = await _context.ContestEntries
        //        .Include(e => e.EntryImages)
        //        .FirstOrDefaultAsync(e => e.Id == model.Id &&
        //                                e.ContestId == model.ContestId &&
        //                                e.ParticipantId == currentUserId &&
        //                                !e.IsDeleted);

        //    if (entry == null) return false;

        //    // Check if editing is allowed
        //    var now = DateTime.UtcNow;
        //    var canEdit = contest.IsActive &&
        //                 now >= contest.SubmissionStartDate &&
        //                 now <= contest.SubmissionEndDate;

        //    if (!canEdit) return false;

        //    // Update entry details
        //    entry.Title = model.Title;
        //    entry.Description = model.Description;
        //    entry.UpdatedAt = now;

        //    // Handle image removals
        //    if (model.ImagesToRemove?.Any() == true)
        //    {
        //        var imagesToRemove = entry.EntryImages
        //            .Where(img => model.ImagesToRemove.Contains(img.Id))
        //            .ToList();

        //        foreach (var img in imagesToRemove)
        //        {
        //            img.IsDeleted = true;
        //        }
        //    }

        //    // Add new images
        //    if (model.NewImages?.Any() == true)
        //    {
        //        foreach (var imageUrl in model.NewImages)
        //        {
        //            var newImage = new Data.Models.EntryImage
        //            {
        //                ContestEntryId = entry.Id,
        //                ImageUrl = imageUrl,
        //                CreatedOn = now
        //            };
        //            _context.EntryImages.Add(newImage);
        //        }
        //    }

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        _logger.LogInformation("Contest entry {EntryId} updated successfully by user {UserId}", model.Id, currentUserId);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error updating contest entry {EntryId} for user {UserId}", model.Id, currentUserId);
        //        return false;
        //    }
        //}

        public async Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate submission dates
                if (dto.SubmissionStartDate >= dto.SubmissionEndDate)
                    throw new InvalidOperationException("Submission start date must be before end date");
                if (dto.VotingStartDate <= dto.SubmissionStartDate || dto.VotingEndDate <= dto.VotingStartDate)
                    throw new InvalidOperationException("Start voting date must be after submission start date and before voting end date");
                // Create the contest
                var primaryPrize = new Prize
                {
                    Name = prizeDto.Name,
                    Description = prizeDto.Description,
                    ImageUrl = prizeDto.ImageUrl,
                    Place = 1
                };
                var contest = new Contest
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    ImageFileUrl = dto.ImageFileUrl,
                    SubmissionStartDate = dto.SubmissionStartDate,
                    SubmissionEndDate = dto.SubmissionEndDate,
                    VotingStartDate = dto.VotingStartDate,
                    VotingEndDate = dto.VotingEndDate,
                    ResultDate = dto.ResultDate,
                    CreatedBy = createdBy,
                    IsActive = true,
                    IsDeleted = false,
                    Prizes = new List<Prize> { primaryPrize },
                };
                await _context.Contests.AddAsync(contest);
                await _context.SaveChangesAsync(); // Save to get the contest ID
                _logger.LogInformation("Contest {ContestId} created by user {UserId}", contest.Id, createdBy);
                await transaction.CommitAsync();
                return contest;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create contest by user {UserId}", createdBy);
                throw;
            }
        }

        //public async Task<ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName)
        //{
        //    using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Validate submission period
        //        var contest = await _context.Contests
        //            .FirstOrDefaultAsync(c => c.Id == dto.ContestId && c.IsActive && !c.IsDeleted);

        //        if (contest == null)
        //            throw new NotFoundException("Contest not found");

        //        if (DateTime.UtcNow < contest.SubmissionStartDate || DateTime.UtcNow > contest.SubmissionEndDate)
        //            throw new InvalidOperationException("Contest submission period is not active");

        //        // Check if user already has an entry
        //        var existingEntry = await _context.ContestEntries
        //            .FirstOrDefaultAsync(e => e.ContestId == dto.ContestId && e.ParticipantId == userId && !e.IsDeleted);

        //        if (existingEntry != null)
        //            throw new InvalidOperationException("User already has an entry in this contest");

        //        // Create the contest entry
        //        var entry = new ContestEntry
        //        {
        //            ContestId = dto.ContestId,
        //            ParticipantId = userId,
        //            Title = dto.Title,
        //            Description = dto.Description,
        //            SubmittedAt = DateTime.UtcNow,
        //            IsActive = true,
        //            IsDeleted = false,
        //            EntryImages = GetEntryImages(dto.EntryImages)
        //        };

        //        await _context.ContestEntries.AddAsync(entry);
        //        await _context.SaveChangesAsync(); // Save to get the entry ID

        //        await transaction.CommitAsync();

        //        _logger.LogInformation("Entry {EntryId} submitted by user {UserId} for contest {ContestId}",
        //            entry.Id, userId, dto.ContestId);

        //        return entry;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        _logger.LogError(ex, "Failed to submit entry for user {UserId} in contest {ContestId}",
        //            userId, dto.ContestId);
        //        throw;
        //    }
        //}

        public async Task<Vote> CastVoteAsync(int contestId, int entryId, string userId, string userName, string? ipAddress = null)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate voting period
                var contest = await _context.Contests
                    .FirstOrDefaultAsync(c => c.Id == contestId && c.IsActive && !c.IsDeleted);

                if (contest == null)
                    throw new NotFoundException("Contest not found");

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                    throw new InvalidOperationException("Contest voting period is not active");

                // Validate the entry exists and is active
                var entry = await _context.ContestEntries
                    .FirstOrDefaultAsync(e => e.Id == entryId && e.ContestId == contestId && !e.IsDeleted);

                if (entry == null)
                    throw new NotFoundException("Entry not found");

                // Check if user is trying to vote for their own entry
                if (entry.ParticipantId == userId)
                    throw new InvalidOperationException("Users cannot vote for their own entries");

                // Check if user already voted in this contest
                var existingVote = await _context.Votes
                    .Include(v => v.ContestEntry)
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);

                if (existingVote != null)
                    throw new InvalidOperationException("User has already voted in this contest");

                // Create the vote
                var vote = new Vote
                {
                    ContestEntryId = entryId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow,
                    IpAddress = ipAddress
                };

                await _context.Votes.AddAsync(vote);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Vote cast by user {UserId} for entry {EntryId} in contest {ContestId}",
                    userId, entryId, contestId);

                return vote;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to cast vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task<Vote> ChangeVoteAsync(int contestId, int newEntryId, string userId, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate voting period
                var contest = await _context.Contests
                    .FirstOrDefaultAsync(c => c.Id == contestId && c.IsActive && !c.IsDeleted);

                if (contest == null)
                    throw new NotFoundException("Contest not found");

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                    throw new InvalidOperationException("Contest voting period is not active");

                // Get existing vote
                var existingVote = await _context.Votes
                    .Include(v => v.ContestEntry)
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);

                if (existingVote == null)
                    throw new NotFoundException("No existing vote found for this user");

                // Validate new entry
                var newEntry = await _context.ContestEntries
                    .FirstOrDefaultAsync(e => e.Id == newEntryId && e.ContestId == contestId && !e.IsDeleted);

                if (newEntry == null)
                    throw new NotFoundException("New entry not found");

                if (newEntry.ParticipantId == userId)
                    throw new InvalidOperationException("Users cannot vote for their own entries");

                // Update the vote
                existingVote.ContestEntryId = newEntryId;
                existingVote.VotedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Vote changed by user {UserId} to entry {EntryId} in contest {ContestId}",
                    userId, newEntryId, contestId);

                return existingVote;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to change vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }

        public async Task RemoveVoteAsync(int contestId, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate voting period
                var contest = await _context.Contests
                    .FirstOrDefaultAsync(c => c.Id == contestId && c.IsActive && !c.IsDeleted);

                if (contest == null)
                {
                    throw new NotFoundException("Contest not found");
                }

                if (DateTime.UtcNow < contest.VotingStartDate || DateTime.UtcNow > contest.VotingEndDate)
                {
                    throw new InvalidOperationException("Contest voting period is not active");
                }

                // Get existing vote
                var existingVote = await _context.Votes
                    .Include(v => v.ContestEntry)
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.ContestEntry.ContestId == contestId);

                if (existingVote == null)
                {
                    throw new NotFoundException("No existing vote found for this user");
                }

                // Remove the vote
                _context.Votes.Remove(existingVote);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Vote removed by user {UserId} in contest {ContestId}",
                    userId, contestId);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to remove vote for user {UserId} in contest {ContestId}",
                    userId, contestId);
                throw;
            }
        }
        public async Task<ContestEntryDetailsViewModel?> GetContestEntryDetailsAsync(int contestId, int entryId, string? currentUserId = null)
        {
            var entry = await _context.ContestEntries
                .Include(e => e.Contest)
                    .ThenInclude(c => c.Winners)
                .Include(e => e.Participant)
                .Include(e => e.EntryImages)
                .Include(e => e.Votes)
                    .ThenInclude(v => v.User)
                .Include(e => e.Winner)
                .FirstOrDefaultAsync(e => e.Id == entryId && e.ContestId == contestId && !e.IsDeleted);

            if (entry == null)
                return null;

            var contest = entry.Contest;
            var now = DateTime.UtcNow;

            // Get all entries in this contest for ranking calculation
            var allContestEntries = await _context.ContestEntries
                .Where(e => e.ContestId == contestId && !e.IsDeleted)
                .Include(e => e.Votes)
                .Include(e => e.Participant)
                .Include(e => e.EntryImages)
                .ToListAsync();

            // Calculate ranking
            var rankedEntries = allContestEntries
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .ToList();

            var entryRanking = rankedEntries.FindIndex(e => e.Id == entryId) + 1;
            var totalVotesInContest = allContestEntries.Sum(e => e.Votes.Count);
            var votePercentage = totalVotesInContest > 0 ? (double)entry.Votes.Count / totalVotesInContest * 100 : 0;

            // Check if user has voted for this entry
            var userVote = !string.IsNullOrEmpty(currentUserId) ?
                await _context.Votes.FirstOrDefaultAsync(v => v.UserId == currentUserId && v.ContestEntryId == entryId) :
                null;

            // Determine contest phase
            string contestPhase;
            if (now < contest.SubmissionEndDate)
                contestPhase = "Submission";
            else if (now < contest.VotingEndDate)
                contestPhase = "Voting";
            else
                contestPhase = "Results";

            // Check user permissions
            bool isOwnEntry = !string.IsNullOrEmpty(currentUserId) && entry.ParticipantId == currentUserId;
            bool canUserVote = !string.IsNullOrEmpty(currentUserId) &&
                              now >= contest.VotingStartDate &&
                              now <= contest.VotingEndDate &&
                              !isOwnEntry &&
                              userVote == null;
            bool canEdit = isOwnEntry && now <= contest.SubmissionEndDate && contest.IsActive;

            // Check if entry is winner
            bool isWinner = contest.Winners.Any(w => w.ContestEntryId == entryId);
            int? winnerPosition = contest.Winners.FirstOrDefault(w => w.ContestEntryId == entryId)?.Position;

            return new ContestEntryDetailsViewModel
            {
                // Entry Information
                Id = entry.Id,
                Title = entry.Title,
                Description = entry.Description,
                SubmittedAt = entry.SubmittedAt,
                IsActive = entry.IsActive,

                // Participant Information
                ParticipantId = entry.ParticipantId,
                ParticipantName = entry.Participant.UserName ?? "Unknown",

                // Contest Information
                ContestId = contest.Id,
                ContestTitle = contest.Title,
                ContestDescription = contest.Description,
                ContestSubmissionStartDate = contest.SubmissionStartDate,
                ContestSubmissionEndDate = contest.SubmissionEndDate,
                ContestVotingStartDate = contest.VotingStartDate,
                ContestVotingEndDate = contest.VotingEndDate,
                ContestPhase = contestPhase,
                IsContestActive = contest.IsActive,

                // Entry Images
                Images = entry.EntryImages
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new EntryImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        Caption = img.Caption,
                        DisplayOrder = img.DisplayOrder,
                        UploadedAt = img.UploadedAt
                    }).ToList(),

                // Voting Information
                VoteCount = entry.Votes.Count,
                Votes = entry.Votes
                    .OrderByDescending(v => v.VotedAt)
                    .Select(v => new VoteDetailViewModel
                    {
                        Id = v.Id,
                        VoterName = v.User.UserName ?? "Anonymous",
                        VotedAt = v.VotedAt,
                        IsAnonymous = true // Keep voter names private by default
                    }).ToList(),

                // User Context
                IsOwnEntry = isOwnEntry,
                CanUserVote = canUserVote,
                HasUserVoted = userVote != null,
                CanEdit = canEdit,

                // Competition Information
                EntryRanking = entryRanking,
                TotalEntriesInContest = allContestEntries.Count,
                VotePercentage = votePercentage,
                IsWinner = isWinner,
                WinnerPosition = winnerPosition,

                // Statistics
                LastVoteDate = entry.Votes.Any() ? entry.Votes.Max(v => v.VotedAt) : null,
                FirstVoteDate = entry.Votes.Any() ? entry.Votes.Min(v => v.VotedAt) : null,

                // Related Entries (top 5 other entries from same contest)
                RelatedEntries = rankedEntries
                    .Where(e => e.Id != entryId)
                    .Take(5)
                    .Select(e => new RelatedEntryViewModel
                    {
                        Id = e.Id,
                        Title = e.Title,
                        ParticipantName = e.Participant.UserName ?? "Unknown",
                        ThumbnailImageUrl = e.EntryImages.OrderBy(img => img.DisplayOrder).FirstOrDefault()?.ImageUrl,
                        VoteCount = e.Votes.Count,
                        IsWinner = contest.Winners.Any(w => w.ContestEntryId == e.Id)
                    }).ToList()
            };
        }

        private ICollection<EntryImage> GetEntryImages(List<string> imageUrls)
        {
            if (imageUrls == null || !imageUrls.Any())
                throw new ArgumentException("Entry images cannot be null or empty");

            var entryImages = new List<EntryImage>();
            for (int i = 0; i < imageUrls.Count; i++)
            {
                entryImages.Add(new EntryImage
                {
                    ImageUrl = imageUrls[i],
                    DisplayOrder = i + 1,
                    UploadedAt = DateTime.UtcNow
                });
            }

            return entryImages;
        }

        /// <summary>
        /// Automatically determines and sets the winner for a contest when voting ends.
        /// Winner is determined by highest vote count, with ties broken by earliest submission.
        /// </summary>
        /// <param name="contestId">The contest ID to determine winner for</param>
        /// <returns>The contest winner, or null if no entries or winner already exists</returns>
        public async Task<ContestWinner?> DetermineAndSetWinnerAsync(int contestId)
        {
            var contest = await _context.Contests
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Winners)
                .FirstOrDefaultAsync(c => c.Id == contestId);

            if (contest == null)
            {
                _logger.LogWarning("Contest with ID {ContestId} not found for winner determination", contestId);
                return null;
            }

            // Check if voting has ended
            if (contest.IsVotingOpen)
            {
                _logger.LogInformation("Contest {ContestId} voting is still open, cannot determine winner yet", contestId);
                return null;
            }

            // Check if winner already exists
            if (contest.PrimaryWinner != null)
            {
                _logger.LogInformation("Contest {ContestId} already has a winner: Entry {EntryId}",
                    contestId, contest.PrimaryWinner.ContestEntryId);
                return contest.PrimaryWinner;
            }

            // Check if there are any entries
            if (!contest.Entries.Any())
            {
                _logger.LogInformation("Contest {ContestId} has no entries, cannot determine winner", contestId);
                return null;
            }

            // Find winner: highest vote count, then earliest submission for ties
            var winnerEntry = contest.Entries
                .OrderByDescending(e => e.Votes.Count)
                .ThenBy(e => e.SubmittedAt)
                .First();

            var winner = new ContestWinner
            {
                ContestId = contestId,
                ContestEntryId = winnerEntry.Id,
                Position = 1,
                WonAt = DateTime.UtcNow,
                AwardTitle = "Contest Winner",
                Notes = $"Won with {winnerEntry.Votes.Count} votes"
            };

            _context.ContestWinners.Add(winner);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Winner determined for contest {ContestId}: Entry {EntryId} with {VoteCount} votes",
                contestId, winnerEntry.Id, winnerEntry.Votes.Count);

            return winner;
        }

        /// <summary>
        /// Checks all contests where voting has recently ended and determines winners automatically.
        /// </summary>
        /// <returns>List of newly determined winners</returns>
        public async Task<List<ContestWinner>> ProcessEndedContestsAsync()
        {
            var newWinners = new List<ContestWinner>();

            // Find contests where voting ended recently but no winner has been determined
            var endedContests = await _context.Contests
                .Include(c => c.Entries)
                    .ThenInclude(e => e.Votes)
                .Include(c => c.Winners)
                .Where(c => c.VotingEndDate <= DateTime.UtcNow &&
                           !c.Winners.Any(w => w.Position == 1) &&
                           c.Entries.Any())
                .ToListAsync();

            foreach (var contest in endedContests)
            {
                try
                {
                    var winner = await DetermineAndSetWinnerAsync(contest.Id);
                    if (winner != null)
                    {
                        newWinners.Add(winner);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error determining winner for contest {ContestId}", contest.Id);
                }
            }

            if (newWinners.Any())
            {
                _logger.LogInformation("Processed {Count} ended contests and determined {WinnerCount} new winners",
                    endedContests.Count, newWinners.Count);
            }

            return newWinners;
        }
    }
}
