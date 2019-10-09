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

    public class CommonPlayerData
    {
        public string Name { get; set; }

        public ulong ControlledBy { get; set; }
    }
}
