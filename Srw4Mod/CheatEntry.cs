using Castle.Components.DictionaryAdapter.Xml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Srw4Mod
{
    internal class CheatEntry
    {
        public CheatEntry(CheatGroup parentGroup)
        {        
            ParentGroup = parentGroup;
        }

        public List<string> Codes { get; set; } = new List<string>();
        public List<string> Comments { get; set; } = new List<string>();
        public CheatGroup ParentGroup { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var code in Codes)
            {
                sb.AppendLine(string.Format("Code:{0}", code));
            }
            foreach (var comment in Comments)
            {
                Debug.Assert(!comment.Contains((char)0xfe0f));
            }
            return sb.ToString();
        }

        internal void FormatEntry(StringBuilder sb, string indent, bool isLast)
        {
            sb.Append(indent);
            if (!string.IsNullOrEmpty(indent))
            {
                sb.Append(isLast ? "└── " : "├── ");
            }
            if (Codes.Count > 0)
            {
                sb.AppendLine("Codes");

                for (int i = 0; i < Codes.Count; i++)
                {
                    CheatGroup.FormatStringNode(sb, indent, Codes[i], i == Codes.Count - 1);
                }
            }
            if (Comments.Count > 0)
            {
                sb.AppendLine(indent + "Comments");

                for (int i = 0; i < Comments.Count; i++)
                {
                    CheatGroup.FormatStringNode(sb, indent, Comments[i], i == Comments.Count - 1);
                }
            }
        }
    }
}
