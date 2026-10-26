using DAL.EF.Tables;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

public partial class NewsArchiveDbContext : DbContext
{
    public NewsArchiveDbContext()
    {
    }

    public NewsArchiveDbContext(DbContextOptions<NewsArchiveDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Article> Articles { get; set; }

    public virtual DbSet<ArticleStatus> ArticleStatuses { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserToken> UserTokens { get; set; }

    public virtual DbSet<Bookmark> Bookmarks { get; set; }

    public virtual DbSet<ReadingHistory> ReadingHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer("Name=DbConn");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticleStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK_ArticleStatuses");

            entity.Property(e => e.StatusId).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(30);

            entity.HasIndex(e => e.Name, "UX_ArticleStatuses_Name").IsUnique();

            entity.HasData(
                new ArticleStatus { StatusId = ArticleStatusIds.Draft, Name = "Draft" },
                new ArticleStatus { StatusId = ArticleStatusIds.Pending, Name = "Pending" },
                new ArticleStatus { StatusId = ArticleStatusIds.Published, Name = "Published" },
                new ArticleStatus { StatusId = ArticleStatusIds.Rejected, Name = "Rejected" },
                new ArticleStatus { StatusId = ArticleStatusIds.Scheduled, Name = "Scheduled" });
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.LanguageCode).HasName("PK_Languages");

            entity.Property(e => e.LanguageCode).HasMaxLength(5);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NativeName).HasMaxLength(50);

            entity.HasData(
                new Language { LanguageCode = LanguageCodes.English, Name = "English", NativeName = "English" },
                new Language { LanguageCode = LanguageCodes.Bangla, Name = "Bangla", NativeName = "বাংলা" });
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("PK__Articles__9C6270E827220E8D");

            entity.HasIndex(e => new { e.LanguageCode, e.StatusId, e.PublishDate }, "IX_Articles_LanguageCode_StatusId_PublishDate")
                .IsDescending(false, false, true);

            entity.HasIndex(e => new { e.StatusId, e.PublishDate }, "IX_Articles_StatusId_PublishDate")
                .IsDescending(false, true);

            entity.HasIndex(e => new { e.StatusId, e.ScheduledAt }, "IX_Articles_StatusId_ScheduledAt");

            entity.Property(e => e.ScheduledAt).HasColumnType("datetime");

            entity.HasIndex(e => new { e.CategoryId, e.StatusId, e.PublishDate }, "IX_Articles_CategoryId_StatusId_PublishDate")
                .IsDescending(false, false, true);

            entity.HasIndex(e => new { e.AuthorId, e.StatusId }, "IX_Articles_AuthorId_StatusId");

            entity.HasIndex(e => new { e.IsBreakingNews, e.StatusId, e.PublishDate }, "IX_Articles_IsBreakingNews_StatusId_PublishDate")
                .IsDescending(false, false, true);

            entity.HasIndex(e => e.ViewsCount, "IX_Articles_ViewsCount");

            entity.HasIndex(e => e.ReviewedById, "IX_Articles_ReviewedById");

            entity.ToTable(t => t.HasCheckConstraint("CK_Articles_ViewsCount", "[ViewsCount] >= 0"));

