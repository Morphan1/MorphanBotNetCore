using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore.Games.DnD
{
    public class DnDMigrate
    {
        public static DnDPlayerCharacter Migrate(DnDPlayerCharacter player)
        {
            if (player == null || player.Migrated)
            {
                return player;
            }
            DnDBasicCharacterInfo basicInfo = player.BasicInfo;
            if (basicInfo.AbilityMods == null)
            {
                basicInfo.AbilityMods = new List<DnDAbilityModDescriptor>();
            }
            if (basicInfo.SkillMods == null)
            {
                basicInfo.SkillMods = new List<DnDSkillModDescriptor>();
            }
            player.BasicInfo = basicInfo;
            player.Migrated = true;
            return player;
        }
    }
}
