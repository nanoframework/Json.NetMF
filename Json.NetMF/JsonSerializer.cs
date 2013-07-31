// Source code is modified from Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357)

using System;
using Microsoft.SPOT;
using System.Reflection;
using System.Collections;

namespace Json.NETMF
{
	/// <summary>
	/// .NET Micro Framework JSON Serializer and Deserializer.
	/// Mimics, as closely as possible, the excellent JSON (de)serializer at http://www.json.org.
	/// You can serialize just about any object that contains real property values:
	/// Value Types (int, bool, string, etc), Classes, Arrays, Dictionaries, Hashtables, etc.
	/// Caveats:
	///   1) Each property to be serialized must be public, and contain BOTH a property getter and setter.
	///   2) You can't serialize interfaces, virtual or abstract properties, private properties.
	///      Your class can contain these objects, but their values are not serialized.
	///   3) DateTime objects can be serialized, and their format in JSON will be ISO 8601 format by default. 
    ///   To use "ASP.NET AJAX" format - specify in the dateTimeFormat agrument in the or static method.
	///   4) Guids can be serialized.
	///   3) You can't use Array or IList because they are abstract (or an interface).  Use ArrayList instead.
	///   4) You can't use IDictionaryEntry, use DictionaryEntry instead.
	///   5) .NET MF floating point seems to have very little precision, at least on my GHI USBizi hardware.
	///      I get only about 3 or 4 decimal places of accuracy.
	/// 
	/// </summary>

	/// <summary>
	/// .NET Micro Framework JSON Serializer and Deserializer.
	/// </summary>
    public class JsonSerializer
	{
        /// <summary>
        /// Enumeration of the popular formats of time and date
        /// within Json.  It's not a standard, so you have to
        /// know which on you're using.
        /// </summary>
        public enum DateTimeFormat
        {
            Default = 0,
            ISO8601 = 1,
            Ajax = 2
        }

        public enum SerializeStatus
        {
            None = 0,
            Serialize = 1
        }

        public JsonSerializer(DateTimeFormat dateTimeFormat = DateTimeFormat.Default)
		{
            DateFormat = dateTimeFormat;
		}

	    /// <summary>
	    /// Gets/Sets the format that will be used to display
	    /// and parse dates in the Json data.
	    /// </summary>
        public DateTimeFormat DateFormat { get; set; }

		/// <summary>
		/// Serializes an object into a Json string.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public string Serialize(object o)
		{
            return SerializeObject(o, this.DateFormat);
		}

        /// <summary>
        /// Desrializes a Json string into an object.
        /// </summary>
        /// <param name="json"></param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
        public object Deserialize(string json)
        {
            return DeserializeString(json);
        }

		/// <summary>
		/// Deserializes a Json string into an object.
		/// </summary>
		/// <param name="json"></param>
        /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
		public static object DeserializeString(string json)
		{
			return JsonParser.JsonDecode(json);
		}

