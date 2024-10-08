using System.Text;

namespace ipk24_chat_client;

public static class CommandTranslator
{
    public static string Translate(string input, MessageData messageData, ServerSettings serverSettings)
    {
        string[] tokens = input.Split(' ');

        switch (tokens[0])
        {
            case "/auth":
                return TranslateAuth(tokens, messageData, serverSettings);
            case "/join":
                return TranslateJoin(tokens, messageData);
            case "/rename":
                return TranslateRename(tokens, messageData);
            case "/help":
                return TranslateHelp();
            case "BYE":
                return TranslateBye();
            case "ERR":
                return TranslateError(tokens, messageData);
            default:
                return TranslateMessage(tokens, messageData);
        }
    }

    private static string TranslateAuth(string[] tokens, MessageData messageData, ServerSettings serverSettings)
    {
        if (MessageData.AuthExecuted)
        {
            return "ERR: /auth command has already been executed. For help use /help\r\n";
        }
        
        if (tokens.Length != 4)
        {
            return "ERR: Invalid /auth command format\r\n";
        }
        
        MessageData.AuthExecuted = true; // the first execution of /auth
        messageData.MessageType = "AUTH";
        messageData.Username = tokens[1];
        messageData.DisplayName = tokens[3];
        messageData.Secret = tokens[2];

        if (serverSettings.Transport == "udp")
        {
            return ""; // (not used)
        }

        return $"AUTH {tokens[1]} AS {tokens[3]} USING {tokens[2]}\r\n";
    }

    
    private static string TranslateJoin(string[] tokens, MessageData messageData)
    {
        if (!MessageData.AuthExecuted)
        {
            return "ERR: /auth command must be executed first. For help use /help\r\n";
        }
        if (tokens.Length != 2)
        {
            return "ERR: Invalid /join command format\r\n";
        }

        messageData.MessageType = "JOIN";
        messageData.ChannelId = tokens[1];

        return $"JOIN {tokens[1]} AS {messageData.DisplayName}\r\n";
    }

    private static string TranslateRename(string[] tokens, MessageData messageData)
    {
        if (tokens.Length != 2)
        {
            return "ERR: Invalid /rename command format. For help use /help\r\n";
        }
        messageData.DisplayName = tokens[1]; // Locally change the display name
        return $"NOW YOUR DISPLAY_NAME IS: {tokens[1]}\r\n";
    }

    public static string TranslateHelp()
    {
        StringBuilder helpMessage = new StringBuilder();
        helpMessage.AppendLine("Supported local commands:");
        helpMessage.AppendLine("/auth {Username} {Secret} {DisplayName} - Sends AUTH message to the server.");
        helpMessage.AppendLine("/join {ChannelID} - Sends JOIN message to the server.");
        helpMessage.AppendLine("/rename {DisplayName} - Locally changes the display name of the user.");
        helpMessage.AppendLine("/help - Prints out supported local commands with their parameters and a description.");
        return helpMessage.ToString();
    }
    
    private static string TranslateError(string[] tokens, MessageData messageData)
    {
        messageData.MessageType = "ERR";
        messageData.ErrorMessage = string.Join(" ", tokens);
        return $"ERR FROM {messageData.DisplayName} IS {messageData.ErrorMessage}\r\n";
    }
    public static string TranslateBye()
    {
        return "BYE\r\n";
    }
    
    private static string TranslateMessage(string[] tokens, MessageData messageData)
    {
        if (tokens.Length == 0)
        {
            return "ERR: Invalid message command format\r\n";
        }
        if (MessageData.AuthExecuted == false)
        {
            return "ERR: /auth command must be executed first. For help use /help\r\n";
        }

        messageData.MessageType = "MSG";
        messageData.MessageContent = string.Join(" ", tokens);
        Console.WriteLine("{0}: {1}", messageData.DisplayName, messageData.MessageContent);
        return $"MSG FROM {messageData.DisplayName} IS {messageData.MessageContent}\r\n";
    }
}