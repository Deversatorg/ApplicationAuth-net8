using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ApplicationAuth.Common.Extensions
{
    public static class StringExtensions
    {
        public static void ThrowsWhenNullOrEmpty(this string str, string paramName = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (string.IsNullOrEmpty(paramName))
                    throw new ArgumentNullException();
                else
                    throw new ArgumentNullException(paramName);
            }
        }

        public static string Сapitalize(this string str)
        {
            str.ThrowsWhenNullOrEmpty();

            return $"{char.ToUpper(str[0])}{str.Substring(1)}";
        }

        public static string GetFirstLine(this string str) => str.Split(Environment.NewLine.ToCharArray()).FirstOrDefault();

        public static string HumanizePascalCase(this string str) => Regex.Replace(str, "[a-z][A-Z]", m => m.Value[0] + " " + char.ToLower(m.Value[1]));
    }
}
