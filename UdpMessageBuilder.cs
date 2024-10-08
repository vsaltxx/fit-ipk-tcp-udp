using System.Text;

namespace ipk24_chat_client;

public static class UdpMessageBuilder
{
    private static UInt16 _refId = 0;
    
    private static UInt16 NextId()
    {
        return _refId++;
    }

    public static byte[] BuildAuth(string username, string displayName, string secret)
    {
        byte[] messageBuffer = new byte[1 + 2 + username.Length + 1 + displayName.Length + 1 + secret.Length + 1];
        byte[] bUsername = Encoding.ASCII.GetBytes(username);
        byte[] bDisplayName = Encoding.ASCII.GetBytes(displayName);
        byte[] bSecret = Encoding.ASCII.GetBytes(secret);
        messageBuffer[0] = (byte)MessageType.AUTH;
        BitConverter.GetBytes(NextId()).CopyTo(messageBuffer, 1);
        bUsername.CopyTo(messageBuffer, 3);
        messageBuffer[3 + bUsername.Length] = 0x00;
        bDisplayName.CopyTo(messageBuffer, 4 + bUsername.Length);
        messageBuffer[4 + bUsername.Length + bDisplayName.Length] = 0x00;
        bSecret.CopyTo(messageBuffer, 5 + bUsername.Length + bDisplayName.Length);
        messageBuffer[5 + bUsername.Length + bDisplayName.Length + bSecret.Length] = 0x00;
        return messageBuffer;
    }
    
    public static byte[] BuildMsg(string displayName, string messageContent)
    {
        byte[] messageBuffer = new byte[1 + 2 + displayName.Length + 1 + messageContent.Length + 1];
        byte[] bDisplayName = Encoding.ASCII.GetBytes(displayName);
        byte[] bMessageContent = Encoding.ASCII.GetBytes(messageContent);
        messageBuffer[0] = (byte)MessageType.MSG;
        BitConverter.GetBytes(NextId()).CopyTo(messageBuffer, 1);
        bDisplayName.CopyTo(messageBuffer, 3);
        messageBuffer[3 + bDisplayName.Length] = 0x00;
        bMessageContent.CopyTo(messageBuffer, 4 + bDisplayName.Length);
        messageBuffer[4 + bDisplayName.Length + bMessageContent.Length] = 0x00;
        return messageBuffer;
    }
    
    public static byte[] BuildJoin(string displayName, string channel)
    {
        byte[] messageBuffer = new byte[1 + 2 + displayName.Length + 1 + channel.Length + 1];
        byte[] bDisplayName = Encoding.ASCII.GetBytes(displayName);
        byte[] bChannel = Encoding.ASCII.GetBytes(channel);
        messageBuffer[0] = (byte)MessageType.JOIN;
        BitConverter.GetBytes(NextId()).CopyTo(messageBuffer, 1);
        bChannel.CopyTo(messageBuffer, 3);
        messageBuffer[3 + bChannel.Length] = 0x00;
        bDisplayName.CopyTo(messageBuffer, 4 + bChannel.Length);
        messageBuffer[4 + bChannel.Length + bDisplayName.Length] = 0x00;
        return messageBuffer;
    }
    
    public static byte[] BuildError(string displayName,string errorMessage)
    {
        byte[] messageBuffer = new byte[1 + 2 + displayName.Length + 1 + errorMessage.Length + 1];
        byte[] bDisplayName = Encoding.ASCII.GetBytes(displayName);
        byte[] bErrorMessage = Encoding.ASCII.GetBytes(errorMessage);
        messageBuffer[0] = (byte)MessageType.ERR;
        BitConverter.GetBytes(NextId()).CopyTo(messageBuffer, 1);
        bDisplayName.CopyTo(messageBuffer, 3);
        messageBuffer[3 + bDisplayName.Length] = 0x00;
        bErrorMessage.CopyTo(messageBuffer, 4 + bDisplayName.Length);
        messageBuffer[4 + bDisplayName.Length + bErrorMessage.Length] = 0x00;
        return messageBuffer;
    }
    
    public static byte[] BuildConfirm(UInt16 refId)
    {
        byte[] messageBuffer = new byte[3];
        messageBuffer[0] = (byte)MessageType.CONFIRM;
        BitConverter.GetBytes(refId).CopyTo(messageBuffer, 1);
        return messageBuffer;
    }
    
    public static byte[] BuildBye()
    {
        byte[] messageBuffer = new byte[3];
        messageBuffer[0] = (byte)MessageType.BYE;
        BitConverter.GetBytes(NextId()).CopyTo(messageBuffer, 1);
        return messageBuffer;
    }
    
}

public static class UdpMessagePrinter
{
    public static void PrintMessage(byte[] messageBuffer)
    {
        MessageType messageType = (MessageType)messageBuffer[0];
        switch (messageType)
        {
            case MessageType.MSG:
                string nameAndMessage = Encoding.ASCII.GetString(messageBuffer[3..^1]);
                var msgParts = nameAndMessage.Split("\0");
                var msgName = msgParts[0];
                var msgMessage = msgParts[1];
                Console.WriteLine($"{msgName}: {msgMessage}");
                break;
            case MessageType.REPLY:
                string replyMessage = Encoding.ASCII.GetString(messageBuffer[6..^1]);
                bool result = messageBuffer[3] == 0x01;
                if (result)
                {
                    Console.WriteLine($"Success: {replyMessage}");
                }
                else
                {
                    Console.Error.WriteLine($"Failure: {replyMessage}");
                }
                break;
            case MessageType.ERR:
                string errorNameMessage = Encoding.ASCII.GetString(messageBuffer[3..^1]);
                var errorParts = errorNameMessage.Split("\0");
                var errorName = errorParts[0];
                var errorMessage = errorParts[1];
                Console.Error.WriteLine($"ERR FROM {errorName}: {errorMessage}");
                break;
            default:
                Console.WriteLine($"Unprintable packet type: {messageType}");
                break;
        }
    }
}
