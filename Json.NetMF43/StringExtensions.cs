// Source code is modified from Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357)

using System;
using Microsoft.SPOT;
using System.Collections;

namespace Json.NETMF
{
	public static class StringExtensions
	{
		public static string PadLeft(this string source, int count, char pad)
		{
			for(int i=0; i<count; i++)
			{
				source = pad.ToString() + source;
			}

			return source;
		}

        public static bool EndsWith(this string s, string value)
        {
            return s.IndexOf(value) == s.Length - value.Length;
        }

        public static bool StartsWith(this string s, string value)
        {
            return s.IndexOf(value) == 0;
        }

        public static bool Contains(this string s, string value)
        {
            return s.IndexOf(value) >= 0;
        }
	}
}
