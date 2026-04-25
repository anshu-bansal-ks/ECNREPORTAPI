using Microsoft.EntityFrameworkCore;

namespace ECNREPORTAPI.Models
{
    public class DashboardContext : DbContext
    {
        public DashboardContext(DbContextOptions<DashboardContext> options) : base(options) { }

        public DbSet<DataSource> DataSources { get; set; } = null!;
        public DbSet<TblLoc> TblLocs { get; set; } = null!;
        public DbSet<Show> Shows { get; set; } = null!;
        public DbSet<Promo> Promos { get; set; } = null!;
        public DbSet<LnkItemCategory> LnkItemCategories { get; set; } = null!;
        public DbSet<PricePage> PricePages { get; set; } = null!;
        public DbSet<ShipToSource> ShipToSources { get; set; } = null!;
        public DbSet<UserUnifiedPortal> UserUnifiedPortals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DataSource>(entity =>
            {
                entity.ToTable("datasources", "dbo");
                entity.HasKey(e => e.code);
                entity.Property(e => e.code).HasColumnName("code");
                entity.Property(e => e.dsource).HasColumnName("dsource");
                entity.Property(e => e.dbName).HasColumnName("dbName");
                entity.Property(e => e.dbUser).HasColumnName("dbUser");
                entity.Property(e => e.dbPassword).HasColumnName("dbPassword");
            });

            modelBuilder.Entity<TblLoc>().ToTable("tbl_loc", "dbo");
            modelBuilder.Entity<Show>().ToTable("SHOW", "dbo");
            modelBuilder.Entity<Promo>().ToTable("PROMO", "dbo");
            modelBuilder.Entity<LnkItemCategory>().ToTable("lnk_itemcategory", "dbo").HasNoKey();
            modelBuilder.Entity<PricePage>().ToTable("PRICE_PAGE", "dbo");
            modelBuilder.Entity<ShipToSource>().ToTable("SHIP_TO_SOURCE", "dbo");
            modelBuilder.Entity<UserUnifiedPortal>().ToTable("users_unifiedportal", "dbo");

            modelBuilder.Entity<UserUnifiedPortal>().HasKey(u => u.UserId);
            modelBuilder.Entity<TblLoc>().HasKey(t => t.Id);
            modelBuilder.Entity<Show>().HasKey(s => s.ShowId);
            modelBuilder.Entity<Promo>().HasKey(p => p.PromoId);
            modelBuilder.Entity<PricePage>().HasKey(p => p.price_page_uid);
            modelBuilder.Entity<ShipToSource>().HasKey(s => s.ship_to_id);
        }
    }
}

