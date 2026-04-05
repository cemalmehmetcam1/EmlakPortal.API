using EmlakPortal.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EmlakPortal.API.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Estate> Estates { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // İlişkileri kurma
            builder.Entity<Estate>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Estates)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ÇÖZÜM BURADA: Fiyat alanı için hassasiyet ayarı (Toplam 18 basamak, 2'si virgülden sonra)
            builder.Entity<Estate>()
                .Property(e => e.Price)
                .HasColumnType("decimal(18,2)");
        }
    }
}