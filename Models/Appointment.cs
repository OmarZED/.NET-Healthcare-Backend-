using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{

    public enum AppointmentStatus
    {
        Scheduled,
        Completed,
        CancelledByPatient,
        CancelledByDoctor,
        NoShow
    }
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime AppointmentDateTime { get; set; }

        [Required]
        public TimeSpan Duration { get; set; } // Or use int DurationMinutes

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

        public string? ReasonForVisit { get; set; } // Patient provides this

        public string? DoctorNotes { get; set; } // Doctor fills this after visit

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Relationships ---

        [Required]
        public string PatientId { get; set; } = string.Empty; // Foreign Key to ApplicationUser (Patient)

        [ForeignKey("PatientId")]
        public virtual ApplicationUser? Patient { get; set; } // Navigation Property

        [Required]
        public string DoctorId { get; set; } = string.Empty; // Foreign Key to ApplicationUser (Doctor)

        [ForeignKey("DoctorId")]
        public virtual ApplicationUser? Doctor { get; set; } // Navigation Property
    }
}
