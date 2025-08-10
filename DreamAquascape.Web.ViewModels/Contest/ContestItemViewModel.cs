using System;
using System.Collections.Generic;

namespace DreamAquascape.Web.ViewModels.Contest
{
    public class ContestItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime SubmissionStartDate { get; set; }
        public DateTime SubmissionEndDate { get; set; }
        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public int VoteCount { get; set; }
        public int TotalEntries { get; set; }
        public int TotalVotes { get; set; }
        public List<PrizeViewModel> Prizes { get; set; } = new List<PrizeViewModel>();
        public List<string> Categories { get; set; } = new List<string>();
    }
}
