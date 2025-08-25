// Models/VesselType.cs
using System.ComponentModel.DataAnnotations;

namespace Break_Bulk_System.Models
{
    public class VesselType
    {
        [Key]
        [MaxLength(2)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Description { get; set; } = string.Empty;
    }
}
