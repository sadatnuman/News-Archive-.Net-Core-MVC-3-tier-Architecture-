using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? AvatarPath { get; set; }

    public string? Bio { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Article> ReviewedArticles { get; set; } = new List<Article>();

    public virtual ICollection<UserToken> Tokens { get; set; } = new List<UserToken>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();

    public virtual ICollection<ReadingHistory> ReadingHistory { get; set; } = new List<ReadingHistory>();

    public virtual ICollection<CommentReport> CommentReports { get; set; } = new List<CommentReport>();

    public virtual ICollection<Comment> ModeratedComments { get; set; } = new List<Comment>();

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual Role Role { get; set; } = null!;
}
