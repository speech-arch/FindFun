using FindFun.Server.Domain;
using Microsoft.EntityFrameworkCore;

namespace FindFun.Server.Infrastructure;

public class FindFunDbContext(DbContextOptions<FindFunDbContext> options) : DbContext(options)
{
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Street> Streets => Set<Street>();
    public DbSet<Park> Parks => Set<Park>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<ParkAmenity> ParkAmenities => Set<ParkAmenity>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<ClosingSchedule> ClosingSchedules => Set<ClosingSchedule>();
    public DbSet<ParkImage> ParkImages => Set<ParkImage>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FindFunDbContext).Assembly);
    }
}
