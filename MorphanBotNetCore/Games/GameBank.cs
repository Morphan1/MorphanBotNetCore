using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games
{
    public class GameBank
    {
        public List<GameItem> Games { get; set; } = new List<GameItem>();
    }

    public class GameItem
    {
        public string Game { get; set; }

        public string InternalTitle { get; set; }

        public DateTime LastActivity { get; set; }
    }
}
