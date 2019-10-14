using Discord;
using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDPlayerCharacter
    {
        [FileName]
        public string Name { get; set; }

        public ulong ControlledBy { get; set; }

        public bool Alive { get; set; }

        public string LastKnownCampaign { get; set; }

        public DnDBasicCharacterInfo BasicInfo { get; set; }

        public DnDPlayerLevel Level { get; set; }

        public int ProficiencyBonus { get; set; }

        public List<DnDAbilityScores> SaveProficiencies { get; set; }

        public string Background { get; set; }

        public bool Inspiration { get; set; }

        public DnDDeathSaves DeathSaves { get; set; }

        public Embed CreateInfoEmbed()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .AddField("Name", Name, true)
                .AddField("Controlled By", ControlledBy != 0UL ? $"<@{ControlledBy}>" : "Nobody", true)
                .AddField("Alive", Alive ? "Yes" : "No", true)
                .AddField("Class", BasicInfo.Class, true)
                .AddField("Race", BasicInfo.Race, true)
                .AddField("HP", (BasicInfo.Health.Current + BasicInfo.Health.Temporary) + " / " + BasicInfo.Health.Max
                            + " (" + (int)((BasicInfo.Health.Current + BasicInfo.Health.Temporary) / (double)BasicInfo.Health.Max * 100) + "%)", true)
                .AddField("Strength", GetAbilityText(BasicInfo.Strength), true)
                .AddField("Dexterity", GetAbilityText(BasicInfo.Dexterity), true)
                .AddField("Constitution", GetAbilityText(BasicInfo.Constitution), true)
                .AddField("Intelligence", GetAbilityText(BasicInfo.Intelligence), true)
                .AddField("Wisdom", GetAbilityText(BasicInfo.Wisdom), true)
                .AddField("Charisma", GetAbilityText(BasicInfo.Charisma), true)
                .WithFooter(LastKnownCampaign != null ? "This character is currently part of the campaign: " + LastKnownCampaign
                            : "This character is not currently part of any campaign.");
            if (BasicInfo.Appearance.Image != null)
            {
                builder.WithImageUrl(BasicInfo.Appearance.Image);
            }
            return builder.Build();
        }

        private static string GetAbilityText(int score)
        {
            int mod = ((score / 2) - 5);
            return score + " (" + (mod > 0 ? "+" : "") + mod + ")";
        }
    }

    public struct DnDPlayerLevel
    {
        public int Current { get; set; }

        public int Experience { get; set; }

        public int NextLevelXP { get; set; }
    }

    public struct DnDDeathSaves
    {
        public int Successes { get; set; }

        public int Failures { get; set; }
    }
}
