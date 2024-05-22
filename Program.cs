using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DotNetEnv;

namespace DiscordPingBot
{
    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly string _token;

        public Program()
        {
            // Load environment variables from .env file
            Env.Load();

            // Retrieve the bot token from environment variables
            _token = Env.GetString("DISCORD_BOT_TOKEN");

            var config = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);

            _client.Log += LogAsync;
            _client.Ready += ReadyAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected and ready!");
            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(SocketMessage message)
        {
            if (message.Author.IsBot) return;

            Console.WriteLine($"Received message from {message.Author}: {message.Content}");

            if (message.Content == "!ping")
            {
                Console.WriteLine("Ping command received.");
                var stopwatch = Stopwatch.StartNew();

                // Measure bot processing time
                stopwatch.Stop();
                var processingTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Bot processing time: {processingTime} ms");

                // Measure response send time
                stopwatch.Restart();
                var responseMessage = await message.Channel.SendMessageAsync("Pong!");
                stopwatch.Stop();
                var responseTime = stopwatch.ElapsedMilliseconds;
                Console.WriteLine($"Response send time: {responseTime} ms");

                // Log Discord API latency
                var apiLatency = _client.Latency;
                Console.WriteLine($"Discord API latency: {apiLatency} ms");

                // Send a detailed response
                await responseMessage.ModifyAsync(msg => msg.Content = $"Pong!\n" +
                    $"Bot processing time: {processingTime} ms\n" +
                    $"Response send time: {responseTime} ms\n" +
                    $"Discord API latency: {apiLatency} ms");
            }
        }
    }
}
