﻿using System;
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
            builder.Append(s.AsSpan(0, index));
            builder.Append(replacement);
            builder.Append(s.AsSpan(index + length));
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

    public static class IntExtensions
    {
        public static string ShowSign(this int i)
        {
            return (i > 0 ? "+" : i < 0 ? "-" : string.Empty) + Math.Abs(i);
        }
    }
}

