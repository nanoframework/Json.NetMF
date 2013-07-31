// Source code is modified from Mike Jones's JSON Serialization and Deserialization library (https://www.ghielectronics.com/community/codeshare/entry/357)

using System;
using Microsoft.SPOT;
using System.Globalization;

namespace Json.NETMF
{
	public enum NumberStyle
	{
		Decimal = 1,
		Hexadecimal
	}

	public static class DoubleExtensions
	{
		public static bool TryParse(string str, out double result)
		{
			ulong decValue = 0;
			bool hasExpSign = false;
			bool isExpNeg = false;
			int expDigits = 0;
			ulong expValue = 0;
			result = 0;

			if (str == null)
			{
				throw new ArgumentNullException("str");
			}

			int end = str.Length - 1;
			int start = 0;

			// skip whitespaces
			Helper.SkipWhiteSpace(str, ref start, ref end);

			// check for leading sign
			bool hasSign = false;
			bool isNeg = false;
			if (start <= end)
			{
				Helper.CheckSign(str, ref start, end, ref hasSign, ref isNeg);
			}

			// now parse the real number
			int intDigits = 0;
			ulong intValue = Helper.ParseNumberCore(str, ref start, end, Helper.MaxDoubleDigits, ref intDigits, true);

			int decDigits = 0;
			if (start <= end)
			{
				// now check for the decimal point and the decimalplaces
				if (Helper.CheckSeparator(str, CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator, ref start, end))
				{
					if (start <= end)
					{
						decValue = Helper.ParseNumberCore(str, ref start, end, Helper.MaxDoubleDigits - intDigits, ref decDigits, false);
					}
				}
			}

			// now check for the exponent
			if (start <= end)
			{
				char curChar = str[start];
				if (curChar == 'E' || curChar == 'e')
				{
					start++;
					if (start <= end)
					{
						// check for sign
						Helper.CheckSign(str, ref start, end, ref hasExpSign, ref isExpNeg);
						// get exponent
						if (start <= end)
						{
							expValue = Helper.ParseNumberCore(str, ref start, end, 5, ref expDigits, false);
						}
						if (expDigits <= 0)
						{
							return false;
						}
					}
				}
			}

			if (start <= end) // characters left 
			{
				return false;
			}

			// now calculate the value
			result = (double)intValue;
			if (intDigits > Helper.MaxDoubleDigits)
			{
				result *= System.Math.Pow(10, (double)(intDigits - Helper.MaxDoubleDigits));
			}
			if (decDigits > 0)
			{
				result += (decValue * System.Math.Pow(10d, (double)(-decDigits)));
			}
			if (isNeg)
			{
				result *= -1;
			}
			//now the exponent
			if (expDigits > 0)
			{
				if (isExpNeg)
				{
					result *= System.Math.Pow(10d, (double)expValue * -1);
				}
				else
				{
					result *= System.Math.Pow(10d, (double)expValue);
				}
			}
			return true;
		}

		/// <summary>
		/// Converts a Double to a string using a variant of the Double.ToString() method.
		/// The problem with the built-in ToString() method is that it wont automatically
		/// size the precision to fit the number.  So a number like 33.3 gets truncated to
		/// just 33 unless you specify exactly the right amount of precision.  This method
		/// attempts to determine the right amount of precision.
		/// CAUTION!!! I've seen many times when the built-in ToString() method returns
		/// a rounding error when you specify any meaningful precision.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string ToString(double value)
		{
			// First, convert to string with impossibly-long precision
			string fullValue = value.ToString("F64");

			// Find the last non-zero number in the mantissa
			int mantissaLen = 0;
			int decimalLen = 0;
			int i = fullValue.Length - 1;
			for(; i>= 0; i--)
			{
				if(fullValue[i] == '.')
				{
					// we made it all the way to the dot
					decimalLen = i;
					break;
				}
				
				// Stop counting the mantissa once you reach the last non-zero value in the number
				if(fullValue[i] != '0' && (mantissaLen == 0))
				{
					mantissaLen = i;
				}
			}

			// If the entire mantissa was zero, then add 2 for the dot and one mantissa digit
			mantissaLen = (mantissaLen > 0) ? mantissaLen : 2;

			// Truncate the trailing zeros in the mantissa
			fullValue = fullValue.Substring(0, mantissaLen + 1);

			return fullValue;
		}

		public static string ToHexString(double value)
		{
			byte[] bytes = Microsoft.SPOT.Reflection.Serialize(value, typeof(double));
			return new string(ToHexStringArray(bytes));
		}

