namespace ipk24_chat_client;

public static class ServerResponseTranslator
{
    public static string Translate(string serverResponse)
    {
        string[] parts = serverResponse.Split(' ');

        switch (parts[0])
        {
            case "REPLY":
                return TranslateReply(parts);
            case "MSG":
                Console.WriteLine(serverResponse);
                return TranslateMessage(parts);
            case "BYE":
                return TranslateBye();
            case "ERR":
                return TranslateError(parts);
            default:
                return serverResponse;
        }
    }

    private static string TranslateReply(string[] parts)
    {
        if (parts.Length >= 4)
        {
            string replyStatus = parts[1];
            string messageContent = string.Join(" ", parts, 3, parts.Length - 3);
            if (replyStatus == "NOK")
            {
                if (MessageData.AuthExecuted)
                {
                    MessageData.AuthExecuted = false;
                }

                Console.Error.WriteLine($"Failure: {messageContent}");
            }
            else if (replyStatus == "OK")
            {
                Console.WriteLine($"Success: {messageContent}");
            }

            return $"Server Reply: {replyStatus} {messageContent}";
        }
        else
        {
            return "ERR : Invalid REPLY format";
        }
    }

    private static string TranslateMessage(string[] parts)
    {
        if (parts.Length >= 5)
        {
            string serverName = parts[2];
            string messageContent = string.Join(" ", parts, 4, parts.Length - 4);
            return $"{serverName}: {messageContent}";
        }

        return "ERR: Invalid MSG format";
    }

    public static byte[] TranslateConfirm(ushort refMessageId)
    {
        byte[] messageBuffer = new byte[3];
        messageBuffer[0] = (byte) MessageType.CONFIRM;
        BitConverter.GetBytes(refMessageId).CopyTo(messageBuffer, 1);
        return messageBuffer;
    }

    private static string TranslateBye()
    {
        return "Server has closed the connection";
    }

    private static string TranslateError(string[] parts)
    {
        if (parts.Length >= 4)
        {
            string serverName = parts[2];
            string errorMessage = string.Join(" ", parts, 4, parts.Length - 4);
            return $"ERR FROM {serverName}: {errorMessage}";
        }

        return "ERR: Invalid ERR format";
    }
}