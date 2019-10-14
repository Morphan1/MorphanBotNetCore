using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public struct DnDSpellBook
    {
        public List<DnDSpell> PreppedSpells { get; set; }

        public List<DnDSpell> Spells { get; set; }

        public int[] TotalSlots { get; set; }

        public int[] FreeSlots { get; set; }
    }

    public struct DnDSpell
    {
        public string Name { get; set; }

        public int Level { get; set; }
    }
}
