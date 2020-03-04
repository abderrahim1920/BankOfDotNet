using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BankOfDotNet.API.Models
{
    public class BankContext : DbContext
    {
        public BankContext(DbContextOptions<BankContext> options) : base(options)
        {
                
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Customer>(entity =>
        //    {
        //        entity.Property<int>("Id");
        //        entity.HasKey("Id");
        //        entity.Property(e => e.FirstName);
        //        entity.Property(e => e.LastName);
        //    });
        //}

        public DbSet<Customer> Customers { get; set; }
    }
}
