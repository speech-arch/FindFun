using FindFun.Server.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FindFun.Server.Infrastructure.Configurations;

public class ParkConfiguration : IEntityTypeConfiguration<Park>
{
    public void Configure(EntityTypeBuilder<Park> builder)
    {
        builder.ToTable("parks");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name");
        builder.Property(x => x.Description)
            .HasColumnName("description");
        builder.Property(x => x.AddressId)
            .HasColumnName("address_id");

        builder.Property(x => x.EntranceFee)
            .HasColumnName("entrance_fee")
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        builder.Property(x => x.IsFree)
            .HasColumnName("is_free")
            .HasDefaultValue(true);

        builder.Property(x => x.Organizer)
            .HasColumnName("organizer")
            .HasMaxLength(255);

        builder.Property(x => x.AgeRecommendation)
            .HasColumnName("age_recommendation")
            .HasMaxLength(100).HasDefaultValue("all");

        builder.Property(x => x.ParkType)
            .HasColumnName("park_type")
            .HasMaxLength(100);

        builder.HasIndex(x => x.AddressId);

        builder.HasOne(x => x.Address)
            .WithMany()
            .HasForeignKey(x => x.AddressId)
            .IsRequired();

        builder.HasOne(x => x.ClosingSchedule)
            .WithOne(x => x.Park)
            .HasForeignKey<ClosingSchedule>(x => x.ParkId)
            .OnDelete(DeleteBehavior.ClientCascade);

        builder.HasMany(x => x.Amenities)
            .WithOne(x => x.Park)
            .HasForeignKey(x => x.ParkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
