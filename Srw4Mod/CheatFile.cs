using Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Collections.Specialized.BitVector32;

namespace Srw4Mod
{
    internal class CheatFile
    {
        public List<CheatGroup> CheatGroups { get; set; } = new List<CheatGroup>();  
        CheatFile() { }

        public static CheatFile CreateFromUrl(string url)
        {
            CheatFile result=new CheatFile();
            using (WebClient webClient = new WebClient())
            {
                var text = webClient.DownloadString(url);
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(text);
                var snesCheatSection= doc.DocumentNode.SelectSingleNode("//section[h2[a[text()='第四次金手指']]]");
                CheatGroup cheatGroup = new CheatGroup("第四次金手指", null);
                ParseSection(3, snesCheatSection, cheatGroup);
                result.CheatGroups.Add(cheatGroup);
                var playStationCheatSection = doc.DocumentNode.SelectSingleNode("//section[h2[a[text()='第四次S金手指']]]");
                cheatGroup = new CheatGroup("第四次S金手指", null);
                ParseSection(3, playStationCheatSection, cheatGroup);
                result.CheatGroups.Add(cheatGroup);

            }
            return result;
        }

        private static void ParseSection(int headerLevel, HtmlNode sectionNode, CheatGroup parent)
        {
            var subsections = sectionNode.SelectNodes("./section");
            if (subsections == null) return;
            foreach (var subsection in subsections)
            {
                var cheatGroupSubsectionHeader = subsection.SelectSingleNode(string.Format("./h{0}/a", headerLevel));
                if (cheatGroupSubsectionHeader == null) continue;
                var subsectionTitle = cheatGroupSubsectionHeader.InnerText;

                var cheatGroupSubsection=new CheatGroup(subsectionTitle, parent);
                ParseGrid(subsection,cheatGroupSubsection);
                ParseSection(headerLevel + 1, subsection, cheatGroupSubsection);
                parent.SubGroups.Add(cheatGroupSubsection);
            }
        }

        private static void ParseGrid(HtmlNode subsection, CheatGroup parent)
        {
            var subsectionCheatTableRow = subsection.SelectSingleNode("./div[contains(@class, 'sd-container-fluid')]/div[contains(@class, 'sd-row')]");
            if (subsectionCheatTableRow != null)
            {
                var subsectionCheatTableColumns = subsectionCheatTableRow.SelectNodes("./div[contains(@class, 'sd-col')]");
                if (subsectionCheatTableColumns != null)
                {
                    foreach (var subsectionCheatTableColumn in subsectionCheatTableColumns)
                    {
                        var cardBody = subsectionCheatTableColumn.SelectSingleNode("./div[contains(@class, 'sd-card')]/div[contains(@class, 'sd-card-body')]");
                        if (cardBody != null)
                        {
                            var cardTitle = cardBody.SelectSingleNode("./div[contains(@class, 'sd-card-title')]");
                            if (cardTitle == null) continue;
                            var cardTitleText = cardTitle.InnerText.Trim();
                            var cardLineBlock = cardBody.SelectSingleNode("./div[contains(@class, 'line-block')]");

                            CheatGroup cheatGroup = new CheatGroup(cardTitleText, parent);
                            if (cardLineBlock == null)
                            {
                                var cardText = cardBody.SelectSingleNode("./div[contains(@class, 'sd-card-text')]");
                                if (cardText == null) continue;
                                AddCardToGroup(cheatGroup, cardTitleText, cardText.InnerText);
                            }
                            else
                            {
                                var cardLines = cardLineBlock.SelectNodes("./div[contains(@class, 'line')]");
                                if (cardLines != null)
                                {
                                    AddCardToGroup(cheatGroup, cardTitleText, cardLines.Select(line => line.InnerText).ToList());
                                }
                            }
                            parent.SubGroups.Add(cheatGroup);
                        }
                    }
                }
            }
        }

