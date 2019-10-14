using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public enum DnDAbilityScores
    {
        Strength,
        Dexterity, 
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }

    public struct DnDAbilityModifier
    {
        public DnDAbilityScores Attribute { get; set; }

        public int Modifier { get; set; }
    }
}
