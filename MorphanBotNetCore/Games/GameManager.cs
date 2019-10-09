using MorphanBotNetCore.Games.DnD;
using MorphanBotNetCore.Storage;
using MorphanBotNetCore.Util;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games
{
    public class GameManager
    {
        public FactoryDictionary<string, IGame> GameFactories;

        public Dictionary<string, IGame> ExistingGames;

        public IStructuredStorage Storage;

        public const string GamesFolder = "data/games/";

        public const string GamesFile = GamesFolder + "games";

        public IGame CurrentGame;

        public GameManager()
        {
            SetupGameFactories();
            SetupStorage();
            SetupGames();
        }

        public void SetupGameFactories()
        {
            GameFactories = new FactoryDictionary<string, IGame>()
            {
                CreateFactory<DnDGame>()
            };
        }

        public void SetupStorage()
        {
            Storage = new JsonStorage((serializer) =>
            {
                serializer.ContractResolver = new JsonFileReferenceResolver()
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                };
            });
        }

        public void SetupGames()
        {
            ExistingGames = new Dictionary<string, IGame>();
            GameBank bank = Storage.Load<GameBank>(GamesFile);
            bank?.Games.ForEach((item) =>
            {
                if (GameFactories.TryCreate(item.Game, out IGame game))
                {
                    game.InternalTitle = item.InternalTitle;
                    game.Load(Storage);
                    ExistingGames.Add(game.InternalTitle, game);
                }
            });
        }

        public void Save()
        {
            GameBank bank = new GameBank();
            foreach (KeyValuePair<string, IGame> pair in ExistingGames)
            {
                bank.Games.Add(new GameItem()
                {
                    Game = pair.Value.InternalName,
                    InternalTitle = pair.Key,
                    LastActivity = pair.Value.Data.LastActivity
                });
                pair.Value.Save(Storage);
            }
            Storage.Write(GamesFolder + "games", bank);
        }

        private static KeyValuePair<string, Func<IGame>> CreateFactory<T>() where T : IGame, new()
        {
            Func<IGame> gameFactory = () => new T();
            string key = gameFactory().InternalName;
            return new KeyValuePair<string, Func<IGame>>(key, gameFactory);
        }
    }
}
