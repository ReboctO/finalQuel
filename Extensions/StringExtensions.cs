using System.Text.RegularExpressions;

namespace TheQuel
{
    public static class StringExtensions
    {
        public static string SplitCamelCase(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
                
            return Regex.Replace(input, "([A-Z])", " $1").Trim();
        }
    }
} 