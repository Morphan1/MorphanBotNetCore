using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class UrbanDictionary : ModuleBase<SocketCommandContext>
    {
        public static readonly string URBAN_URL = "http://api.urbandictionary.com/v0/define?term=";

        [Command("urban")]
        public async Task BasicSearch([Remainder] string input)
        {
            UrbanResponse response = Search(input);
            if (response == null)
            {
                await ReplyAsync("Error! Response not found.");
            }
            else if (response.result_type == "no_results" || response.list.Count == 0)
            {
                await ReplyAsync("No search results found.");
            }
            else
            {
                int num = Utilities.random.Next(response.list.Count);
                UrbanDefinition definition = response.list[num];
                string defString = definition.definition.Replace("\n\n", " ").Replace("\n", " ").Replace("\r", "");
                string exampleString = definition.example.Replace("\n\n", " ").Replace("\n", " ").Replace("\r", "");
                await ReplyAsync(definition.word + " (" + definition.defid + "): " + defString);
                await ReplyAsync("Example: " + exampleString);
            }
        }

        public static UrbanResponse Search(string input)
        {
            string url = URBAN_URL + Uri.EscapeDataString(input);
            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            return Utilities.GetObjectFromWebResponse<UrbanResponse>((HttpWebResponse)wr.GetResponse());
        }
    }

    [DataContract]
    public class UrbanResponse
    {
        [DataMember] public string result_type;
        [DataMember] public List<UrbanDefinition> list;
    }

    [DataContract]
    public class UrbanDefinition
    {
        [DataMember] public string defid;
        [DataMember] public string word;
        [DataMember] public string author;
        [DataMember] public string definition;
        [DataMember] public string example;
    }
}
