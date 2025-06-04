using System.Text.Json;

namespace WebhookReceiver.Models
{
    public class SamsaraEvent
    {
        public string? EventType { get; set; }
        public string? DriverName { get; set; }
        public DateTime? EventTime { get; set; }
        public JsonElement Raw { get; set; }
    }
}
