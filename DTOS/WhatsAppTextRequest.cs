namespace MetaCloudApi_Whatsapp.DTOS

{ 
    public class WhatsAppTextRequest
    {
        public string messaging_product { get; set; } = "whatsapp";
        public string recipient_type { get; set; } = "individual";
        public string to { get; set; } = string.Empty;
        public string type { get; set; } = "text";
        public WhatsAppText text { get; set; } = new();
    }

    public class WhatsAppText
    {
        public string body { get; set; } = string.Empty;
    }
}