        /// <summary>
        /// Convert a value to JSON.
        /// </summary>
        /// <param name="o">The value to convert. Supported types are: Boolean, String, Byte, (U)Int16, (U)Int32, Float, Double, Decimal, JsonObject, JsonArray, Array, Object and null.</param>
        /// <returns>The JSON object as a string or null when the value type is not supported.</returns>
        /// <remarks>For objects, only public fields are converted.</remarks>
        public static string SerializeObject(object o, DateTimeFormat dateTimeFormat = DateTimeFormat.Default)
        {
            if (o == null)
                return "null";

            Type type = o.GetType();

            // All ordinary value types and all objects that are classes that can
            // and shouold be ToString()'d are handled here.  Special objects like
            // arrays and classes that have properties to be enumerated are handled below.
            switch (type.Name)
            {
                case "Boolean":
                    {
                        return (bool)o ? "true" : "false";
                    }
                case "String":
                    {
                        // Encapsulate object in double-quotes if it's not already
                        char v = o.ToString()[0];
                        if (v == '"')
                        {
                            return o.ToString();
                        }
                        else
                        {
                            return "\"" + o.ToString() + "\"";
                        }
                    }
                case "Single":
                case "Double":
                case "Decimal":
                case "Float":
                    {
                        return DoubleExtensions.ToString((double)o);
                    }
                case "Byte":
                case "SByte":
                case "Int16":
                case "UInt16":
                case "Int32":
                case "UInt32":
                case "Int64":
                case "UInt64":
                case "JsonObject":
                case "JsonArray":
                    {
                        return o.ToString();
                    }
                case "Char":
                case "Guid":
                    {
                        return "\"" + o.ToString() + "\"";
                    }
                case "DateTime":
                    {
                        switch (dateTimeFormat)
                        {
                            case DateTimeFormat.Ajax:
                                // This MSDN page describes the problem with JSON dates:
                                // http://msdn.microsoft.com/en-us/library/bb299886.aspx
                                return "\"" + DateTimeExtensions.ToASPNetAjax((DateTime)o) + "\"";
                            case DateTimeFormat.ISO8601:
                            case DateTimeFormat.Default:
                            default:
                                return "\"" + DateTimeExtensions.ToIso8601((DateTime)o) + "\"";
                        }
                    }
            }

            if (type.IsArray)
            {
                JsonArray jsonArray = new JsonArray();
                foreach (object i in (Array)o)
                {
                    // If the array object needs to be serialized first, do it
                    object valueToAdd = string.Empty;
                    SerializeStatus serialize = GetSerializeState(i);
                    if (serialize == SerializeStatus.Serialize)
                    {
                        valueToAdd = SerializeObject(i, dateTimeFormat);
                    }
                    else
                    {
                        valueToAdd = i;
                    }
                    jsonArray.Add(valueToAdd);
                }
                return jsonArray.ToString();
            }

            if (type == typeof(System.Collections.ArrayList))
            {
                JsonArray jsonArray = new JsonArray();
                foreach (object i in (o as ArrayList))
                {
                    // If the array object needs to be serialized first, do it
                    object valueToAdd = string.Empty;
                    SerializeStatus serialize = GetSerializeState(i);
                    if (serialize == SerializeStatus.Serialize)
                    {
                        valueToAdd = SerializeObject(i, dateTimeFormat);
                    }
                    else
                    {
                        valueToAdd = i;
                    }
                    jsonArray.Add(valueToAdd);
                }
                return jsonArray.ToString();
            }

            if (type == typeof(System.Collections.Hashtable))
            {
                JsonObject main = new JsonObject();
                Hashtable table = o as Hashtable;
                JsonObject to = new JsonObject();
                foreach (var key in table.Keys)
                {
                    // If the array object needs to be serialized first, do it
                    object valueToAdd = string.Empty;
                    SerializeStatus serialize = GetSerializeState(table[key]);
                    if (serialize == SerializeStatus.Serialize)
                    {
                        valueToAdd = SerializeObject(table[key], dateTimeFormat);
                    }
                    else
                    {
                        valueToAdd = table[key];
                    }

                    to.Add(key, valueToAdd);
                    //to.Add(key, table[key]);
                }
                //main.Add(type.Name, to.ToString());
                //return main.ToString();
                return to.ToString();
            }

            if (type == typeof(System.Collections.DictionaryEntry))
            {
                DictionaryEntry dict = o as DictionaryEntry;
                JsonObject to = new JsonObject();

                // If the Value property of the DictionaryEntry is an object rather
                // than a string, then serialize it first.
                object valueToAdd = string.Empty;
                SerializeStatus serialize = GetSerializeState(dict.Value);
                if (serialize == SerializeStatus.Serialize)
                {
                    valueToAdd = SerializeObject(dict.Value, dateTimeFormat);
                }
                else
                {
                    valueToAdd = dict.Value;
                }

                to.Add(dict.Key, valueToAdd);

                return to.ToString();
            }

            if (type.IsClass)
            {
                JsonObject jsonObject = new JsonObject();

                // Iterate through all of the methods, looking for GET properties
                MethodInfo[] methods = type.GetMethods();
                foreach (MethodInfo method in methods)
                {
                    // We care only about property getters when serializing
                    if (method.Name.StartsWith("get_"))
                    {
                        // Ignore abstract and virtual objects
                        if ((method.IsAbstract ||
                            (method.IsVirtual) ||
                            (method.ReturnType.IsAbstract)))
                        {
                            continue;
                        }

                        // Ignore delegates and MethodInfos
                        if ((method.ReturnType == typeof(System.Delegate)) ||
                            (method.ReturnType == typeof(System.MulticastDelegate)) ||
                            (method.ReturnType == typeof(System.Reflection.MethodInfo)))
                        {
                            continue;
                        }
                        // Ditto for DeclaringType
                        if ((method.DeclaringType == typeof(System.Delegate)) ||
                            (method.DeclaringType == typeof(System.MulticastDelegate)))
                        {
                            continue;
                        }

                        // If the property returns a Hashtable
                        if (method.ReturnType == typeof(System.Collections.Hashtable))
                        {
                            Hashtable table = method.Invoke(o, null) as Hashtable;
                            JsonObject to = new JsonObject();
                            foreach (var key in table.Keys)
                            {
                                // If the array object needs to be serialized first, do it
                                object valueToAdd = string.Empty;
                                SerializeStatus serialize = GetSerializeState(table[key]);
                                if (serialize == SerializeStatus.Serialize)
                                {
                                    valueToAdd = SerializeObject(table[key], dateTimeFormat);
                                }
                                else
                                {
                                    valueToAdd = table[key];
                                }
                                to.Add(key, valueToAdd);
                                //to.Add(key, table[key]);
                            }
                            jsonObject.Add(method.Name.Substring(4), to.ToString());
                            continue;
                        }

                        // If the property returns an array of objects
                        if (method.ReturnType == typeof(System.Collections.ArrayList))
                        {
                            ArrayList no = method.Invoke(o, null) as ArrayList;
                            JsonArray jsonArray = new JsonArray();
                            foreach (object i in no)
                            {
                                // If the array object needs to be serialized first, do it
                                object valueToAdd = string.Empty;
                                SerializeStatus serialize = GetSerializeState(i);
                                if (serialize == SerializeStatus.Serialize)
                                {
                                    valueToAdd = SerializeObject(i, dateTimeFormat);
                                }
                                else
                                {
                                    valueToAdd = i;
                                }
                                jsonArray.Add(valueToAdd);
                            }
                            jsonObject.Add(method.Name.Substring(4), jsonArray.ToString());
                            continue;
                        }

                        // If the property returns a DictionaryEntry
                        if (method.ReturnType == typeof(System.Collections.DictionaryEntry))
                        {
                            DictionaryEntry dict = method.Invoke(o, null) as DictionaryEntry;

                            // If the Value property of the DictionaryEntry needs to be serialized first, do it
                            object valueToAdd = string.Empty;
                            SerializeStatus serialize = GetSerializeState(dict.Value);
                            if (serialize == SerializeStatus.Serialize)
                            {
                                valueToAdd = SerializeObject(dict.Value, dateTimeFormat);
                            }
                            else
                            {
                                valueToAdd = dict.Value;
                            }

                            // Wrap the DictionaryEntry in a JsonObject
                            JsonObject to = new JsonObject();
                            to.Add(dict.Key, valueToAdd);
                            jsonObject.Add(method.Name.Substring(4), to.ToString());
                            continue;
                        }

                        // If the property is a Class that should NOT be ToString()'d, because
                        // it has properties that must themselves be enumerated and serialized,
                        // then recursively call myself to serialize them.
                        if ((method.ReturnType.IsClass) &&
                            (method.ReturnType.IsArray == false) &&
                            (method.ReturnType.ToString().StartsWith("System.Collections") == false) &&
                            (method.ReturnType.ToString().StartsWith("System.String") == false))
                        {
                            object no = method.Invoke(o, null);
                            string value = SerializeObject(no, dateTimeFormat);
                            jsonObject.Add(method.Name.Substring(4), value);
                            continue;
                        }

                        // All other properties are types that will be handled according to 
                        // their type.  That handler code is the switch statement at the top
                        // of this function.
                        object newo = method.Invoke(o, null);
                        jsonObject.Add(method.Name.Substring(4), newo);


                    }
                }
                return jsonObject.ToString();
            }

            return null;
        }

