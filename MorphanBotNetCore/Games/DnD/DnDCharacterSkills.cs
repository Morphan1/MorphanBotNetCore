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
}
