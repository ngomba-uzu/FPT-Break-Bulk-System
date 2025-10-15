// Models/TransportSea.cs
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Break_Bulk_System.Models
{
    public class TransportSea
    {
        [Key]
        [DisplayName("Transport ID")]
        [MaxLength(10)]
        public string TransportID { get; set; } = string.Empty;

        [DisplayName("Name")]
        [MaxLength(100)]
        public string? Name { get; set; }

        [DisplayName("Carrier Code")]
        [MaxLength(10)]
        public string? CarrierCode { get; set; }

        [DisplayName("Carrier Name")]
        [MaxLength(100)]
        public string? CarrierName { get; set; }
    }
}