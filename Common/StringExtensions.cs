using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Paulus.Common
{

    public static class StringExtensions
    {
        public static int CharacterCount(this string text, char c)=>
             text.Count(x => x == c);
        //{
        //    //int count = 0;
        //    //for (int i = 0; i < text.Length; i++)
        //    //    if (text[i] == c) count++;
        //    //return count;
        //    //

        //    return 
        //}//ok

        public static int CharacterCount(this string text, params char[] c) =>
            text.Count(x => c.Contains(x));
        //{
        //    int count = 0;
        //    for (int i = 0; i < text.Length; i++)
        //        //if (c.Contains<char>(text[i])) count++;
        //        for (int j = 0; j < c.Length; j++)
        //            if (c[j] == text[i]) count++;

        //    return count;
        //    //return text.Count<char>((x) => { return c.Contains<char>(x); });
        //}//ok

        public static List<int> CharacterPositions(this string text, char c)
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < text.Length; i++)
                if (text[i] == c) positions.Add(i);
            return positions;
        }

        public static List<int> CharacterPositions(this string text, params char[] c)
        {
            List<int> positions = new List<int>();
            for (int i = 0; i < text.Length; i++)
                //if (c.Contains<char>(text[i])) { positions.Add(i); }
                for (int j = 0; j < c.Length; j++)
                    if (c[j] == text[i]) positions.Add(i);

            return positions;
        }

        ///// <summary>
        ///// Returns the tokens between two different characters such as [ and ].
        ///// </summary>
        ///// <param name="text"></param>
        ///// <param name="charBeforeToken"></param>
        ///// <param name="charAfterToken"></param>
        ///// <returns></returns>
        //public static List<string> GetTokensBetweenCharacters(this string text, char charBeforeToken, char charAfterToken)
        //{
        //    List<string> ret = new List<string>();
        //    List<int> charBeforePositions = text.CharacterPositions(charBeforeToken);
        //    List<int> charAfterPositions = text.CharacterPositions(charAfterToken);

        //    for (int i = 0; i < charBeforePositions.Count; i++)
        //        ret.Add(text.Substring2(charBeforePositions[i] + 1, charAfterPositions[i] - 1));

        //    return ret;
        //}

        public static bool CharacterNeedsEscapeInsideRegex(char c)
        {
            //closed bracket needs also escaping
            return (new char[] { '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?', '\\', ']' }).Contains(c);
        }

        public static List<string> GetTokensBetweenCharacters(this string text, char charBeforeToken, char charAfterToken)
        {
            string sBeforeToken = CharacterNeedsEscapeInsideRegex(charBeforeToken) ? "\\" + charBeforeToken.ToString() : charBeforeToken.ToString();
            string sAfterToken = CharacterNeedsEscapeInsideRegex(charAfterToken) ? "\\" + charAfterToken.ToString() : charAfterToken.ToString();

            List<string> ret = new List<string>();

            string pattern = sBeforeToken + "([^" + sBeforeToken + sAfterToken + "]+)" + sAfterToken;

            foreach (Match m in Regex.Matches(text, pattern))
                ret.Add(m.Groups[1].Value);

            return ret;
        }

        public static Dictionary<string, string> GetTokensDictionaryBetweenCharacters(this string text, char charBeforeToken, char charAfterToken, string[] keys)
        {
            List<string> tokens = text.GetTokensBetweenCharacters(charBeforeToken, charAfterToken);
            Dictionary<string, string> ret = new Dictionary<string, string>();
            for (int iToken = 0; iToken < tokens.Count; iToken++)
                ret.Add(keys[iToken], tokens[iToken]);
            return ret;
        }

        public static string Substring2(this string text, int start, int end)
        {
            return text.Substring(start, end - start + 1);
        }

        //useful to retrieve a unit from a token eg.
        // Time [s], Time (s) or Time
        public static string Substring2(this string text, int start, char charBeforeSubstring, char charAfterSubstring, string returnValueIfEmpty = "")
        {
            int charBeforePosition = text.IndexOf(charBeforeSubstring, start);
            int charAfterPosition = text.IndexOf(charAfterSubstring, charBeforePosition + 1);
            return charBeforePosition + 1 <= charAfterPosition - 1 ?
                text.Substring2(charBeforePosition + 1, charAfterPosition - 1) : returnValueIfEmpty;
        }

        public static string Substring2(this string text, int start, string textBeforeSubstring, string textAfterSubstring, string returnValueIfEmpty = "")
        {
            int charBeforePosition = text.IndexOf(textBeforeSubstring, start);
            int tl = textBeforeSubstring.Length;
            int charAfterPosition = text.IndexOf(textAfterSubstring, charBeforePosition + tl);
            return charBeforePosition + tl <= charAfterPosition - 1 ?
                text.Substring2(charBeforePosition + tl, charAfterPosition - 1) : returnValueIfEmpty;
        }
        //useful to retrieve a variable name from a token eg.
        // Time [s], Time (s) or Time
        //20/10/2014: Corrected when the start is different than zero.
        public static string SubstringBeforeChar(this string text, int start, char charAfterSubstring, bool ifNotFoundReturnWholeToken = true)
        {
            int charAfterPosition = text.IndexOf(charAfterSubstring, start);
            if (charAfterPosition > 0)
                return text.Substring2(start, charAfterPosition - 1);
            else if (charAfterPosition == 0)
                return "";
            else //not found
                return ifNotFoundReturnWholeToken ? text : "";
        }

        public static string SubstringAfterChar(this string text, int start, char charBeforeSubstring)
        {
            int charBeforePosition = text.IndexOf(charBeforeSubstring, start);
            return text.Substring(charBeforePosition + 1);
        }

        public static string RemoveCharacters(this string text, params char[] charsToRemove)
        {
            string ret = text;
            for (int i = 0; i < charsToRemove.Length; i++)
                ret = ret.Replace(charsToRemove[i].ToString(), "");
            return ret;
        }

        public static bool StartsEndsWith(this string text, string startValue, string endValue)
        {
            return text.StartsWith(startValue) && text.EndsWith(endValue);
        }

        public static bool StartsEndsWith(this string text, string startEndValue)
        {
            return text.StartsEndsWith(startEndValue, startEndValue);
        }


        //public static IEnumerable IndexOfAll(this string haystack, string needle)
        //{
        //    int pos, offset = 0;
        //    while ((pos = haystack.IndexOf(needle)) > 0)
        //    {
        //        haystack = haystack.Substring(pos + needle.Length);
        //        offset += pos;
        //        yield return offset;
        //    }
        //}

        public static double[] GetDoubleArrayFromString(this string text, char separator, CultureInfo culture)
        {
            string[] tokens = text.Split(new char[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            double[] ret = new double[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
                ret[i] = double.Parse(tokens[i], culture);
            return ret;
        }

        public static string[] SplitBySpace(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return text.Split(new char[] { ' ' }, options);
        }

        public static string[] SplitByComma(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return text.Split(new char[] { ',' }, options);
        }
        public static string[] SplitByTab(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return text.Split(new char[] { '\t' }, options);
        }

        public static string[] SplitBySemicolon(this string text, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return text.Split(new char[] { ';' }, options);
        }

        // Convert the string to camel case.
        public static string ToCamelCase(this string text)
        {
            // If there are 0 or 1 characters, just return the string.
            if (text == null || text.Length < 2) return text;

            // Split the string into words.
            string[] words = text.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = words[0].ToLower();
            for (int i = 1; i < words.Length; i++)
            {
                result +=
                    words[i].Substring(0, 1).ToUpper() +
                    words[i].Substring(1);
            }

            return result;
        }

        // Convert the string to Pascal case.
        public static string ToPascalCase(this string text)
        {
            // If there are 0 or 1 characters, just return the string.
            if (text == null) return text;
            if (text.Length < 2) return text.ToUpper();

            // Split the string into words.
            string[] words = text.Split(
                new char[] { },
                StringSplitOptions.RemoveEmptyEntries);

            // Combine the words.
            string result = "";
            foreach (string word in words)
            {
                result +=
                    word.Substring(0, 1).ToUpper() +
                    word.Substring(1);
            }

            return result;
        }

        public static string ToTitleCase(this string text)
        {
            List<int> separatorPositions = text.CharacterPositions('-', ' ', '.', ',', ':', ';');

            char[] chars = text.ToCharArray();

            chars[0] = Char.ToUpper(chars[0]);
            for (int i = 0; i < separatorPositions.Count; i++)
            //turn next letter to uppercase
            {
                int pos = separatorPositions[i];
                if (pos < text.Length - 1) chars[pos + 1] = char.ToUpper(chars[pos + 1]);
            }
            return new string(chars);
        }

        public static string Repeat(this string text, int times)
        {
            //09-11-2013: Repeat the text times.
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < times; i++) sb.Append(text);
            return sb.ToString();
        }


        ///// <summary>
        ///// Returns true if the substring is contained within the source string.
        ///// </summary>
        ///// <param name="str"></param>
        ///// <param name="value"></param>
        ///// <param name="comparison"></param>
        ///// <returns></returns>
        //public static bool ContainsSubstring(this string str, string value, StringComparison comparison = StringComparison.CurrentCultureIgnoreCase)
        //{
        //    return str.IndexOf(value, comparison) > 0;
        //}

        public static bool EqualsOneOf(this string str,IEnumerable<string> values,StringComparison comparisonType )
        {
            foreach(string s in values)
                if (str.Equals(s, comparisonType)) return true;
            return false;
        }

        public static bool EqualsOneOf(this string str, IEnumerable<string> values)
        {
            return str.EqualsOneOf(values, StringComparison.CurrentCultureIgnoreCase);
        }

        public struct VariableAndUnit
        {
            public string Name, Unit;

            public override string ToString()
            {
                return string.Format("{0} [{1}]", Name, Unit);
            }
        }

        public static VariableAndUnit GetVariableNameAndUnit(this string text, char charBeforeUnit, char charAfterUnit, string emptyUnitText = "")
        {
            string sBeforeToken = CharacterNeedsEscapeInsideRegex(charBeforeUnit) ? "\\" + charBeforeUnit.ToString() : charBeforeUnit.ToString();
            string sAfterToken = CharacterNeedsEscapeInsideRegex(charAfterUnit) ? "\\" + charAfterUnit.ToString() : charAfterUnit.ToString();
            string pattern = @"\s*(?<name>\S([^" + sBeforeToken + sAfterToken + @"])+)\s*(" + sBeforeToken + "(?<unit>[^" + sBeforeToken + sAfterToken + "]*)" + sAfterToken + ")?\\s*";

            Match m = Regex.Match(text, pattern);
            if (m.Success)
            {
                VariableAndUnit v = new VariableAndUnit() { Name = m.Groups["name"].Value.Trim(), Unit = m.Groups["unit"].Value.Trim() };
                if (v.Unit.Length == 0) v.Unit = emptyUnitText;
                return v;
            }
            else return new VariableAndUnit(); //return null item
        }


        public static bool IsNumeric(this string text)
        {
            double tmp;
            return double.TryParse(text, out tmp);
        }

        public static bool IsNumeric(this string text, IFormatProvider provider)
        {
            double tmp;
            return double.TryParse(text,NumberStyles.Any,provider, out tmp);
        }

        public static string RemoveTextInParentheses(this string text, string toReplace="")
        {
            return new Regex(@" \(.+?\)").Replace(text, toReplace);
        }
    }
}
