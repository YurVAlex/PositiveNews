namespace PositiveNews.Application.Common;

/// <summary>
/// Stable application error codes returned in <see cref="Error"/> results.
/// </summary>
public static class ErrorCodes
{
    /// <summary>Cross-cutting validation failures.</summary>
    public static class Validation
    {
        public const string Failed = "Validation.Failed";
    }

    /// <summary>Authentication and account errors.</summary>
    public static class Auth
    {
        public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
        public const string RoleMissing = "Auth.RoleMissing";
        public const string InvalidCredentials = "Auth.InvalidCredentials";
        public const string UserNotFound = "Auth.UserNotFound";
        public const string AccountInactive = "Auth.AccountInactive";
        public const string UserUnavailable = "Auth.UserUnavailable";
        public const string InvalidRefreshToken = "Auth.InvalidRefreshToken";
        public const string UserInactive = "Auth.UserInactive";
    }

    /// <summary>Article read errors.</summary>
    public static class Article
    {
        public const string NotFound = "Article.NotFound";
    }

    /// <summary>Public article feed query errors.</summary>
    public static class ArticleFeed
    {
        public const string TopicNotFound = "ArticleFeed.TopicNotFound";
        public const string SourceNotFound = "ArticleFeed.SourceNotFound";
        public const string PageNotFound = "ArticleFeed.PageNotFound";
    }

    /// <summary>Comment-related errors.</summary>
    public static class Comment
    {
        public const string NotFound = "Comment.NotFound";
        public const string SelfComplaint = "Comment.SelfComplaint";
    }

    /// <summary>Complaint submission errors.</summary>
    public static class Complaint
    {
        public const string AlreadySubmitted = "Complaint.AlreadySubmitted";
    }

    /// <summary>User lookup errors outside auth flows.</summary>
    public static class User
    {
        public const string NotFound = "User.NotFound";
    }

    /// <summary>RSS ingestion pipeline errors.</summary>
    public static class Ingestion
    {
        public const string AlreadyRunning = "Ingestion.AlreadyRunning";
        public const string DomainInvariantViolation = "Ingestion.DomainInvariantViolation";
        public const string Unexpected = "Ingestion.Unexpected";
    }

    /// <summary>Administrative moderation and management errors.</summary>
    public static class Admin
    {
        public const string CommentNotFound = "Admin.CommentNotFound";
        public const string CommentUnchanged = "Admin.CommentUnchanged";
        public const string UserNotFound = "Admin.UserNotFound";
        public const string UserUnchanged = "Admin.UserUnchanged";
        public const string SourceNotFound = "Admin.SourceNotFound";
        public const string SourceUnchanged = "Admin.SourceUnchanged";
        public const string SourceIdInvalid = "Admin.SourceIdInvalid";
        public const string ArticleNotFound = "Admin.ArticleNotFound";
        public const string ArticleUnchanged = "Admin.ArticleUnchanged";
    }

    /// <summary>User feed preference errors.</summary>
    public static class FeedPreferences
    {
        public const string TopicNotFound = "FeedPreferences.TopicNotFound";
        public const string SourceNotFound = "FeedPreferences.SourceNotFound";
        public const string SaveFailed = "FeedPreferences.SaveFailed";
    }
}
