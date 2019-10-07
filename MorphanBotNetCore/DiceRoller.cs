using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class DiceRoller : ModuleBase<SocketCommandContext>
    {
        [Command("roll")]
        public async Task RollDice([Remainder] string input)
        {
            input = input.ToLowerInvariant();
            Match match = Regex.Match(input, @"(\d+)?d(\d+)(d(\d+))?");
            if (match.Success)
            {
                while (match.Success)
                {
                    int dice = 1;
                    List<int> lowest = new List<int>();
                    int lowestCount = 0;
                    if (match.Groups[1].Success)
                    {
                        dice = Utilities.StringToInt(match.Groups[1].Value);
                    }
                    if (match.Groups[4].Success)
                    {
                        lowestCount = Utilities.StringToInt(match.Groups[4].Value);
                    }
                    int sides = Utilities.StringToInt(match.Groups[2].Value);
                    StringBuilder sb = new StringBuilder();
                    if (dice > 1 || lowestCount > 0)
                    {
                        sb.Append("(");
                    }
                    for (int i = 0; i < dice; i++)
                    {
                        int roll = Utilities.random.Next(1, sides + 1);
                        sb.Append(roll).Append(" + ");
                        if (lowest.Count < lowestCount)
                        {
                            lowest.Add(roll);
                        }
                        else
                        {
                            for (int x = 0; x < lowestCount; x++)
                            {
                                if (roll < lowest[x])
                                {
                                    lowest[x] = roll;
                                    break;
                                }
                            }
                        }
                    }
                    sb.Remove(sb.Length - 3, 3);
                    foreach (int low in lowest)
                    {
                        if (low == 0)
                        {
                            break;
                        }
                        sb.Append(" - ").Append(low);
                    }
                    if (dice > 1 || lowestCount > 0)
                    {
                        sb.Append(")");
                    }
                    string final = sb.Length == 0 ? "0" : sb.ToString();
                    input = input.Replace(match.Index, match.Length, final);
                    match = Regex.Match(input, @"(\d+)?d(\d+)");
                }
                List<MathOperation> calc = MonkeyMath.Parse(input, out string err);
                if (err != null)
                {
                    await ReplyAsync("Failed: " + err);
                    return;
                }
                if (!MonkeyMath.Verify(calc, MonkeyMath.BaseFunctions, out err))
                {
                    await ReplyAsync("Failed to verify: " + err);
                    return;
                }
                double total = await MonkeyMath.CalculateAsync(calc, MonkeyMath.BaseFunctions);
                await ReplyAsync("You rolled: " + input);
                await ReplyAsync("Total roll: " + total);
            }
            else
            {
                await ReplyAsync("You must specify at least set of dice to roll!");
            }
        }
    }
}
