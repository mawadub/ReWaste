using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RE_Waste.Models
{
    // The logged-in user. Inherits everything Identity provides (email, password hash, lockout) and adds our own fields.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [PersonalData]
        public string Name { get; set; } = string.Empty;

        // Navigation property
        public ICollection<SavedResponse> SavedResponses { get; set; } = new List<SavedResponse>();
    }
}