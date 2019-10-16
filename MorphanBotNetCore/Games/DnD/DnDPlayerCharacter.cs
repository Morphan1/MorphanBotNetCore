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
                .AddField("Strength", GetAbilityText(DnDAbilityScores.Strength), true)
                .AddField("Dexterity", GetAbilityText(DnDAbilityScores.Dexterity), true)
                .AddField("Constitution", GetAbilityText(DnDAbilityScores.Constitution), true)
                .AddField("Intelligence", GetAbilityText(DnDAbilityScores.Intelligence), true)
                .AddField("Wisdom", GetAbilityText(DnDAbilityScores.Wisdom), true)
                .AddField("Charisma", GetAbilityText(DnDAbilityScores.Charisma), true)
                .WithFooter(LastKnownCampaign != null ? "This character is currently part of the campaign: " + LastKnownCampaign
                            : "This character is not currently part of any campaign.");
            if (BasicInfo.Appearance.Image != null)
            {
                builder.WithThumbnailUrl(BasicInfo.Appearance.Image);
            }
            return builder.Build();
        }

        public int GetSkillMod(DnDCharacterSkills skill)
        {
            int mod = BasicInfo.GetAbilityMod(DnDSkillHelper.GetAttachedAbility(skill));
            if (BasicInfo.SkillProficiencies.Contains(skill))
            {
                mod += ProficiencyBonus;
            }
            return mod;
        }

        private string GetAbilityText(DnDAbilityScores ability)
        {
            int mod = BasicInfo.GetAbilityMod(ability);
            return BasicInfo.GetAbilityScore(ability) + " (" + (mod > 0 ? "+" : "") + mod + ")";
        }

        internal bool Migrated = false;
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
