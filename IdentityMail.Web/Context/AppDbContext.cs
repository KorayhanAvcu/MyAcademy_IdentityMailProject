using IdentityMail.Web.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityMail.Web.Context
{
    public class AppDbContext : IdentityDbContext<AppUser,AppRole,int>
    {
        public AppDbContext(DbContextOptions options): base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<AppUser>().HasMany(message => message.SentMessages)
                                     .WithOne(s=> s.Sender)
                                     .HasForeignKey(x=>x.SenderId)
                                     .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<AppUser>().HasMany(message => message.ReceivedMessages)
                                     .WithOne(s => s.Receiver)
                                     .HasForeignKey(x => x.ReceiverId)
                                     .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Category>()
                                    .HasOne(c => c.User)
                                    .WithMany()
                                    .HasForeignKey(c => c.UserId)
                                    .IsRequired(false)
                                    .OnDelete(DeleteBehavior.SetNull);
            base.OnModelCreating(builder);

        }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
