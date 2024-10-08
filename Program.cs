using System.Net.Sockets;
using System.Text;

namespace ipk24_chat_client;

static class Program
{
    // Define a flag to indicate if the user has requested to exit the program
    private static bool _exitRequested;

    private static string _displayName = "anonymous";

    // Ctrl+C handler
    private static void HandleCtrlC(object sender, ConsoleCancelEventArgs e)
    {
        _exitRequested = true; // User has requested to exit the program
        e.Cancel = true; // Prevent the program from terminating immediately
        Environment.ExitCode = 0; // Set exit code to 0 to indicate successful termination
    }

    private static UInt16 _id = 1;


    private static async Task Main(string[] args)
    {

        MessageData messageData = new MessageData();

        // Parse command line arguments
        ServerSettings? serverSettings = ArgumentParser.ParseArguments(args);
        if (serverSettings == null)
        {
            await Console.Error.WriteLineAsync("Invalid arguments. Usage -h for help.");
            return;
        }

        if (serverSettings.HelpRequested)
        {
            return;
        }

        if (serverSettings.Transport == "udp")
        {
            if (serverSettings.ServerIpAddress != null)
            {
                var udpClient = new UdpClientWrapper();
                udpClient.ResolveServerAddress(serverSettings.ServerIpAddress, serverSettings.ServerPort,
                    serverSettings);
                udpClient.Bind();
                
                udpClient.ReceiveAsync();

                Console.CancelKeyPress += async (_, e) =>
                {
                    e.Cancel = true;
                    _exitRequested = true;
                    await Task.Yield();
                    Environment.Exit(0);
                };


                Console.CancelKeyPress += HandleCtrlC!;

                while (!_exitRequested)
                {
                    string? input = Console.ReadLine();

                    if (input == null || input.Trim().ToLower() == "exit")
                    {
                        var byeMessage = UdpMessageBuilder.BuildBye();
                        await udpClient.SendAsync(byeMessage);
                        _exitRequested = true;
                        break;
                    }

                    var inputParts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    switch (inputParts[0])
                    {
                        case "/auth":
                            var authMessage = UdpMessageBuilder.BuildAuth(inputParts[1], inputParts[3], inputParts[2]);
                            await udpClient.SendAsync(authMessage);
                            _displayName = inputParts[3];
                            break;
                        case "/join":
                            var joinMessage = UdpMessageBuilder.BuildJoin(_displayName, inputParts[1]);
                            await udpClient.SendAsync(joinMessage);
                            break;
                        case "/rename":
                            _displayName = inputParts[1];
                            Console.WriteLine($"Display name changed to: {_displayName}");
                            break;
                        case "/help":
                            Console.WriteLine("Supported local commands:");
                            Console.WriteLine(
                                "/auth {Username} {Secret} {DisplayName} - Sends AUTH message to the server.");
                            Console.WriteLine("/join {ChannelID} - Sends JOIN message to the server.");
                            Console.WriteLine("/rename {DisplayName} - Locally changes the display name of the user.");
                            Console.WriteLine(
                                "/help - Prints out supported local commands with their parameters and a description.");
                            break;
                        default:
                            var msgMessage = UdpMessageBuilder.BuildMsg(_displayName, input);
                            await udpClient.SendAsync(msgMessage);
                            // msg processing
                            break;
                    }
                }
            }
        }
        else         /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        {
            TcpClient? client = null;
            try
            {
                client = new TcpClient();
                Console.WriteLine("Connecting to server...");
                if (serverSettings.ServerIpAddress != null)
                    await client.ConnectAsync(serverSettings.ServerIpAddress, serverSettings.ServerPort);
                Console.WriteLine("Connected to server.");

                NetworkStream stream = client.GetStream();

                var sendTask = Task.Run(async () =>
                {
                    while (!_exitRequested)
                    {
                        string? input = Console.ReadLine();
                        string translatedCommand;
                        if (input == null || input.Trim().ToLower() == "exit")
                        {
                            translatedCommand = CommandTranslator.TranslateBye();
                            byte[] byeMessageBytes = Encoding.ASCII.GetBytes(translatedCommand);
                            await stream.WriteAsync(byeMessageBytes, 0, byeMessageBytes.Length);
                            Console.WriteLine($"BYE");

                            client.Close();
                            _exitRequested = true;
                            break;
                        }

                        if (input.StartsWith("/help"))
                        {
                            // Generate help message
                            translatedCommand = CommandTranslator.TranslateHelp();
                            Console.WriteLine(translatedCommand); // Print help message locally
                        }
                        else if (input.StartsWith("/rename"))
                        {
                            translatedCommand = CommandTranslator.Translate(input, messageData, serverSettings);
                            Console.WriteLine(translatedCommand); // Locally print the command and new display name
                        }
                        else if (input.StartsWith("BYE"))
                        {
                            translatedCommand = CommandTranslator.TranslateBye();
                            byte[] byeMessageBytes = Encoding.ASCII.GetBytes(translatedCommand);
                            await stream.WriteAsync(byeMessageBytes, 0, byeMessageBytes.Length);
                            Console.WriteLine($"BYE");

                            client.Close();
                            _exitRequested = true;
                            break;
                        }
                        else // translate the other commands
                        {
                            translatedCommand = CommandTranslator.Translate(input, messageData, serverSettings);

                            if (translatedCommand.StartsWith("ERR"))
                            {
                                Console.WriteLine(translatedCommand); // Print error message locally
                                continue; // Skip sending the message to the server
                            }

                            byte[] messageBytes = Encoding.ASCII.GetBytes(translatedCommand);
                            await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                            //Console.WriteLine("Sent: {0}", translatedCommand);
                        }

                    }
                });

                var receiveTask = Task.Run(async () =>
                {
                    while (!_exitRequested)
                    {
                        try
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            //Console.WriteLine("Received: {0}", response);

                            string translatedResponse = ServerResponseTranslator.Translate(response);
                            //Console.WriteLine(translatedResponse);
                            if (response.StartsWith("ERR"))
                            {
                                await Console.Error.WriteLineAsync(translatedResponse);
                                //Environment.Exit(1); 
                            }
                            else if (response.StartsWith("BYE"))
                            {
                                client.Close();
                                _exitRequested = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine("Error reading response: {0}", ex.Message);
                            _exitRequested = true; // Exit the loop if there's an error
                        }
                    }
                });

                await Task.WhenAny(sendTask, receiveTask); // Wait for either task to complete
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("ERR: {0}", ex.Message);
            }
            finally
            {
                // Close the client connection
                client?.Close();
            }
        }
    }
}