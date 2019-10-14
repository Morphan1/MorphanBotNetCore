using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games
{
    public class CommonGameData
    {
        public string Title { get; set; } = null;

        public DateTime LastActivity { get; set; } = DateTime.Now;
    }
}
