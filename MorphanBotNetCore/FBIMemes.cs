using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class FBIMemes : ModuleBase<SocketCommandContext>
    {
        private const string FBIMemeFolder = "data/fbimemes/";

        [Command("fbi")]
        public async Task CallTheFBI()
        {
            string[] files = Directory.GetFiles(FBIMemeFolder);
            await Context.Channel.SendFileAsync(files[Utilities.random.Next(files.Length)]);
        }
    }
}
