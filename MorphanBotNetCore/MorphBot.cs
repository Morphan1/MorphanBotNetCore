using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MorphanBotNetCore.Games;
using MorphanBotNetCore.Storage;
using System;
using System.Reflection;
using System.Threading.Tasks;
using YamlDotNet.Serialization.NamingConventions;

namespace MorphanBotNetCore
{
    public class MorphBot
    {
        public DiscordSocketClient Client;

        public CommandService Commands;

        public IServiceProvider Services;

        public IStructuredStorage PrimaryStorage;

        public BotSettings Configuration;

        public GameManager Games;

        static void Main(string[] args)
        {
            new MorphBot().StartAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync()
        {
            PrimaryStorage = new YamlStorage(new UnderscoredNamingConvention());
            Configuration = PrimaryStorage.Load<BotSettings>("config");
            if (Configuration.Discord == null)
            {
                Console.WriteLine($"No 'discord' key with a token in config.{PrimaryStorage.FileExtension}!");
                return;
            }
            if ((WolframAlpha.AppID = Configuration.Wolfram) == null)
            {
                Console.WriteLine($"No 'wolfram' key with an app ID in config.{PrimaryStorage.FileExtension}!");
                return;
            }
            Client = new DiscordSocketClient();
            Client.MessageReceived += HandleCommandAsync;
            Commands = new CommandService();
            Games = new GameManager(this);
            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .AddSingleton(Games)
                .BuildServiceProvider();
            Games.Init();
            await Commands.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

            Client.Log += async (e) =>
            {
                Console.WriteLine($"[{e.Severity}] {e.Source}: {e.Message}");
                await Task.Delay(0);
            };
            Client.UserJoined += async (e) =>
            {
                if (!e.IsBot)
                {
                    await e.Guild.DefaultChannel.SendMessageAsync("Welcome to " + e.Guild.Name + ", **" + e.Username + "**!");
                }
            };
            Client.UserLeft += async (e) =>
            {
                if (!e.IsBot)
                {
                    await e.Guild.DefaultChannel.SendMessageAsync("**" + e.Username + "** left the server.");
                }
            };

            await Client.LoginAsync(TokenType.Bot, Configuration.Discord);
            await Client.StartAsync();
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage is SocketUserMessage message && !message.Author.IsBot)
            {
                int argPos = 0;
                if (!message.HasStringPrefix("//", ref argPos) && !message.HasStringPrefix("!!", ref argPos)
                    && !message.HasCharPrefix('/', ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)
                    && !message.HasCharPrefix('.', ref argPos))
                {
                    return;
                }
                ICommandContext context = new SocketCommandContext(Client, message);
                IResult result = await Commands.ExecuteAsync(context, argPos, Services);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync(result.ErrorReason);
                }
            }
        }
    }
}