        private static void AddCardToGroup(CheatGroup parent, string cardTitleText, List<string> list)
        {
            bool foundFirstCode = false;
            CheatGroup? lastCheatGroup = null;
            bool isLastLineCheatCode = false;

            for (int i = 0; i < list.Count; i++)
            {
                var line = list[i];
                CheatCodeParseResult cheatCodeParseResult = TryParseCheatCode(line);
                if (!string.IsNullOrEmpty(cheatCodeParseResult.CheatCode))
                {
                    //this line is code
                    if (isLastLineCheatCode)
                    {
                        //last line is code and this line is code
                        //add this line to the code section of last group
                        Debug.Assert(lastCheatGroup != null);
                        var lastCheatEntry = lastCheatGroup.Entries.Last;
                        Debug.Assert(lastCheatEntry != null);
                        if (lastCheatEntry != null && lastCheatEntry.Value != null)
                        {
                            lastCheatEntry.Value.Codes.Add(cheatCodeParseResult.CheatCode);

                            if(!string.IsNullOrWhiteSpace(cheatCodeParseResult.Comment))
                                lastCheatEntry.Value.Comments.Add(cheatCodeParseResult.Comment);
                        }
                    }
                    else
                    {
                        //last line is comments or null, and this line is code
                        if (!foundFirstCode)
                        {
                            //last line is null, and this line is code
                            //this flag is only used when dealing comments
                            foundFirstCode = true;
                        }
                        CheatGroup cheatSubGroup = new CheatGroup(string.Empty, parent);
                        CheatEntry cheatEntry = new CheatEntry(cheatSubGroup);
                        cheatEntry.Codes.Add(cheatCodeParseResult.CheatCode);
                        if (!string.IsNullOrWhiteSpace(cheatCodeParseResult.Comment))
                            cheatEntry.Comments.Add(cheatCodeParseResult.Comment);
                        cheatSubGroup.Entries.AddLast(
                            new LinkedListNode<CheatEntry>(
                                cheatEntry
                            ));
                        parent.SubGroups.Add(cheatSubGroup);
                        lastCheatGroup = cheatSubGroup;

                    }
                }
                else
                {
                    //this line is comment
                    if (!foundFirstCode)
                    {
                        parent.Comments.Add(line);
                    }
                    else {
                        if (lastCheatGroup != null)
                        {
                            var lastCheatEntry = lastCheatGroup.Entries.Last;
                            Debug.Assert(lastCheatEntry != null);
                            if (lastCheatEntry != null && lastCheatEntry.Value != null)
                            {
                                lastCheatEntry.Value.Comments.Add(line);
                            }
                        }
                    }
                }
                isLastLineCheatCode = !string.IsNullOrWhiteSpace(cheatCodeParseResult.CheatCode);
            }
            if (parent.SubGroups.Count == 1)
            {
                //do not add this group to parent when it is single entry
                //add the entry instead
                var singleEntry = parent.SubGroups[0];
                foreach (var entry in singleEntry.Entries)
                {
                    parent.Entries.AddLast(entry);
                }
                parent.Comments.AddRange(singleEntry.Comments);
                parent.SubGroups.Remove(singleEntry);
            }
        }
        static Regex regexSnesCode1 = new Regex(@"^([0-9A-Fa-f]{6}=[0-9A-Fa-f]{2}\?[0-9A-Fa-f]{2})(.*)", RegexOptions.Compiled |
    RegexOptions.IgnoreCase);
        static Regex regexSnesCode2 = new Regex(@"^([A-Fa-f0-9]{6}=[A-Fa-f0-9]{2})(.*)", RegexOptions.Compiled |RegexOptions.IgnoreCase);


        static Regex regexDuckStationCode1 = new Regex(@"^[0-9A-Fa-f]{8}\s*?:[0-9A-Fa-f]{4}|[0-9A-Fa-f]{8}(.*)$", RegexOptions.Compiled |
            RegexOptions.IgnoreCase);

        static Regex regex8BitHex = new Regex(@"^[0-9A-Fa-f]{8}(.*$)", RegexOptions.Compiled |
            RegexOptions.IgnoreCase);


