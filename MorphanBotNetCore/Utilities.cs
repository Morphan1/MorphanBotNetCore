﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace MorphanBotNetCore
{
    public class Utilities
    {
        public static readonly Random random = new Random();

        public static readonly UTF8Encoding DefaultEncoding = new UTF8Encoding();

        public static int StringToInt(string input)
        {
            if (int.TryParse(input, out int outp))
            {
                return outp;
            }
            return 0;
        }

        public static string Join(string[] input, string joinedBy, int start = 0)
        {
            StringBuilder outp = new StringBuilder();
            for (int i = start; i < input.Length; i++)
            {
                outp.Append(input[i]);
                if (i + 1 < input.Length)
                {
                    outp.Append(joinedBy);
                }
            }
            return outp.ToString();
        }

        public static T GetObjectFromWebResponse<T>(HttpWebResponse response)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(response.GetResponseStream());
        }

        public static IEnumerable<T> GetEnumValues<T>() where T : Enum
        {
            return (T[])Enum.GetValues(typeof(T));
        }
    }
}
