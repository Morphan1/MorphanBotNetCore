using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore.Games
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        public GameManager Games { get; set; }

        [Command("savegames")]
        public async Task SaveGames()
        {
            await ReplyAsync("Saving...");
            Games.Save();
            await ReplyAsync("Done.");
        }

        [Command("gameinfo")]
        public async Task GameInfo([Remainder] string title = null)
        {
            if (title == null)
            {
                if (Games.CurrentGame != null)
                {
                    await ReplyAsync(embed: Games.CurrentGame.CreateInfoEmbed());
                }
                else
                {
                    await ReplyAsync($"No game is currently running.");
                }
                return;
            }
            title = title.ToLowerInvariant().StripNonAlphaNumeric();
            if (Games.ExistingGames.TryGetValue(title, out IGame game))
            {
                await ReplyAsync(embed: game.CreateInfoEmbed());
            }
            else
            {
                await ReplyAsync($"No existing game was found with the title: {title}");
            }
        }

        [Command("newgame")]
        public async Task NewGame(string gameName, [Remainder] string title)
        {
            gameName = gameName.ToLowerInvariant();
            string internalTitle = title.ToLowerInvariant().StripNonAlphaNumeric();
            if (Games.ExistingGames.ContainsKey(internalTitle))
            {
                await ReplyAsync($"A game already exists with the title: {internalTitle}");
            }
            else if (Games.GameFactories.TryCreate(gameName, out IGame game))
            {
                game.Init(title, internalTitle);
                Games.ExistingGames.Add(internalTitle, game);
                await ReplyAsync($"Game created:");
                await ReplyAsync(embed: game.CreateInfoEmbed());
                await ReplyAsync($"To set this as your current game, use /loadgame " + title + "\nAlternatively, you can use /loadgame " + internalTitle);
            }
            else
            {
                await ReplyAsync($"No game found with the name: {gameName}");
            }
        }

        [Command("loadgame")]
        public async Task LoadGame([Remainder] string title)
        {
            title = title.ToLowerInvariant().StripNonAlphaNumeric();
            if (Games.ExistingGames.TryGetValue(title, out IGame game))
            {
                await ReplyAsync("Loaded " + game.FullName + " game " + game.Data.Title + ".");
                Games.SetGame(game);
            }
            else
            {
                await ReplyAsync($"No existing game was found with the title: {title}");
            }
        }
    }
}
