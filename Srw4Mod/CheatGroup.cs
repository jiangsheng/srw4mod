using System.Text;

namespace Srw4Mod
{
    internal class CheatGroup
    {
        public CheatGroup(string name, CheatGroup? parentGroup) {
            Name = name; 
            ParentGroup = parentGroup;
        }
        public string Name { get; set; }
        public CheatGroup? ParentGroup { get; set; }
        public LinkedList<CheatEntry> Entries { get; set; }= new LinkedList<CheatEntry>();
        public List<CheatGroup> SubGroups { get; set; } = new List<CheatGroup>();
        public List<string> Comments { get; set; }=new List<string>();
        public string GetPath()
        { 
            List<string> paths = new List<string>();
            CheatGroup? cheatGroup = this;
            while (cheatGroup!=null)
            {
                paths.Add(cheatGroup.Name);
                cheatGroup= cheatGroup?.ParentGroup;
            }
            paths.Reverse();
            return string.Join('\\', paths);
        }
        public override string ToString()
        {
             var sb = new StringBuilder();
            FormatGroup(sb, "", true);
            return sb.ToString();
        }

        private void FormatGroup(StringBuilder sb, string indent, bool isLast)
        {
            sb.Append(indent);
            if (!string.IsNullOrEmpty(indent))
            {
                sb.Append(isLast ? "└── " : "├── ");
            }
            sb.AppendLine(Name);
            // Prepare indentation for children
            indent += isLast ? "    " : "│   ";
            if (Comments.Count > 0)
            {
                sb.AppendLine(indent + "Comments");

                for (int i = 0; i < Comments.Count; i++)
                {
                    FormatStringNode(sb, indent, Comments[i],i == Comments.Count - 1);
                }
            }
            if (Entries.Count > 0)
            {
                sb.AppendLine(indent + "Entries");
                var entryNode = Entries.First;
                while (entryNode != null) {
                    var entry = entryNode.Value;
                    entry.FormatEntry(sb,indent, entryNode.Next==null);
                    entryNode = entryNode.Next;
                }
            }
            if (SubGroups.Count > 0)
            {
                sb.AppendLine(indent + "SubGroups");

                for (int i = 0; i < SubGroups.Count; i++)
                {
                    SubGroups[i].FormatGroup(sb, indent, i == SubGroups.Count - 1);
                }
            }
        }

        public static void FormatStringNode(StringBuilder sb, string indent, string name, bool isLast)
        {
            sb.Append(indent);

            if (!string.IsNullOrEmpty(indent))
            {
                sb.Append(isLast ? "└── " : "├── ");
            }
            sb.AppendLine(name);
        }
    }
}