        private static CheatCodeParseResult TryParseCheatCode(string line)
        {
            Match regexSnesCode1Match= regexSnesCode1.Match(line);
            if (regexSnesCode1Match.Success)
            {
                string code = string.Empty;
                if (regexSnesCode1Match.Groups.Count > 1)
                {
                    code = regexSnesCode1Match.Groups[1].Value;
                }
                string comment = string.Empty;
                if (regexSnesCode1Match.Groups.Count > 2)
                {
                    comment= regexSnesCode1Match.Groups[2].Value;
                }
                return new CheatCodeParseResult
                {
                    CheatCode = code,
                    Comment = comment
                };
            }

            Match regexSnesCode2Match = regexSnesCode2.Match(line);
            if (regexSnesCode2Match.Success)
            {
                string code = string.Empty;
                if (regexSnesCode2Match.Groups.Count > 1)
                {
                    code = regexSnesCode2Match.Groups[1].Value;
                }
                string comment = string.Empty;
                if (regexSnesCode1Match.Groups.Count > 2)
                {
                    comment = regexSnesCode2Match.Groups[2].Value;
                }
                return new CheatCodeParseResult
                {
                    CheatCode = code,
                    Comment = comment
                };
            }

            Match regexDuckStationCode1Match = regexDuckStationCode1.Match(line);
            if (regexDuckStationCode1Match.Success)
            {
                var captures = regexDuckStationCode1Match.Captures;
                if (captures.Count > 1)
                {
                    var comment = captures[2].Value;
                    int indexComments = line.IndexOf(comment);
                    var code = line.Substring(0, indexComments);
                    return new CheatCodeParseResult
                    {
                        CheatCode = code,
                        Comment = comment
                    };
                }
                else return new CheatCodeParseResult
                {
                    CheatCode = line,
                    Comment = string.Empty
                };
            }
            if (regex8BitHex.IsMatch(line))
            {
                Debug.WriteLine("Potential Cheat Code found:" + line);

            }
            return new CheatCodeParseResult
            {
                CheatCode = string.Empty,
                Comment = line
            };
        }

    
        private static void AddCardToGroup(CheatGroup parentGroup, string cardTitleText, string code)
        {
            AddCardToGroup(parentGroup, cardTitleText, new List<string> { code });
        }