		//convert a hex number to a hex string
		public static char[] ToHexStringArray(byte[] bytes)
		{
			char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}
			return chars;
		}
	}

	public static class Int32Extensions
	{
		public static string ToHexString(Int32 value)
		{
			byte[] bytes = Microsoft.SPOT.Reflection.Serialize(value, typeof(Int32));
			return new string(ToHexStringArray(bytes));
		}

		//convert a hex number to a hex string
		public static char[] ToHexStringArray(byte[] bytes)
		{
			char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
			
			char[] chars = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				int b = bytes[i];
				chars[i * 2] = hexDigits[b >> 4];
				chars[i * 2 + 1] = hexDigits[b & 0xF];
			}
			return chars;
		}

        public static bool TryParse(string str, out Int32 result)
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
                        result = unchecked((Int32)r);
                        return true;
                    }
                }
                else
                {
                    if (r <= 9223372036854775808)
                    {
                        result = unchecked(-((Int32)r));
                        return true;
                    }
                }
            }
            return false;
        }
	}
	
	public static class UInt32Extensions
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

	public static class Int64Extensions
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

	public static class UInt64Extensions
	{
		public static ulong Parse(string str)
		{
			ulong result;
			if (TryParse(str, out result))
			{
				return result;
			}
			throw new Exception();
		}

		public static ulong Parse(string str, NumberStyle style)
		{
			if (style == NumberStyle.Hexadecimal)
			{
				return ParseHex(str);
			}

			return Parse(str);
		}

		public static bool TryParse(string str, out ulong result)
		{
			bool sign;
			return Helper.TryParseUInt64Core(str, false, out result, out sign) && !sign;
		}

		private static ulong ParseHex(string str)
		{
			ulong result;
			if (TryParseHex(str, out result))
			{
				return result;
			}
			throw new Exception();
		}

		private static bool TryParseHex(string str, out ulong result)
		{
			bool sign;
			return Helper.TryParseUInt64Core(str, true, out result, out sign);
		}

	}

	public static class CharExtensions
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

        public static void CheckSign(string str, ref int start, int end, ref bool hasSign, ref bool isNeg)
        {
            int counter;
            int current;
            string sign = CultureInfo.CurrentUICulture.NumberFormat.NegativeSign;
            char signChar = sign[0];
            int signLength = sign.Length;
            char curChar = str[start];

            // check for negative sign at the beginning
            if (curChar == signChar)
            {
                counter = 1;
                current = start + 1; ;

                if (signLength > 1)
                {
                    while (counter < signLength && current <= end)
                    {
                        if (str[current] != sign[counter])
                        {
                            break;
                        }
                        current++;
                        counter++;
                    }
                }
                if (counter >= signLength)
                {
                    hasSign = true;
                    isNeg = true;
                    start = current;
                    return;
                }
            }

            // check for positive sign at the beginning
            sign = CultureInfo.CurrentUICulture.NumberFormat.PositiveSign;
            signChar = sign[0];
            signLength = sign.Length;
            if (curChar == signChar)
            {
                counter = 1;
                current = start + 1;

                if (signLength > 1)
                {
                    while (counter < signLength && current <= end)
                    {
                        if (str[current] != sign[counter])
                        {
                            break;
                        }
                        current++;
                        counter++;
                    }
                }
                if (counter >= signLength)
                {
                    hasSign = true;
                    start = current;
                }
            }
        }

        public static bool CheckSeparator(string str, string sep, ref int start, int end)
        {
            int strLength = sep.Length;

            if (strLength > 0)
            {
                char curChar = str[start];
                char strChar = sep[0];

                // check for first Character at the beginning
                if (curChar == strChar)
                {
                    int counter = 1;
                    int current = start + 1;
                    while (counter < strLength && current <= end)
                    {
                        if (str[current] != sep[counter])
                        {
                            break;
                        }
                        current++;
                        counter++;
                    }
                    if (counter >= strLength) // string found
                    {
                        // so update to new start position
                        start = current;
                        return true;
                    }
                }
            }
            return false;
        }

        public static void SkipWhiteSpace(string str, ref int start, ref int end)
        {
            while (start <= end && IsWhiteSpace(str[start]))
            {
                start++;
            }

            // remove trailing whitespaces
            if (start <= end)
            {
                while (start <= end && IsWhiteSpace(str[end]))
                {
                    end--;
                }
            }
        }

        public static bool IsWhiteSpace(char ch)
        {
            return ch == ' ';
        }

        // parse the number beginning at str[start] up to maximal str[end].
        // passed parameters:
        // str character array containing the data to parse
        // style and nfi are the formatspecs
        // start and end are the indexes of the first and last character to parse
        // maxDigits must not extend 18 else an overflow may occure
        // numdigits must be 0
        // on exit start points to the first character after the last parsed digit.
        // end is unchanged. 
        // numDigits is updated to the number of significant digits parsed.
        // if numDigits > maxDigits the value has to be calculated 
        // returnvalue * Math.Pof(10, (numDigits - maxDigits)).
        public static ulong ParseNumberCore(string str, ref int start, int end, int maxDigits, ref int numDigits, bool allowGroupSep)
        {
            char curChar;
            ulong ulwork = 0;

            // now parse the real number
            string sep = CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator;
            while (start <= end)
            {
                curChar = str[start];
                if (curChar >= '0' && curChar <= '9')
                {
                    if (numDigits < maxDigits)
                    {
                        ulwork = ulwork * 10 + unchecked((uint)(curChar - '0'));
                    }
                    start++;
                    numDigits++;
                }
                else
                {
                    // check for groupseparator if allowed
                    if (!allowGroupSep || !CheckSeparator(str, sep, ref start, end))
                    {
                        break;
                    }
                }
            }
            return ulwork;
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
