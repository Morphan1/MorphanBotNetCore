﻿using MorphanBotNetCore.Games.DnD;
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

        public MorphBot Bot;

        public GameManager(MorphBot bot)
        {
            Bot = bot;
        }

        public void Init()
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
            for (int i = 0; i < bank?.Games.Count; i++)
            {
                GameItem item = bank.Games[i];
                if (GameFactories.TryCreate(item.Game, out IGame game))
                {
                    game.InternalTitle = item.InternalTitle;
                    game.Load(Storage);
                    ExistingGames.Add(game.InternalTitle, game);
                    if (bank.LastGame == i)
                    {
                        SetGame(game);
                    }
                }
            }
        }

        public void SetGame(IGame game)
        {
            CurrentGame?.UnregisterModules(Bot.Commands);
            CurrentGame = game;
            CurrentGame?.RegisterModules(Bot.Commands, Bot.Services);
        }

        public void Save()
        {
            GameBank bank = new GameBank();
            int lastGame = -1;
            int curr = 0;
            foreach (KeyValuePair<string, IGame> pair in ExistingGames)
            {
                if (pair.Value == CurrentGame)
                {
                    lastGame = curr;
                }
                bank.Games.Add(new GameItem()
                {
                    Game = pair.Value.InternalName,
                    InternalTitle = pair.Key,
                    LastActivity = pair.Value.Data.LastActivity
                });
                pair.Value.Save(Storage);
                curr++;
            }
            bank.LastGame = lastGame;
            Storage.Write(GamesFolder + "games", bank);
        }

        private static KeyValuePair<string, Func<IGame>> CreateFactory<T>() where T : IGame, new()
        {
            string key = gameFactory().InternalName;
            return new KeyValuePair<string, Func<IGame>>(key, gameFactory);

            static IGame gameFactory() => new T();
        }
    }
}
