using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace medical.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        public DateTime? ReadAt { get; set; } // When the recipient read the message

        // --- Relationships ---

        [Required]
        public string SenderId { get; set; } = string.Empty; // Foreign Key to ApplicationUser

        [ForeignKey("SenderId")]
        public virtual ApplicationUser? Sender { get; set; } // Navigation Property

        [Required]
        public string ReceiverId { get; set; } = string.Empty; // Foreign Key to ApplicationUser

        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser? Receiver { get; set; } // Navigation Property
    }
}

