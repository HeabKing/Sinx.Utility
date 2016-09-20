using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sinx.Utility.Extension
{
    public static class RegexEx
    {
        public static bool IsEmail(string input)
        {
            return Regex.IsMatch(input, GetPatterns().Email);
        }

        public static bool IsUrl(string input)
        {
            return Regex.IsMatch(input, GetPatterns().Url);
        }

        public static Patterns GetPatterns(this Regex regex)
        {
            return GetPatterns();
        }

        public static Patterns GetPatterns()
        {
            return new Patterns();
        }

        public class Patterns
        {
            /// <summary>
            /// 关于网址验证的正则 https://mathiasbynens.be/demo/url-regex
            /// </summary>
            /// <returns></returns>
            public string Url => @"^(https?|ftp)://[^\s/$.?#].[^\s]*$";

            /// <summary>
            /// 邮箱正则 https://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            /// </summary>
            /// <returns></returns>
            public string Email => @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
        }
    }
}
