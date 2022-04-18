using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace Core
{
    public static class Program
    {
        static void Main(string[] args)
        {
            Bot bot = new Bot();
            Console.ReadLine();
        }
    }

    public class Bot
    {
        public static readonly string Username = "justinfan123";
        public static readonly string Token = "justinfan";
        public static readonly string InitialChannel = "forsen";
        public static TwitchClient client { get; private set; } = new();

        private Queue<string> ChannelsToJoin { get; } = new();
        private List<string> JoinedChannels { get; } = new();

        public Bot()
        {
            ConnectionCredentials credentials = new ConnectionCredentials(Username, Token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            client = new TwitchClient(customClient);
            client.Initialize(credentials, InitialChannel);

            client.OnJoinedChannel += (s, e) =>
            {
                JoinedChannels.Add(e.Channel);
            };
            client.OnMessageReceived += (s, e) =>
            {
                MessageCount++;
                QueueForJoin(e.ChatMessage.Username);

                Console.WriteLine($"{e.ChatMessage.Message}");
            };
            client.OnUserJoined += (s, e) =>
            {
                QueueForJoin(e.Username);
            };
            client.OnConnected += (s, e) =>
            {
                Console.WriteLine("connected");
                startJoiner();
            };

            client.Connect();
        }

        private void QueueForJoin(string channel)
        {
            if (!ChannelsToJoin.Contains(channel) && !JoinedChannels.Contains(channel))
            {
                ChannelsToJoin.Enqueue(channel);
            }
        }

        private void startJoiner()
        {
            System.Timers.Timer timer = new();
            timer.Interval = 200;
            timer.AutoReset = true;
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        private int TimerCount { get; set; } = 0;
        private int MessageCount { get; set; } = 0;

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            TimerCount++;
            bool s = ChannelsToJoin.TryDequeue(out string? channel);

            if (s)
            {
                try
                {
                    client.JoinChannel(channel!);
                }
                catch (Exception)
                {
                    string v = string.Join(';', JoinedChannels);
                }
            }
            if (TimerCount % 5 == 0)
            {
                Console.Title = $"{client.JoinedChannels.Count} <- {ChannelsToJoin.Count} | {MessageCount} messages/s";
                MessageCount = 0;
            }
        }
    }
}