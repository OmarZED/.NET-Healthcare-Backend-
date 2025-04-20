using System.ComponentModel.DataAnnotations;

namespace medical.Dto.Appointment
{
    public class ScheduleAppointmentDto
    {
        [Required]
        public string DoctorId { get; set; } = string.Empty;
        [Required]
        public DateTime AppointmentDateTime { get; set; }
        [Required]
        public TimeSpan Duration { get; set; } // Or int minutes
        public string? ReasonForVisit { get; set; }
    }
}
