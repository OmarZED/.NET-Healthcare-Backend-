using System.ComponentModel.DataAnnotations;

namespace medical.Dto.Auth
{
    public class RegisterDoctorDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string Specialization { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LicenseNumber { get; set; } = string.Empty;

        // Add other MANDATORY doctor fields needed at registration
        public int YearsOfExperience { get; set; }
    }
}
