// Source code is modified from Huysentruit Wouter's JSON generation library (https://www.ghielectronics.com/community/codeshare/entry/282)

using System;
using Microsoft.SPOT;
using System.Collections;

namespace Json.NETMF
{
    /// <summary>
    /// A Json Object.
    /// Programmed by Huysentruit Wouter (https://www.ghielectronics.com/community/codeshare/entry/282)
    /// See the Json.ToJson method for more information.
    /// </summary>
    public class JsonObject : Hashtable
    {
        /// <summary>
        /// Convert the object to its JSON representation.
        /// </summary>
        /// <returns>A string containing the JSON representation of the object.</returns>
        public override string ToString()
        {
            string result = "";

            string[] keys = new string[Count];
            object[] values = new object[Count];

            Keys.CopyTo(keys, 0);
            Values.CopyTo(values, 0);

            for (int i = 0; i < Count; i++)
            {
                if (result.Length > 0)
                {
                    result += ",";
                }

                // If this string is already JSON'd, as denoted by start of object {
                // or start of array [, then use it as-is.  Otherwise, encode it to JSON
                string value = string.Empty;
                char v = values[i].ToString()[0];
                if ((v == '{') || (v == '['))
                {
                    value = values[i].ToString();
                }
                else
                {
                    value = JsonSerializer.SerializeObject(values[i]);
                }
                if (value == null)
                {
                    continue;
                }

                result += "\"" + keys[i] + "\"";
                result += ":";
                result += value;
            }

            return "{" + result + "}";
        }

    }
}
