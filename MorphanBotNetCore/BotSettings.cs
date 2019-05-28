using System;
using System.Collections.Generic;
using System.Text;

namespace MorphanBotNetCore
{
    /// <summary>
    /// The bot settings structure.
    /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// The Discord bot token to use.
        /// </summary>
        public string Discord { get; set; }

        /// <summary>
        /// The Wolfram app ID to use.
        /// </summary>
        public string Wolfram { get; set; }
    }
}
