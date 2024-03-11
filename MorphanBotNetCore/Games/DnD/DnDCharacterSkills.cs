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
            return skill switch
            {
                DnDCharacterSkills.Athletics
                    => DnDAbilityScores.Strength,

                DnDCharacterSkills.Acrobatics or
                DnDCharacterSkills.SleightOfHand or
                DnDCharacterSkills.Stealth
                    => DnDAbilityScores.Dexterity,

                DnDCharacterSkills.Arcana or
                DnDCharacterSkills.History or
                DnDCharacterSkills.Investigation or
                DnDCharacterSkills.Nature or
                DnDCharacterSkills.Religion
                    => DnDAbilityScores.Intelligence,

                DnDCharacterSkills.AnimalHandling or
                DnDCharacterSkills.Insight or
                DnDCharacterSkills.Medicine or
                DnDCharacterSkills.Perception or
                DnDCharacterSkills.Survival
                    => DnDAbilityScores.Wisdom,

                DnDCharacterSkills.Deception or
                DnDCharacterSkills.Intimidation or
                DnDCharacterSkills.Performance or
                DnDCharacterSkills.Persuasion
                    => DnDAbilityScores.Charisma,

                _ => (DnDAbilityScores)(-1),
            };
        }
    }
}
