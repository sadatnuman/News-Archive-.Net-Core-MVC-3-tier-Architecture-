using System;
using DAL.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DAL.EF.Migrations
{
    [DbContext(typeof(NewsArchiveDbContext))]
    partial class NewsArchiveDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("ContentLabel", b =>
                {
                    b.Property<int>("ArticleId")
                        .HasColumnType("int");

                    b.Property<int>("TagId")
                        .HasColumnType("int");

                    b.HasKey("ArticleId", "TagId")
                        .HasName("PK__ContentL__4A35BF72DD4B7AFD");

                    b.HasIndex(new[] { "TagId" }, "IX_ContentLabels_TagId");

                    b.ToTable("ContentLabels", (string)null);
                });

            modelBuilder.Entity("DAL.EF.Tables.Article", b =>
                {
                    b.Property<int>("ArticleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ArticleId"));

                    b.Property<int>("AuthorId")
                        .HasColumnType("int");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsBreakingNews")
                        .HasColumnType("bit");

                    b.Property<string>("LanguageCode")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)")
                        .HasDefaultValue("en");

                    b.Property<DateTime>("PublishDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("ReviewNote")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime?>("ReviewedAt")
                        .HasColumnType("datetime");

                    b.Property<int?>("ReviewedById")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ScheduledAt")
                        .HasColumnType("datetime");

                    b.Property<string>("Slug")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.Property<string>("Summary")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("datetime");

                    b.Property<int>("ViewsCount")
                        .HasColumnType("int");

                    b.HasKey("ArticleId")
                        .HasName("PK__Articles__9C6270E827220E8D");

                    b.HasIndex(new[] { "AuthorId", "StatusId" }, "IX_Articles_AuthorId_StatusId");

                    b.HasIndex(new[] { "CategoryId", "StatusId", "PublishDate" }, "IX_Articles_CategoryId_StatusId_PublishDate")
                        .IsDescending(false, false, true);

                    b.HasIndex(new[] { "IsBreakingNews", "StatusId", "PublishDate" }, "IX_Articles_IsBreakingNews_StatusId_PublishDate")
                        .IsDescending(false, false, true);

                    b.HasIndex(new[] { "LanguageCode", "StatusId", "PublishDate" }, "IX_Articles_LanguageCode_StatusId_PublishDate")
                        .IsDescending(false, false, true);

                    b.HasIndex(new[] { "ReviewedById" }, "IX_Articles_ReviewedById");

                    b.HasIndex(new[] { "StatusId", "PublishDate" }, "IX_Articles_StatusId_PublishDate")
                        .IsDescending(false, true);

                    b.HasIndex(new[] { "StatusId", "ScheduledAt" }, "IX_Articles_StatusId_ScheduledAt");

                    b.HasIndex(new[] { "ViewsCount" }, "IX_Articles_ViewsCount");

                    b.ToTable("Articles", t =>
                        {
                            t.HasCheckConstraint("CK_Articles_ViewsCount", "[ViewsCount] >= 0");
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.ArticleStatus", b =>
                {
                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("StatusId")
                        .HasName("PK_ArticleStatuses");

                    b.HasIndex(new[] { "Name" }, "UX_ArticleStatuses_Name")
                        .IsUnique();

                    b.ToTable("ArticleStatuses");

                    b.HasData(
                        new
                        {
                            StatusId = 1,
                            Name = "Draft"
                        },
                        new
                        {
                            StatusId = 2,
                            Name = "Pending"
                        },
                        new
                        {
                            StatusId = 3,
                            Name = "Published"
                        },
                        new
                        {
                            StatusId = 4,
                            Name = "Rejected"
                        },
                        new
                        {
                            StatusId = 5,
                            Name = "Scheduled"
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.AuditLog", b =>
                {
                    b.Property<long>("AuditLogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("AuditLogId"));

                    b.Property<string>("Action")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Details")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("EntityId")
                        .HasColumnType("int");

                    b.Property<string>("EntityType")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(45)
                        .HasColumnType("nvarchar(45)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("AuditLogId")
                        .HasName("PK_AuditLogs");

                    b.HasIndex(new[] { "CreatedAt" }, "IX_AuditLogs_CreatedAt")
                        .IsDescending();

                    b.HasIndex(new[] { "EntityType", "CreatedAt" }, "IX_AuditLogs_EntityType_CreatedAt")
                        .IsDescending(false, true);

                    b.HasIndex(new[] { "UserId" }, "IX_AuditLogs_UserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("DAL.EF.Tables.Bookmark", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("ArticleId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.HasKey("UserId", "ArticleId")
                        .HasName("PK_Bookmarks");

                    b.HasIndex(new[] { "ArticleId" }, "IX_Bookmarks_ArticleId");

                    b.HasIndex(new[] { "UserId", "CreatedAt" }, "IX_Bookmarks_UserId_CreatedAt")
                        .IsDescending(false, true);

                    b.ToTable("Bookmarks");
                });

            modelBuilder.Entity("DAL.EF.Tables.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("DescriptionBn")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("NameBn")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Slug")
                        .HasMaxLength(120)
                        .HasColumnType("nvarchar(120)");

                    b.HasKey("CategoryId")
                        .HasName("PK__Categori__19093A0BC1E2D8D4");

                    b.HasIndex(new[] { "Name" }, "UX_Categories_Name")
                        .IsUnique();

                    b.HasIndex(new[] { "NameBn" }, "UX_Categories_NameBn")
                        .IsUnique()
                        .HasFilter("[NameBn] IS NOT NULL");

                    b.HasIndex(new[] { "Slug" }, "UX_Categories_Slug")
                        .IsUnique()
                        .HasFilter("[Slug] IS NOT NULL");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("DAL.EF.Tables.Comment", b =>
                {
                    b.Property<int>("CommentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CommentId"));

                    b.Property<int>("ArticleId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<bool>("IsHidden")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("ModeratedAt")
                        .HasColumnType("datetime");

                    b.Property<int?>("ModeratedById")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("CommentId")
                        .HasName("PK__Comments__C3B4DFCA5955C019");

                    b.HasIndex(new[] { "ArticleId", "CreatedDate" }, "IX_Comments_ArticleId_CreatedDate")
                        .IsDescending(false, true);

                    b.HasIndex(new[] { "ArticleId", "IsHidden" }, "IX_Comments_ArticleId_IsHidden");

                    b.HasIndex(new[] { "ModeratedById" }, "IX_Comments_ModeratedById");

                    b.HasIndex(new[] { "UserId" }, "IX_Comments_UserId");

                    b.ToTable("Comments");
                });

            modelBuilder.Entity("DAL.EF.Tables.CommentReport", b =>
                {
                    b.Property<int>("CommentId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("Note")
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.Property<int>("ReasonId")
                        .HasColumnType("int");

                    b.HasKey("CommentId", "UserId")
                        .HasName("PK_CommentReports");

                    b.HasIndex(new[] { "ReasonId" }, "IX_CommentReports_ReasonId");

                    b.HasIndex(new[] { "UserId" }, "IX_CommentReports_UserId");

                    b.ToTable("CommentReports");
                });

            modelBuilder.Entity("DAL.EF.Tables.Language", b =>
                {
                    b.Property<string>("LanguageCode")
                        .HasMaxLength(5)
                        .HasColumnType("nvarchar(5)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("NativeName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("LanguageCode")
                        .HasName("PK_Languages");

                    b.ToTable("Languages");

                    b.HasData(
                        new
                        {
                            LanguageCode = "en",
                            Name = "English",
                            NativeName = "English"
                        },
                        new
                        {
                            LanguageCode = "bn",
                            Name = "Bangla",
                            NativeName = "বাংলা"
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.ReadingHistory", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<int>("ArticleId")
                        .HasColumnType("int");

                    b.Property<DateTime>("FirstReadAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime>("LastReadAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<int>("ReadCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasDefaultValue(1);

                    b.HasKey("UserId", "ArticleId")
                        .HasName("PK_ReadingHistory");

                    b.HasIndex(new[] { "ArticleId" }, "IX_ReadingHistory_ArticleId");

                    b.HasIndex(new[] { "UserId", "LastReadAt" }, "IX_ReadingHistory_UserId_LastReadAt")
                        .IsDescending(false, true);

                    b.ToTable("ReadingHistory", null, t =>
                        {
                            t.HasCheckConstraint("CK_ReadingHistory_ReadCount", "[ReadCount] >= 1");
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.ReportReason", b =>
                {
                    b.Property<int>("ReasonId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("ReasonId")
                        .HasName("PK_ReportReasons");

                    b.HasIndex(new[] { "Name" }, "UX_ReportReasons_Name")
                        .IsUnique();

                    b.ToTable("ReportReasons");

                    b.HasData(
                        new
                        {
                            ReasonId = 1,
                            Name = "Spam"
                        },
                        new
                        {
                            ReasonId = 2,
                            Name = "Abuse or harassment"
                        },
                        new
                        {
                            ReasonId = 3,
                            Name = "Off topic"
                        },
                        new
                        {
                            ReasonId = 4,
                            Name = "Misinformation"
                        },
                        new
                        {
                            ReasonId = 5,
                            Name = "Other"
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id")
                        .HasName("PK_Roles");

                    b.HasIndex(new[] { "RoleName" }, "UX_Roles_RoleName")
                        .IsUnique();

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            RoleName = "Admin"
                        },
                        new
                        {
                            Id = 2,
                            RoleName = "Reader"
                        },
                        new
                        {
                            Id = 3,
                            RoleName = "Author"
                        });
                });

            modelBuilder.Entity("DAL.EF.Tables.Tag", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TagId"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("NameBn")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Slug")
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.HasKey("TagId")
                        .HasName("PK__Tags__657CF9AC4036D6BB");

                    b.HasIndex(new[] { "Name" }, "UX_Tags_Name")
                        .IsUnique();

                    b.HasIndex(new[] { "NameBn" }, "UX_Tags_NameBn")
                        .IsUnique()
                        .HasFilter("[NameBn] IS NOT NULL");

                    b.HasIndex(new[] { "Slug" }, "UX_Tags_Slug")
                        .IsUnique()
                        .HasFilter("[Slug] IS NOT NULL");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("DAL.EF.Tables.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("AvatarPath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Bio")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())", "DF__User__CreatedAt__4D94879B");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(150)
                        .HasColumnType("nvarchar(150)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserId")
                        .HasName("PK__User__1788CC4C12AF141B");

                    b.HasIndex(new[] { "RoleId" }, "IX_Users_Role");

                    b.HasIndex(new[] { "Email" }, "UQ__User__A9D1053417333C75")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("DAL.EF.Tables.UserToken", b =>
                {
                    b.Property<int>("TokenId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TokenId"));

                    b.Property<string>("ClientType")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime");

                    b.Property<DateTime?>("RevokedAt")
                        .HasColumnType("datetime");

                    b.Property<string>("TokenHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nchar(64)")
                        .IsFixedLength();

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("TokenId")
                        .HasName("PK_UserTokens");

                    b.HasIndex(new[] { "UserId", "ExpiresAt" }, "IX_UserTokens_UserId_ExpiresAt");

                    b.HasIndex(new[] { "TokenHash" }, "UX_UserTokens_TokenHash")
                        .IsUnique();

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("ContentLabel", b =>
                {
                    b.HasOne("DAL.EF.Tables.Article", null)
                        .WithMany()
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ContentLabels_Articles");

                    b.HasOne("DAL.EF.Tables.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ContentLabels_Tags");
                });

            modelBuilder.Entity("DAL.EF.Tables.Article", b =>
                {
                    b.HasOne("DAL.EF.Tables.User", "Author")
                        .WithMany("Articles")
                        .HasForeignKey("AuthorId")
                        .IsRequired()
                        .HasConstraintName("FK_Articles_Users");

                    b.HasOne("DAL.EF.Tables.Category", "Category")
                        .WithMany("Articles")
                        .HasForeignKey("CategoryId")
                        .IsRequired()
                        .HasConstraintName("FK_Articles_Categories");

                    b.HasOne("DAL.EF.Tables.Language", "Language")
                        .WithMany("Articles")
                        .HasForeignKey("LanguageCode")
                        .IsRequired()
                        .HasConstraintName("FK_Articles_Languages");

                    b.HasOne("DAL.EF.Tables.User", "ReviewedBy")
                        .WithMany("ReviewedArticles")
                        .HasForeignKey("ReviewedById")
                        .HasConstraintName("FK_Articles_ReviewedBy_Users");

                    b.HasOne("DAL.EF.Tables.ArticleStatus", "Status")
                        .WithMany("Articles")
                        .HasForeignKey("StatusId")
                        .IsRequired()
                        .HasConstraintName("FK_Articles_ArticleStatuses");

                    b.Navigation("Author");

                    b.Navigation("Category");

                    b.Navigation("Language");

                    b.Navigation("ReviewedBy");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("DAL.EF.Tables.AuditLog", b =>
                {
                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("AuditLogs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("FK_AuditLogs_Users");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.Bookmark", b =>
                {
                    b.HasOne("DAL.EF.Tables.Article", "Article")
                        .WithMany("Bookmarks")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Bookmarks_Articles");

                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("Bookmarks")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Bookmarks_Users");

                    b.Navigation("Article");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.Comment", b =>
                {
                    b.HasOne("DAL.EF.Tables.Article", "Article")
                        .WithMany("Comments")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Comments_Articles");

                    b.HasOne("DAL.EF.Tables.User", "ModeratedBy")
                        .WithMany("ModeratedComments")
                        .HasForeignKey("ModeratedById")
                        .HasConstraintName("FK_Comments_ModeratedBy_Users");

                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_Comments_Users");

                    b.Navigation("Article");

                    b.Navigation("ModeratedBy");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.CommentReport", b =>
                {
                    b.HasOne("DAL.EF.Tables.Comment", "Comment")
                        .WithMany("Reports")
                        .HasForeignKey("CommentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_CommentReports_Comments");

                    b.HasOne("DAL.EF.Tables.ReportReason", "Reason")
                        .WithMany("Reports")
                        .HasForeignKey("ReasonId")
                        .IsRequired()
                        .HasConstraintName("FK_CommentReports_ReportReasons");

                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("CommentReports")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK_CommentReports_Users");

                    b.Navigation("Comment");

                    b.Navigation("Reason");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.ReadingHistory", b =>
                {
                    b.HasOne("DAL.EF.Tables.Article", "Article")
                        .WithMany("Readers")
                        .HasForeignKey("ArticleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ReadingHistory_Articles");

                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("ReadingHistory")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_ReadingHistory_Users");

                    b.Navigation("Article");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.User", b =>
                {
                    b.HasOne("DAL.EF.Tables.Role", "Role")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .IsRequired()
                        .HasConstraintName("FK_Roles_Users");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("DAL.EF.Tables.UserToken", b =>
                {
                    b.HasOne("DAL.EF.Tables.User", "User")
                        .WithMany("Tokens")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_UserTokens_Users");

                    b.Navigation("User");
                });

            modelBuilder.Entity("DAL.EF.Tables.Article", b =>
                {
                    b.Navigation("Bookmarks");

                    b.Navigation("Comments");

                    b.Navigation("Readers");
                });

            modelBuilder.Entity("DAL.EF.Tables.ArticleStatus", b =>
                {
                    b.Navigation("Articles");
                });

            modelBuilder.Entity("DAL.EF.Tables.Category", b =>
                {
                    b.Navigation("Articles");
                });

            modelBuilder.Entity("DAL.EF.Tables.Comment", b =>
                {
                    b.Navigation("Reports");
                });

            modelBuilder.Entity("DAL.EF.Tables.Language", b =>
                {
                    b.Navigation("Articles");
                });

            modelBuilder.Entity("DAL.EF.Tables.ReportReason", b =>
                {
                    b.Navigation("Reports");
                });

            modelBuilder.Entity("DAL.EF.Tables.Role", b =>
                {
                    b.Navigation("Users");
                });

            modelBuilder.Entity("DAL.EF.Tables.User", b =>
                {
                    b.Navigation("Articles");

                    b.Navigation("AuditLogs");

                    b.Navigation("Bookmarks");

                    b.Navigation("CommentReports");

                    b.Navigation("Comments");

                    b.Navigation("ModeratedComments");

                    b.Navigation("ReadingHistory");

                    b.Navigation("ReviewedArticles");

                    b.Navigation("Tokens");
                });
#pragma warning restore 612, 618
        }
    }
}
