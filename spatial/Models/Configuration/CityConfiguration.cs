using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpatialDemo.Models.Configuration
{
    class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.Property(c => c.Id).HasColumnName("CityID");
            builder.Property(c => c.Name).HasColumnName("CityName");
            builder.HasOne(c => c.State).WithMany(s => s.Cities)
                .HasForeignKey("StateProvinceID");
        }
    }
}
