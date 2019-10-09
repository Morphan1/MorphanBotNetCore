using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDCampaignData : CommonGameData
    {
        public ulong DungeonMaster { get; set; }

        [FileReference(List = true, Folder = DnDGame.CharactersFolder)]
        public List<DnDPlayerCharacter> Players { get; set; } = new List<DnDPlayerCharacter>();
    }
}
