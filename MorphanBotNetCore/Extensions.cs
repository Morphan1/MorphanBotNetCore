using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public static class StringExtensions
    {
        public static string Replace(this string s, int index, int length, string replacement)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(s.Substring(0, index));
            builder.Append(replacement);
            builder.Append(s.Substring(index + length));
            return builder.ToString();
        }

        public static string StripNonAlphaNumeric(this string s)
        {
            return Regex.Replace(s.Replace(" ", "_"), "[^a-zA-Z0-9_]+", string.Empty);
        }

        public static string StripNonNumeric(this string s)
        {
            return Regex.Replace(s, "[^0-9+-]+", string.Empty);
        }
    }
}

