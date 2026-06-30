namespace PositiveNews.Domain.Constants;

/// <summary>
/// Maximum field lengths aligned with EF Core column constraints.
/// </summary>
public static class FieldLengths
{
    /// <summary>User profile and credential fields.</summary>
    public static class User
    {
        public const int Email = 300;
        public const int Name = 200;
        public const int AvatarUrl = 1000;
    }

    /// <summary>Community comment fields.</summary>
    public static class Comment
    {
        public const int Content = 2000;
    }

    /// <summary>Complaint fields.</summary>
    public static class Complaint
    {
        public const int Reason = 500;
    }

    /// <summary>Article metadata fields.</summary>
    public static class Article
    {
        public const int ExternalId = 300;
        public const int Title = 500;
        public const int Author = 300;
        public const int Url = 1000;
        public const int SummaryShort = 2000;
        public const int LanguageCode = 10;
        public const int RegionCode = 10;
    }

    /// <summary>News source fields.</summary>
    public static class Source
    {
        public const int Name = 200;
        public const int Description = 1000;
        public const int Url = 500;
    }

    /// <summary>Authentication-related validation limits.</summary>
    public static class Auth
    {
        public const int PasswordMin = 8;
        public const int PasswordMax = 128;
        public const int RefreshToken = 256;
    }
}
