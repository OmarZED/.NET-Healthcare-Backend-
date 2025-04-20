using System.ComponentModel.DataAnnotations;

namespace medical.Dto.Message
{
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty; // ID of the doctor to send to
        [Required]
        [StringLength(1000, MinimumLength = 1)] // Example validation
        public string Content { get; set; } = string.Empty;
    }
}


