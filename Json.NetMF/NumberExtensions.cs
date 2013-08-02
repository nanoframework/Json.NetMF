// Source code is modified from Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357)

using System;
using Microsoft.SPOT;
using System.Globalization;

namespace Json.NETMF
{
	internal enum NumberStyle
	{
		Decimal = 1,
		Hexadecimal
	}

	internal static class UInt32Extensions
	{
		public static bool TryParse(string str, NumberStyle style, out UInt32 result)
		{
			bool sign;
			ulong tmp;

			bool bresult = Helper.TryParseUInt64Core(str, style == NumberStyle.Hexadecimal ? true : false, out tmp, out sign);
			result = (UInt32)tmp;

			return bresult && !sign;
		}
	}

	internal static class Int64Extensions
	{
		public static long Parse(string str)
		{
			long result;
			if (TryParse(str, out result))
			{
				return result;
			}
			throw new Exception();
		}

		public static long Parse(string str, NumberStyle style)
		{
			if (style == NumberStyle.Hexadecimal)
			{
				return ParseHex(str);
			}

			return Parse(str);
		}

		public static bool TryParse(string str, out long result)
		{
			result = 0;
			ulong r;
			bool sign;
			if (Helper.TryParseUInt64Core(str, false, out r, out sign))
			{
				if (!sign)
				{
					if (r <= 9223372036854775807)
					{
						result = unchecked((long)r);
						return true;
					}
				}
				else
				{
					if (r <= 9223372036854775808)
					{
						result = unchecked(-((long)r));
						return true;
					}
				}
			}
			return false;
		}

		private static long ParseHex(string str)
		{
			ulong result;
			if (TryParseHex(str, out result))
			{
				return (long)result;
			}
			throw new Exception();
		}

		private static bool TryParseHex(string str, out ulong result)
		{
			bool sign;
			return Helper.TryParseUInt64Core(str, true, out result, out sign);
		}

	}

	internal static class CharExtensions
	{
		/// <summary>
		/// Converts a Unicode character to a string of its ASCII equivalent.
		/// Very simple, it works only on ordinary characters.
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		public static string ConvertFromUtf32(int p)
		{
			char c = (char)p;
			return c.ToString();
		}
	}

    #region Private Static Helper Methods

    internal static class Helper
    {
        public const int MaxDoubleDigits = 16;

        public static bool IsWhiteSpace(char ch)
        {
            return ch == ' ';
        }

        // Parse integer values using localized number format information.
        public static bool TryParseUInt64Core(string str, bool parseHex, out ulong result, out bool sign)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }

            // If number contains the Hex '0x' prefix, then make sure we're
            // managing a Hex number, and skip over the '0x'
            if (str.Length >= 2 && str.Substring(0, 2).ToLower() == "0x")
            {
                str = str.Substring(2);
                parseHex = true;
            }

            char ch;
            bool noOverflow = true;
            result = 0;

            // Skip leading white space.
            int len = str.Length;
            int posn = 0;
            while (posn < len && IsWhiteSpace(str[posn]))
            {
                posn++;
            }

            // Check for leading sign information.
            NumberFormatInfo nfi = CultureInfo.CurrentUICulture.NumberFormat;
            string posSign = nfi.PositiveSign;
            string negSign = nfi.NegativeSign;
            sign = false;
            while (posn < len)
            {
                ch = str[posn];
                if (!parseHex && ch == negSign[0])
                {
                    sign = true;
                    ++posn;
                }
                else if (!parseHex && ch == posSign[0])
                {
                    sign = false;
                    ++posn;
                }
                /*      else if (ch == thousandsSep[0])
                        {
                            ++posn;
                        }*/
                else if ((parseHex && ((ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f'))) ||
                         (ch >= '0' && ch <= '9'))
                {
                    break;
                }
                else
                {
                    return false;
                }
            }

            // Bail out if the string is empty.
            if (posn >= len)
            {
                return false;
            }

            // Parse the main part of the number.
            uint low = 0;
            uint high = 0;
            uint digit;
            ulong tempa, tempb;
            if (parseHex)
            {
                #region Parse a hexadecimal value.
                do
                {
                    // Get the next digit from the string.
                    ch = str[posn];
                    if (ch >= '0' && ch <= '9')
                    {
                        digit = (uint)(ch - '0');
                    }
                    else if (ch >= 'A' && ch <= 'F')
                    {
                        digit = (uint)(ch - 'A' + 10);
                    }
                    else if (ch >= 'a' && ch <= 'f')
                    {
                        digit = (uint)(ch - 'a' + 10);
                    }
                    else
                    {
                        break;
                    }

                    // Combine the digit with the result, and check for overflow.
                    if (noOverflow)
                    {
                        tempa = ((ulong)low) * ((ulong)16);
                        tempb = ((ulong)high) * ((ulong)16);
                        tempb += (tempa >> 32);
                        if (tempb > ((ulong)0xFFFFFFFF))
                        {
                            // Overflow has occurred.
                            noOverflow = false;
                        }
                        else
                        {
                            tempa = (tempa & 0xFFFFFFFF) + ((ulong)digit);
                            tempb += (tempa >> 32);
                            if (tempb > ((ulong)0xFFFFFFFF))
                            {
                                // Overflow has occurred.
                                noOverflow = false;
                            }
                            else
                            {
                                low = unchecked((uint)tempa);
                                high = unchecked((uint)tempb);
                            }
                        }
                    }
                    ++posn; // Advance to the next character.
                } while (posn < len);
                #endregion
            }
            else
            {
                #region Parse a decimal value.
                do
                {
                    // Get the next digit from the string.
                    ch = str[posn];
                    if (ch >= '0' && ch <= '9')
                    {
                        digit = (uint)(ch - '0');
                    }
                    /*       else if (ch == thousandsSep[0])
                           {
                               // Ignore thousands separators in the string.
                               ++posn;
                               continue;
                           }*/
                    else
                    {
                        break;
                    }

                    // Combine the digit with the result, and check for overflow.
                    if (noOverflow)
                    {
                        tempa = ((ulong)low) * ((ulong)10);
                        tempb = ((ulong)high) * ((ulong)10);
                        tempb += (tempa >> 32);
                        if (tempb > ((ulong)0xFFFFFFFF))
                        {
                            // Overflow has occurred.
                            noOverflow = false;
                        }
                        else
                        {
                            tempa = (tempa & 0xFFFFFFFF) + ((ulong)digit);
                            tempb += (tempa >> 32);
                            if (tempb > ((ulong)0xFFFFFFFF))
                            {
                                // Overflow has occurred.
                                noOverflow = false;
                            }
                            else
                            {
                                low = unchecked((uint)tempa);
                                high = unchecked((uint)tempb);
                            }
                        }
                    }
                    ++posn;// Advance to the next character.
                } while (posn < len);
                #endregion
            }

            // Process trailing white space.
            if (posn < len)
            {
                do
                {
                    ch = str[posn];
                    if (IsWhiteSpace(ch))
                        ++posn;
                    else
                        break;
                } while (posn < len);

                if (posn < len)
                {
                    return false;
                }
            }

            // Return the results to the caller.
            result = (((ulong)high) << 32) | ((ulong)low);
            return noOverflow;
        }
    }

    #endregion
}