            entity.Property(e => e.ImagePath).HasMaxLength(255);
            entity.Property(e => e.PublishDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Summary).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.StatusId).HasDefaultValue(ArticleStatusIds.Draft);
            entity.Property(e => e.LanguageCode).HasMaxLength(5).HasDefaultValue(LanguageCodes.English);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.ReviewNote).HasMaxLength(500);

            entity.HasOne(d => d.Author).WithMany(p => p.Articles)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Articles_Users");

            entity.HasOne(d => d.Category).WithMany(p => p.Articles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Articles_Categories");

            entity.HasOne(d => d.Status).WithMany(p => p.Articles)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Articles_ArticleStatuses");

            entity.HasOne(d => d.Language).WithMany(p => p.Articles)
                .HasForeignKey(d => d.LanguageCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Articles_Languages");

            entity.HasOne(d => d.ReviewedBy).WithMany(p => p.ReviewedArticles)
                .HasForeignKey(d => d.ReviewedById)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Articles_ReviewedBy_Users");

            entity.HasMany(d => d.Tags).WithMany(p => p.Articles)
                .UsingEntity<Dictionary<string, object>>(
                    "ContentLabel",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("FK_ContentLabels_Tags"),
                    l => l.HasOne<Article>().WithMany()
                        .HasForeignKey("ArticleId")
                        .HasConstraintName("FK_ContentLabels_Articles"),
                    j =>
                    {
                        j.HasKey("ArticleId", "TagId").HasName("PK__ContentL__4A35BF72DD4B7AFD");
                        j.ToTable("ContentLabels");
                        j.HasIndex(new[] { "TagId" }, "IX_ContentLabels_TagId");
                    });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BC1E2D8D4");

            entity.HasIndex(e => e.Name, "UX_Categories_Name").IsUnique();

            entity.HasIndex(e => e.Slug, "UX_Categories_Slug").IsUnique().HasFilter("[Slug] IS NOT NULL");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Slug).HasMaxLength(120);
            entity.Property(e => e.NameBn).HasMaxLength(100);
            entity.Property(e => e.DescriptionBn).HasMaxLength(255);

            entity.HasIndex(e => e.NameBn, "UX_Categories_NameBn").IsUnique().HasFilter("[NameBn] IS NOT NULL");
        });

        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ArticleId }).HasName("PK_Bookmarks");

            entity.HasIndex(e => e.ArticleId, "IX_Bookmarks_ArticleId");
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_Bookmarks_UserId_CreatedAt").IsDescending(false, true);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Bookmarks_Users");

            entity.HasOne(d => d.Article).WithMany(p => p.Bookmarks)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Bookmarks_Articles");
        });

        modelBuilder.Entity<ReadingHistory>(entity =>
        {
            entity.ToTable("ReadingHistory", t => t.HasCheckConstraint("CK_ReadingHistory_ReadCount", "[ReadCount] >= 1"));

            entity.HasKey(e => new { e.UserId, e.ArticleId }).HasName("PK_ReadingHistory");

            entity.HasIndex(e => e.ArticleId, "IX_ReadingHistory_ArticleId");
            entity.HasIndex(e => new { e.UserId, e.LastReadAt }, "IX_ReadingHistory_UserId_LastReadAt").IsDescending(false, true);

            entity.Property(e => e.FirstReadAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LastReadAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReadCount).HasDefaultValue(1);

            entity.HasOne(d => d.User).WithMany(p => p.ReadingHistory)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ReadingHistory_Users");

            entity.HasOne(d => d.Article).WithMany(p => p.Readers)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ReadingHistory_Articles");
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__Comments__C3B4DFCA5955C019");

            entity.HasIndex(e => new { e.ArticleId, e.CreatedDate }, "IX_Comments_ArticleId_CreatedDate")
                .IsDescending(false, true);

            entity.HasIndex(e => e.UserId, "IX_Comments_UserId");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Text).HasMaxLength(500);

            entity.HasOne(d => d.Article).WithMany(p => p.Comments)
                .HasForeignKey(d => d.ArticleId)
                .HasConstraintName("FK_Comments_Articles");

            entity.HasOne(d => d.User).WithMany(p => p.Comments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Comments_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Roles");

            entity.HasIndex(e => e.RoleName, "UX_Roles_RoleName").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);

            entity.HasData(
                new Role { Id = RoleIds.Admin, RoleName = "Admin" },
                new Role { Id = RoleIds.Reader, RoleName = "Reader" },
                new Role { Id = RoleIds.Author, RoleName = "Author" });
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.TagId).HasName("PK__Tags__657CF9AC4036D6BB");

            entity.HasIndex(e => e.Name, "UX_Tags_Name").IsUnique();

            entity.HasIndex(e => e.NameBn, "UX_Tags_NameBn").IsUnique().HasFilter("[NameBn] IS NOT NULL");

            entity.HasIndex(e => e.Slug, "UX_Tags_Slug").IsUnique().HasFilter("[Slug] IS NOT NULL");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameBn).HasMaxLength(50);
            entity.Property(e => e.Slug).HasMaxLength(80);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C12AF141B");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053417333C75").IsUnique();

            entity.HasIndex(e => e.RoleId, "IX_Users_Role");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())", "DF__User__CreatedAt__4D94879B")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.AvatarPath).HasMaxLength(255);
            entity.Property(e => e.Bio).HasMaxLength(500);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Roles_Users");
        });

        modelBuilder.Entity<UserToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK_UserTokens");

            entity.HasIndex(e => e.TokenHash, "UX_UserTokens_TokenHash").IsUnique();

            entity.HasIndex(e => new { e.UserId, e.ExpiresAt }, "IX_UserTokens_UserId_ExpiresAt");

            entity.Property(e => e.TokenHash).HasMaxLength(64).IsFixedLength();
            entity.Property(e => e.ClientType).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.RevokedAt).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_UserTokens_Users");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
