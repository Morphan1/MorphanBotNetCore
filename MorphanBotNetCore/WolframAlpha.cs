using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace MorphanBotNetCore
{
    public class WolframAlpha : ModuleBase<SocketCommandContext>
    {
        public static readonly string WOLFRAM_URL = "http://api.wolframalpha.com/v2/";

        private static readonly HttpClient Http = new HttpClient();

        public static string AppID;

        [Command("search")]
        [Alias("wolfram", "wolf", "wa")]
        public async Task BasicSearch([Remainder] string input)
        {
            QueryResult output = await Query(input);
            string result = output.Result;
            if (output.Error || !output.Success || result == null)
            {
                if (output.Suggestion != null)
                {
                    await ReplyAsync("Sorry, I don't know the meaning of that. Did you mean '" + output.Suggestion + "'?");
                }
                else
                {
                    await ReplyAsync("There was an error while parsing that statement.");
                }
            }
            else
            {
                if (output.SpellCheck != null)
                {
                    await ReplyAsync(output.SpellCheck);
                }
                await ReplyAsync(output.Input + " = " + output.Result);
            }
        }

        public static async Task<QueryResult> Query(string input)
        {
            string url = WOLFRAM_URL + "query?input=" + Uri.EscapeDataString(input) + "&appid=" + AppID + "&format=plaintext";
            HttpResponseMessage response = await Http.GetAsync(url);
            XmlDocument doc = new XmlDocument();
            doc.Load(await response.Content.ReadAsStreamAsync());
            return new QueryResult(doc);
        }

        public class QueryResult
        {
            public bool Success;
            public bool Error;
            public string SpellCheck;
            public string Input;
            public string PodID;
            public string Result;
            public string Suggestion;

            private static readonly Regex DECODING_REGEX = new Regex(@"\:(?<Value>[a-fA-F0-9]{4})", RegexOptions.Compiled);

            private static readonly string[] equals = new string[]
            {
                "Substitution", "UnitSystem", "Encodings"
            };

            private static readonly string[] startswith = new string[]
            {
                "Identification", "CityLocation", "Definition", "BasicInformation", "Taxonomy", "BasicProperties",
                "PhysicalCharacteristics", "TranslationsToEnglish", "HostInformationPodIP", "FlightStatus", "Area"
            };

            public QueryResult(XmlDocument document)
            {
                Success = document.DocumentElement.GetAttribute("success").StartsWith("t");
                Error = document.DocumentElement.GetAttribute("error").StartsWith("t");
                XmlNodeList warnings = document.DocumentElement.SelectNodes("warnings");
                if (warnings.Count > 0)
                {
                    XmlNodeList spellcheck = warnings[0].SelectNodes("spellcheck");
                    SpellCheck = spellcheck.Count > 0 ? spellcheck[0].Attributes["text"].Value.Replace("&quot;", "'") : null;
                }
                XmlNode resultPod = null;
                foreach (XmlNode pod in document.DocumentElement.SelectNodes("pod"))
                {
                    string podId = pod.Attributes["id"].Value;
                    switch (podId)
                    {
                        case "Input":
                            Input = pod.SelectSingleNode("subpod").SelectSingleNode("plaintext").InnerText;
                            break;
                        case "Result":
                            resultPod = pod;
                            break;
                        default:
                            if (CheckForResult(pod))
                            {
                                PodID = podId;
                                SetResult(pod);
                                return;
                            }
                            break;
                    }
                }
                if (resultPod != null)
                {
                    PodID = "Result";
                    SetResult(resultPod);
                    return;
                }
                XmlNodeList futures = document.DocumentElement.SelectNodes("futuretopic");
                if (futures.Count > 0)
                {
                    XmlNode future = futures[0];
                    Result = future.Attributes["topic"].Value + " = " + future.Attributes["msg"].Value;
                    return;
                }
                XmlNodeList suggestions = document.DocumentElement.SelectNodes("didyoumeans");
                if (suggestions.Count > 0)
                {
                    XmlNodeList didyoumeans = suggestions[0].SelectNodes("didyoumean");
                    if (didyoumeans.Count > 0)
                    {
                        Suggestion = didyoumeans[0].InnerText;
                        return;
                    }
                }
            }

            private static bool CheckForResult(XmlNode pod)
            {
                if (pod.Attributes["primary"] != null)
                {
                    return true;
                }
                string podId = pod.Attributes["id"].Value;
                if (equals.Contains(podId))
                {
                    return true;
                }
                if (podId.Contains(':'))
                {
                    foreach (string s in startswith)
                    {
                        if (podId.StartsWith(s))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static string DecodeUnicode(string value)
            {
                return DECODING_REGEX.Replace(value.Replace(@"\", ""), (m) => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString()).Replace("\n", " - ");
            }

            private void SetResult(XmlNode pod)
            {
                string result = pod.SelectSingleNode("subpod").SelectSingleNode("plaintext").InnerText;
                SetResult(DecodeUnicode(result));
            }

            private void SetResult(string result)
            {
                if (PodID == "Substitution")
                {
                    foreach (string sub in Input.Replace("{", "").Replace("}", "").Split(','))
                    {
                        string substitution = sub.Trim();
                        if (!substitution.Contains('='))
                        {
                            continue;
                        }
                        string[] split = substitution.Split('=');
                        result = result.Replace(split[0].Replace(" ", ""), split[1].Replace(" ", ""));
                    }
                    if (result.Contains('='))
                    {
                        string[] split = result.Split('=');
                        Input = split[0].Trim();
                        result = result[split[0].Length..];
                    }
                }
                Result = result.Trim();
            }
        }
    }
}
