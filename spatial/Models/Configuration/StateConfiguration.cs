using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpatialDemo.Models.Configuration
{
    class StateConfiguration : IEntityTypeConfiguration<State>
    {
        public void Configure(EntityTypeBuilder<State> builder)
        {
            builder.ToTable("StateProvinces");

            builder.Property(s => s.Id).HasColumnName("StateProvinceID");
            builder.Property(s => s.Name).HasColumnName("StateProvinceName");
            builder.HasOne(s => s.Country).WithMany(c => c.States).HasForeignKey("CountryID");
        }
    }
}
