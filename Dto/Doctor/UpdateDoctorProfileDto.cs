namespace medical.Dto.Doctor
{
    public class UpdateDoctorProfileDto
    {
        // Fields the doctor can update about themselves
        public string? ClinicAddress { get; set; }
        public string? ProfessionalBio { get; set; }
        public int? YearsOfExperience { get; set; } // Nullable if optional update
        // Maybe available hours? (Could be complex)
        // Note: Specialization/License might require admin approval to change
    }
}
