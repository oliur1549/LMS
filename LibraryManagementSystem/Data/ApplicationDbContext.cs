using LibraryManagementSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Library> Libraries { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<BorrowRecord> BorrowRecords { get; set; }
        public DbSet<MembershipType> MembershipTypes { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Fine> Fines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Library configuration
            modelBuilder.Entity<Library>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Book configuration
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Genre).HasMaxLength(100);
                entity.HasIndex(e => e.ISBN).IsUnique();
                entity.HasQueryFilter(e => !e.IsDeleted);

                // One-to-Many: Library -> Books
                entity.HasOne(b => b.Library)
                      .WithMany(l => l.Books)
                      .HasForeignKey(b => b.LibraryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // MembershipType configuration
            modelBuilder.Entity<MembershipType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            // Member configuration
            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(m => m.MembershipType)
                      .WithMany(mt => mt.Members)
                      .HasForeignKey(m => m.MembershipTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Fine configuration
            modelBuilder.Entity<Fine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DailyRate).HasColumnType("decimal(18,2)");
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(f => f.BorrowRecord)
                      .WithOne(br => br.Fine)
                      .HasForeignKey<Fine>(f => f.BorrowRecordId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Member)
                      .WithMany(m => m.Fines)
                      .HasForeignKey(f => f.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Reservation configuration
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasQueryFilter(e => !e.IsDeleted);

                entity.HasOne(r => r.Book)
                      .WithMany(b => b.Reservations)
                      .HasForeignKey(r => r.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.Member)
                      .WithMany(m => m.Reservations)
                      .HasForeignKey(r => r.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BorrowRecord configuration
            modelBuilder.Entity<BorrowRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>();
                entity.HasQueryFilter(e => !e.IsDeleted);

                // Many-to-One: BorrowRecord -> Book
                entity.HasOne(br => br.Book)
                      .WithMany(b => b.BorrowRecords)
                      .HasForeignKey(br => br.BookId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Many-to-One: BorrowRecord -> Member
                entity.HasOne(br => br.Member)
                      .WithMany(m => m.BorrowRecords)
                      .HasForeignKey(br => br.MemberId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
