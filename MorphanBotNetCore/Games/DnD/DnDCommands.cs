using Discord;
using Discord.Commands;
using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.IO;
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
            DnDPlayerCharacter player;
            if (name == null)
            {
                player = CurrentGame.GetPlayer(Context.User.Id);
                if (player == null)
                {
                    await ReplyAsync("You don't have an active character! Try /pcinfo Some Name Here");
                    return;
                }
            }
            else
            {
                player = CurrentGame.GetPlayer(Games.Storage, name.ToLowerInvariant().StripNonAlphaNumeric());
                if (player == null)
                {
                    await ReplyAsync("No player character exists with that name!");
                    return;
                }
            }
            await ReplyAsync(embed: player.CreateInfoEmbed());
        }
    }
}
