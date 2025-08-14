using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Break_Bulk_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [DisplayName("Last Name")]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string Role { get; set; } = string.Empty;

        public DateTime CreatedOnDateTime { get; set; }

        [DisplayName("Full Name")]
        public string FullName { get; set; } = string.Empty;

    }
}
