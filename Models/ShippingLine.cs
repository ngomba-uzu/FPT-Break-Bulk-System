// Models/ShippingLine.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Break_Bulk_System.Models
{
    public class ShippingLine
    {
        [Key]
        [DisplayName("Code")]
        [MaxLength(6)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [DisplayName("Name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}