// Models/ShippingLine.cs
using System.ComponentModel.DataAnnotations;

namespace Break_Bulk_System.Models
{
    public class ShippingLine
    {
        [Key]
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}