namespace ipk24_chat_client;

public class MessageData
{
    public static bool AuthExecuted { get; set; } // flag for checking if /auth was executed
    public string MessageType { get; set; } //udp
    public string MessageId { get; set; }
    public string Username { get; set; }
    public string Secret { get; set; }
    public string DisplayName { get; set; }
    public string ChannelId { get; set; } //udp
    public string MessageContent { get; set; } //udp
    public string ErrorMessage { get; set; }
}
