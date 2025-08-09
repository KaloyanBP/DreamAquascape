namespace DreamAquascape.GCommon
{
    public static class ApplicationConstants
    {
        public const string AdminRoleName = "Admin";
        public const string UserRoleName = "User";
        public const string AnonymousUser = "Anonymous user";
        public const string UnknownUserName = "Unknown";
        public const string ApplicationName = "Dream Aquascape";
        public const string ApplicationVersion = "1.0.0";
        public const string ApplicationDescription = "An application for aquascaping enthusiasts to share and discover beautiful aquascapes.";
        public const string DefaultImageUrl = "/images/default-aquascape.jpg";
        public const string DefaultNavigationUrl = "/home";
        public const string IsDeletedPropertyName = "IsDeleted";

        public const string UploadPath = "uploads";
        public const string EntryImageUploadPath = "entries";
        public const string ContestImageUploadPath = "contests";
        public const string PrizeImageUploadPath = "prizes";
        public const long MaxFileSize = 5 * 1024 * 1024;
        public static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
    }
}
