using FirebaseAdmin.Messaging;
using Newtonsoft.Json;

namespace tzer0mApi.Services.FCM
{
    /// <summary>
    /// Message
    /// </summary>
    /// <param name="token">FCM token</param>
    /// <param name="title">Notification title</param>
    /// <param name="body">Notification body</param>
    /// <param name="url">Optional link to open when the notification is tapped</param>
    public class Message(string token, string title, string body, string? url)
    {
        /// <summary>
        /// Notification
        /// </summary>
        [JsonProperty(PropertyName = "notification")]
        public Notification Notification { get; set; } = new Notification(title, body);

        /// <summary>
        /// Data payload read by TingMobile, holding the link to open on tap
        /// </summary>
        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string>? Data { get; set; } = url == null ? null : new Dictionary<string, string> { ["url"] = url };

        /// <summary>
        /// FCM token
        /// </summary>
        [JsonProperty(PropertyName = "token")]
        public string Token { get; set; } = token;

        /// <summary>
        /// Android-specific delivery options
        /// </summary>
        [JsonProperty(PropertyName = "android")]
        public AndroidConfig Android { get; set; } = new AndroidConfig { Priority = Priority.High };
    }
}