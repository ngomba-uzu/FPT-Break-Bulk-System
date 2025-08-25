// Models/Manifest.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Break_Bulk_System.Models
{
    public class Manifest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DisplayName("Ship No")]
        public string VesselCode { get; set; } = string.Empty;

        [Required]
        [DisplayName("Bill No")]
        [MaxLength(26)]
        public string BillNo { get; set; } = string.Empty;

        [DisplayName("Mark")]
        [MaxLength(20)]
        public string? Mark { get; set; }

        [DisplayName("Pack(Kg)")]
        public decimal? PackKg { get; set; }

        [DisplayName("Mark 2")]
        [MaxLength(20)]
        public string? Mark2 { get; set; }

        [DisplayName("Mark 3")]
        [MaxLength(20)]
        public string? Mark3 { get; set; }

        [DisplayName("Ld Port")]
        [MaxLength(15)]
        public string? LdPort { get; set; }

        [DisplayName("Expected Qty")]
        public decimal? ExpectedQty { get; set; }

        [DisplayName("Description")]
        [MaxLength(20)]
        public string? Description { get; set; }

        [DisplayName("Net Weight (Kg)")]
        public decimal? NetWeightKg { get; set; }

        [DisplayName("Gross Unit (Kg)")]
        public decimal? GrossUnitKg { get; set; }

        [DisplayName("Location")]
        [MaxLength(20)]
        public string? Location { get; set; }

        [DisplayName("L-Order Comp.")]
        [MaxLength(1)]
        public string? LOrderComp { get; set; }

        [DisplayName("Cargo Type")]
        [MaxLength(3)]
        public string? CargoType { get; set; }

        [DisplayName("Commodity")]
        [MaxLength(6)]
        public string? Commodity { get; set; }

        [DisplayName("SubCommodity")]
        [MaxLength(6)]
        public string? SubCommodity { get; set; }

        [DisplayName("Customer")]
        [MaxLength(6)]
        public string? Customer { get; set; }

        [DisplayName("(I)mp(E)xp")]
        [MaxLength(1)]
        public string? ImpExp { get; set; }

        [DisplayName("Transhipment Y/N")]
        [MaxLength(1)]
        public string? Transhipment { get; set; }

        [DisplayName("Handling account")]
        [MaxLength(26)]
        public string? HandlingAccount { get; set; }

        [DisplayName("Storage account")]
        [MaxLength(26)]
        public string? StorageAccount { get; set; }

        [DisplayName("ExclW/End")]
        [MaxLength(1)]
        public string? ExclWEnd { get; set; }

        [DisplayName("ExPRBC")]
        [MaxLength(6)]
        public string? ExPRBC { get; set; }


        public virtual VesselMaster VesselMaster { get; set; } = null!;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}