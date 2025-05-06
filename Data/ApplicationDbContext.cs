using Microsoft.EntityFrameworkCore;
using TheQuel.Core;

namespace TheQuel.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<AnnouncementRecipient> AnnouncementRecipients { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure User entity
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
                
            // Configure Property entity
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Owner)
                .WithOne(u => u.Property)
                .HasForeignKey<Property>(p => p.OwnerId);
                
            // Configure Payment entity
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Property)
                .WithMany(prop => prop.Payments)
                .HasForeignKey(p => p.PropertyId);
                
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId);
                
            // Configure Complaint entity
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId);
                
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.AssignedTo)
                .WithMany()
                .HasForeignKey(c => c.AssignedToId);
                
            // Configure Event entity
            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById);
                
            // Configure EventAttendee entity
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.Event)
                .WithMany(e => e.Attendees)
                .HasForeignKey(ea => ea.EventId);
                
            modelBuilder.Entity<EventAttendee>()
                .HasOne(ea => ea.User)
                .WithMany()
                .HasForeignKey(ea => ea.UserId);
                
            // Configure UserPermission entity
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.Permissions)
                .HasForeignKey(up => up.UserId);
                
            modelBuilder.Entity<UserPermission>()
                .HasIndex(up => new { up.UserId, up.Permission })
                .IsUnique();
                
            // Configure Announcement entity
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById);
                
            // Configure AnnouncementRecipient entity
            modelBuilder.Entity<AnnouncementRecipient>()
                .HasOne(ar => ar.Announcement)
                .WithMany(a => a.Recipients)
                .HasForeignKey(ar => ar.AnnouncementId);
                
            modelBuilder.Entity<AnnouncementRecipient>()
                .HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId);
        }
    }
} 