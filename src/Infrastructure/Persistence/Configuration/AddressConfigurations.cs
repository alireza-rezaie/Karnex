using Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Configuration
{
    public class AddressConfigurations : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.Property(x=>x.State);
            builder.Property(x => x.City);
            builder.Property(x => x.FirstName);
            builder.Property(x => x.PostalCode).HasMaxLength(10);
            builder.Property(x => x.Number).IsRequired().HasMaxLength(11);
            builder.HasOne(x=>x.User).WithMany().HasForeignKey(x=>x.UserId); 



        }

    }
}
