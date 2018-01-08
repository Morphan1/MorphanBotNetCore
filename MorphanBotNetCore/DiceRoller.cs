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
            Match match = Regex.Match(input, @"(\d+)?d(\d+)");
            if (match.Success)
            {
                while (match.Success)
                {
                    int dice = 1;
                    if (match.Groups[1].Success)
                    {
                        dice = Utilities.StringToInt(match.Groups[1].Value);
                    }
                    int sides = Utilities.StringToInt(match.Groups[2].Value);
                    StringBuilder sb = new StringBuilder();
                    if (dice > 1)
                    {
                        sb.Append("(");
                    }
                    for (int i = 0; i < dice; i++)
                    {
                        int roll = Utilities.random.Next(1, sides + 1);
                        sb.Append(roll).Append(" + ");
                    }
                    sb.Remove(sb.Length - 3, 3);
                    if (dice > 1)
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
