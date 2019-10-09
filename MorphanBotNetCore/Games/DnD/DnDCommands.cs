using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
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
                await ReplyAsync("Player created. It has no active controller because you have an active character. To take control, use /takepc " + name + "\n"
                               + "You can also use /takepc " + name.ToLowerInvariant().StripNonAlphaNumeric());
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
    }
}
