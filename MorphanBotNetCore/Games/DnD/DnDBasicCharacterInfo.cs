﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public struct DnDBasicCharacterInfo
    {
        public string Class { get; set; }

        public string Race { get; set; }

        public string Deity { get; set; }

        public string Alignment { get; set; }

        public int Speed { get; set; }

        public string HitDice { get; set; }

        public DnDCharacterHealth Health { get; set; }

        public DnDInventory Inventory { get; set; }

        public DnDWallet Wallet { get; set; }

        public int Strength { get; set; }

        public int Dexterity { get; set; }

        public int Constitution { get; set; }

        public int Intelligence { get; set; }

        public int Wisdom { get; set; }

        public int Charisma { get; set; }

        public List<DnDAbilityModDescriptor> AbilityMods { get; set; }

        public List<DnDSkillModDescriptor> SkillMods { get; set; }

        public List<DnDCharacterSkills> SkillProficiencies { get; set; }

        public DnDAbilityScores? CastingAbility { get; set; }

        public DnDSpellBook SpellBook { get; set; }

        public DnDAppearance Appearance { get; set; }

        public List<string> Languages { get; set; }

        public readonly int GetAbilityScore(DnDAbilityScores ability)
        {
            return ability switch
            {
                DnDAbilityScores.Strength => Strength,
                DnDAbilityScores.Dexterity => Dexterity,
                DnDAbilityScores.Constitution => Constitution,
                DnDAbilityScores.Intelligence => Intelligence,
                DnDAbilityScores.Wisdom => Wisdom,
                DnDAbilityScores.Charisma => Charisma,
                _ => throw new InvalidOperationException($"Invalid ability score: {ability}"),
            };
        }

        public readonly int GetAbilityMod(DnDAbilityScores ability)
        {
            int totalMod = ((GetAbilityScore(ability) / 2) - 5);
            totalMod += AbilityMods.Where((mod) => mod.AbilityMod.Ability == ability).Sum((mod) => mod.AbilityMod.Modifier);
            return totalMod;
        }
    }

    public struct DnDCharacterHealth
    {
        public int Current { get; set; }

        public int Temporary { get; set; }

        public int Max { get; set; }
    }

    public struct DnDInventory
    {
        public List<DnDItem> Equipped { get; set; }

        public List<DnDItem> Storage { get; set; }

        public int Capacity { get; set; }
    }

    public struct DnDWallet
    {
        public int Copper { get; set; }

        public int Silver { get; set; }

        public int Electrum { get; set; }

        public int Gold { get; set; }

        public int Platinum { get; set; }
    }

    public struct DnDAppearance
    {
        public string Image { get; set; }

        public string Height { get; set; }

        public string Weight { get; set; }

        public string Age { get; set; }

        public string Gender { get; set; }

        public string Hair { get; set; }

        public string Eyes { get; set; }

        public string Skin { get; set; }

        public string HairColor { get; set; }

        public string EyesColor { get; set; }

        public string SkinColor { get; set; }
    }

    public struct DnDAbilityModDescriptor
    {
        public string SourceDescription { get; set; }

        public DnDAbilityModifier AbilityMod { get; set; }
    }

    public struct DnDSkillModDescriptor
    {
        public string SourceDescription { get; set; }

        public DnDSkillModifier SkillMod { get; set; }
    }
}
