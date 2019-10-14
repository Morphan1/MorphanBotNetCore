using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Discord;
using Discord.Commands;
using MorphanBotNetCore.Storage;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDGame : Game<DnDCampaignData>
    {
        public const string _InternalName = "dnd5e";

        public const string CharactersFolder = GameManager.GamesFolder + _InternalName + "/characters/";

        public override string InternalName => _InternalName;

        public override string Name => "Dungeons & Dragons";

        public override string Version => "5e";

        public override string[] ExtraFolders => new string[] { CharactersFolder };

        public override Type[] Modules => new Type[] { typeof(DnDCommands) };

        public Dictionary<string, DnDPlayerCharacter> CachedPlayers = new Dictionary<string, DnDPlayerCharacter>();

        public DnDPlayerCharacter GetPlayer(ulong userId)
        {
            return GameData.Players.Where((p) => p.ControlledBy == userId).FirstOrDefault();
        }

        public DnDPlayerCharacter GetPlayer(IStructuredStorage storage, string name)
        {
            name = name.ToLowerInvariant().StripNonAlphaNumeric();
            if (!CachedPlayers.TryGetValue(name, out DnDPlayerCharacter player))
            {
                player = GameData.Players.Where((p) => p.Name.ToLowerInvariant().StripNonAlphaNumeric() == name).FirstOrDefault();
                if (player == null)
                {
                    player = storage.Load<DnDPlayerCharacter>(CharactersFolder + name);
                }
                if (player != null)
                {
                    CachedPlayers.Add(name, player);
                }
            }
            return player;
        }

        public void AddPlayer(DnDPlayerCharacter character)
        {
            GameData.Players.Add(character);
            character.LastKnownCampaign = InternalTitle;
        }

        public void RemovePlayer(DnDPlayerCharacter character)
        {
            character.ControlledBy = 0UL;
            character.LastKnownCampaign = null;
            GameData.Players.Remove(character);
        }

        public DnDPlayerCharacter CreatePlayer(string race, string name)
        {
            return new DnDPlayerCharacter()
            {
                Name = name,
                Alive = true,
                BasicInfo = new DnDBasicCharacterInfo()
                {
                    Race = race
                },
                Inspiration = false
            };
        }

        public bool DeletePlayer(string name)
        {
            name = name.ToLowerInvariant().StripNonAlphaNumeric();
            bool existed = CachedPlayers.Remove(name)
                        || GameData.Players.RemoveAll((p) => p.Name.ToLowerInvariant().StripNonAlphaNumeric() == name) > 0
                        || File.Exists(CharactersFolder + name);
            File.Delete(CharactersFolder + name);
            return existed;
        }

        public override EmbedBuilder CreateInfoEmbed(EmbedBuilder builder)
        {
            return builder.AddField("Dungeon Master", GameData.DungeonMaster != 0UL ? $"<@{GameData.DungeonMaster}>" : "None", true)
                          .AddField("Players Alive", GameData.Players.Count((p) => p.Alive), true)
                          .AddField("Players Dead", GameData.Players.Count((p) => !p.Alive), true);
        }
    }
}
