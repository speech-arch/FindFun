using FindFun.Server.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindFun.Server.Infrastructure.Configurations;

public class ParkAmenityConfiguration : IEntityTypeConfiguration<ParkAmenity>
{
    public void Configure(EntityTypeBuilder<ParkAmenity> builder)
    {
        builder.ToTable("park_amenities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        builder.Property(x => x.ParkId).HasColumnName("park_id");
        builder.Property(x => x.AmenityId).HasColumnName("amenity_id");
        builder.HasOne(x => x.Park)
            .WithMany(x => x.Amenities)
            .HasForeignKey(x => x.ParkId)
            .IsRequired();

        builder.HasOne(x => x.Amenity)
            .WithMany(x => x.ParkAmenities)
            .HasForeignKey(x => x.AmenityId)
            .IsRequired();

    }
}
