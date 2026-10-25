using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Article
{
    public int ArticleId { get; set; }

    public string Title { get; set; } = null!;

    public string? Slug { get; set; }

    public string Summary { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime PublishDate { get; set; }

    public int ViewsCount { get; set; }

    public bool IsBreakingNews { get; set; }

    public string? ImagePath { get; set; }

    public int CategoryId { get; set; }

    public int AuthorId { get; set; }

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? ReviewedById { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public string? ReviewNote { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual ArticleStatus Status { get; set; } = null!;

    public virtual User? ReviewedBy { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
