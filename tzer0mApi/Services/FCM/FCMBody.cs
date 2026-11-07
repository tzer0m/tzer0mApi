using Newtonsoft.Json;

namespace tzer0mApi.Services.FCM
{
    /// <summary>
    /// Request body
    /// </summary>
    /// <param name="token">FCM token</param>
    /// <param name="title">Message title</param>
    /// <param name="body">Message body</param>
    /// <param name="url">Optional link to open when the notification is tapped</param>
    public class FCMBody(string token, string title, string body, string? url)
    {
        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public Message Message { get; set; } = new Message(token, title, body, url);
    }
}