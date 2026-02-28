namespace ApplicationAuth.SharedModels.ResponseModels.Telegram
{
    public record TelegramStickerResponseModel
    {
        public int Id { get; set; }
        public string StickerId { get; set; }
        public string FileUniqueId { get; set; }
        public int CountOfUsage { get; set; }
        public string DateOfFirstUsage { get; set; }
    }
}
