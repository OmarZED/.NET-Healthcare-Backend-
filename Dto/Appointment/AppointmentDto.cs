namespace medical.Dto.Appointment
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public DateTime AppointmentDateTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Status { get; set; } = string.Empty; // Use string representation of enum
        public string? ReasonForVisit { get; set; }
        public string? DoctorNotes { get; set; } // Patients might see this after completion?
        public string PatientId { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty; // Useful to display
        public string PatientName { get; set; } = string.Empty; // Useful to display
    }
}

