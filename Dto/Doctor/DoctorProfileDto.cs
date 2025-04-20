namespace medical.Dto.Doctor
{
    public class DoctorProfileDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ProfessionalBio { get; set; }
        public bool IsVerified { get; set; }
        // Add any other fields a doctor (or patient viewing doctor) should see
    }
}
