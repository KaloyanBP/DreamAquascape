namespace DreamAquascape.Data.Common
{
    public static class EntityConstants
    {
        public static class ContestEntry
        {
            public const int TitleMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
        }
        public static class Contest
        {
            public const int TitleMaxLength = 100;
            public const int DescriptionMaxLength = 1000;
            public const int ImageFileUrlMaxLength = 2048; // URL length
            public const int CreatedByMaxLength = 100;
        }

        public static class ContestCategory
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 500;
        }

        public static class User
        {
            public const int UsernameMaxLength = 50;
            public const int EmailMaxLength = 256;
        }

        public static class Prize
        {
            public const int NameMaxLength = 100;
            public const int DescriptionMaxLength = 500;
            public const int ImageUrlMaxLength = 2048; // URL length
            public const int NavigationUrlMaxLength = 2048; // URL length
            public const int SponsorNameMaxLength = 100;
        }

        public static class Vote
        {
            public const int UserNameMaxLength = 100;
            public const int IpAddressMaxLength = 45; // IPv6 length
        }

        public static class EntryImage
        {             
            public const int ImageUrlMaxLength = 2048; // URL length
            public const int CaptionMaxLength = 200;
            public const int DescriptionMaxLength = 500;
        }

        public static class UserContestParticipation
        {
        }
    }
}
