using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpatialDemo.Models.Configuration
{
    class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.Property(c => c.Id).HasColumnName("CountryID");
            builder.Property(c => c.Name).HasColumnName("CountryName");
        }
    }
}
