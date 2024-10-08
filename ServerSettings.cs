namespace ipk24_chat_client;
public class ServerSettings
{
    public string? ServerIpAddress { get; set; }
    public int ServerPort { get; set; }
    public string? Transport { get; set; }
    public int Timeout { get; set; }
    public int Retransmissions { get; set; }
    public bool HelpRequested { get; set; }
}

public class ArgumentParser
{
    public static ServerSettings? ParseArguments(string[] args)
    {
        ServerSettings settings = new ServerSettings();

        settings.ServerPort = 4567;
        
        bool serverProvided = false;
        bool transportProvided = false;
        
        for (int i = 0; i < args.Length; i += 1)
        {
            switch (args[i])
            {
                case "-s":
                    settings.ServerIpAddress = args[i++ + 1]; // server address
                    serverProvided = true;
                    break;
                case "-p":
                    settings.ServerPort = int.Parse(args[i++ + 1]); // server port
                    break;
                case "-t":
                    settings.Transport = args[i++ + 1]; // transport protocol (tcp/udp)
                    transportProvided = true;
                    break;
                case "-d":
                    settings.Timeout = int.Parse(args[i++ + 1]); // UDP only
                    break;
                case "-r":
                    settings.Retransmissions = int.Parse(args[i++ + 1]); // UDP only
                    break;
                case "-h":
                    settings.HelpRequested = true;
                    break;
            }
        }
        
        if (settings.HelpRequested)
        {
            Console.WriteLine(
                "Usage: ipk24-chat-client -s server -p port -t transport (tcp/udp) -d timeout (udp only) -r retransmissions (udp only)");
            return settings;
        }
        
        if ((!serverProvided || !transportProvided))
        {
            Console.WriteLine("Error: Both -s (server) and -t (transport) arguments are required.");
            return null;
        }
        
        return settings;
    }
}
