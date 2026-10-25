namespace DAL.EF.Tables;

public partial class UserToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public string ClientType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; } = null!;
}
