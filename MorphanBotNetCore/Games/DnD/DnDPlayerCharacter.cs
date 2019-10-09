using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDPlayerCharacter : CommonPlayerData
    {
        public string Race { get; set; }

        public bool Alive { get; set; }
    }
}
