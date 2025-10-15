// Data/ApplicationDbContext.cs
using Break_Bulk_System.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Break_Bulk_System.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<TransportSea> TransportSeas { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<VesselMaster> VesselMasters { get; set; }
        public DbSet<Manifest> Manifests { get; set; }
        public DbSet<VesselType> VesselTypes { get; set; }
        public DbSet<ShippingLine> ShippingLines { get; set; }

        public DbSet<Charterer> Charterers { get; set; }
        // Data/ApplicationDbContext.cs (update the ShippingLine relationship)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransportSea>(entity =>
            {
                entity.HasKey(ts => ts.TransportID);
                entity.Property(ts => ts.TransportID).HasMaxLength(10);
                entity.Property(ts => ts.Name).HasMaxLength(100);
                entity.Property(ts => ts.CarrierCode).HasMaxLength(10);
                entity.Property(ts => ts.CarrierName).HasMaxLength(100);
            });


            modelBuilder.Entity<Charterer>(entity =>
            {
                entity.HasKey(c => c.KeyCode);
                entity.Property(c => c.KeyCode).HasMaxLength(6);
                entity.Property(c => c.Description).HasMaxLength(50);
                entity.Property(c => c.LongDescription).HasMaxLength(100);
            });


            // Configure VesselType
            modelBuilder.Entity<VesselType>(entity =>
            {
                entity.HasKey(vt => vt.Code);
                entity.Property(vt => vt.Code).HasMaxLength(2);
                entity.Property(vt => vt.Description).HasMaxLength(50);
            });

            // Configure ShippingLine
            modelBuilder.Entity<ShippingLine>(entity =>
            {
                entity.HasKey(sl => sl.Code);
                entity.Property(sl => sl.Code).HasMaxLength(6);
                entity.Property(sl => sl.Name).HasMaxLength(100);
            });

            // Configure VesselMaster
            modelBuilder.Entity<VesselMaster>(entity =>
            {
                entity.HasKey(vm => vm.VesselCode);
                entity.Property(vm => vm.VesselCode).HasMaxLength(10);
                entity.Property(vm => vm.VesselName).HasMaxLength(26);
                entity.Property(vm => vm.LoadingBerth).HasMaxLength(2);
                entity.Property(vm => vm.VesselTypeCode).HasMaxLength(2);
                entity.Property(vm => vm.ImpExp).HasMaxLength(3);
                entity.Property(vm => vm.BillNo).HasMaxLength(26);
                entity.Property(vm => vm.StockCompleted).HasMaxLength(1);
                entity.Property(vm => vm.VoyageNumber).HasMaxLength(12);
                entity.Property(vm => vm.CallSign).HasMaxLength(8);
                entity.Property(vm => vm.Charterer).HasMaxLength(6);
                entity.Property(vm => vm.VPM).HasMaxLength(8);
                entity.Property(vm => vm.ShippingLineCode).HasMaxLength(6);

                // Configure relationships
                entity.HasOne(vm => vm.VesselType)
                    .WithMany()
                    .HasForeignKey(vm => vm.VesselTypeCode)
                    .OnDelete(DeleteBehavior.Restrict);

                // CHANGE THIS LINE: Set DeleteBehavior.SetNull for ShippingLine
                entity.HasOne(vm => vm.ShippingLine)
                    .WithMany()
                    .HasForeignKey(vm => vm.ShippingLineCode)
                    .OnDelete(DeleteBehavior.SetNull); // Changed from Restrict to SetNull
            });

            // Configure Manifest
            modelBuilder.Entity<Manifest>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.VesselCode).HasMaxLength(10);
                entity.Property(m => m.BillNo).HasMaxLength(26);
                entity.Property(m => m.Mark).HasMaxLength(20);
                entity.Property(m => m.Mark2).HasMaxLength(20);
                entity.Property(m => m.Mark3).HasMaxLength(20);
                entity.Property(m => m.LdPort).HasMaxLength(15);
                entity.Property(m => m.Description).HasMaxLength(20);
                entity.Property(m => m.Location).HasMaxLength(20);
                entity.Property(m => m.LOrderComp).HasMaxLength(1);
                entity.Property(m => m.CargoType).HasMaxLength(3);
                entity.Property(m => m.Commodity).HasMaxLength(6);
                entity.Property(m => m.SubCommodity).HasMaxLength(6);
                entity.Property(m => m.Customer).HasMaxLength(6);
                entity.Property(m => m.ImpExp).HasMaxLength(1);
                entity.Property(m => m.Transhipment).HasMaxLength(1);
                entity.Property(m => m.HandlingAccount).HasMaxLength(26);
                entity.Property(m => m.StorageAccount).HasMaxLength(26);
                entity.Property(m => m.ExclWEnd).HasMaxLength(1);
                entity.Property(m => m.ExPRBC).HasMaxLength(6);

                // Configure relationship with VesselMaster
                entity.HasOne(m => m.VesselMaster)
                    .WithMany(vm => vm.Manifests)
                    .HasForeignKey(m => m.VesselCode)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Remove the seed data for ShippingLines since we're uploading via CSV
            // modelBuilder.Entity<ShippingLine>().HasData(
            //     new ShippingLine { Code = "MAEU", Name = "Maersk Line" },
            //     new ShippingLine { Code = "MSC", Name = "Mediterranean Shipping Company" },
            //     new ShippingLine { Code = "CMDU", Name = "CMA CGM" },
            //     new ShippingLine { Code = "COSU", Name = "COSCO Shipping" }
            // );

            // Keep VesselType seed data
            modelBuilder.Entity<VesselType>().HasData(
                new VesselType { Code = "01", Description = "Container Ship" },
                new VesselType { Code = "02", Description = "Bulk Carrier" },
                new VesselType { Code = "03", Description = "Tanker" },
                new VesselType { Code = "04", Description = "General Cargo" }
            );
        }


    }
}