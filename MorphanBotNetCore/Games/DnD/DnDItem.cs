using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public struct DnDItem
    {
        public string Name { get; set; }

        public int ArmorClass { get; set; }

        public bool HeavyArmor { get; set; }

        public int AttackBonus { get; set; }

        public string Damage { get; set; }

        public int Weight { get; set; }

        public string Worth { get; set; }

        public string Location { get; set; }
    }
}
