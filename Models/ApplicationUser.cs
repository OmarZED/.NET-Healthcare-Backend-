using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty; // Initialize non-nullable strings

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties (optional but helpful)
        // A user might have ONE PatientProfile OR ONE DoctorProfile
        public virtual PatientProfile? PatientProfile { get; set; } // Nullable because a user isn't necessarily a patient
        public virtual DoctorProfile? DoctorProfile { get; set; }   // Nullable because a user isn't necessarily a doctor

        // A user can send many messages
        public virtual ICollection<Message> SentMessages { get; set; } = new List<Message>();

        // A user can receive many messages
        public virtual ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();

        // A patient has appointments
        public virtual ICollection<Appointment> PatientAppointments { get; set; } = new List<Appointment>();

        // A doctor has appointments
        public virtual ICollection<Appointment> DoctorAppointments { get; set; } = new List<Appointment>();
    }
}
