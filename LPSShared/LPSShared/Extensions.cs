using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace LPSClient
{
    public static class Extensions
    {
        /// <summary>
        /// Returns true if string is null or equals String.Empty
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return (s == null || s == String.Empty);
        }

        /// <summary>
        /// Returns true if string is null or string.Trim() equals String.Empty
        /// </summary>
        public static bool IsNullOrTrimEmpty(this string s)
        {
            return (s == null || s.Trim() == String.Empty);
        }

        /// <summary>
        /// Email address validation througt regular expression
        /// </summary>
        /// <returns>true if string represents valid email address</returns>
        public static bool IsValidEmailAddress(this string s)
        {
            Regex regex = new
              Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
            return regex.IsMatch(s);
        }
    }
}

