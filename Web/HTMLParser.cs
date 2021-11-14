using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Paulus.Web
{ 
    public static class HtmlParser
    {
        //example:
        //will return only the texts inside the tags:
        //will return the list {"asrkodasdas","sdasd"}
        //string text=@"<div id=""test"">asrkodasdas</div></div><div id=""test"">sdasd</div></div>";
        //var tags=HTMLParser.getTokenPairContents(text,@"<div id=""test"">","</div>");

        //need to specify the exact start tag, and the exact end tag as tokens
        public static List<string> getTokenPairContents(string text, string startToken, string endToken)
        {

            string pattern = startToken + @"(?<content>[\s\S]*?)" + endToken;
            //@"<div class=""summary"">(?<content>[\s\S]*?)</div>";

            List<string> list = new List<string>();

            MatchCollection mc = Regex.Matches(text, pattern);
            foreach (Match m in mc)
                list.Add(m.Groups["content"].Value);

            return list;
        }

        //example:
        //allows tags that may contain multiple attributes
        //will return the list {"asrkodasdas","sdasd"}
        //string text=@"<div id=""test"">asrkodasdas</div></div><div id=""tt"">sdasd</div></div>";
        //var tags=HTMLParser.getHTMLTagContents(text,"div");
        public static List<string> getHTMLTagContents(string text, string startEndTag)
        {
            //string pattern = @"<a([\s\S]*?)>" + @"(?<content>[\s\S]*?)"  + "</a>";
            string pattern = "<" + startEndTag + @"([\s\S]*?)>" + @"(?<content>[\s\S]*?)" + @"</" + startEndTag + ">";

            List<string> list = new List<string>();
            MatchCollection mc = Regex.Matches(text, pattern);

            foreach (Match m in mc)
                list.Add(m.Groups["content"].Value);

            return list;
        }
    }
}
