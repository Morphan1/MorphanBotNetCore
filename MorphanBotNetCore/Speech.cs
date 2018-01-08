using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class Speech : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        public async Task BasicTTS([Remainder] string text)
        {
            await ReplyAsync(text, true);
        }
    }
}
