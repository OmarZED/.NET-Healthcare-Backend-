namespace medical.Dto.Doctor
{
    public class DoctorSummaryDto
    {
        public string DoctorUserId { get; set; } = string.Empty; // Renamed for clarity
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        // Add other summary fields if needed (e.g., Clinic City?)
    }
}
