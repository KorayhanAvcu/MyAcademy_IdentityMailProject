using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Context
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // User -> Gönderdiği mesajlar
            builder.Entity<AppUser>()
                .HasMany(x => x.SentMessages)
                .WithOne(x => x.Sender)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> Aldığı mesajlar
            builder.Entity<AppUser>()
                .HasMany(x => x.ReceivedMessages)
                .WithOne(x => x.Receiver)
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category -> User (UserId nullable)
            builder.Entity<Category>()
                .HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Conversation -> Mesajlar
            builder.Entity<Conversation>()
                .HasMany(x => x.Messages)
                .WithOne(x => x.Conversation)
                .HasForeignKey(x => x.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);

            // MessageReport -> Raporlanan mesaj
            builder.Entity<MessageReport>()
                .HasOne(x => x.Message)
                .WithMany()
                .HasForeignKey(x => x.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // MessageReport -> Raporlayan kullanıcı
            builder.Entity<MessageReport>()
                .HasOne(x => x.Reporter)
                .WithMany()
                .HasForeignKey(x => x.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            // MessageReport -> İnceleyen admin
            builder.Entity<MessageReport>()
                .HasOne(x => x.ReviewedBy)
                .WithMany()
                .HasForeignKey(x => x.ReviewedById)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(builder);
        }

        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<MessageReport> MessageReports { get; set; }
    }
}