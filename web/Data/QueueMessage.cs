using System.Text.Json.Serialization;

namespace web.Data;
    public class QueueMessage
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("subject")]
        public string? Subject { get; set; }

        [JsonPropertyName("data")]
        public Data? Data { get; set; }

        [JsonPropertyName("eventType")]
        public string? EventType { get; set; }

        [JsonPropertyName("dataVersion")]
        public string? DataVersion { get; set; }

        [JsonPropertyName("metadataVersion")]
        public string? MetadataVersion { get; set; }

        [JsonPropertyName("eventTime")]
        public DateTimeOffset EventTime { get; set; }



        public bool IsConnectedEvent => EventType == "Microsoft.SignalRService.ClientConnectionConnected";
        public bool IsDisConnectedEvent => EventType == "Microsoft.SignalRService.ClientConnectionDisconnected";
    }

    public class Data
    {
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonPropertyName("hubName")]
        public string? HubName { get; set; }

        [JsonPropertyName("connectionId")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("userId")]
        public string? UserId { get; set; }
    }