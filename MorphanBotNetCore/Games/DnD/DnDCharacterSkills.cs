using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public enum DnDCharacterSkills
    {
        // Strength
        Athletics,
        
        // Dexterity
        Acrobatics,
        SleightOfHand,
        Stealth,

        // Intelligence
        Arcana,
        History,
        Investigation,
        Nature,
        Religion,

        // Wisdom
        AnimalHandling,
        Insight,
        Medicine,
        Perception,
        Survival,

        // Charisma
        Deception,
        Intimidation,
        Performance,
        Persuasion
    }

    public struct DnDSkillModifier
    {
        public DnDCharacterSkills Skill { get; set; }

        public int Modifier { get; set; }
    }

    public class DnDSkillHelper
    {
        public static DnDAbilityScores GetAttachedAbility(DnDCharacterSkills skill)
        {
            switch (skill)
            {
                case DnDCharacterSkills.Athletics:
                    return DnDAbilityScores.Strength;

                case DnDCharacterSkills.Acrobatics:
                case DnDCharacterSkills.SleightOfHand:
                case DnDCharacterSkills.Stealth:
                    return DnDAbilityScores.Dexterity;

                case DnDCharacterSkills.Arcana:
                case DnDCharacterSkills.History:
                case DnDCharacterSkills.Investigation:
                case DnDCharacterSkills.Nature:
                case DnDCharacterSkills.Religion:
                    return DnDAbilityScores.Intelligence;

                case DnDCharacterSkills.AnimalHandling:
                case DnDCharacterSkills.Insight:
                case DnDCharacterSkills.Medicine:
                case DnDCharacterSkills.Perception:
                case DnDCharacterSkills.Survival:
                    return DnDAbilityScores.Wisdom;

                case DnDCharacterSkills.Deception:
                case DnDCharacterSkills.Intimidation:
                case DnDCharacterSkills.Performance:
                case DnDCharacterSkills.Persuasion:
                    return DnDAbilityScores.Charisma;
            }
            return (DnDAbilityScores)(-1);
        }
    }
}
