using Discord;
using Discord.Commands;
using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MorphanBotNetCore.Games
{
    public abstract class Game<T> : IGame where T : CommonGameData, new()
    {
        public abstract string InternalName { get; }

        public abstract string Name { get; }

        public abstract string Version { get; }

        public abstract string[] ExtraFolders { get; }

        public abstract Type[] Modules { get; }

        public T GameData { get; set; }

        public CommonGameData Data => GameData;

        public string FullName => $"{Name} {Version}";

        public string InternalTitle { get; set; }

        public string GameFolder => GameManager.GamesFolder + InternalName + "/";

        public string SessionsFolder => GameFolder + "sessions/";

        public string SessionFolder => SessionsFolder + InternalTitle + "/";

        public string MainFile => SessionFolder + "game";

        public void Load(IStructuredStorage storage)
        {
            GameData = storage.Load<T>(MainFile);
        }

        public void Save(IStructuredStorage storage)
        {
            storage.Write(MainFile, GameData);
        }

        public void RegisterModules(CommandService commands, IServiceProvider services)
        {
            foreach (Type module in Modules)
            {
                commands.AddModuleAsync(module, services);
            }
        }

        public void UnregisterModules(CommandService commands)
        {
            foreach (Type module in Modules)
            {
                commands.RemoveModuleAsync(module);
            }
        }

        public Embed CreateInfoEmbed()
        {
            return CreateInfoEmbed(new EmbedBuilder()
                .WithTitle(GameData.Title)
                .AddField("Game", FullName, true)
                .AddField("Last Activity", GameData.LastActivity.ToString(), true))
                .Build();
        }

        public abstract EmbedBuilder CreateInfoEmbed(EmbedBuilder builder);

        public void Init(string title, string internalTitle)
        {
            InternalTitle = internalTitle;
            Init();
            GameData.Title = title;
        }

        public virtual void Init()
        {
            Directory.CreateDirectory(GameFolder);
            Directory.CreateDirectory(SessionsFolder);
            Directory.CreateDirectory(SessionFolder);
            foreach (string folder in ExtraFolders)
            {
                Directory.CreateDirectory(folder);
            }
            GameData = new T();
        }
    }

    public interface IGame
    {
        string InternalName { get; }

        CommonGameData Data { get; }

        string FullName { get; }

        string InternalTitle { get; set; }

        void Load(IStructuredStorage storage);

        void Save(IStructuredStorage storage);

        void RegisterModules(CommandService commands, IServiceProvider services);

        void UnregisterModules(CommandService commands);

        Embed CreateInfoEmbed();

        EmbedBuilder CreateInfoEmbed(EmbedBuilder builder);

        void Init(string title, string internalTitle);
    }
}
