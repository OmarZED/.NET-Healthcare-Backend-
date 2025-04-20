using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class DoctorProfile
    {
        [Key] // Primary Key for this profile table
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Specialization { get; set; } = string.Empty; // e.g., Cardiology, Pediatrics

        [Required]
        [StringLength(100)]
        public string LicenseNumber { get; set; } = string.Empty; // Important for verification

        public int YearsOfExperience { get; set; }

        public string? ClinicAddress { get; set; }

        public string? ProfessionalBio { get; set; }

        public bool IsVerified { get; set; } = false; // Admin might verify doctors

        // --- Relationship to ApplicationUser ---

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty; // Foreign Key

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; } // Navigation property back to the user
    }
}