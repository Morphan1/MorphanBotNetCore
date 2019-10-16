using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            return DnDMigrate.Migrate(GameData.Players.Where((p) => p.ControlledBy == userId).FirstOrDefault());
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
                    CachedPlayers.Add(name, DnDMigrate.Migrate(player));
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

        private const string IgnoreMath = @"[^\d\*\/\+\-\^\%]*";

        public override string SpecialRoll(ulong userId, string input)
        {
            input = Regex.Replace(input, "adv" + IgnoreMath, "2d20d1");
            input = Regex.Replace(input, "dis" + IgnoreMath, "2d20k1");
            DnDPlayerCharacter player = GetPlayer(userId);
            if (player != null)
            {
                foreach (DnDAbilityScores ability in Utilities.GetEnumValues<DnDAbilityScores>())
                {
                    // first 3 letters... str, dex, con, int, wis, cha
                    string name = ability.ToString().Substring(0, 3).ToLowerInvariant() + IgnoreMath;
                    // only an ability score -> roll d20 + mod
                    input = Regex.Replace(input, "^" + name + "$", "1d20" + GetModString(player, ability));
                    // somewhere in a string -> assume it can be replaced with the mod
                    input = Regex.Replace(input, name, "(" + player.BasicInfo.GetAbilityMod(ability) + ")");
                }
                foreach (DnDCharacterSkills skill in Utilities.GetEnumValues<DnDCharacterSkills>())
                {
                    // first 4 letters... athl, acro, slei, stea, arca, hist, inve, natu, reli, anim, insi, medi, perc, surv, dece, inti, perf, pers
                    string name = skill.ToString().Substring(0, 4).ToLowerInvariant() + IgnoreMath;
                    // only a skill -> roll d20 + mod
                    input = Regex.Replace(input, "^" + name + "$", "1d20" + GetModString(player, skill));
                    // somewhere in a string -> assume it can be replaced with the mod
                    input = Regex.Replace(input, name, "(" + player.GetSkillMod(skill) + ")");
                }
            }
            return input;
        }

        private static string GetModString(DnDPlayerCharacter player, DnDAbilityScores ability)
        {
            int mod = player.BasicInfo.GetAbilityMod(ability);
            return (mod < 0 ? " - " : " + ") + Math.Abs(mod);
        }

        private static string GetModString(DnDPlayerCharacter player, DnDCharacterSkills skill)
        {
            int mod = player.GetSkillMod(skill);
            return (mod < 0 ? " - " : " + ") + Math.Abs(mod);
        }
    }
}
