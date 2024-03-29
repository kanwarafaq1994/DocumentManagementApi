using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentManagement.Data.Common
{
    public static class HelperMethods
    {
        public static string CleanUserName(string username)
        {
            username.Trim().ToLower();
            username.Replace(" ", string.Empty);
            username = Regex.Replace(username, @"\s+", "");
            var outUser = username.Split('@');

            if (outUser[0].Length > 16)
                return outUser[0].Substring(0, 16);
            else
                return outUser[0];
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer = comparer ?? Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }

                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }

                return min;
            }
        }

        public static string ConvertToString(this object expression)
        {
            try
            {
                if ((string)expression != "")
                    return Convert.ToString(expression);
                else
                    return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static int ConvertToInt(this object expression)
        {
            try
            {
                if ((string)expression != "")
                    return Convert.ToInt32(expression);
                else
                    return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static bool IsLinkActive(DateTime? userActivationDate)
        {
            try
            {
                var isExpired = (StaticDateTimeProvider.Today - userActivationDate?.Date).Value.Days < 28; // Four weeks, not one month. 

                return isExpired;
            }

            catch (Exception ex)
            {
                new InfoDto(ex.Message);
                throw;
            }
        }
       
        public static bool IsNumeric(this object expression)
        {
            try
            {
                if (expression == null) return false;

                var result2 = Convert.ToDouble(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsDateTime(this object expression)
        {
            try
            {
                var result2 = Convert.ToDateTime(expression.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsBoolean(this object expression)
        {
            try
            {
                var result2 = Convert.ToBoolean(expression);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsUrl(this string address)
        {
            var strRegex =
                @"(https:[/][/]|http:[/][/]|www.)[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*$";
            var re = new Regex(strRegex);
            if (re.IsMatch(address))
                return true;
            else
                return false;
        }

        public static bool IsValidFileName(this string fileName)
        {
            var strRegex =
                @"^(?!((CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(\..*)?|[\.\ ].*|.*[\.\ ]$))([^\x00-\x1F\x22\x25\x2A\x2F\x3A\x3C\x3E\x3F\x5C\x7C\x7F]{1,255})";
            var re = new Regex(strRegex);
            return re.IsMatch(fileName);
        }

        public static bool IsEmail(this string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            var strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" + @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                           @".)+))([a-zA-Z]{2,63}|[0-9]{1,3})(\]?)$";
            var re = new Regex(strRegex, RegexOptions.Compiled);

            if (re.IsMatch(email))
                return true;
            else
                return false;
        }

        public static bool IsPhoneNumber(this string phoneNumber)
        {
            if (phoneNumber == null) return false;

            var trimNumber = phoneNumber.ToString().Replace(" ", "").Replace("-", "").Trim();

            // Phone number should not contain any letter
            // Phone number should not exceed max limit 
            if (trimNumber.StartsWith('+') || trimNumber.StartsWith('0'))
                if (trimNumber.Substring(1, trimNumber.Length - 1).All(char.IsDigit))
                    return true;

            return false;
        }

        public static Guid ConvertToGuid(this object expression)
        {
            try
            {
                if (expression != null) return Guid.Parse(expression.ToString());
            }
            catch
            {
                throw new Exception("value cannot convert to GUID");
            }

            return Guid.Empty;
        }

        public static bool ConvertToBool(this object expression)
        {
            try
            {
                if ((string)expression != "")
                    return Convert.ToBoolean(expression);
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static long ConvertToLong(this object expression)
        {
            try
            {
                if ((string)expression != "")
                    return Convert.ToInt64(expression);
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static decimal ConvertToDecimal(this object expression)
        {
            try
            {
                if ((string)expression != "")
                    return Convert.ToDecimal(expression);
                return 0;
            }
            catch
            {
                return 0;
            }
        }


        public static DateTime ConvertToDateTime(this object expression)
        {
            return Convert.ToDateTime(expression);
        }

        public static string FormatDateTime(this DateTime dateTime)
        {
            if (dateTime.TimeOfDay == TimeSpan.Zero)
                return dateTime.ToString("dd MMM yyyy");
            return dateTime.ToString("dd MMM yyyy hh:mm tt");
        }

        public static string FormatDateTime(this DateTime? dateTime)
        {
            if (dateTime.HasValue)
                return dateTime.Value.FormatDateTime();
            return "";
        }

        /// <summary>
        /// Generate Random Text
        /// </summary>
        /// <param name="textLength"></param>
        /// <param name="type">AlphaNumeric | Alpha | Numeric</param>
        /// <returns></returns>
        public static string GenerateRandomText(int textLength = 8, string type = "AlphaNumeric")
        {
            const string alphabats = "ABCDEFGHIJKLMNPOQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            var Chars = alphabats + numbers;
            if (type.ToLower() == "numeric")
                Chars = numbers;
            else if (type.ToLower() == "alpha") Chars = alphabats;

            var random = new Random();
            var result =
                new string(Enumerable.Repeat(Chars, textLength).Select(s => s[random.Next(s.Length)]).ToArray());
            return result;
        }

        //Function to get random number
        public static int GetRandomNumber(int min = 5, int max = 8)
        {
            var random = new Random();
            return random.Next(min, max);
        }

        public static string UppercaseFirstLetter(string str)
        {
            var firstChar = str[0];
            var upperCaseFirstCharacter = char.ToUpper(firstChar);
            var convertedFirstCharToUpper = upperCaseFirstCharacter + str.Substring(1);
            return convertedFirstCharToUpper;
        }

        public static void AddIfNotnull<T>(this List<T> list, T value)
        {
            if (value != null)
            {
                list.Add(value);
            }
        }

        public static void AddRangeIfNotnull<T>(this List<T> list, IEnumerable<T> values)
        {
            if (values != null)
            {
                list.AddRange(values);
            }
        }
    }
}
