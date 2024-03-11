using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class UrbanDictionary : ModuleBase<SocketCommandContext>
    {
        public static readonly string URBAN_API = "http://api.urbandictionary.com/v0/";

        public static readonly string DEFINE_TERM = URBAN_API + "define?term=";

        public static readonly string DEFINE_ID = URBAN_API + "define?defid=";

        public static readonly string RANDOM = URBAN_API + "random";

        private static readonly HttpClient Http = new HttpClient();

        [Command("urban")]
        public async Task BasicSearch([Remainder] string input)
        {
            input = input.Trim();
            string lower = input.ToLower();
            UrbanResponse response;
            if (lower == "random")
            {
                response = await Random();
            }
            else
            {
                response = await Search(input);
            }
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
                UrbanDefinition definition = response.list[0];
                string defString = definition.definition.Replace("\n\n", " ").Replace("\n", " ").Replace("\r", "");
                string exampleString = definition.example.Replace("\n\n", " ").Replace("\n", " ").Replace("\r", "");
                await ReplyAsync(definition.word + " (" + definition.defid + "): " + defString);
                await ReplyAsync("Example: " + exampleString);
            }
        }

        public static async Task<UrbanResponse> GetUrbanResponse(string url)
        {
            HttpResponseMessage response = await Http.GetAsync(url);
            return await Utilities.GetObjectFromWebResponse<UrbanResponse>(response);
        }

        public static async Task<UrbanResponse> Random()
        {
            return await GetUrbanResponse(RANDOM);
        }

        public static async Task<UrbanResponse> Search(string input)
        {
            return await GetUrbanResponse(DEFINE_TERM + Uri.EscapeDataString(input));
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
