// Models/Charterer.cs
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Break_Bulk_System.Models
{
    public class Charterer
    {
        [Key]
        [DisplayName("Key Code")]
        [MaxLength(6)]
        public string KeyCode { get; set; } = string.Empty;

        [DisplayName("Description")]
        [MaxLength(50)]
        public string Description { get; set; } = string.Empty;

        [DisplayName("Long Description")]
        [MaxLength(100)]
        public string? LongDescription { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}