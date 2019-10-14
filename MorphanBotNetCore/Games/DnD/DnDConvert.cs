using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDConvert
    {
        public static DnDPlayerCharacter ConvertPlayer(DnDMWData data)
        {
            return new DnDPlayerCharacter()
            {
                Name = data.Name,
                Alive = true,
                BasicInfo = new DnDBasicCharacterInfo()
                {
                    Class = data.Class,
                    Race = data.Race,
                    Deity = data.Deity,
                    Alignment = data.Alignment,
                    Speed = ParseInt(data.Speed),
                    HitDice = data.HitDice,
                    Health = new DnDCharacterHealth()
                    {
                        Current = ParseInt(data.Hp),
                        Temporary = ParseInt(data.TempHp),
                        Max = ParseInt(data.MaxHp ?? data.Hp)
                    },
                    Inventory = new DnDInventory()
                    {
                        Equipped = ParseBool(data.HeavyArmor) ? UnknownHeavyItem : EmptyItems,
                        Storage = FilterItems(
                            // Weapons
                            new DnDItem() { Name = data.Weapon1Name, AttackBonus = ParseInt(data.Weapon1Attack), Damage = data.Weapon1Dmg },
                            new DnDItem() { Name = data.Weapon2Name, AttackBonus = ParseInt(data.Weapon2Attack), Damage = data.Weapon2Dmg },
                            new DnDItem() { Name = data.Weapon3Name, AttackBonus = ParseInt(data.Weapon3Attack), Damage = data.Weapon3Dmg },
                            new DnDItem() { Name = data.Weapon4Name, AttackBonus = ParseInt(data.Weapon4Attack), Damage = data.Weapon4Dmg },
                            new DnDItem() { Name = data.Weapon5Name, AttackBonus = ParseInt(data.Weapon5Attack), Damage = data.Weapon5Dmg },
                            // Equipment
                            new DnDItem() { Name = data.Equip1_, Weight = ParseInt(data.Equip1Weight), Worth = data.Equip1Worth, Location = data.Equip1Loc },
                            new DnDItem() { Name = data.Equip2_, Weight = ParseInt(data.Equip2Weight), Worth = data.Equip2Worth, Location = data.Equip2Loc },
                            new DnDItem() { Name = data.Equip3_, Weight = ParseInt(data.Equip3Weight), Worth = data.Equip3Worth, Location = data.Equip3Loc },
                            new DnDItem() { Name = data.Equip4_, Weight = ParseInt(data.Equip4Weight), Worth = data.Equip4Worth, Location = data.Equip4Loc },
                            new DnDItem() { Name = data.Equip5_, Weight = ParseInt(data.Equip5Weight), Worth = data.Equip5Worth, Location = data.Equip5Loc },
                            new DnDItem() { Name = data.Equip6_, Weight = ParseInt(data.Equip6Weight), Worth = data.Equip6Worth, Location = data.Equip6Loc },
                            new DnDItem() { Name = data.Equip7_, Weight = ParseInt(data.Equip7Weight), Worth = data.Equip7Worth, Location = data.Equip7Loc },
                            new DnDItem() { Name = data.Equip8_, Weight = ParseInt(data.Equip8Weight), Worth = data.Equip8Worth, Location = data.Equip8Loc },
                            new DnDItem() { Name = data.Equip9_, Weight = ParseInt(data.Equip9Weight), Worth = data.Equip9Worth, Location = data.Equip9Loc },
                            new DnDItem() { Name = data.Equip10_, Weight = ParseInt(data.Equip10Weight), Worth = data.Equip10Worth, Location = data.Equip10Loc },
                            new DnDItem() { Name = data.Equip11_, Weight = ParseInt(data.Equip11Weight), Worth = data.Equip11Worth, Location = data.Equip11Loc },
                            new DnDItem() { Name = data.Equip12_, Weight = ParseInt(data.Equip12Weight), Worth = data.Equip12Worth, Location = data.Equip12Loc },
                            new DnDItem() { Name = data.Equip13_, Weight = ParseInt(data.Equip13Weight), Worth = data.Equip13Worth, Location = data.Equip13Loc },
                            new DnDItem() { Name = data.Equip14_, Weight = ParseInt(data.Equip14Weight), Worth = data.Equip14Worth, Location = data.Equip14Loc },
                            new DnDItem() { Name = data.Equip15_, Weight = ParseInt(data.Equip15Weight), Worth = data.Equip15Worth, Location = data.Equip15Loc },
                            new DnDItem() { Name = data.Equip16_, Weight = ParseInt(data.Equip16Weight), Worth = data.Equip16Worth, Location = data.Equip16Loc },
                            new DnDItem() { Name = data.Equip17_, Weight = ParseInt(data.Equip17Weight), Worth = data.Equip17Worth, Location = data.Equip17Loc },
                            new DnDItem() { Name = data.Equip18_, Weight = ParseInt(data.Equip18Weight), Worth = data.Equip18Worth, Location = data.Equip18Loc },
                            new DnDItem() { Name = data.Equip19_, Weight = ParseInt(data.Equip19Weight), Worth = data.Equip19Worth, Location = data.Equip19Loc },
                            new DnDItem() { Name = data.Equip20_, Weight = ParseInt(data.Equip20Weight), Worth = data.Equip20Worth, Location = data.Equip20Loc },
                            // Explorer's Pack
                            new DnDItem() { Name = ParseBool(data.AdventurersPack) ? "Explorer's Pack" : null } // technically separate items but eh
                        )
                    },
                    Wallet = new DnDWallet()
                    {
                        Copper = ParseInt(data.CurrencyCp),
                        Silver = ParseInt(data.CurrencySp),
                        Electrum = ParseInt(data.CurrencyEp),
                        Gold = ParseInt(data.CurrencyGp),
                        Platinum = ParseInt(data.CurrencyPp)
                    },
                    Strength = ParseInt(data.Strength),
                    Dexterity = ParseInt(data.Dexterity),
                    Constitution = ParseInt(data.Constitution),
                    Intelligence = ParseInt(data.Intelligence),
                    Wisdom = ParseInt(data.Wisdom),
                    Charisma = ParseInt(data.Charisma),
                    SkillProficiencies = GetSkillProficiencies(data),
                    CastingAbility = ParseAbility(data.CastingAbility),
                    SpellBook = new DnDSpellBook()
                    {
                        Spells = FilterSpells(
                            // Cantrips
                            new DnDSpell() { Name = data.Spell_0_1, Level = 0 }, new DnDSpell() { Name = data.Spell_0_2, Level = 0 }, new DnDSpell() { Name = data.Spell_0_3, Level = 0 },
                            new DnDSpell() { Name = data.Spell_0_4, Level = 0 }, new DnDSpell() { Name = data.Spell_0_5, Level = 0 }, new DnDSpell() { Name = data.Spell_0_6, Level = 0 },
                            new DnDSpell() { Name = data.Spell_0_7, Level = 0 }, new DnDSpell() { Name = data.Spell_0_8, Level = 0 }, new DnDSpell() { Name = data.Spell_0_9, Level = 0 },
                            new DnDSpell() { Name = data.Spell_0_10, Level = 0 }, new DnDSpell() { Name = data.Spell_0_11, Level = 0 }, new DnDSpell() { Name = data.Spell_0_12, Level = 0 },
                            new DnDSpell() { Name = data.Spell_0_13, Level = 0 }, new DnDSpell() { Name = data.Spell_0_14, Level = 0 }, new DnDSpell() { Name = data.Spell_0_15, Level = 0 },
                            new DnDSpell() { Name = data.Spell_0_16, Level = 0 },
                            // Level 1
                            new DnDSpell() { Name = data.Spell_1_1, Level = 1 }, new DnDSpell() { Name = data.Spell_1_2, Level = 1 }, new DnDSpell() { Name = data.Spell_1_3, Level = 1 },
                            new DnDSpell() { Name = data.Spell_1_4, Level = 1 }, new DnDSpell() { Name = data.Spell_1_5, Level = 1 }, new DnDSpell() { Name = data.Spell_1_6, Level = 1 },
                            new DnDSpell() { Name = data.Spell_1_7, Level = 1 }, new DnDSpell() { Name = data.Spell_1_8, Level = 1 }, new DnDSpell() { Name = data.Spell_1_9, Level = 1 },
                            new DnDSpell() { Name = data.Spell_1_10, Level = 1 }, new DnDSpell() { Name = data.Spell_1_11, Level = 1 }, new DnDSpell() { Name = data.Spell_1_12, Level = 1 },
                            new DnDSpell() { Name = data.Spell_1_13, Level = 1 },
                            // Level 2
                            new DnDSpell() { Name = data.Spell_2_1, Level = 2 }, new DnDSpell() { Name = data.Spell_2_2, Level = 2 }, new DnDSpell() { Name = data.Spell_2_3, Level = 2 },
                            new DnDSpell() { Name = data.Spell_2_4, Level = 2 }, new DnDSpell() { Name = data.Spell_2_5, Level = 2 }, new DnDSpell() { Name = data.Spell_2_6, Level = 2 },
                            new DnDSpell() { Name = data.Spell_2_7, Level = 2 }, new DnDSpell() { Name = data.Spell_2_8, Level = 2 }, new DnDSpell() { Name = data.Spell_2_9, Level = 2 },
                            new DnDSpell() { Name = data.Spell_2_10, Level = 2 }, new DnDSpell() { Name = data.Spell_2_11, Level = 2 }, new DnDSpell() { Name = data.Spell_2_12, Level = 2 },
                            new DnDSpell() { Name = data.Spell_2_13, Level = 2 },
                            // Level 3
                            new DnDSpell() { Name = data.Spell_3_1, Level = 3 }, new DnDSpell() { Name = data.Spell_3_2, Level = 3 }, new DnDSpell() { Name = data.Spell_3_3, Level = 3 },
                            new DnDSpell() { Name = data.Spell_3_4, Level = 3 }, new DnDSpell() { Name = data.Spell_3_5, Level = 3 }, new DnDSpell() { Name = data.Spell_3_6, Level = 3 },
                            new DnDSpell() { Name = data.Spell_3_7, Level = 3 }, new DnDSpell() { Name = data.Spell_3_8, Level = 3 }, new DnDSpell() { Name = data.Spell_3_9, Level = 3 },
                            new DnDSpell() { Name = data.Spell_3_10, Level = 3 }, new DnDSpell() { Name = data.Spell_3_11, Level = 3 }, new DnDSpell() { Name = data.Spell_3_12, Level = 3 },
                            new DnDSpell() { Name = data.Spell_3_13, Level = 3 },
                            // Level 4
                            new DnDSpell() { Name = data.Spell_4_1, Level = 4 }, new DnDSpell() { Name = data.Spell_4_2, Level = 4 }, new DnDSpell() { Name = data.Spell_4_3, Level = 4 },
                            new DnDSpell() { Name = data.Spell_4_4, Level = 4 }, new DnDSpell() { Name = data.Spell_4_5, Level = 4 }, new DnDSpell() { Name = data.Spell_4_6, Level = 4 },
                            new DnDSpell() { Name = data.Spell_4_7, Level = 4 }, new DnDSpell() { Name = data.Spell_4_8, Level = 4 }, new DnDSpell() { Name = data.Spell_4_9, Level = 4 },
                            new DnDSpell() { Name = data.Spell_4_10, Level = 4 }, new DnDSpell() { Name = data.Spell_4_11, Level = 4 }, new DnDSpell() { Name = data.Spell_4_12, Level = 4 },
                            new DnDSpell() { Name = data.Spell_4_13, Level = 4 },
                            // Level 5
                            new DnDSpell() { Name = data.Spell_5_1, Level = 5 }, new DnDSpell() { Name = data.Spell_5_2, Level = 5 }, new DnDSpell() { Name = data.Spell_5_3, Level = 5 },
                            new DnDSpell() { Name = data.Spell_5_4, Level = 5 }, new DnDSpell() { Name = data.Spell_5_5, Level = 5 }, new DnDSpell() { Name = data.Spell_5_6, Level = 5 },
                            new DnDSpell() { Name = data.Spell_5_7, Level = 5 }, new DnDSpell() { Name = data.Spell_5_8, Level = 5 }, new DnDSpell() { Name = data.Spell_5_9, Level = 5 },
                            new DnDSpell() { Name = data.Spell_5_10, Level = 5 }, new DnDSpell() { Name = data.Spell_5_11, Level = 5 }, new DnDSpell() { Name = data.Spell_5_12, Level = 5 },
                            new DnDSpell() { Name = data.Spell_5_13, Level = 5 },
                            // Level 6
                            new DnDSpell() { Name = data.Spell_6_1, Level = 6 }, new DnDSpell() { Name = data.Spell_6_2, Level = 6 }, new DnDSpell() { Name = data.Spell_6_3, Level = 6 },
                            new DnDSpell() { Name = data.Spell_6_4, Level = 6 }, new DnDSpell() { Name = data.Spell_6_5, Level = 6 }, new DnDSpell() { Name = data.Spell_6_6, Level = 6 },
                            new DnDSpell() { Name = data.Spell_6_7, Level = 6 }, new DnDSpell() { Name = data.Spell_6_8, Level = 6 }, new DnDSpell() { Name = data.Spell_6_9, Level = 6 },
                            new DnDSpell() { Name = data.Spell_6_10, Level = 6 }, new DnDSpell() { Name = data.Spell_6_11, Level = 6 }, new DnDSpell() { Name = data.Spell_6_12, Level = 6 },
                            new DnDSpell() { Name = data.Spell_6_13, Level = 6 },
                            // Level 7
                            new DnDSpell() { Name = data.Spell_7_1, Level = 7 }, new DnDSpell() { Name = data.Spell_7_2, Level = 7 }, new DnDSpell() { Name = data.Spell_7_3, Level = 7 },
                            new DnDSpell() { Name = data.Spell_7_4, Level = 7 }, new DnDSpell() { Name = data.Spell_7_5, Level = 7 }, new DnDSpell() { Name = data.Spell_7_6, Level = 7 },
                            new DnDSpell() { Name = data.Spell_7_7, Level = 7 }, new DnDSpell() { Name = data.Spell_7_8, Level = 7 }, new DnDSpell() { Name = data.Spell_7_9, Level = 7 },
                            new DnDSpell() { Name = data.Spell_7_10, Level = 7 }, new DnDSpell() { Name = data.Spell_7_11, Level = 7 }, new DnDSpell() { Name = data.Spell_7_12, Level = 7 },
                            new DnDSpell() { Name = data.Spell_7_13, Level = 7 },
                            // Level 8
                            new DnDSpell() { Name = data.Spell_8_1, Level = 8 }, new DnDSpell() { Name = data.Spell_8_2, Level = 8 }, new DnDSpell() { Name = data.Spell_8_3, Level = 8 },
                            new DnDSpell() { Name = data.Spell_8_4, Level = 8 }, new DnDSpell() { Name = data.Spell_8_5, Level = 8 }, new DnDSpell() { Name = data.Spell_8_6, Level = 8 },
                            new DnDSpell() { Name = data.Spell_8_7, Level = 8 }, new DnDSpell() { Name = data.Spell_8_8, Level = 8 }, new DnDSpell() { Name = data.Spell_8_9, Level = 8 },
                            new DnDSpell() { Name = data.Spell_8_10, Level = 8 }, new DnDSpell() { Name = data.Spell_8_11, Level = 8 }, new DnDSpell() { Name = data.Spell_8_12, Level = 8 },
                            new DnDSpell() { Name = data.Spell_8_13, Level = 8 },
                            // Level 9
                            new DnDSpell() { Name = data.Spell_9_1, Level = 9 }, new DnDSpell() { Name = data.Spell_9_2, Level = 9 }, new DnDSpell() { Name = data.Spell_9_3, Level = 9 },
                            new DnDSpell() { Name = data.Spell_9_4, Level = 9 }, new DnDSpell() { Name = data.Spell_9_5, Level = 9 }, new DnDSpell() { Name = data.Spell_9_6, Level = 9 },
                            new DnDSpell() { Name = data.Spell_9_7, Level = 9 }, new DnDSpell() { Name = data.Spell_9_8, Level = 9 }, new DnDSpell() { Name = data.Spell_9_9, Level = 9 },
                            new DnDSpell() { Name = data.Spell_9_10, Level = 9 }, new DnDSpell() { Name = data.Spell_9_11, Level = 9 }, new DnDSpell() { Name = data.Spell_9_12, Level = 9 },
                            new DnDSpell() { Name = data.Spell_9_13, Level = 9 }
                        ),
                        PreppedSpells = new List<DnDSpell>(), // ignore prepped spell data for now
                        TotalSlots = new int[]
                        {
                            ParseInt(data.Spell_1Slots), ParseInt(data.Spell_2Slots), ParseInt(data.Spell_3Slots),
                            ParseInt(data.Spell_4Slots), ParseInt(data.Spell_5Slots), ParseInt(data.Spell_6Slots),
                            ParseInt(data.Spell_7Slots), ParseInt(data.Spell_8Slots), ParseInt(data.Spell_9Slots)
                        },
                        FreeSlots = new int[] // ignore expended spell slots for now
                        {
                            ParseInt(data.Spell_1Slots), ParseInt(data.Spell_2Slots), ParseInt(data.Spell_3Slots),
                            ParseInt(data.Spell_4Slots), ParseInt(data.Spell_5Slots), ParseInt(data.Spell_6Slots),
                            ParseInt(data.Spell_7Slots), ParseInt(data.Spell_8Slots), ParseInt(data.Spell_9Slots)
                        }
                    },
                    Appearance = new DnDAppearance()
                    {
                        Image = data.CharacterPortrait,
                        Height = data.Height,
                        Weight = data.Weight,
                        Age = data.Age,
                        Gender = data.Gender,
                        Hair = data.Hair, Eyes = data.Eyes, Skin = data.Skin,
                        HairColor = data.HairColor, EyesColor = data.EyesColor, SkinColor = data.SkinColor
                    },
                    Languages = FilterStrings(
                        data.Language_1, data.Language_2, data.Language_3, data.Language_4, data.Language_5, data.Language_6, data.Language_7,
                        data.Language_8, data.Language_9, data.Language_10, data.Language_11, data.Language_12, data.Language_13, data.Language_14
                    )
                },
                Level = new DnDPlayerLevel()
                {
                    Current = ParseInt(data.Level),
                    Experience = ParseInt(data.Experience),
                    NextLevelXP = ParseInt(data.NextLevel)
                },
                ProficiencyBonus = ParseInt(data.ProficiencyBonus),
                SaveProficiencies = GetSaveProficiencies(data),
                Background = data.Background,
                Inspiration = ParseBool(data.Inspiration),
                DeathSaves = new DnDDeathSaves()
                {
                    Successes = ParseInt(data.DeathsavesSuccesses),
                    Failures = ParseInt(data.DeathsavesFailures)
                }
            };
        }

        private static int ParseInt(string s)
        {
            if (s == null)
            {
                return 0;
            }
            int.TryParse(s.StripNonNumeric(), out int result);
            return result;
        }

        private static bool ParseBool(string s)
        {
            if (s == null || s == "0")
            {
                return false;
            }
            if (s == "1")
            {
                return true;
            }
            bool.TryParse(s, out bool result);
            return result;
        }

        private static List<DnDItem> FilterItems(params DnDItem[] items)
        {
            return items.Where((item) => !string.IsNullOrEmpty(item.Name)).ToList();
        }

        private static List<DnDSpell> FilterSpells(params DnDSpell[] items)
        {
            return items.Where((spell) => !string.IsNullOrEmpty(spell.Name)).ToList();
        }

        private static List<string> FilterStrings(params string[] strings)
        {
            return strings.Where((s) => !string.IsNullOrEmpty(s)).ToList();
        }

        private static DnDAbilityScores? ParseAbility(string s)
        {
            if (s == null)
            {
                return null;
            }
            s = s.ToLowerInvariant().Trim().Substring(0, 3);
            switch (s)
            {
                case "str": return DnDAbilityScores.Strength;
                case "dex": return DnDAbilityScores.Dexterity;
                case "con": return DnDAbilityScores.Constitution;
                case "int": return DnDAbilityScores.Intelligence;
                case "wis": return DnDAbilityScores.Wisdom;
                case "cha": return DnDAbilityScores.Charisma;
            }
            return null;
        }

        private static List<DnDAbilityScores> GetSaveProficiencies(DnDMWData data)
        {
            List<DnDAbilityScores> proficiencies = new List<DnDAbilityScores>();
            if (ParseBool(data.StrengthSaveCc))
            {
                proficiencies.Add(DnDAbilityScores.Strength);
            }
            if (ParseBool(data.DexteritySaveCc))
            {
                proficiencies.Add(DnDAbilityScores.Dexterity);
            }
            if (ParseBool(data.ConstitutionSaveCc))
            {
                proficiencies.Add(DnDAbilityScores.Constitution);
            }
            if (ParseBool(data.IntimidationCc))
            {
                proficiencies.Add(DnDAbilityScores.Intelligence);
            }
            if (ParseBool(data.WisdomSaveCc))
            {
                proficiencies.Add(DnDAbilityScores.Wisdom);
            }
            if (ParseBool(data.CharismaSaveCc))
            {
                proficiencies.Add(DnDAbilityScores.Charisma);
            }
            return proficiencies;
        }

        private static List<DnDCharacterSkills> GetSkillProficiencies(DnDMWData data)
        {
            List<DnDCharacterSkills> proficiencies = new List<DnDCharacterSkills>();
            if (ParseBool(data.AthleticsCc))
            {
                proficiencies.Add(DnDCharacterSkills.Athletics);
            }
            if (ParseBool(data.AcrobaticsCc))
            {
                proficiencies.Add(DnDCharacterSkills.Acrobatics);
            }
            if (ParseBool(data.SleightOfHandCc))
            {
                proficiencies.Add(DnDCharacterSkills.SleightOfHand);
            }
            if (ParseBool(data.StealthCc))
            {
                proficiencies.Add(DnDCharacterSkills.Stealth);
            }
            if (ParseBool(data.ArcanaCc))
            {
                proficiencies.Add(DnDCharacterSkills.Arcana);
            }
            if (ParseBool(data.HistoryCc))
            {
                proficiencies.Add(DnDCharacterSkills.History);
            }
            if (ParseBool(data.InvestigationCc))
            {
                proficiencies.Add(DnDCharacterSkills.Investigation);
            }
            if (ParseBool(data.NatureCc))
            {
                proficiencies.Add(DnDCharacterSkills.Nature);
            }
            if (ParseBool(data.ReligionCc))
            {
                proficiencies.Add(DnDCharacterSkills.Religion);
            }
            if (ParseBool(data.AnimalHandlingCc))
            {
                proficiencies.Add(DnDCharacterSkills.AnimalHandling);
            }
            if (ParseBool(data.InsightCc))
            {
                proficiencies.Add(DnDCharacterSkills.Insight);
            }
            if (ParseBool(data.MedicineCc))
            {
                proficiencies.Add(DnDCharacterSkills.Medicine);
            }
            if (ParseBool(data.PerceptionCc))
            {
                proficiencies.Add(DnDCharacterSkills.Perception);
            }
            if (ParseBool(data.SurvivalCc))
            {
                proficiencies.Add(DnDCharacterSkills.Survival);
            }
            if (ParseBool(data.DeceptionCc))
            {
                proficiencies.Add(DnDCharacterSkills.Deception);
            }
            if (ParseBool(data.IntimidationCc))
            {
                proficiencies.Add(DnDCharacterSkills.Intimidation);
            }
            if (ParseBool(data.PerformanceCc))
            {
                proficiencies.Add(DnDCharacterSkills.Performance);
            }
            if (ParseBool(data.PersuasionCc))
            {
                proficiencies.Add(DnDCharacterSkills.Persuasion);
            }
            return proficiencies;
        }

        private static List<DnDItem> EmptyItems => new List<DnDItem>();

        private static List<DnDItem> UnknownHeavyItem => new List<DnDItem>() { new DnDItem() { Name = "UNKNOWN HEAVY ARMOR", HeavyArmor = true } };
    }
}