        internal void WriteToFile(string snes9xCheatFileName, string bsnesCheatFileName, string duckstationCheatFileName)
        {
            StringBuilder sbSnes9x = new StringBuilder();
            StringBuilder sbBsnes = new StringBuilder();
            StringBuilder sbDuckStation = new StringBuilder();
            
            foreach (var cheatGroup in CheatGroups)
            {
                if (string.Compare(cheatGroup.Name, "第四次S金手指", StringComparison.Ordinal) == 0)
                {
                    //Debug.WriteLine(cheatGroup.ToString());
                    WriteCheatGroup(sbDuckStation, cheatGroup,true);
                }
                if (string.Compare(cheatGroup.Name, "第四次金手指", StringComparison.Ordinal) == 0)
                {
                    
                    WriteCheatGroup(sbBsnes, cheatGroup,false);
                    WriteCheatGroup(sbSnes9x, cheatGroup, false);
                }
            }
            if (sbDuckStation.Length > 0)
            {
                File.WriteAllText(duckstationCheatFileName, sbDuckStation.ToString());
            }
            if (sbSnes9x.Length > 0)
            {
                File.WriteAllText(snes9xCheatFileName, sbSnes9x.ToString());
            }
            if (sbBsnes.Length > 0)
            {
                File.WriteAllText(bsnesCheatFileName, sbBsnes.ToString());
            }
        }
        private static void WriteCheatGroup(StringBuilder stringBuilder, CheatGroup cheatGroup, bool isPlayStation)
        {           
            if (cheatGroup.Entries.Count > 0)
            {
                if (!string.IsNullOrWhiteSpace(cheatGroup.Name))
                {
                    if (isPlayStation)
                    {
                        stringBuilder.AppendLine(string.Format("[{0}]", cheatGroup.GetPath().Replace("第四次S金手指\\", string.Empty)));
                        stringBuilder.AppendLine("Type = Gameshark");
                        stringBuilder.AppendLine("Activation = EndFrame");
                    }
                    else {
                        stringBuilder.AppendLine("cheat");
                        stringBuilder.AppendLine(string.Format("  name: {0}", cheatGroup.GetPath().Replace("第四次金手指\\", string.Empty)));
                    }
                }
                if (isPlayStation)
                {
                    foreach (var comment in cheatGroup.Comments)
                    {
                        stringBuilder.AppendLine(comment);
                    }

                    foreach (var cheatEntry in cheatGroup.Entries)
                    {
                        foreach (var code in cheatEntry.Codes)
                        {
                            stringBuilder.AppendLine(code);
                        }
                        foreach (var comments in cheatEntry.Comments)
                        {
                            stringBuilder.AppendLine(comments.Replace("[", string.Empty).Replace("]", string.Empty));
                        }
                    }
                }
                else {
                    StringBuilder codes = new StringBuilder();
                    StringBuilder comments = new StringBuilder();
                    foreach (var cheatEntry in cheatGroup.Entries)
                    {
                        foreach (var code in cheatEntry.Codes)
                        {
                            if (codes.Length > 0)
                                codes.Append('+');
                            codes.Append(code);
                        }
                        foreach (var comment in cheatEntry.Comments)
                        {
                            if (comments.Length > 0)
                                comments.Append('+');
                            comments.Append(comment.Replace("+", "⊕"));
                        }
                        stringBuilder.Append("  code: ");
                        stringBuilder.AppendLine(codes.ToString());
                        if (comments.Length > 0)
                        {
                            stringBuilder.Append("  comments: ");
                            stringBuilder.AppendLine(comments.ToString());
                        }
                    }
                    stringBuilder.AppendLine();
                }
            }
            else
            {
                var anySubGroupHasName = cheatGroup.SubGroups.Select(g => g.Name).Any(name => !string.IsNullOrWhiteSpace(name));
                if (isPlayStation)
                {
                    if(!anySubGroupHasName)
                    {
                        //this is a card
                        if (!string.IsNullOrWhiteSpace(cheatGroup.Name))
                        {
                            stringBuilder.AppendLine(string.Format("[{0}]", cheatGroup.GetPath().Replace("第四次S金手指\\", string.Empty)));
                            stringBuilder.AppendLine("Type = Gameshark");
                            stringBuilder.AppendLine("Activation = EndFrame");
                        }

                    }
                    foreach (var cheatSubGroup in cheatGroup.SubGroups)
                    {
                        WriteCheatGroup(stringBuilder, cheatSubGroup, isPlayStation);
                    }
                }
                else if (!anySubGroupHasName)
                {
                    //this is a card
                    //as snes9x and bsnes has no concept of multi-line cheats 
                    //the whole card needs to be placed in the same cheat code
                    StringBuilder codes = new StringBuilder();
                    StringBuilder comments = new StringBuilder();
                    if (!string.IsNullOrWhiteSpace(cheatGroup.Name))
                    {
                        stringBuilder.AppendLine("cheat");
                        stringBuilder.Append("  name: ");
                        stringBuilder.AppendLine(cheatGroup.GetPath().Replace("第四次金手指\\", string.Empty));
                    }
                    if (cheatGroup.Comments.Count > 0)
                    {
                        foreach (var comment in cheatGroup.Comments)
                        {
                            if (comments.Length > 0)
                                comments.Append('+');
                            comments.Append(comment.Replace("+", "⊕"));
                        }
                    }
                    //unnamed groups within the same card
                    if (cheatGroup.SubGroups.Count > 0)
                    {
                        foreach (var cheatSubGroup in cheatGroup.SubGroups)
                        {
                            foreach (var cheatEntry in cheatSubGroup.Entries)
                            {
                                foreach (var code in cheatEntry.Codes)
                                {
                                    if (codes.Length > 0)
                                        codes.Append('+');
                                    codes.Append(code);
                                }
                                foreach (var comment in cheatEntry.Comments)
                                {
                                    if (comments.Length > 0)
                                        comments.Append('+');
                                    comments.Append(comment.Replace("+", "⊕"));
                                }
                            }
                        }
                    }
                    if (codes.Length > 0)
                    {
                        stringBuilder.Append("  code: ");
                        stringBuilder.AppendLine(codes.ToString());
                    }
                    if (comments.Length > 0)
                    {
                        stringBuilder.Append("  comments: ");
                        stringBuilder.AppendLine(comments.ToString());
                    }
                    stringBuilder.AppendLine();
                }
                else
                {
                    foreach (var cheatSubGroup in cheatGroup.SubGroups)
                    {
                        WriteCheatGroup(stringBuilder, cheatSubGroup, isPlayStation);
                    }
                }
            }
        }
        public void ConvertPlayStationCheatsToSnes(string str,Rom snesRom, Rom playstationRom)
        {
            foreach (var cheatGroup in CheatGroups)
            {
                if (string.Compare(cheatGroup.Name, "第四次S金手指", StringComparison.Ordinal) == 0)
                {
                    foreach (var subGroup in cheatGroup.SubGroups)
                    {
                        if (string.Compare(subGroup.Name, "机师修改", StringComparison.Ordinal) == 0)
                        {
                            foreach (var subGroup1 in subGroup.SubGroups)
                            {

                            }
                        }
                    }
                }
            }

        }
    }
}
