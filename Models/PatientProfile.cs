using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class PatientProfile
    {
        [Key] // Primary Key for this profile table
        public int Id { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        public string? Address { get; set; } // Example: Street, City, PostalCode, Country

        public string? MedicalHistorySummary { get; set; } // Could be more detailed later

        public string? Allergies { get; set; } // Consider a separate related table if complex

        public string? CurrentMedications { get; set; } // Consider a separate related table if complex

        // --- Relationship to ApplicationUser ---

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty; // Foreign Key

        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; } // Navigation property back to the user
    }
}
