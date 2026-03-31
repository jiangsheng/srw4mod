using Castle.Components.DictionaryAdapter.Xml;
using Entities;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Reflection.Metadata;
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
        public string? SourceCode { get; set; }
        CheatFile() { }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in CheatGroups)
            {
                stringBuilder.AppendLine(item.ToString());
            }
            return stringBuilder.ToString();
        }

        public static CheatFile CreateFromUrl(string url)
        {
            CheatFile result = new CheatFile();
            using (WebClient webClient = new WebClient())
            {
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(webClient.DownloadString(url));
                var snesCheatSection = doc.DocumentNode.SelectSingleNode("//section[h2[a[text()='第四次金手指']]]");
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

                var cheatGroupSubsection = new CheatGroup(subsectionTitle, parent);
                ParseGrid(subsection, cheatGroupSubsection);
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

                            if (!string.IsNullOrWhiteSpace(cheatCodeParseResult.Comment))
                                lastCheatEntry.Value.Comments.Add(cheatCodeParseResult.Comment);
                        }
                    }
                    else
                    {
                        Debug.Assert(!line.Contains((char)0xfef));

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
                    Debug.Assert(!line.Contains((char)0xfef));
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

        private const string arrowChar = "→";
        static Regex regexSnesCode1 = new Regex(@"^([0-9A-Fa-f]{6}=[0-9A-Fa-f]{2}\?[0-9A-Fa-f]{2})(.*)", RegexOptions.Compiled |
    RegexOptions.IgnoreCase);
        static Regex regexSnesCode2 = new Regex(@"^([A-Fa-f0-9]{6}=[A-Fa-f0-9]{2})(.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        static Regex regexDuckStationCode1 = new Regex(@"^[0-9A-Fa-f]{8}\s*?:[0-9A-Fa-f]{4}|[0-9A-Fa-f]{8}(.*)$", RegexOptions.Compiled |
            RegexOptions.IgnoreCase);

        static Regex regex8BitHex = new Regex(@"^[0-9A-Fa-f]{8}(.*$)", RegexOptions.Compiled |
            RegexOptions.IgnoreCase);


        private static CheatCodeParseResult TryParseCheatCode(string line)
        {
            Match regexSnesCode1Match = regexSnesCode1.Match(line);
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
                    comment = regexSnesCode1Match.Groups[2].Value;
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
                    WriteCheatGroup(sbDuckStation, cheatGroup, true);
                }
                if (string.Compare(cheatGroup.Name, "第四次金手指", StringComparison.Ordinal) == 0)
                {

                    WriteCheatGroup(sbBsnes, cheatGroup, false);
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
                    if (!anySubGroupHasName)
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
        public void ConvertPlayStationCheatsToSnes(string fileName, Rom snesRom, Rom playstationRom)
        {
            var lines = this.SourceCode?.Split("\r\n")?.ToList();
            if (lines == null || lines.Count == 0) return;

            var playStationCheatGroups = CheatGroups.Where(g => g.Name == "第四次S金手指").First();
            var pilotChanges = playStationCheatGroups.SubGroups.Where(g => g.Name == "机师修改").First();
            lines = ConvertPlayStationCheatGroupToSnes(lines, pilotChanges, "srw4_cheat_pilots_snes_begin", "srw4_cheat_pilots_snes_end", snesRom.RomOffsets.Pilots.DataOffset, playstationRom.RomOffsets.Pilots.DataOffset, EntityType.Pilot, snesRom, playstationRom);
            var unitChanges = playStationCheatGroups.SubGroups.Where(g => g.Name == "机体修改").First();
            lines = ConvertPlayStationCheatGroupToSnes(lines, unitChanges, "srw4_cheat_units_snes_begin", "srw4_cheat_units_snes_", snesRom.RomOffsets.Units.DataOffset
                , playstationRom.RomOffsets.Units.DataOffset, EntityType.Unit, snesRom, playstationRom);
            var weaponChanges = playStationCheatGroups.SubGroups.Where(g => g.Name == "武器修改").First();

            lines = ConvertPlayStationCheatGroupToSnes(lines, weaponChanges, "srw4_cheat_weapons_snes_begin", "srw4_cheat_weapons_snes_end", snesRom.RomOffsets.Weapons.DataOffset, playstationRom.RomOffsets.Weapons.DataOffset, EntityType.Weapon, null, null);
            var chipChanges = playStationCheatGroups.SubGroups.Where(g => g.Name == "芯片修改").First();

            lines = ConvertPlayStationCheatGroupToSnes(lines, chipChanges, "srw4_cheat_chips_snes_begin", "srw4_cheat_chips_snes_end", snesRom.RomOffsets.Chips, playstationRom.RomOffsets.Chips, EntityType.Chip, null, null);

            var specialtyChanges = playStationCheatGroups.SubGroups.Where(g => g.Name == "机体特殊技能修改").First();

            lines = ConvertPlayStationCheatGroupToSnes(lines, specialtyChanges, "srw4_cheat_unit_specialty_snes_begin", "srw4_cheat_unit_specialty_snes_end", snesRom.RomOffsets.Units.DataOffset
                , playstationRom.RomOffsets.Units.DataOffset, EntityType.Unit, snesRom, playstationRom);
            File.WriteAllLines(fileName, lines);

        }



        private List<string> ConvertPlayStationCheatGroupToSnes(List<string> lines, CheatGroup cheatGroup, string markerBegin
            , string markerEnd, int dataOffsetSnes, int dataOffsetPlayStation, EntityType entityType, Rom? snesRom, Rom? playstationRom)
        {
            List<string> result = new List<string>();
            bool isReplacing = false;
            List<string> replaced = ConvertSubGroupsToSnes(cheatGroup, dataOffsetSnes, dataOffsetPlayStation, entityType, snesRom, playstationRom);
            foreach (var line in lines)
            {
                if (isReplacing)
                {
                    if (line.Contains(markerEnd))
                    {
                        isReplacing = false;
                        result.Add(line);
                    }
                    else
                        continue;
                }
                else
                {
                    if (line.Contains(markerBegin))
                    {
                        isReplacing = true;
                        result.Add(line);
                        result.Add(string.Empty);
                        result.AddRange(replaced);
                    }
                    else
                    {
                        result.Add(line);
                    }
                }
            }
            Debug.Assert(result.Contains(".. _srw4_cheat_unit_specialty_ps_end:"));
            return result;
        }

        private List<string> ConvertSubGroupsToSnes(CheatGroup parentGroup, int dataOffsetSnes,
            int dataOffsetPlayStation, EntityType entityType, Rom? snesRom,
            Rom? playstationRom, bool isTransform = false
            )
        {

            List<string> result = new List<string>();

            switch (parentGroup.Name)
            {
                default:
                    result.Add(".. grid::");
                    result.Add(string.Empty);
                    foreach (var group in parentGroup.SubGroups)
                    {
                        result.Add(string.Format("    .. grid-item-card:: {0}", group.Name));
                        result.Add("      :columns: auto\r\n");
                        INamedItem groupEntity = null;
                        if (group.Comments != null)
                        {
                            foreach (var comment in group.Comments)
                            {
                                result.Add(string.Format("      | {0}", comment));

                            }
                        }
                        switch (entityType)
                        {
                            case EntityType.Pilot:
                                {
                                    //additional skills added in ps remake, so addresses are different
                                    var pilotName = group.Name;
                                    var pilot = playstationRom?.Pilots.Where(
                                        p => p.Name == pilotName ||
                                        p.CallSign == pilotName.Replace("ー", "－") ||
                                        p.DisplayName == pilotName.Replace("·", "＝").Replace("ー", "－")).FirstOrDefault();
                                    Debug.Assert(pilot != null);
                                    groupEntity = pilot;
                                    dataOffsetPlayStation = playstationRom.RomOffsets.Pilots.IndexedLocations[pilot.Id];
                                    dataOffsetSnes = snesRom.RomOffsets.Pilots.IndexedLocations[pilot.Id];
                                }
                                break;
                            case EntityType.Unit:
                                {
                                    var unitName = group.Name;
                                    Unit unit = GetUnitByName(playstationRom, unitName);

                                    Debug.Assert(unit != null);
                                    groupEntity = unit;
                                    dataOffsetPlayStation = playstationRom.RomOffsets.Units.IndexedLocations[unit.Id];
                                    dataOffsetSnes = snesRom.RomOffsets.Units.IndexedLocations[unit.Id];
                                }
                                break;
                            case EntityType.Weapon:
                                {
                                    var weaponName = group.Name;
                                    switch (weaponName)
                                    {
                                        case "ドリルテンペスト":
                                            //真·ゲッター2 has weapon changed in the playstation remake
                                            //ミラ－ジュドリル was removed
                                            //and ドリルテンペスト took its place
                                            //apply cheat code to ミラ－ジュドリル instead
                                            dataOffsetSnes += 0x10;
                                            break;
                                    }
                                    groupEntity = GetWeaponByName(playstationRom, weaponName);

                                }
                                break;
                        }

                        foreach (var subGroup in group.SubGroups)
                        {
                            Debug.Assert(string.IsNullOrEmpty(subGroup.Name));
                            ConvertEntries(result, subGroup.Entries, dataOffsetSnes, dataOffsetPlayStation, entityType, groupEntity, playstationRom, isTransform);
                        }
                        ConvertEntries(result, group.Entries, dataOffsetSnes, dataOffsetPlayStation, entityType, groupEntity, playstationRom, isTransform);
                        result.Add(string.Empty);
                    }
                    break;
                case "机体特殊技能修改":
                    foreach (var subgroup in parentGroup.SubGroups)
                    {
                        string header = subgroup.Name;
                        string seperatorLine = new string('"', header.Length * 2);
                        result.Add(seperatorLine);
                        result.Add(header);
                        result.Add(seperatorLine);
                        result.Add(string.Empty);
                        result.AddRange(ConvertSubGroupsToSnes(subgroup, dataOffsetSnes, dataOffsetPlayStation, entityType, snesRom, playstationRom
                            , header.Contains("变形")));
                    }
                    break;
            }
            return result;
        }

        private static Weapon? GetWeaponByName(Rom? playstationRom, string weaponName)
        {
            return playstationRom?.Weapons?.Where(
                u => u.Name == weaponName).FirstOrDefault();
        }

        private static Unit GetUnitByName(Rom? playstationRom, string unitName)
        {
            Unit unit;
            if (unitName.EndsWith("变形")|| unitName.EndsWith("分离"))
            {
                var realUnitName = unitName.Substring(0, unitName.Length - 2);
                unit = playstationRom.Units.Where(
                u => u.Name == realUnitName ||
                u.DisplayName == realUnitName.Replace("ー", "－").Replace("·", "＝") ||
                u.ChineseName == realUnitName ||
                u.EnglishName == realUnitName).FirstOrDefault();
            }
            else if (unitName.StartsWith("机体0x"))
            {
                var unitIdString = unitName.Substring(4, 2);
                var unitId = int.Parse(unitIdString, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                unit = playstationRom.Units.Where(u => u.Id == unitId).FirstOrDefault();
            }
            else
                unit = playstationRom.Units.Where(
                u => u.Name == unitName ||
                u.DisplayName == unitName.Replace("ー", "－").Replace("·", "＝") ||
                u.ChineseName == unitName ||
                u.EnglishName == unitName).FirstOrDefault();
            return unit;
        }

        private static void ConvertEntries(List<string> result, LinkedList<CheatEntry> entries, int dataOffsetSnes, int dataOffsetPlayStation, EntityType entityType, INamedItem? groupEntity, Rom? playstationRom, bool isTransform = false)
        {
            foreach (var entry in entries)
            {
                foreach (var code in entry.Codes)
                {
                    var codeParts = code.Split(" ");
                    var opCode = codeParts[0].Substring(0, 2);
                    var addressInHex = codeParts[0].Substring(2, 6);
                    var valueInHex = codeParts[1];
                    var value = int.Parse(valueInHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                    var address = int.Parse(addressInHex, NumberStyles.HexNumber, CultureInfo.InvariantCulture) - 0x20000;
                    var addressOfset = address - dataOffsetPlayStation;
                    var firstComment = entry.Comments.Count > 0 ? entry.Comments[0] : string.Empty;
                    var secondComment = entry.Comments.Count > 1 ? entry.Comments[1] : string.Empty;
                    var thirdComment = entry.Comments.Count > 2 ? entry.Comments[2] : string.Empty;
                    if (secondComment.StartsWith(arrowChar) || firstComment.EndsWith(arrowChar))
                    {
                        //playstation pilots has extra byte after face 
                        firstComment = firstComment + secondComment;
                        secondComment = string.Empty;
                    }
                    if (thirdComment.StartsWith(arrowChar) || secondComment.EndsWith(arrowChar))
                    {
                        firstComment = firstComment + secondComment + thirdComment;
                        thirdComment = secondComment = string.Empty;
                    }
                    switch (entityType)
                    {
                        case EntityType.Pilot:
                            {
                                if (addressOfset > 0)
                                {
                                    addressOfset -= 1;
                                    var pilot = groupEntity as Pilot;
                                    ValidatePilotCode(opCode, firstComment, secondComment, address, value, pilot);
                                }

                            }
                            break;
                        case EntityType.Unit:
                            {
                                var unit = groupEntity as Unit;
                                if (playstationRom != null)
                                    ValidateUnitCode(opCode, firstComment, secondComment, thirdComment, address, value, unit, playstationRom, isTransform);
                            }
                            break;
                        case EntityType.Weapon:
                            {
                                ValidateWeaponCode(opCode, firstComment, secondComment, address, value);
                            }
                            break;

                    }
                    var snesAddresOfset = dataOffsetSnes + addressOfset;
                    var snesAddress = snesAddresOfset + 0xC00000;
                    switch (opCode)
                    {
                        case "30"://single byte write
                            Debug.Assert(value < 0x100);
                            result.Add(string.Format("      | {0:X6}={1:X2}",
                                 snesAddress, value));
                            break;
                        case "80"://double byte write
                            result.Add(string.Format("      | {0:X6}={1:X2}",
                                snesAddress, value % 256));
                            result.Add(string.Format("      | {0:X6}={1:X2}",
                                snesAddress + 1, value / 256));
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
                foreach (var comment in entry.Comments)
                {
                    result.Add(string.Format("      | {0}",
                                comment));
                }
            }
        }

        private static void ValidateWeaponCode(string opCode, string firstComment, string secondComment, int address, int value)
        {
            if (firstComment.Contains(arrowChar))
            {
                var firstLineParts = firstComment.Split(arrowChar);
                var attributeName = firstLineParts[0].Trim();
                switch (attributeName)
                {
                    case "弹数":
                        {
                            Debug.Assert((address & 0xF) == 0xC);
                            Debug.Assert(opCode == "30");
                        }
                        break;
                    case "射程":
                        {
                            switch (opCode)
                            {
                                case "30":
                                    Debug.Assert((address & 0xF) == 0xA);
                                    break;
                                case "80":
                                    Debug.Assert((address & 0xF) == 0x9);
                                    break;
                            }
                        }
                        break;
                    case "战斗动画":
                        {
                            Debug.Assert((address & 0xF) == 0x3);
                            Debug.Assert(opCode == "80");
                        }
                        break;
                    case "地形适应":
                        {
                            Debug.Assert((address & 0xF) == 0xB);
                            Debug.Assert(opCode == "30");
                        }
                        break;
                    case "伤害":
                        {
                            Debug.Assert((address & 0xF) == 0x5);
                            Debug.Assert(opCode == "80");
                        }
                        break;
                    case "台词":
                        {
                            Debug.Assert((address & 0xF) == 0x2);
                            Debug.Assert(opCode == "30");
                        }
                        break;
                    case "Ⓟ":
                        {
                            Debug.Assert((address & 0xf) == 0x1);
                            Debug.Assert(opCode == "30");
                        }
                        break;
                }
            }
        }

        static UnitWeapon? FindUnitWeapon(Unit? unit, string firstComment)
        {
            var weaponName = Regex.Replace(firstComment, @"\([^)]*\)", string.Empty).Replace(((char)0xfe0f).ToString(), string.Empty);
            return unit.Weapons?.Where(w => string.Compare(w.Weapon.Name.Replace("🗺", string.Empty).Replace("－", "ー")
                , weaponName.Replace("－", "ー"),
                StringComparison.InvariantCulture) == 0).FirstOrDefault();
        }
        private static void ValidateUnitCode(string opCode, string firstComment, string secondComment,
            string thirdComment, int address, int value, Unit? unit, Rom? playstationRom, bool isTransform)
        {

            UnitWeapon? unitWeaponFound = null;
            Unit? unitLookup = null;
            string? unitName = null;
            if(secondComment.StartsWith("("))
            {
                unitName = secondComment.Replace("(", string.Empty).Replace(")", string.Empty);
                unitLookup = GetUnitByName(playstationRom, unitName);
                Debug.Assert(unitLookup != null);
                unitWeaponFound = FindUnitWeapon(unitLookup, firstComment);
                Debug.Assert(unitLookup != null);
                var thirdLineParts = thirdComment.Split(arrowChar);
                var attribute = thirdLineParts[0];
                switch (attribute)
                {
                    case "弹药槽/序号":

                        Debug.Assert(address == unitWeaponFound.BaseOffset + 1);
                        Debug.Assert(opCode == "80");
                        return;
                    case "序号":
                        Debug.Assert(address == unitWeaponFound.BaseOffset + 2);
                        Debug.Assert(opCode == "30");
                        return;
                }
                Debug.Assert(false);
                return;
            }
            switch (firstComment)
            {
                //commands that apply to the unit itself
                case "乘换機動戦士系":
                    Debug.Assert(opCode == "80");
                    Debug.Assert(address == unit.BaseOffset + 4);
                    Debug.Assert(value == 0);
                    return;
                case "乘换ダンバイン系":
                    Debug.Assert(opCode == "80");
                    Debug.Assert(address == unit.BaseOffset + 4);
                    Debug.Assert(value == 0x200);
                    return;
                case "移动类型空陆":
                    Debug.Assert(address == unit.BaseOffset + 0x15);
                    Debug.Assert(value == 1);
                    return;
                case "移动类型/力":
                    Debug.Assert(address == unit.BaseOffset + 0x14);
                    Debug.Assert(opCode == "80");
                    return;
                case "地形适应全A":
                    Debug.Assert(address == unit.BaseOffset + 0x16);
                    Debug.Assert(value == 0x4444);
                    return;
                case "地形适应空A宇A":
                    Debug.Assert(address == unit.BaseOffset + 0x16);
                    Debug.Assert(value == 0x4040);
                    return;
                case "武器/弹药槽数量":
                    Debug.Assert(address == unit.BaseOffset + 0x1E);
                    Debug.Assert(opCode == "80");
                    return;
                case "武器数量":
                    Debug.Assert(address == unit.BaseOffset + 0x1E);
                    Debug.Assert(opCode == "30");
                    return;
                case "弹药槽数量":
                    Debug.Assert(address == unit.BaseOffset + 0x1F);
                    Debug.Assert(opCode == "30");
                    return;
                case "メガカノン砲":
                    Debug.Assert(unit?.Id == 0x106);
                    return;
                case "ギガブラスター":
                    Debug.Assert(unit?.Id == 0x10d);
                    return;
                case "可被合体":
                case "合体可":
                    Debug.Assert(address == unit.BaseOffset + 0x7);
                    Debug.Assert(opCode == "30");
                    return;
                case "二段变身":
                    Debug.Assert(address == unit.BaseOffset + 0x9);
                    Debug.Assert(opCode == "30");
                    Debug.Assert((value & 0x40) != 0);
                    return;
            }
            switch (secondComment)
            {
                //secondary commands that apply to the unit or weapon 
                //mentioned in the first comment
                case "武器再编号":
                case "弹药槽再编号":
                case "武器/弹药槽数量":
                case "武器数量":
                case "分离可":
                case "分离结果":
                    Debug.Assert(isTransform);
                    var unitFor = GetUnitByName(playstationRom, firstComment);
                    bool? found = false;
                    switch (secondComment)
                    {
                        case "武器再编号":
                            found = unitFor?.Weapons?.Any(w => w.BaseOffset + 2 == address);
                            Debug.Assert(opCode == "30");
                            return;
                        case "弹药槽再编号":
                            found = unitFor?.Weapons?.Any(w => w.BaseOffset + 1 == address);
                            Debug.Assert(opCode == "30");
                            return;
                        case "武器/弹药槽数量":
                            found = unitFor.BaseOffset + 0x1E == address;
                            Debug.Assert(opCode == "80");
                            return;
                        case "武器数量":
                            found = unitFor.BaseOffset + 0x1E == address;
                            Debug.Assert(opCode == "30");
                            return;
                        case "分离可":
                            Debug.Assert(address == unitFor.BaseOffset + 0x7);
                            Debug.Assert(opCode == "30");
                            Debug.Assert(value == 0x10);
                            return;
                        case "分离结果":
                            Debug.Assert(address == unitFor.BaseOffset + 0x7);
                            Debug.Assert(opCode == "30");
                            Debug.Assert(value == 0x11);
                            return;

                    }
                    Debug.Assert(found.HasValue && found.Value);
                    return;
                case "弹药槽/序号":
                    unitWeaponFound = FindUnitWeapon(unit, firstComment);
                    Debug.Assert(unitWeaponFound != null);
                    Debug.Assert(address == unitWeaponFound.BaseOffset + 1);
                    Debug.Assert(opCode == "80");                    
                    return;
            }
            var firstLineParts = firstComment.Split(arrowChar);
            var attributeName = firstLineParts[0].Trim();
            switch (attributeName)
            {
                case "图像":
                    Debug.Assert(address == unit.BaseOffset + 0x2);
                    return;
                case "图标":
                    Debug.Assert(address == unit.BaseOffset);
                    return;
                case "装甲":
                    Debug.Assert(address == unit.BaseOffset + 0x18);
                    Debug.Assert(opCode == "30");
                    return;
                case "运动性":
                    Debug.Assert(address == unit.BaseOffset + 0x19);
                    Debug.Assert(opCode == "30");
                    return;
                case "限界":
                    Debug.Assert(address == unit.BaseOffset + 0x1a);
                    Debug.Assert(opCode == "30");
                    return;
                case "移动力":
                    Debug.Assert(address == unit.BaseOffset + 0x14);
                    Debug.Assert(opCode == "30"); 
                    return;
                case "弹药槽数量":
                    Debug.Assert(address == unit.BaseOffset + 0x1F);
                    Debug.Assert(opCode == "30");
                    return;
                case "无"://adding weapon
                    Debug.Assert(address >= unit.BaseOffset + 0x20);
                    Debug.Assert(address <= unit.BaseOffset + 0x29);
                    Debug.Assert((address - unit.BaseOffset) % 3 == 2);
                    return;

            }

            var weaponName = attributeName;
            unitWeaponFound = FindUnitWeapon(unit, weaponName);
            if (unitWeaponFound != null)
            {
                //weapon commands
                //or wearpon changes
                var secondCommentParts = secondComment.Split(arrowChar);
                switch (secondCommentParts[0])
                {
                    case "弹药槽":
                        Debug.Assert(address == unitWeaponFound.BaseOffset + 1);
                        Debug.Assert(opCode == "30");
                        return;
                    case "序号":
                        Debug.Assert(address == unitWeaponFound.BaseOffset + 2);
                        Debug.Assert(opCode == "30");
                        return;
                    default:
                        //no recorgnized command, must be a weapon name
                        Debug.Assert(unitWeaponFound.BaseOffset == address);
                        return;
                }
            }            
            
            unitName = weaponName;            

            unitLookup = GetUnitByName(playstationRom, unitName);
            if (unitLookup != null)
            {
                //must be unit a->b transform commands
                Debug.Assert(address == unitLookup.BaseOffset + 7);
                Debug.Assert(value == 0x08
                    || value == 0x09
                    || value == 0x18
                    || value == 0x19
                    || value == 0x1A);
                return;
            }

            Debug.Assert(false);

        }
        private static void ValidatePilotCode(string opCode, string firstComment, string secondComment, int address, int value, Pilot? pilot)
        {
            var firstLineParts = firstComment.Split(arrowChar);

            if (firstLineParts.Length > 1)
            {
                var spriritCommandOrSkillName = firstLineParts[0];
                var newSpriritCommandOrSkillName = firstLineParts[1];
                if (string.IsNullOrEmpty(newSpriritCommandOrSkillName))
                {
                    newSpriritCommandOrSkillName = secondComment;
                }
                if (spriritCommandOrSkillName.Contains("/"))
                {
                    var spriritCommandOrSkillNameParts = spriritCommandOrSkillName.Split("/");
                    if (spriritCommandOrSkillNameParts[1].Length > 1)
                    {
                        spriritCommandOrSkillName = spriritCommandOrSkillNameParts[1];
                        if (newSpriritCommandOrSkillName.StartsWith("LV"))
                            newSpriritCommandOrSkillName = spriritCommandOrSkillName;
                    }
                    else
                    {
                        spriritCommandOrSkillName = spriritCommandOrSkillNameParts[0].Substring(0, spriritCommandOrSkillNameParts[0].Length - 1) + spriritCommandOrSkillNameParts[1];
                    }
                }
                else if (firstLineParts[1].Length == 1)
                {
                    if (firstLineParts[0].Contains(" LV"))
                    {
                        spriritCommandOrSkillName = firstLineParts[0].Substring(0, firstLineParts[0].IndexOf(" LV"));
                        newSpriritCommandOrSkillName = spriritCommandOrSkillName;
                    }
                }
                else if (newSpriritCommandOrSkillName.ToLower().StartsWith("lv"))
                {
                    newSpriritCommandOrSkillName = spriritCommandOrSkillName;
                }
                var spiritCommmand = pilot?.SpiritCommandsOrSkills?.Where(s =>
                PilotSpiritCommandsOrSkill.Format(0, s.SpiritCommandsOrSkill, 0, false) == spriritCommandOrSkillName).FirstOrDefault();
                if (spiritCommmand != null)
                {
                    Debug.Assert(address == spiritCommmand.BaseOffset);
                    Debug.Assert(PilotSpiritCommandsOrSkill.Format(0, (byte)(value % 256), 0, false) == newSpriritCommandOrSkillName);
                }
                else
                {
                    if (firstLineParts[0] == "无")
                    {
                        Debug.Assert(pilot?.SpiritCommandsOrSkills?.Count == 2);
                        Debug.Assert(pilot?.SpiritCommandsOrSkills?[0].SpiritCommandsOrSkill == 0);
                        Debug.Assert(pilot?.SpiritCommandsOrSkills?[1].SpiritCommandsOrSkill == 0);
                    }
                    else
                    {
                        //spriritCommandOrSkillName=
                        Debug.Assert(false);
                    }

                }
            }
            else
            {
                switch (firstComment)
                {
                    case "乘换可":
                        Debug.Assert(address == pilot.BaseOffset + 3);
                        Debug.Assert((value & 0x40) == 0);
                        Debug.Assert(opCode == "30");
                        break;
                    case "乘换機動戦士系":
                        Debug.Assert(address == pilot.BaseOffset + 3);
                        Debug.Assert((value & 0x0f) == 0);
                        Debug.Assert(opCode == "30");
                        break;
                    case "乘换ダンバイン系":
                        Debug.Assert(address == pilot.BaseOffset + 3);
                        Debug.Assert((value & 0x0f) == 2);
                        Debug.Assert(opCode == "30");
                        break;
                    case "地形适应":
                        Debug.Assert(address == pilot.BaseOffset + 9);
                        break;
                    case "能力":
                        Debug.Assert(address >= pilot.BaseOffset + 0xB
                            && address <= pilot.BaseOffset + 16);
                        break;
                    case "SP值":
                        Debug.Assert(address == pilot.BaseOffset + 0x11);
                        Debug.Assert(opCode == "30");
                        break;
                    default:
                        if (firstComment.StartsWith("颜"))
                        {
                            Debug.Assert(address == pilot.BaseOffset);
                            Debug.Assert(opCode == "30");
                            break;
                        }
                        else
                            Debug.Assert(false);
                        break;
                }
            }
        }
    }
}
