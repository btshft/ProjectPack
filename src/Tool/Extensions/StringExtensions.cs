using System;
using System.Collections.Generic;
using System.Linq;

namespace PackProject.Tool.Extensions
{
    public static class StringExtensions
    {
        public static bool NotNull(this string str) 
            => !string.IsNullOrEmpty(str);

        public static string Concat(this IEnumerable<string> parts, string separator)
        {
            return string.Join(separator, parts);
        }

        public static string Quote(this string str)
        {
            return $"\"{str}\"";
        }

        public static string[] Remove(this string[] values, string value)
        {
            return values
                .Where(v => !string.Equals(v, value, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
        }
    }
}