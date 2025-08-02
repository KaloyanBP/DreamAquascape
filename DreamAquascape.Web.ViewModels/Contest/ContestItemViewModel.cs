using System;

namespace DreamAquascape.Web.ViewModels.Contest
{
    public class ContestItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public int VoteCount { get; set; }
    }
}
