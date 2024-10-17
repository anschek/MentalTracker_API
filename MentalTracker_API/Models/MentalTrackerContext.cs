using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MentalTracker_API.Models
{
    public partial class MentalTrackerContext : DbContext
    {
        public MentalTrackerContext()
        {
        }

        public MentalTrackerContext(DbContextOptions<MentalTrackerContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Article> Articles { get; set; } = null!;
        public virtual DbSet<ArticleTag> ArticleTags { get; set; } = null!;
        public virtual DbSet<ArticleType> ArticleTypes { get; set; } = null!;
        public virtual DbSet<DailyState> DailyStates { get; set; } = null!;
        public virtual DbSet<Metric> Metrics { get; set; } = null!;
        public virtual DbSet<MetricInDailyState> MetricInDailyStates { get; set; } = null!;
        public virtual DbSet<MetricType> MetricTypes { get; set; } = null!;
        public virtual DbSet<Mood> Moods { get; set; } = null!;
        public virtual DbSet<MoodBasis> MoodBases { get; set; } = null!;
        public virtual DbSet<TagMetricMatch> TagMetricMatches { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //global
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseNpgsql("Host=ngknn.ru;Port=5442;Database=mental_tracker;Username=31P;Password=12345");
                //local
                //optionsBuilder.UseNpgsql("Host=edu.pg.ngknn.local;Port=5432;Database=mental_tracker;Username=31P;Password=12345");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Article>(entity =>
            {
                entity.ToTable("articles");

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("nextval('atricles_id_seq'::regclass)");

                entity.Property(e => e.Author)
                    .HasMaxLength(200)
                    .HasColumnName("author");

                entity.Property(e => e.Content)
                    .HasMaxLength(4000)
                    .HasColumnName("content");

                entity.Property(e => e.PostingDate).HasColumnName("posting_date");

                entity.Property(e => e.Title)
                    .HasMaxLength(255)
                    .HasColumnName("title");

                entity.Property(e => e.TypeId).HasColumnName("type_id");

                entity.HasOne(d => d.Type)
                    .WithMany(p => p.Articles)
                    .HasForeignKey(d => d.TypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("atricles_type_id_fkey");

                entity.HasMany(d => d.Tags)
                    .WithMany(p => p.Articles)
                    .UsingEntity<Dictionary<string, object>>(
                        "ArticlesTag",
                        l => l.HasOne<ArticleTag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("articles_tags_tag_id_fkey"),
                        r => r.HasOne<Article>().WithMany().HasForeignKey("ArticleId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("articles_tags_article_id_fkey"),
                        j =>
                        {
                            j.HasKey("ArticleId", "TagId").HasName("pk_articles_tags");

                            j.ToTable("articles_tags");

                            j.IndexerProperty<int>("ArticleId").HasColumnName("article_id");

                            j.IndexerProperty<int>("TagId").HasColumnName("tag_id");
                        });
            });

            modelBuilder.Entity<ArticleTag>(entity =>
            {
                entity.ToTable("article_tags");

                entity.HasIndex(e => e.Name, "article_tags_name_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .HasColumnName("name");

                entity.HasMany(d => d.Moods)
                    .WithMany(p => p.Tags)
                    .UsingEntity<Dictionary<string, object>>(
                        "TagMoodMatch",
                        l => l.HasOne<Mood>().WithMany().HasForeignKey("MoodId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("tag_mood_matches_mood_id_fkey"),
                        r => r.HasOne<ArticleTag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("tag_mood_matches_tag_id_fkey"),
                        j =>
                        {
                            j.HasKey("TagId", "MoodId").HasName("tag_mood_matches_pkey");

                            j.ToTable("tag_mood_matches");

                            j.IndexerProperty<int>("TagId").HasColumnName("tag_id");

                            j.IndexerProperty<int>("MoodId").HasColumnName("mood_id");
                        });
            });

            modelBuilder.Entity<ArticleType>(entity =>
            {
                entity.ToTable("article_types");

                entity.HasIndex(e => e.Name, "article_types_name_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(30)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<DailyState>(entity =>
            {
                entity.ToTable("daily_states");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.GeneralMoodAssessment).HasColumnName("general_mood_assessment");

                entity.Property(e => e.MoodId).HasColumnName("mood_id");

                entity.Property(e => e.Note)
                    .HasMaxLength(300)
                    .HasColumnName("note");

                entity.Property(e => e.NoteDate).HasColumnName("note_date");

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.HasOne(d => d.Mood)
                    .WithMany(p => p.DailyStates)
                    .HasForeignKey(d => d.MoodId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("daily_states_mood_id_fkey");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.DailyStates)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("daily_states_user_id_fkey");
            });

            modelBuilder.Entity<Metric>(entity =>
            {
                entity.ToTable("metrics");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.IsPositive).HasColumnName("is_positive");

                entity.Property(e => e.MetricTypeId).HasColumnName("metric_type_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasColumnName("name");

                entity.HasOne(d => d.MetricType)
                    .WithMany(p => p.Metrics)
                    .HasForeignKey(d => d.MetricTypeId)
                    .HasConstraintName("metrics_metric_type_id_fkey");
            });

            modelBuilder.Entity<MetricInDailyState>(entity =>
            {
                entity.HasKey(e => new { e.MetricId, e.DailyStateId })
                    .HasName("pk_metric_in_daily_states");

                entity.ToTable("metric_in_daily_states");

                entity.Property(e => e.MetricId).HasColumnName("metric_id");

                entity.Property(e => e.DailyStateId).HasColumnName("daily_state_id");

                entity.Property(e => e.Assessment).HasColumnName("assessment");

                entity.HasOne(d => d.DailyState)
                    .WithMany(p => p.MetricInDailyStates)
                    .HasForeignKey(d => d.DailyStateId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("metric_in_daily_states_daily_state_id_fkey");

                entity.HasOne(d => d.Metric)
                    .WithMany(p => p.MetricInDailyStates)
                    .HasForeignKey(d => d.MetricId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("metric_in_daily_states_metrics_fk");
            });

            modelBuilder.Entity<MetricType>(entity =>
            {
                entity.ToTable("metric_types");

                entity.HasIndex(e => e.Name, "metric_types_name_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Mood>(entity =>
            {
                entity.ToTable("moods");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.MoodBaseId).HasColumnName("mood_base_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasColumnName("name");

                entity.HasOne(d => d.MoodBase)
                    .WithMany(p => p.Moods)
                    .HasForeignKey(d => d.MoodBaseId)
                    .HasConstraintName("moods_mood_base_id_fkey");
            });

            modelBuilder.Entity<MoodBasis>(entity =>
            {
                entity.ToTable("mood_bases");

                entity.HasIndex(e => e.Name, "mood_bases_name_key")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Name)
                    .HasMaxLength(20)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<TagMetricMatch>(entity =>
            {
                entity.HasKey(e => new { e.TagId, e.MetricId })
                    .HasName("tag_metric_matches_pkey");

                entity.ToTable("tag_metric_matches");

                entity.Property(e => e.TagId).HasColumnName("tag_id");

                entity.Property(e => e.MetricId).HasColumnName("metric_id");

                entity.Property(e => e.EndingWith).HasColumnName("ending_with");

                entity.Property(e => e.StartingWith).HasColumnName("starting_with");

                entity.HasOne(d => d.Metric)
                    .WithMany(p => p.TagMetricMatches)
                    .HasForeignKey(d => d.MetricId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tag_metric_matches_metric_id_fkey");

                entity.HasOne(d => d.Tag)
                    .WithMany(p => p.TagMetricMatches)
                    .HasForeignKey(d => d.TagId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tag_metric_matches_tag_id_fkey");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasIndex(e => e.Mail, "users_mail_key")
                    .IsUnique();

                entity.HasIndex(e => e.Mail, "users_mail_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");

                entity.Property(e => e.Mail)
                    .HasMaxLength(30)
                    .HasColumnName("mail");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
