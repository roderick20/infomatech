using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace SealWebRTC.Models
{
    public partial class EFContext : DbContext
    {
        public EFContext()
        {
        }

        public EFContext(DbContextOptions<EFContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Archive> Archives { get; set; }
        public virtual DbSet<Config> Configs { get; set; }
        public virtual DbSet<Feriado> Feriados { get; set; }
        public virtual DbSet<Meeting> Meetings { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<Typeattention> Typeattentions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=localhost;port=3306;uid=root;password=root;database=webrtc", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.27-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_0900_ai_ci");

            modelBuilder.Entity<Archive>(entity =>
            {
                entity.ToTable("archive");

                entity.HasIndex(e => e.MeetingId, "FK_archive_meeting");

                entity.HasIndex(e => e.UserId, "FK_archive_user");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Extension).HasMaxLength(1000);

                entity.Property(e => e.Name).HasMaxLength(1000);

                entity.Property(e => e.Path).HasMaxLength(1000);

                entity.Property(e => e.UniqueId).HasMaxLength(36);

                entity.HasOne(d => d.Meeting)
                    .WithMany(p => p.Archives)
                    .HasForeignKey(d => d.MeetingId)
                    .HasConstraintName("FK_archive_meeting");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Archives)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_archive_user");
            });

            modelBuilder.Entity<Config>(entity =>
            {
                entity.ToTable("config");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Key)
                    .HasMaxLength(50)
                    .HasColumnName("key");

                entity.Property(e => e.Value)
                    .HasMaxLength(50)
                    .HasColumnName("value");
            });

            modelBuilder.Entity<Feriado>(entity =>
            {
                entity.ToTable("feriados");
            });

            modelBuilder.Entity<Meeting>(entity =>
            {
                entity.ToTable("meeting");

                entity.HasIndex(e => e.TypeAttentionId, "FK_meeting_typeattention");

                entity.HasIndex(e => e.UserClientId, "FK_meeting_user");

                entity.HasIndex(e => e.UserManagerId, "FK_meeting_user_2");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.DurationBegin).HasColumnType("datetime");

                entity.Property(e => e.DurationEnd).HasColumnType("datetime");

                entity.Property(e => e.MeetingDateBegin).HasColumnType("datetime");

                entity.Property(e => e.MeetingDateEnd).HasColumnType("datetime");

                entity.Property(e => e.PeerClient).HasMaxLength(50);

                entity.Property(e => e.PeerIdCliente).HasMaxLength(50);

                entity.Property(e => e.PeerIdManager).HasMaxLength(50);

                entity.Property(e => e.PeerManager).HasMaxLength(50);

                entity.Property(e => e.UniqueId).HasMaxLength(36);

                entity.HasOne(d => d.TypeAttention)
                    .WithMany(p => p.Meetings)
                    .HasForeignKey(d => d.TypeAttentionId)
                    .HasConstraintName("FK_meeting_typeattention");

                entity.HasOne(d => d.UserClient)
                    .WithMany(p => p.MeetingUserClients)
                    .HasForeignKey(d => d.UserClientId)
                    .HasConstraintName("FK_meeting_user");

                entity.HasOne(d => d.UserManager)
                    .WithMany(p => p.MeetingUserManagers)
                    .HasForeignKey(d => d.UserManagerId)
                    .HasConstraintName("FK_meeting_user_2");
            });

            modelBuilder.Entity<Message>(entity =>
            {
                entity.ToTable("message");

                entity.HasIndex(e => e.MeetingId, "FK_message_meeting");

                entity.HasIndex(e => e.UserId, "FK_message_user");

                entity.Property(e => e.MessageDate).HasColumnType("datetime");

                entity.Property(e => e.MessageText).HasMaxLength(1000);

                entity.Property(e => e.UniqueId).HasMaxLength(36);

                entity.HasOne(d => d.Meeting)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.MeetingId)
                    .HasConstraintName("FK_message_meeting");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Messages)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_message_user");
            });

            modelBuilder.Entity<Typeattention>(entity =>
            {
                entity.ToTable("typeattention");

                entity.Property(e => e.Name).HasMaxLength(1000);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.Property(e => e.AccessKeyId)
                    .HasMaxLength(1000)
                    .HasColumnName("accessKeyId");

                entity.Property(e => e.CellPhone).HasMaxLength(1000);

                entity.Property(e => e.ChannelName)
                    .HasMaxLength(1000)
                    .HasColumnName("channelName");

                entity.Property(e => e.Created).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(1000);

                entity.Property(e => e.FirstName).HasMaxLength(1000);

                entity.Property(e => e.LastAccess).HasColumnType("datetime");

                entity.Property(e => e.LastName).HasMaxLength(1000);

                entity.Property(e => e.Modified).HasColumnType("datetime");

                entity.Property(e => e.NumberDoc).HasMaxLength(12);

                entity.Property(e => e.Password).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.Region)
                    .HasMaxLength(1000)
                    .HasColumnName("region");

                entity.Property(e => e.SecretAccessKey)
                    .HasMaxLength(1000)
                    .HasColumnName("secretAccessKey");

                entity.Property(e => e.Suministro).HasMaxLength(50);

                entity.Property(e => e.TypeDoc).HasMaxLength(18);

                entity.Property(e => e.UniqueId).HasMaxLength(36);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
