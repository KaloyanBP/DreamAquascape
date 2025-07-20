using DreamAquascape.Data;
using DreamAquascape.Data.Models;
using DreamAquascape.Services.Common.Exceptions;
using DreamAquascape.Services.Core.Interfaces;
using DreamAquascape.Web.ViewModels.Contest;
using DreamAquascape.Web.ViewModels.ContestEntry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DreamAquascape.Services.Core
{
    public class ContestService: IContestService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContestService> _logger;

        public ContestService(ApplicationDbContext context, ILogger<ContestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ContestItemViewModel>> GetActiveContestsAsync()
        {
            return await _context.Contests
                .Where(c => c.IsActive && !c.IsDeleted && c.SubmissionStartDate <= DateTime.UtcNow && c.SubmissionEndDate >= DateTime.UtcNow)
                .OrderByDescending(c => c.SubmissionStartDate)
                .Select(c => new ContestItemViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    ImageUrl = c.ImageFileUrl ?? "",
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    //Description = c.Description,
                    //ImageFileUrl = c.ImageFileUrl,
                    //SubmissionStartDate = c.SubmissionStartDate,
                    //SubmissionEndDate = c.SubmissionEndDate,
                    //VotingStartDate = c.VotingStartDate,
                    //VotingEndDate = c.VotingEndDate,
                    //ResultDate = c.ResultDate,
                    //CreatedBy = c.CreatedBy,
                    IsActive = c.IsActive,
                })
                .ToListAsync();
        }

        public async Task<ContestDetailsViewModel?> GetContestWithEntriesAsync(int contestId)
        {
            return await _context.Contests
                .Include(c => c.Entries)
                .AsNoTracking()
                .Where(c => c.Id == contestId && c.IsActive && !c.IsDeleted)
                .Select(c => new ContestDetailsViewModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    StartDate = c.SubmissionStartDate,
                    EndDate = c.VotingEndDate,
                    IsActive = c.IsActive,
                    CanSubmitEntry = true, // TODO: Add logic to determine if user can submit entry
                    CanVote = true,
                    Prize = c.Prize != null ? new PrizeViewModel
                    {
                        Name = c.Prize.Name,
                        Description = c.Prize.Description
                    } : null,
                    Entries = c.Entries.Select(e => new ContestEntryViewModel
                    {
                        Id = e.Id,
                        //UserName = e.Participant.UserName, 
                        Description = e.Description,
                        EntryImages = e.EntryImages.Select(img => img.ImageUrl).ToList(),
                        VoteCount = e.Votes.Count()
                    }).ToList(),
                    WinnerEntryId = c.WinnerEntryId,
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Contest> SubmitContestAsync(CreateContestViewModel dto, PrizeViewModel prizeDto, string createdBy)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate submission dates
                if (dto.SubmissionStartDate >= dto.SubmissionEndDate)
                    throw new InvalidOperationException("Submission start date must be before end date");
                if (dto.VotingStartDate <= dto.SubmissionEndDate || dto.VotingEndDate <= dto.VotingStartDate)
                    throw new InvalidOperationException("Voting dates must be after submission end date and before voting end date");
                // Create the contest
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
                    Prize = new Prize
                    {
                        Name = prizeDto.Name,
                        Description = prizeDto.Description,
                        ImageUrl = prizeDto.ImageUrl
                    }
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

        public async Task<ContestEntry> SubmitEntryAsync(CreateContestEntryViewModel dto, string userId, string userName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate submission period
                var contest = await _context.Contests
                    .FirstOrDefaultAsync(c => c.Id == dto.ContestId && c.IsActive && !c.IsDeleted);

                if (contest == null)
                    throw new NotFoundException("Contest not found");

                if (DateTime.UtcNow < contest.SubmissionStartDate || DateTime.UtcNow > contest.SubmissionEndDate)
                    throw new InvalidOperationException("Contest submission period is not active");

                // Check if user already has an entry
                var existingEntry = await _context.ContestEntries
                    .FirstOrDefaultAsync(e => e.ContestId == dto.ContestId && e.ParticipantId == userId && !e.IsDeleted);

                if (existingEntry != null)
                    throw new InvalidOperationException("User already has an entry in this contest");

                // Create the contest entry
                var entry = new ContestEntry
                {
                    ContestId = dto.ContestId,
                    ParticipantId = userId,
                    Title = dto.Title,
                    Description = dto.Description,
                    SubmittedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false,
                    EntryImages = GetEntryImages(dto.EntryImages)
                };

                await _context.ContestEntries.AddAsync(entry);
                await _context.SaveChangesAsync(); // Save to get the entry ID

                // Update or create participation record
                var participation = await _context.UserContestParticipations
                    .FirstOrDefaultAsync(p => p.ContestId == dto.ContestId && p.UserId == userId);

                if (participation == null)
                {
                    participation = new UserContestParticipation
                    {
                        ContestId = dto.ContestId,
                        UserId = userId,
                        ParticipationDate = DateTime.UtcNow,
                        HasSubmittedEntry = true,
                        SubmittedEntryId = entry.Id,
                        EntrySubmittedAt = DateTime.UtcNow,
                        HasVoted = false
                    };
                    await _context.UserContestParticipations.AddAsync(participation);
                }
                else
                {
                    participation.HasSubmittedEntry = true;
                    participation.SubmittedEntryId = entry.Id;
                    participation.EntrySubmittedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Entry {EntryId} submitted by user {UserId} for contest {ContestId}",
                    entry.Id, userId, dto.ContestId);

                return entry;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to submit entry for user {UserId} in contest {ContestId}",
                    userId, dto.ContestId);
                throw;
            }
        }

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
                    .FirstOrDefaultAsync(v => v.ContestId == contestId && v.UserId == userId);

                if (existingVote != null)
                    throw new InvalidOperationException("User has already voted in this contest");

                // Create the vote
                var vote = new Vote
                {
                    ContestId = contestId,
                    ContestEntryId = entryId,
                    UserId = userId,
                    VotedAt = DateTime.UtcNow,
                    IpAddress = ipAddress
                };

                await _context.Votes.AddAsync(vote);

                // Update or create participation record
                var participation = await _context.UserContestParticipations
                    .FirstOrDefaultAsync(p => p.ContestId == contestId && p.UserId == userId);

                if (participation == null)
                {
                    participation = new UserContestParticipation
                    {
                        ContestId = contestId,
                        UserId = userId,
                        ParticipationDate = DateTime.UtcNow,
                        HasVoted = true,
                        VotedForEntryId = entryId,
                        VotedAt = DateTime.UtcNow,
                        HasSubmittedEntry = false
                    };
                    await _context.UserContestParticipations.AddAsync(participation);
                }
                else
                {
                    participation.HasVoted = true;
                    participation.VotedForEntryId = entryId;
                    participation.VotedAt = DateTime.UtcNow;
                }

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
                    .FirstOrDefaultAsync(v => v.ContestId == contestId && v.UserId == userId);

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

                // Update participation record
                var participation = await _context.UserContestParticipations
                    .FirstOrDefaultAsync(p => p.ContestId == contestId && p.UserId == userId);

                if (participation != null)
                {
                    participation.VotedForEntryId = newEntryId;
                    participation.VotedAt = DateTime.UtcNow;
                }

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
    }
}
