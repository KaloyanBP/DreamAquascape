namespace DreamAquascape.GCommon
{
    public static class ExceptionMessages
    {
        public const string InterfaceNotFoundMessage =
            "The {0} could not be added to the Service Collection, because no interface matching the convention could be found! Convention for Interface naming: I{0}.";
    
        public const string ContestNotFoundMessage =
            "The contest with ID {0} was not found. Please check the contest ID and try again.";

        public const string ContestEntryNotFoundMessage = "Contest entry not found";

        public const string ContestVotingPeriodNotActiveMessage = "Contest voting period is not active";

        public const string UserAlreadyVotedInContestMessage = "User has already voted in this contest";

        public const string NoExistingVoteFoundMessage = "No existing vote found for this user";

        public const string UserCannotVoteForOwnEntryMessage = "Users cannot vote for their own entries";
    }
}
