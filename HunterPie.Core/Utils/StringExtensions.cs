﻿using System;
using System.Text.RegularExpressions;

namespace HunterPie.Utils
{
    public static class StringExtensions
    {

        /// <summary>
        /// Removes specific characters from a string
        /// </summary>
        /// <param name="value">string</param>
        /// <param name="chars">characters to remove (default is ' ', '\x0A', '\x0B', '\x0C', '\x0D')</param>
        /// <returns>Pretty string</returns>
        public static string RemoveChars(this string value, char[] chars = null)
        {
            if (chars is null)
                chars = new char[]{ ' ', '\x0A', '\x0B', '\x0C', '\x0D' };

            // Apparently this is faster than using Regex to replace
            string[] temp = value.Split(chars, StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" ", temp);
        }

        /// <summary>
        /// Filters style from a GMD string
        /// </summary>
        /// <param name="value">Text</param>
        /// <returns>Filtered Text</returns>
        public static string FilterStyles(this string value)
        {
            // STYL tag
            Regex tags = new Regex("<[(/\\w )]+>");
            return tags.Replace(value, string.Empty);
        }

        /// <summary>
        /// Allows case insensitive checks
        /// </summary>
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// OwO what's this?
        /// </summary>
        /// <param name="source">Text</param>
        /// <returns>A harder-to-read text</returns>
        public static string AprilFoolsify(this string source)
        {

            string OMEGALUL(Regex pattern, string text, string replacement)
            {
                int matches = pattern.Matches(text).Count;
                int rng = new Random().Next(Math.Min(1, matches), matches);
                return pattern.Replace(text, replacement, rng);
            }

            try {
                Regex owofy = new Regex("o", RegexOptions.IgnoreCase);
                Regex uwufy = new Regex("u", RegexOptions.IgnoreCase);
                string owofied = OMEGALUL(owofy, source, "OwO");
                string uwufied = OMEGALUL(uwufy, owofied, "UwU");
                return uwufied;
            } catch
            {
                return source;
            }
        }
    }
}
