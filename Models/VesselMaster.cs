// Models/VesselMaster.cs (update to use proper foreign keys)
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Break_Bulk_System.Models
{
    public class VesselMaster
    {
        [Key]
        [DisplayName("Vessel No")]
        [MaxLength(10)]
        public string VesselCode { get; set; } = string.Empty;

        [Required]
        [DisplayName("Vessel Name")]
        [MaxLength(26)]
        public string VesselName { get; set; } = string.Empty;

        [DisplayName("Loading Berth")]
        [MaxLength(2)]
        public string? LoadingBerth { get; set; }

        [DisplayName("Loading Date")]
        [DataType(DataType.Date)]
        public DateTime? LoadingDate { get; set; }

        [DisplayName("Loading Time")]
        [DataType(DataType.Time)]
        public TimeSpan? LoadingTime { get; set; }

        [DisplayName("Vessel Type")]
        [MaxLength(2)]
        public string? VesselTypeCode { get; set; }

        // Navigation property for VesselType
        public virtual VesselType? VesselType { get; set; }

        [DisplayName("IMP/EXP")]
        [MaxLength(3)]
        public string? ImpExp { get; set; }

        [DisplayName("Arrival Date")]
        [DataType(DataType.Date)]
        public DateTime? ArrivalDate { get; set; }

        [DisplayName("Arrival Time")]
        [DataType(DataType.Time)]
        public TimeSpan? ArrivalTime { get; set; }

        [DisplayName("Sail Date")]
        [DataType(DataType.Date)]
        public DateTime? SailDate { get; set; }

        [DisplayName("Sail Time")]
        [DataType(DataType.Time)]
        public TimeSpan? SailTime { get; set; }

        [DisplayName("Bill No")]
        [MaxLength(26)]
        public string? BillNo { get; set; }

        [DisplayName("Stock Completed")]
        [MaxLength(1)]
        public string? StockCompleted { get; set; }

        [DisplayName("Voyage Number")]
        [MaxLength(12)]
        public string? VoyageNumber { get; set; }

        [DisplayName("Call Sign")]
        [MaxLength(8)]
        public string? CallSign { get; set; }

        [DisplayName("Charterer")]
        [MaxLength(6)]
        public string? Charterer { get; set; }

        [DisplayName("VPM")]
        [MaxLength(8)]
        public string? VPM { get; set; }

        [DisplayName("Shipping Line")]
        [MaxLength(6)]
        public string? ShippingLineCode { get; set; }

        // Navigation property for ShippingLine
        public virtual ShippingLine? ShippingLine { get; set; }

        [DisplayName("Storage Date")]
        [DataType(DataType.Date)]
        public DateTime? StorageDate { get; set; }

        // Navigation property for Manifests
        public virtual ICollection<Manifest> Manifests { get; set; } = new List<Manifest>();

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}