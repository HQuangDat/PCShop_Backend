using Microsoft.EntityFrameworkCore;
using PCShop_Backend.Models;

namespace PCShop_Backend.Data
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public virtual DbSet<CartItem> CartItems { get; set; }

        public virtual DbSet<Component> Components { get; set; }

        public virtual DbSet<ComponentCategory> ComponentCategories { get; set; }

        public virtual DbSet<ComponentSpec> ComponentSpecs { get; set; }

        public virtual DbSet<Pcbuild> Pcbuilds { get; set; }

        public virtual DbSet<PcbuildComponent> PcbuildComponents { get; set; }

        public virtual DbSet<Receipt> Receipts { get; set; }

        public virtual DbSet<ReceiptItem> ReceiptItems { get; set; }

        public virtual DbSet<Role> Roles { get; set; }

        public virtual DbSet<Ticket> Tickets { get; set; }

        public virtual DbSet<TicketComment> TicketComments { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<PasswordReset> PasswordResets { get; set; } = default!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Server=HQD;Database=PCShopDB;Trusted_Connection=true;TrustServerCertificate=true");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.CartItemId).HasName("PK__CartItem__488B0B0A729581FD");

                entity.Property(e => e.AddedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Build).WithMany(p => p.CartItems)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_CartItems_Builds");

                entity.HasOne(d => d.Component).WithMany(p => p.CartItems)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_CartItems_Components");

                entity.HasOne(d => d.User).WithMany(p => p.CartItems).HasConstraintName("FK_CartItems_Users");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.HasKey(e => e.ComponentId).HasName("PK__Componen__D79CF04EE1652162");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Category).WithMany(p => p.Components)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Components_Categories");
            });

            modelBuilder.Entity<ComponentCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId).HasName("PK__Componen__19093A0B5E829719");
            });

            modelBuilder.Entity<ComponentSpec>(entity =>
            {
                entity.HasKey(e => e.SpecId).HasName("PK__Componen__883D567B767B7E6C");

                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

                entity.HasOne(d => d.Component).WithMany(p => p.ComponentSpecs).HasConstraintName("FK_ComponentSpecs_Components");
            });

            modelBuilder.Entity<Pcbuild>(entity =>
            {
                entity.HasKey(e => e.BuildId).HasName("PK__PCBuilds__C51051415153D322");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsPublic).HasDefaultValue(false);
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Pcbuilds)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_PCBuilds_Users");
            });

            modelBuilder.Entity<PcbuildComponent>(entity =>
            {
                entity.HasKey(e => e.BuildComponentId).HasName("PK__PCBuildC__3A8113A4C1A0682C");

                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Build).WithMany(p => p.PcbuildComponents).HasConstraintName("FK_PCBuildComponents_Builds");

                entity.HasOne(d => d.Component).WithMany(p => p.PcbuildComponents)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PCBuildComponents_Components");
            });

            modelBuilder.Entity<Receipt>(entity =>
            {
                entity.HasKey(e => e.ReceiptId).HasName("PK__Receipts__CC08C4204027D566");

                entity.Property(e => e.Country).HasDefaultValue("Vietnam");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Status).HasDefaultValue("Pending");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User).WithMany(p => p.Receipts)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Receipts_Users");
            });

            modelBuilder.Entity<ReceiptItem>(entity =>
            {
                entity.HasKey(e => e.ReceiptItemId).HasName("PK__ReceiptI__AF7BE10DA8833DF8");

                entity.Property(e => e.Quantity).HasDefaultValue(1);

                entity.HasOne(d => d.Build).WithMany(p => p.ReceiptItems)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ReceiptItems_Builds");

                entity.HasOne(d => d.Component).WithMany(p => p.ReceiptItems)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_ReceiptItems_Components");

                entity.HasOne(d => d.Receipt).WithMany(p => p.ReceiptItems).HasConstraintName("FK_ReceiptItems_Receipts");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AEB74034C");
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC607C4179BC6");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Priority).HasDefaultValue("Medium");
                entity.Property(e => e.Status).HasDefaultValue("New");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.AssignedToUser).WithMany(p => p.TicketAssignedToUsers)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Tickets_AssignedTo");

                entity.HasOne(d => d.User).WithMany(p => p.TicketUsers)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tickets_Users");
            });

            modelBuilder.Entity<TicketComment>(entity =>
            {
                entity.HasKey(e => e.CommentId).HasName("PK__TicketCo__C3B4DFCAE8A4ADD1");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Ticket).WithMany(p => p.TicketComments).HasConstraintName("FK_TicketComments_Tickets");

                entity.HasOne(d => d.User).WithMany(p => p.TicketComments)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TicketComments_Users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C09C75399");

                entity.Property(e => e.Country).HasDefaultValue("Vietnam");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LoyaltyPoints).HasDefaultValue(0);

                entity.HasOne(d => d.Role).WithMany(p => p.Users)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Roles");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
