namespace Imperium.Service.DTOs.Telegram
{
    public class TelegramBotInfo
    {
        public long Id { get; set; }
        public bool IsBot { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? Username { get; set; }
        public bool CanJoinGroups { get; set; }
        public bool CanReadAllGroupMessages { get; set; }
        public bool SupportsInlineQueries { get; set; }
    }
}
