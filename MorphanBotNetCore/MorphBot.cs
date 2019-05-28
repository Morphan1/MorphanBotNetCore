using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
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

        public BotSettings Configuration;

        static void Main(string[] args)
        {
            new MorphBot().StartAsync().GetAwaiter().GetResult();
        }

        public async Task StartAsync()
        {
            using (FileStream stream = File.OpenRead("config.yml"))
            {
                Configuration = new YamlStorage(new UnderscoredNamingConvention()).Load<BotSettings>(stream);
            }
            if (Configuration.Discord == null)
            {
                Console.WriteLine("No 'discord' key with a token in config.yml!");
                return;
            }
            if ((WolframAlpha.AppID = Configuration.Wolfram) == null)
            {
                Console.WriteLine("No 'wolfram' key with an app ID in config.yml!");
                return;
            }
            Client = new DiscordSocketClient();
            Client.MessageReceived += HandleCommandAsync;
            Commands = new CommandService();
            Services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(Commands)
                .BuildServiceProvider();
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
                    && !message.HasCharPrefix('/', ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
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

        private static string GetConfigString()
        {
            try
            {
                return File.ReadAllText("config.yml");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to read config.yml!");
                return "";
            }
        }
    }
}
