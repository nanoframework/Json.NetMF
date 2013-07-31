// Source code is modified from Huysentruit Wouter's JSON generation library (https://www.ghielectronics.com/community/codeshare/entry/282)

using System;
using Microsoft.SPOT;
using System.Collections;

namespace Json.NETMF
{
    /// <summary>
    /// A Json Array.
    /// Programmed by Huysentruit Wouter (https://www.ghielectronics.com/community/codeshare/entry/282)
    /// See the Json.ToJson method for more information.
    /// </summary>
    public class JsonArray : ArrayList
    {
        /// <summary>
        /// Convert the array to its JSON representation.
        /// </summary>
        /// <returns>A string containing the JSON representation of the array.</returns>
        public override string ToString()
        {
            string[] parts = new string[Count];

            for (int i = 0; i < Count; i++)
            {
                // Encapsulate in quotes if not a JSON object or not already in quotes
                char c = this[i].ToString()[0];
                if (c == '{' || c == '[' || c == '"')
                {
                    parts[i] = this[i].ToString();
                }
                else
                {
                    parts[i] = "\"" + this[i].ToString() + "\"";
                }
            }

            string result = "";

            foreach (string part in parts)
            {
                if (result.Length > 0)
                {
                    result += ",";
                }

                result += part;
            }

            return "[" + result + "]";
        }
    }
}