        /// <summary>
        /// Determines if the specified object needs to be serialized.  It needs to be serialized if it's a 
        /// class that contains properties that need enumeration.  All other objects that can be directly
        /// returned, such as ints, strings, etc, do not need to be serialized.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static SerializeStatus GetSerializeState(object o)
        {
            Type type = o.GetType();

            // Ignore delegates and MethodInfos
            if ((type == typeof(System.Delegate)) ||
                (type == typeof(System.MulticastDelegate)) ||
                (type == typeof(System.Reflection.MethodInfo)))
            {
                return SerializeStatus.None;
            }

            // If the property returns a Hashtable
            if (type == typeof(System.Collections.Hashtable))
            {
                return SerializeStatus.None;
            }

            // If the property returns an array of objects
            if (type == typeof(System.Collections.ArrayList))
            {
                return SerializeStatus.Serialize;
            }

            // If the property returns a DictionaryEntry
            if (type == typeof(System.Collections.DictionaryEntry))
            {
                return SerializeStatus.Serialize;
            }

            // If the property is a Class that should NOT be ToString()'d, because
            // it has properties that must themselves be enumerated and serialized,
            // then recursively call myself to serialize them.
            if ((type.IsClass) &&
                (type.IsArray == false) &&
                (type.ToString().StartsWith("System.Collections") == false) &&
                (type.ToString().StartsWith("System.String") == false))
            {
                return SerializeStatus.Serialize;
            }

            // All other properties are types that will be handled according to 
            // their type.  That handler code is the switch statement at the top
            // of this function.
            return SerializeStatus.None;
        }
	}
}
