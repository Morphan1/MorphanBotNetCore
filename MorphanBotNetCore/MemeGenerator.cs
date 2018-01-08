using Discord.Commands;
using System;
using System.Collections.Generic;
using System.DrawingCore;
using System.DrawingCore.Drawing2D;
using System.DrawingCore.Imaging;
using System.DrawingCore.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class MemeGenerator : ModuleBase<SocketCommandContext>
    {
        private const string MemeFolder = "data/meme/";

        [Command("meme")]
        public async Task GenerateMeme(string imageName, [Remainder] string text)
        {
            try
            {
                imageName = MemeFolder + imageName.ToLower() + ".jpg";
                bool exists = false;
                StringBuilder sb = new StringBuilder();
                foreach (string file in Directory.EnumerateFiles(MemeFolder))
                {
                    string lower = file.ToLower();
                    if (lower == imageName)
                    {
                        exists = true;
                        imageName = file;
                        break;
                    }
                    sb.Append(", ").Append(lower.Substring(MemeFolder.Length).Replace(".jpg", ""));
                }
                if (!exists)
                {
                    await ReplyAsync("Invalid meme image! I currently have: " + sb.Remove(0, 2).ToString());
                    return;
                }
                Bitmap bitmap = new Bitmap(imageName);
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    graphics.SmoothingMode = SmoothingMode.HighQuality;
                    graphics.InterpolationMode = InterpolationMode.High;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    graphics.CompositingQuality = CompositingQuality.HighQuality;
                    GraphicsPath path = new GraphicsPath();
                    StringFormat sf = new StringFormat();
                    sf.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces | StringFormatFlags.FitBlackBox | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
                    FontFamily fontFamily = new FontFamily("Impact");
                    Font font = new Font(fontFamily, 72F, FontStyle.Regular, GraphicsUnit.Pixel);
                    List<string> wrapped = WrapText(graphics, text, bitmap.Width, font, sf);
                    while (font.Size > 0 && wrapped.Count > 2)
                    {
                        font = new Font(fontFamily, font.Size - 12F, FontStyle.Regular, GraphicsUnit.Pixel);
                        wrapped = WrapText(graphics, text, bitmap.Width, font, sf);
                    }
                    if (font.Size <= 0)
                    {
                        await ReplyAsync("Failed to write text correctly! Try shorter text!");
                        return;
                    }
                    int y = 60;
                    foreach (string s in wrapped)
                    {
                        path.AddString(s, font.FontFamily, (int)font.Style, font.Size, new Point((int)(bitmap.Width / 2F) - (int)(graphics.MeasureString(s, font).Width / 2F), y), sf);
                        y += (int)graphics.MeasureString(s, font).Height + 5;
                    }
                    graphics.FillPath(new SolidBrush(Color.White), path);
                    // TODO: figure out outlining on Linux - it's borked right up!
                    //graphics.DrawPath(new Pen(Brushes.Black, 5) { LineJoin = LineJoin.Round }, path);
                }
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, ImageFormat.Jpeg);
                    stream.Seek(0, SeekOrigin.Begin);
                    await Context.Channel.SendFileAsync(stream, "generated_meme.jpg");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static List<string> WrapText(Graphics graphics, string text, double pixels, Font font, StringFormat sf)
        {
            string[] originalLines = text.Split(new string[] { " " }, StringSplitOptions.None);

            List<string> wrappedLines = new List<string>();

            StringBuilder actualLine = new StringBuilder();
            double actualWidth = 0;

            foreach (string item in originalLines)
            {
                string word = item + " ";
                actualWidth += graphics.MeasureString(word, font, new PointF(0, 0), sf).Width;

                if (actualWidth > pixels)
                {
                    wrappedLines.Add(actualLine.ToString().TrimEnd());
                    actualLine.Clear();
                    actualWidth = 0;
                }
                actualLine.Append(word);
            }

            if (actualLine.Length > 0)
            {
                wrappedLines.Add(actualLine.ToString().TrimEnd());
            }

            return wrappedLines;
        }
    }
}
