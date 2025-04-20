namespace medical.Dto.Message
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty; // Helpful for display
        public string ReceiverId { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty; // Helpful for display
    }
}
