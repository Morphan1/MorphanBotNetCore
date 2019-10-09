using MorphanBotNetCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDPlayerCharacter : CommonPlayerData, IFileReferenceable
    {
        public string Race { get; set; }

        public bool Alive { get; set; }

        public string FilePath;

        public DnDPlayerCharacter(string filePath)
        {
            FilePath = filePath;
        }

        public string GetFilePath()
        {
            return FilePath;
        }
    }
}
