using Discord;
using Discord.Commands;
using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MorphanBotNetCore.Games.DnD
{
    [DontAutoLoad]
    public class DnDCommands : ModuleBase<SocketCommandContext>
    {
        public GameManager Games { get; set; }

        public DnDGame CurrentGame => (DnDGame)Games.CurrentGame;

        [Command("setdm")]
        public async Task SetDM(IUser user)
        {
            if (CurrentGame.GameData.DungeonMaster != 0UL && CurrentGame.GameData.DungeonMaster != user.Id)
            {
                await ReplyAsync("This campaign already has a Dungeon Master. Only they can set a new DM.");
                return;
            }
            CurrentGame.GameData.DungeonMaster = user.Id;
            await ReplyAsync(user.Mention + " is now the Dungeon Master of campaign: " + CurrentGame.GameData.Title);
            CurrentGame.Save(Games.Storage);
        }

        [Command("newpc")]
        public async Task NewPlayer(string race, [Remainder] string name)
        {
            if (CurrentGame.GetPlayer(Games.Storage, name) != null)
            {
                await ReplyAsync("A player already exists with that name!");
                return;
            }
            DnDPlayerCharacter currentPC = CurrentGame.GetPlayer(Context.User.Id);
            DnDPlayerCharacter newPC = CurrentGame.CreatePlayer(race, name);
            if (currentPC == null)
            {
                newPC.ControlledBy = Context.User.Id;
                await ReplyAsync("Player created and is now controlled by you, as you have no active character. Joining current campaign " + CurrentGame.Data.Title + ".");
                CurrentGame.AddPlayer(newPC);
                CurrentGame.Save(Games.Storage);
            }
            else if (!currentPC.Alive)
            {
                currentPC.ControlledBy = 0UL;
                newPC.ControlledBy = Context.User.Id;
                await ReplyAsync("Player created and is now controlled by you, as your old one is dead. Nice going! Replacing " + currentPC.Name + " in current campaign " + CurrentGame.Data.Title + ".");
                CurrentGame.RemovePlayer(currentPC);
                CurrentGame.AddPlayer(newPC);
                CurrentGame.Save(Games.Storage);
            }
            else
            {
                CurrentGame.AddPlayer(newPC);
                CurrentGame.Save(Games.Storage);
                CurrentGame.RemovePlayer(newPC);
                await ReplyAsync("Player created. It has no active controller because you have an active character. To take control, use /takepc " + name + "\n"
                               + "You can also use /takepc " + name.ToLowerInvariant().StripNonAlphaNumeric());
            }
        }

        [Command("givepc")]
        public async Task GivePlayer(IUser user, [Remainder] string name)
        {
            DnDPlayerCharacter player = CurrentGame.GetPlayer(Games.Storage, name);
            if (player == null)
            {
                await ReplyAsync("No player exists with that name!");
            }
            else if (player.ControlledBy != 0UL && CurrentGame.GameData.DungeonMaster != Context.User.Id && player.ControlledBy != Context.User.Id)
            {
                await ReplyAsync("That player already has a controller!" + (player.ControlledBy == user.Id ? " (It's them.)" : ""));
            }
            else
            {
                DnDPlayerCharacter old = CurrentGame.GetPlayer(user.Id);
                string joining = "Joining";
                if (old != null)
                {
                    joining = "Replacing " + old.Name + " in";
                    CurrentGame.RemovePlayer(old);
                }
                player.ControlledBy = user.Id;
                await ReplyAsync(user.Mention + " now controls " + player.Name + ". " + joining + " current campaign " + CurrentGame.Data.Title + ".");
                CurrentGame.AddPlayer(player);
                CurrentGame.Save(Games.Storage);
            }
        }

        [Command("takepc")]
        public async Task TakePlayer([Remainder] string name)
        {
            DnDPlayerCharacter player = CurrentGame.GetPlayer(Games.Storage, name);
            if (player == null)
            {
                await ReplyAsync("No player exists with that name!");
            }
            else if (player.ControlledBy != 0UL)
            {
                await ReplyAsync("That player already has a controller!" + (player.ControlledBy == Context.User.Id ? " (It's you.)" : ""));
            }
            else
            {
                DnDPlayerCharacter old = CurrentGame.GetPlayer(Context.User.Id);
                string joining = "Joining"; 
                if (old != null)
                {
                    joining = "Replacing " + old.Name + " in";
                    CurrentGame.RemovePlayer(old);
                }
                player.ControlledBy = Context.User.Id;
                await ReplyAsync("You now control " + player.Name + ". " + joining + " current campaign " + CurrentGame.Data.Title + ".");
                CurrentGame.AddPlayer(player);
                CurrentGame.Save(Games.Storage);
            }
        }

        [Command("importpc")]
        public async Task ImportPlayer([Remainder] string url)
        {
            Match match = Regex.Match(url, @".*myth-weavers.com/sheet.html#id=(\d+)");
            if (!match.Success)
            {
                await ReplyAsync("Provided URL is not a MythWeavers sheet.");
                return;
            }
            DnDMythWeavers mw;
            WebRequest wr = WebRequest.Create("https://www.myth-weavers.com/api/v1/sheets/sheets/" + match.Groups[1].Value);
            using (WebResponse response = wr.GetResponse())
            using (Stream stream = response.GetResponseStream())
            {
                mw = Games.Storage.Load<DnDMythWeavers>(stream);
            }
            if (mw.Error || mw.Sheetdata.Private == 1)
            {
                await ReplyAsync("Error retrieving data. Is the sheet private?");
                return;
            }
            string name = mw.Sheetdata.Data.Name;
            string internalName = name.ToLowerInvariant().StripNonAlphaNumeric();
            Games.Storage.Write("data/mythweavers/" + internalName, mw);
            DnDPlayerCharacter newPC = DnDConvert.ConvertPlayer(mw.Sheetdata.Data);
            CurrentGame.AddPlayer(newPC);
            CurrentGame.Save(Games.Storage);
            CurrentGame.RemovePlayer(newPC);
            CurrentGame.Save(Games.Storage);
            await ReplyAsync("Player imported. It has no active controller because it might contain flaws and should be manually edited before taking control. " +
                "To take control, use /takepc " + name + "\nYou can also use /takepc " + name.ToLowerInvariant().StripNonAlphaNumeric());
        }

        [Command("pcinfo")]
        [Alias("infopc")]
        public async Task PlayerInfo([Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name, false);
            if (player != null)
            {
                await ReplyAsync(embed: player.CreateInfoEmbed());
            }
        }

        [Command("setstats")]
        public async Task SetStats(int strength, int dexterity, int constitution, int intelligence, int wisdom, int charisma, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                basicInfo.Strength = strength;
                basicInfo.Dexterity = dexterity;
                basicInfo.Constitution = constitution;
                basicInfo.Intelligence = intelligence;
                basicInfo.Wisdom = wisdom;
                basicInfo.Charisma = charisma;
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully modified stats.");
            }
        }

        [Command("addamod")]
        public async Task AddAbilityMod(string ability, int mod, string source, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDAbilityScores abilityScore = GetAbility(ability);
                if ((int)abilityScore == -1)
                {
                    await ReplyAsync("Invalid ability score! Valid: STR, DEX, CON, INT, WIS, CHA");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                basicInfo.AbilityMods.Add(new DnDAbilityModDescriptor()
                {
                    SourceDescription = source,
                    AbilityMod = new DnDAbilityModifier()
                    {
                        Ability = abilityScore,
                        Modifier = mod
                    }
                });
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully added ability modifier.");
            }
        }

        [Command("listamods")]
        [Alias("listamod")]
        public async Task ListAbilityMod(string ability = null, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                EmbedBuilder builder = new EmbedBuilder();
                DnDAbilityScores abilityScore = GetAbility(ability);
                if ((int)abilityScore == -1)
                {
                    builder.WithTitle("List of all active ability modifiers");
                    int i = 0;
                    DnDAbilityScores lastAbility = DnDAbilityScores.Strength;
                    foreach (DnDAbilityModDescriptor descriptor in player.BasicInfo.AbilityMods.OrderBy((amd) => amd.AbilityMod.Ability))
                    {
                        if (descriptor.AbilityMod.Ability != lastAbility)
                        {
                            i = 0;
                            lastAbility = descriptor.AbilityMod.Ability;
                        }
                        builder.Description += lastAbility.ToString().ToUpper().Substring(0, 3) + " " + (i + 1) + ": "
                                            + descriptor.SourceDescription + " (" + descriptor.AbilityMod.Modifier.ShowSign() + ")\n";
                        i++;
                    }
                }
                else
                {
                    builder.WithTitle("List of active " + abilityScore.ToString().ToLowerInvariant() + " modifiers");
                    int i = 0;
                    foreach (DnDAbilityModDescriptor descriptor in player.BasicInfo.AbilityMods.Where((amd) => amd.AbilityMod.Ability == abilityScore))
                    {
                        builder.Description += (i + 1) + ": " + descriptor.SourceDescription + " (" + descriptor.AbilityMod.Modifier.ShowSign() + ")\n";
                        i++;
                    }
                }
                if (string.IsNullOrEmpty(builder.Description))
                {
                    builder.Description = "None";
                }
                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("delamod")]
        [Alias("remamod")]
        public async Task DeleteAbilityMod(string ability, int num, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDAbilityScores abilityScore = GetAbility(ability);
                if ((int)abilityScore == -1)
                {
                    await ReplyAsync("Invalid ability score! Valid: STR, DEX, CON, INT, WIS, CHA");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                DnDAbilityModDescriptor match = basicInfo.AbilityMods.Where((amd) => amd.AbilityMod.Ability == abilityScore).ElementAtOrDefault(num - 1);
                if (match.Equals(default(DnDAbilityModDescriptor)))
                {
                    await ReplyAsync("No valid modifier found. Must be a number from .listamods " + ability);
                    return;
                }
                basicInfo.AbilityMods.Remove(match);
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully deleted ability modifier.");
            }
        }

        [Command("addsprof")]
        public async Task AddSkillProficiency(string skillName, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDCharacterSkills skill = GetSkill(skillName);
                if ((int)skill == -1)
                {
                    await ReplyAsync("Invalid skill! Valid: athl, acro, slei, stea, arca, hist, inve, natu, reli, anim, insi, medi, perc, surv, dece, inti, perf, pers");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                if (basicInfo.SkillProficiencies.Contains(skill))
                {
                    await ReplyAsync(player.Name + " already has a proficiency in " + skill.ToString().ToLowerInvariant() + "!");
                    return;
                }
                basicInfo.SkillProficiencies.Add(skill);
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully added skill proficiency.");
            }
        }

        [Command("listsprofs")]
        [Alias("listsprof")]
        public async Task ListSkillProficiencies([Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                string skills = Utilities.Join(player.BasicInfo.SkillProficiencies.Select((skill) => skill.ToString().ToLowerInvariant()).ToArray(), ", ");
                if (string.IsNullOrEmpty(skills))
                {
                    skills = "None";
                }
                await ReplyAsync(player.Name + " is proficient in the following skills: " + skills);
            }
        }

        [Command("delsprof")]
        [Alias("remsprof")]
        public async Task DeleteSkillProficiency(string skillName, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDCharacterSkills skill = GetSkill(skillName);
                if ((int)skill == -1)
                {
                    await ReplyAsync("Invalid skill! Valid: athl, acro, slei, stea, arca, hist, inve, natu, reli, anim, insi, medi, perc, surv, dece, inti, perf, pers");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                if (!basicInfo.SkillProficiencies.Contains(skill))
                {
                    await ReplyAsync(player.Name + " does not have a proficiency in " + skill.ToString().ToLowerInvariant() + "!");
                    return;
                }
                basicInfo.SkillProficiencies.Remove(skill);
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully deleted skill proficiency.");
            }
        }

        [Command("addsmod")]
        public async Task AddSkillMod(string skillName, int mod, string source, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDCharacterSkills skill = GetSkill(skillName);
                if ((int)skill == -1)
                {
                    await ReplyAsync("Invalid skill! Valid: athl, acro, slei, stea, arca, hist, inve, natu, reli, anim, insi, medi, perc, surv, dece, inti, perf, pers");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                basicInfo.SkillMods.Add(new DnDSkillModDescriptor()
                {
                    SourceDescription = source,
                    SkillMod = new DnDSkillModifier()
                    {
                        Skill = skill,
                        Modifier = mod
                    }
                });
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully added skill modifier.");
            }
        }

        [Command("listsmods")]
        [Alias("listsmod")]
        public async Task ListSkillMod(string skillName = null, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                EmbedBuilder builder = new EmbedBuilder();
                DnDCharacterSkills skill = GetSkill(skillName);
                if ((int)skill == -1)
                {
                    builder.WithTitle("List of all active skill modifiers");
                    int i = 0;
                    DnDCharacterSkills lastSkill = DnDCharacterSkills.Athletics;
                    foreach (DnDSkillModDescriptor descriptor in player.BasicInfo.SkillMods.OrderBy((smd) => smd.SkillMod.Skill))
                    {
                        if (descriptor.SkillMod.Skill != lastSkill)
                        {
                            i = 0;
                            lastSkill = descriptor.SkillMod.Skill;
                        }
                        builder.Description += lastSkill.ToString().ToUpper().Substring(0, 4) + " " + (i + 1) + ": "
                                            + descriptor.SourceDescription + " (" + descriptor.SkillMod.Modifier.ShowSign() + ")\n";
                        i++;
                    }
                }
                else
                {
                    builder.WithTitle("List of active " + skill.ToString().ToLowerInvariant() + " modifiers");
                    int i = 0;
                    foreach (DnDSkillModDescriptor descriptor in player.BasicInfo.SkillMods.Where((smd) => smd.SkillMod.Skill == skill))
                    {
                        builder.Description += (i + 1) + ": " + descriptor.SourceDescription + " (" + descriptor.SkillMod.Modifier.ShowSign() + ")\n";
                        i++;
                    }
                }
                if (string.IsNullOrEmpty(builder.Description))
                {
                    builder.Description = "None";
                }
                await ReplyAsync(embed: builder.Build());
            }
        }

        [Command("delsmod")]
        [Alias("remsmod")]
        public async Task DeleteSkillMod(string skillName, int num, [Remainder] string name = null)
        {
            DnDPlayerCharacter player = await GetPlayerOrCurrent(name);
            if (player != null)
            {
                DnDCharacterSkills skill = GetSkill(skillName);
                if ((int)skill == -1)
                {
                    await ReplyAsync("Invalid skill! Valid: athl, acro, slei, stea, arca, hist, inve, natu, reli, anim, insi, medi, perc, surv, dece, inti, perf, pers");
                    return;
                }
                DnDBasicCharacterInfo basicInfo = player.BasicInfo;
                DnDSkillModDescriptor match = basicInfo.SkillMods.Where((smd) => smd.SkillMod.Skill == skill).ElementAtOrDefault(num - 1);
                if (match.Equals(default(DnDSkillModDescriptor)))
                {
                    await ReplyAsync("No valid modifier found. Must be a number from .listsmods " + skillName);
                    return;
                }
                basicInfo.SkillMods.Remove(match);
                player.BasicInfo = basicInfo;
                await ReplyAsync("Successfully deleted skill modifier.");
            }
        }

        private static DnDAbilityScores GetAbility(string ability)
        {
            if (ability == null)
            {
                return (DnDAbilityScores)(-1);
            }
            ability = ability.ToLowerInvariant();
            foreach (DnDAbilityScores abilityScore in Utilities.GetEnumValues<DnDAbilityScores>())
            {
                if (ability.StartsWith(abilityScore.ToString().Substring(0, 3).ToLowerInvariant()))
                {
                    return abilityScore;
                }
            }
            return (DnDAbilityScores)(-1);
        }

        private static DnDCharacterSkills GetSkill(string skillName)
        {
            if (skillName == null)
            {
                return (DnDCharacterSkills)(-1);
            }
            skillName = skillName.ToLowerInvariant();
            foreach (DnDCharacterSkills skill in Utilities.GetEnumValues<DnDCharacterSkills>())
            {
                if (skillName.StartsWith(skill.ToString().Substring(0, 4).ToLowerInvariant()))
                {
                    return skill;
                }
            }
            return (DnDCharacterSkills)(-1);
        }

        private async Task<DnDPlayerCharacter> GetPlayerOrCurrent(string name, bool modifying = true)
        {
            DnDPlayerCharacter player;
            if (name == null)
            {
                player = CurrentGame.GetPlayer(Context.User.Id);
                if (player == null)
                {
                    await ReplyAsync("You don't have an active character!");
                }
            }
            else
            {
                player = CurrentGame.GetPlayer(Games.Storage, name.ToLowerInvariant().StripNonAlphaNumeric());
                if (player == null)
                {
                    await ReplyAsync("No player character exists with that name!");
                }
                else if (modifying && player.ControlledBy != Context.User.Id && CurrentGame.GameData.DungeonMaster != Context.User.Id)
                {
                    await ReplyAsync("You don't have permission to modify that character!");
                }
            }
            return player;
        }
    }
}
