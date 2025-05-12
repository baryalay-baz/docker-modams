using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MODAMS.Utilities
{
    public static class InputSanitizer
    {
        public static string CleanText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "";

            var cleaned = input
                .Normalize(NormalizationForm.FormC)
                .Replace("\u200B", "")  
                .Replace("\u200C", "")  
                .Replace("\u200D", "")  
                .Replace("\u00A0", " ") 
                .Replace("\t", " ")     
                .Replace("\r", "")
                .Replace("\n", "")
                .Replace("“", "\"").Replace("”", "\"") 
                .Replace("‘", "'").Replace("’", "'")   
                .Replace("–", "-").Replace("—", "-")   
                .Trim();

            // Remove any remaining Unicode control characters
            cleaned = new string(cleaned.Where(c => !char.IsControl(c)).ToArray());

            // Collapse multiple spaces into one
            cleaned = Regex.Replace(cleaned, @"\s+", " ");

            return cleaned;
        }

        /// <summary>
        /// Detects if the input contains known hidden or special characters often caused by copy-paste.
        /// </summary>
        /// <param name="input">The user input</param>
        /// <returns>True if any hidden characters are found</returns>
        public static bool ContainsHiddenCharacters(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            return Regex.IsMatch(input, @"[\u200B\u200C\u200D\u00A0]");
        }
    }
}
