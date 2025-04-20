using System.ComponentModel.DataAnnotations;

namespace medical.Dto.Auth
{
    public class RegisterPatientDto
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
        public DateTime DateOfBirth { get; set; }

        // Add other MANDATORY patient fields needed at registration
        public string? Address { get; set; }
    }
}

