namespace medical.Dto.Patient
{
    public class UpdatePatientProfileDto
    {
        // Fields the patient can update about themselves
        public string? Address { get; set; }
        public string? MedicalHistorySummary { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        // maybe phone number?
    }
}

