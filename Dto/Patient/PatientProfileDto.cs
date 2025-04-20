namespace medical.Dto.Patient
{
    public class PatientProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? MedicalHistorySummary { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        // Add any other fields a patient (or doctor viewing patient) should see
    }
}
