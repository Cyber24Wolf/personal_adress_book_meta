using Microsoft.EntityFrameworkCore;
using PersonalAdressBookMeta.Domain;

namespace PersonalAdressBookMeta.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Contact>(e =>
                          {
                              e.Property(p => p.FullName).HasMaxLength(200).IsRequired();
                              e.Property(p => p.Address).HasMaxLength(300).IsRequired();
                              e.Property(p => p.Phone).HasMaxLength(50).IsRequired();
                          });
    }
}