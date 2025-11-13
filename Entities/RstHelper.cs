using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class RstHelper
    {
        public static void AppendHeader(StringBuilder stringBuilder, string header, char seperator)
        {
            string seperatorLine = new string(seperator, header.Length * 2);
            stringBuilder.AppendLine(seperatorLine);
            stringBuilder.AppendLine(header);
            stringBuilder.AppendLine(seperatorLine);
        }

        internal static string? GetComments(
            Dictionary<string, string> comments,string label)
        {
            if (!comments.ContainsKey(label)) return "\r\n";
            var content = comments[label].Replace("<BR>", "\r\n");
            return content;
        }

        internal static string GetLabelName(string? englishName)
        {
           StringBuilder result=new StringBuilder(englishName);
            result = result.Replace(" ","_");
            result = result.Replace("-", "_");
            result = result.Replace("(",string.Empty);
            result = result.Replace(")", string.Empty);
            return result.ToString().ToLowerInvariant();
        }
    }
}
