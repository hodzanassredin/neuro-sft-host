namespace LlmCommon.Dtos
{
    public class MessageDto
    {
        public string Text { get; set; }
        public Ids.Id Id { get; set; }
        public User User { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
