namespace medical.Dto.Auth
{
    // Response after successful login or registration
    public class AuthResponseDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty; // The JWT
        public DateTime ExpiresAt { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public string FirstName { get; set; } = string.Empty; // Useful for frontend display
        public string LastName { get; set; } = string.Empty;
    }

}
