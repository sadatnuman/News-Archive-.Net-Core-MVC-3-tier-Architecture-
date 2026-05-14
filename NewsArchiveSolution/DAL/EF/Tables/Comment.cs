using System;
using System.Collections.Generic;

namespace DAL.EF.Tables;

public partial class Comment
{
    public int CommentId { get; set; }

    public int ArticleId { get; set; }

    public int UserId { get; set; }

    public string Text { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
