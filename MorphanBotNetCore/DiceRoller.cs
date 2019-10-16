using Discord;
using Discord.Commands;
using MorphanBotNetCore.Games;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class DiceRoller : ModuleBase<SocketCommandContext>
    {
        public GameManager Games { get; set; }

        [Command("roll")]
        public async Task RollDice([Remainder] string input)
        {
            input = input.ToLowerInvariant();
            if (Games.CurrentGame != null)
            {
                input = Games.CurrentGame.SpecialRoll(Context.User.Id, input);
            }
            Match match = Regex.Match(input, @"(\d+)?d(\d+)((d|k)(\d+))?");
            if (match.Success)
            {
                while (match.Success)
                {
                    int dice = 1;
                    List<int> dropping = new List<int>();
                    bool keepLowest = false;
                    int dropCount = 0;
                    if (match.Groups[1].Success)
                    {
                        dice = Utilities.StringToInt(match.Groups[1].Value);
                    }
                    if (match.Groups[3].Success)
                    {
                        keepLowest = match.Groups[4].Value == "k";
                        dropCount = Utilities.StringToInt(match.Groups[5].Value);
                        if (keepLowest)
                        {
                            dropCount = dice - dropCount;
                        }
                    }
                    int sides = Utilities.StringToInt(match.Groups[2].Value);
                    StringBuilder sb = new StringBuilder();
                    if (dice > 1 || dropCount > 0)
                    {
                        sb.Append("(");
                    }
                    for (int i = 0; i < dice; i++)
                    {
                        int roll = Utilities.random.Next(1, sides + 1);
                        sb.Append(roll).Append(" + ");
                        if (dropping.Count < dropCount)
                        {
                            dropping.Add(roll);
                        }
                        else
                        {
                            for (int x = 0; x < dropCount; x++)
                            {
                                if ((!keepLowest && roll < dropping[x]) || (keepLowest && roll > dropping[x]))
                                {
                                    dropping[x] = roll;
                                    break;
                                }
                            }
                        }
                    }
                    sb.Remove(sb.Length - 3, 3);
                    foreach (int drop in dropping)
                    {
                        if (drop == 0)
                        {
                            break;
                        }
                        sb.Append(" - ").Append(drop);
                    }
                    if (dice > 1 || dropCount > 0)
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
                await ReplyAsync("You must specify at least one set of dice to roll!");
            }
        }
    }
}
