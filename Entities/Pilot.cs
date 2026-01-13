using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pilot : IRstFormatter, INamedItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EnglishName { get; set; }
        public string? ChineseName { get; set; }
        public string? Affiliation { get; set; }
        public string? FranchiseName { get; set; }
        public int BaseOffset { get; private set; }
        public int FaceId { get; set; }
        public byte FranchiseId { get; set; }

        public byte PlayStationFranchise2 { get; private set; }
        public byte TransferTypeGenderPersonality{ get; private set; }
        public byte TransferFranchiseId { get; private set; }
        public bool IsFemale { get; private set; }
        public bool IsFixedSeat { get; private set; }
        public byte Personality { get; private set; }
        public byte Experience { get; private set; }
        public byte AccuracyGrowthType { get; private set; }
        public byte SkillGrowthType { get; private set; }
        public byte NearGrowthType { get; set; }
        public byte FarGrowthType { get; set; }
        public byte SPGrowthType { get; private set; }
        public byte EvasionGrowthType { get; private set; }
        public byte IntuitionGrowthType { get; private set; }

        public TerrainAdaptionSet? TerrainAdaptionSet { get; set; }
        public byte TerrainAdaptionAir
        {
            get
            {
                if (TerrainAdaptionSet == null) throw new ArgumentNullException(nameof(TerrainAdaptionSet));
                return this.TerrainAdaptionSet.GetTerrainAdaptionByIndex(TerrainAdaptionSetIndex.Air);
            }
        }
        public byte TerrainAdaptionSea
        {
            get
            {
                if (TerrainAdaptionSet == null) throw new ArgumentNullException(nameof(TerrainAdaptionSet));
                return this.TerrainAdaptionSet.GetTerrainAdaptionByIndex(TerrainAdaptionSetIndex.Sea);
            }
        }
        public byte TerrainAdaptionLand
        {
            get
            {
                if (TerrainAdaptionSet == null) throw new ArgumentNullException(nameof(TerrainAdaptionSet));
                return this.TerrainAdaptionSet.GetTerrainAdaptionByIndex(TerrainAdaptionSetIndex.Land);
            }
        }
        public byte TerrainAdaptionSpace
        {
            get
            {
                if (TerrainAdaptionSet == null) throw new ArgumentNullException(nameof(TerrainAdaptionSet));
                return this.TerrainAdaptionSet.GetTerrainAdaptionByIndex(TerrainAdaptionSetIndex.Space);
            }
        }
        public byte NearAttack { get; private set; }
        public byte FarAttack { get; private set; }
        public byte Accuracy { get; private set; }
        public byte Skill { get; private set; }
        public byte Evasion { get; private set; }
        public byte Intuition { get; private set; }
        public byte StartSP { get; private set; }
        public List<PilotSpiritCommandsOrSkill>? SpiritCommandsOrSkills { get; set; }

        [Ignore]
        public bool IsPlayStation { get; set; }
        [Ignore]
        public int FirstAppearance { get; set; }
        public string? DisplayName { get; private set; }
        [Ignore]
        public int DisplayNameOffset { get; private set; }
        public string? CallSign { get; private set; }
        [Ignore]
        public int CallSignOffset { get; private set; }

        public string GetLabel()
        {
            return RstHelper.GetLabelName(this.EnglishName);
        }
        public static List<Pilot>? Parse(ReadOnlySpan<byte> romData, IndexTable indexTable,List<PilotMetaData> pilotMetaData, bool isPlayStation, List<EntityName> pilotNames, List<EntityName> pilotCallSigns)
        {
            var result = new List<Pilot>();
            var indexedLocations = indexTable.Read(romData);
            int pilotId = 0;
            foreach (var location in indexedLocations)
            {
                if (location != 0)
                {
                    var pilot = ParsePilot(romData.Slice(location,256), location, pilotId, isPlayStation);
                    if (pilot != null)
                    {
                        FixPilotData(pilot, pilotMetaData, pilotNames, pilotCallSigns);
                        result.Add(pilot);
                    }
                }
                pilotId++;
            }
            return result;
        }

        private static void FixPilotData(Pilot pilot, List<PilotMetaData> pilotMetaData, List<EntityName> pilotNames, List<EntityName> pilotCallSigns)
        {
            var fixPilot = pilotMetaData.Where(p => p.Id == pilot.Id).FirstOrDefault();
            if (fixPilot == null)
            {
                Debug.WriteLine(string.Format("unable to find pilot with pilotId {0}", pilot.Id));
                return;
            }
            pilot.Name = fixPilot.Name;
            pilot.EnglishName = fixPilot.EnglishName;
            pilot.ChineseName = fixPilot.ChineseName;
            pilot.Affiliation = fixPilot.Affiliation;
            pilot.FranchiseName = fixPilot.FranchiseName;
            pilot.FirstAppearance = fixPilot.FirstAppearance;
            if (string.Compare(pilot.FranchiseName, "原创", StringComparison.Ordinal) == 0)
            {
                pilot.FranchiseName = "オリジナル";
            }
            if (string.Compare(pilot.FranchiseName, "マジンガーＺ", StringComparison.Ordinal) == 0)
            {
                pilot.FranchiseName = "マジンガーZ";
            }
            var pilotName=pilotNames.Where(p=>p.Id == pilot.Id).FirstOrDefault(); 
            if (pilotName != null &&!string.IsNullOrWhiteSpace(pilotName.Name))
            {
                pilot.DisplayName = pilotName.Name;
                pilot.DisplayNameOffset = pilotName.Offset;
            }
            var pilotCallsign= pilotCallSigns.Where(p => p.Id == pilot.Id).FirstOrDefault();
            if (pilotCallsign != null && !string.IsNullOrWhiteSpace(pilotCallsign.Name))
            {
                pilot.CallSign = pilotCallsign.Name; 
                pilot.CallSignOffset = pilotCallsign.Offset;
            }
        }

        private static Pilot ParsePilot(ReadOnlySpan<byte> pilotData, int baseOffset, int pilotIndex, bool isPlayStation)
        {
            Pilot pilot = new Pilot();
            pilot.IsPlayStation = isPlayStation;
            pilot.Id = pilotIndex;
            pilot.BaseOffset = baseOffset;
            int offset = 0;
            //offset 0
            pilot.FaceId = pilotData[offset++];
            //offset 1
            var inGameFranchise = pilotData[offset++];
            pilot.FranchiseId = (byte)(inGameFranchise & 0xFE);
            if ((inGameFranchise & 0x01) != 0)
            {
                pilot.FaceId += 256;
            }
            if (isPlayStation)
            {
                //offset 2
                pilot.PlayStationFranchise2 = pilotData[offset++];
            }
            //offset 2/3
            byte transferTypeGenderPersonality = pilotData[offset++];
            pilot.TransferTypeGenderPersonality = transferTypeGenderPersonality;
            pilot.TransferFranchiseId = (byte)(transferTypeGenderPersonality & 0xF);
            pilot.IsFemale = (transferTypeGenderPersonality & 0x80) != 0;
            pilot.IsFixedSeat = (transferTypeGenderPersonality & 0x40) != 0;
            pilot.Personality = (byte)((transferTypeGenderPersonality & 0x30) / 16);
            //offset 3/4
            pilot.Experience = pilotData[offset++];
            //offset 4/5
            byte growthType = pilotData[offset++];
            pilot.AccuracyGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.SkillGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.SkillGrowthType < 4);
            Debug.Assert(pilot.AccuracyGrowthType < 4);
            //offset 5/6
            growthType = pilotData[offset++];
            pilot.NearGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.FarGrowthType = (byte)(growthType & 0xF);
            //Debug.Assert(pilot.NearGrowthType <= 4);
            //Debug.Assert(pilot.FarGrowthType <= 4);
            //offset 6/7
            pilot.SPGrowthType = (byte)((pilotData[offset++]) / 16);
            Debug.Assert(pilot.SPGrowthType < 4);
            //offset 7/8
            growthType = pilotData[offset++];
            pilot.EvasionGrowthType = (byte)((growthType & 0xF0) / 16);
            Debug.Assert(pilot.EvasionGrowthType < 5);
            pilot.IntuitionGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.IntuitionGrowthType < 3);
            //offset 8/9
            var terrainAdaptionLow = pilotData[offset++];
            var terrainAdaptionHigh = pilotData[offset++];
            pilot.TerrainAdaptionSet = TerrainAdaptionSet.FromPilotOrUnitAdaptions(
                terrainAdaptionLow, terrainAdaptionHigh);
            Debug.Assert(pilot.TerrainAdaptionAir < 5);
            Debug.Assert(pilot.TerrainAdaptionSea < 5);
            //offset 9/a
            Debug.Assert(pilot.TerrainAdaptionSpace < 5);
            Debug.Assert(pilot.TerrainAdaptionLand < 5);
            //offset a/b
            pilot.NearAttack = pilotData[offset++];
            //offset b/c
            pilot.FarAttack = pilotData[offset++];
            //offset c/d
            pilot.Accuracy = pilotData[offset++];
            //offset d/e
            pilot.Skill = pilotData[offset++];
            //offset e/f
            pilot.Evasion = pilotData[offset++];
            //offset f/10
            pilot.Intuition = pilotData[offset++];
            //offset 10/11
            pilot.StartSP = pilotData[offset++];
            //offset 11/12
            pilot.SpiritCommandsOrSkills = new List<PilotSpiritCommandsOrSkill>();
            ushort testData = BitConverter.ToUInt16(pilotData.Slice(offset, 2));
            while (testData != 0)
            {                
                var pilotSpiritCommandsOrSkill = pilotData[offset++];
                var acquireAtLevel = pilotData[offset++];
                Debug.Assert(pilotSpiritCommandsOrSkill < 0x40);
                //Debug.Assert(acquireAtLevel < 64);
                PilotSpiritCommandsOrSkill spiritCommandsOrSkill = new PilotSpiritCommandsOrSkill
                {
                    BaseOffset = offset + baseOffset,
                    AcquireAtLevel = acquireAtLevel,
                    SpiritCommandsOrSkill = pilotSpiritCommandsOrSkill
                };
                pilot.SpiritCommandsOrSkills.Add(spiritCommandsOrSkill);
                testData = BitConverter.ToUInt16(pilotData.Slice(offset, 2));
            }
            return pilot;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.AppendFormat(" Id: {0:X}", Id);
            stringBuilder.AppendFormat("\t名: {0:X}:\t{1}\t{2}\t{3}\t{4}", DisplayNameOffset,DisplayName,Name, EnglishName, ChineseName);
            stringBuilder.AppendFormat("\t呼号: {0:X}:\t{1}", CallSignOffset, CallSign);
            stringBuilder.AppendFormat("\t 属: {0:X}", Affiliation);
            stringBuilder.AppendFormat("\t 作: {0:X}", FranchiseName);
            stringBuilder.AppendFormat("\t 颜: {0:X}:{1:X}", BaseOffset, FaceId);
            stringBuilder.AppendFormat("\t 游戏作: {0:X}:{1:X}({2})", BaseOffset + 0x01, FranchiseId, Franchise.FormatFranchise(FranchiseId));
            if (PlayStationFranchise2 != 0)
                stringBuilder.AppendFormat("\t PS游戏作: {0:X}:{1}({2})", BaseOffset + 0x02, PlayStationFranchise2,Franchise.FormatPlayStationFranchise2(PlayStationFranchise2));
            stringBuilder.AppendFormat("\t 换乘: {0:X}:{1:X}({2})", BaseOffset + (this.IsPlayStation ? 3 : 2), TransferTypeGenderPersonality,this.TransferFranchiseId);
            if (IsFemale)
                stringBuilder.Append("\t 女");
            if (IsFixedSeat)
                stringBuilder.Append("\t 換乘不可");
            stringBuilder.AppendFormat("\t 性格: {0}", FormatPersonality(Personality));
            stringBuilder.AppendFormat("\t 经验: {0}", Experience);
            stringBuilder.AppendFormat("\t 能力增长模式: 近{0},远{1},命{2},技{3},回{4},直{5},SP{6}",
                GetNearGrowthType(), GetFarGrowthType(), GetAccuracyGrowthType(), GetSkillGrowthType(), GetEvasionGrowthType(), GetIntuitionGrowthType(), SPGrowthType);

            stringBuilder.AppendFormat("\r\n 地形适应: {0:X}:{1}({2})",
                BaseOffset + (this.IsPlayStation ? 9 : 8),
                Convert.ToHexString(this.TerrainAdaptionSet != null ? this.TerrainAdaptionSet.ToPilotOrUnitAdaptions() : new byte[] { })
                , this.TerrainAdaptionSet?.ToString());
            stringBuilder.AppendFormat("\t 近: {0:X}:{1}", BaseOffset + (this.IsPlayStation ? 0xb : 0xa), NearAttack);
            stringBuilder.AppendFormat("\t 远: {0}", FarAttack);
            stringBuilder.AppendFormat("\t 命: {0}", Accuracy);
            stringBuilder.AppendFormat("\t 技: {0}", Skill);
            stringBuilder.AppendFormat("\t 回: {0}", Evasion);
            stringBuilder.AppendFormat("\t 直: {0}", Intuition);
            stringBuilder.AppendFormat("\t SP: {0}", StartSP);
            stringBuilder.AppendFormat("\t 二动等级: {0}", GetDoubleActLevel());
            stringBuilder.Append("\r\n 精神/技能:");
            stringBuilder.AppendLine(FormatSpiritCommandsOrSkills(SpiritCommandsOrSkills, true, '\t'));
            return stringBuilder.ToString();
        }


        public static string FormatPersonality(byte personality)
        {
            switch (personality)
            {
                case 3: return "超強気";
                case 2: return "強気";
                case 1: return "普通";
                case 0: return "弱気";
                default: return string.Empty;
            }
        }
        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.AppendLine(string.Format("     - {0}", this.Affiliation));
            row.AppendLine(string.Format("     - {0}", RstGetPilotIcon(this.Id)));
            if(!string.IsNullOrEmpty(this.ChineseName))
                row.AppendLine(string.Format("     - \\ :ref:`{0} <srw4_pilot_{1}>`\\ ({2}) ", this.Name, RstHelper.GetLabelName(this.EnglishName),this.ChineseName));
            else
                row.AppendLine(string.Format("     - \\ :ref:`{0} <srw4_pilot_{1}>`\\ ", this.Name, RstHelper.GetLabelName(this.EnglishName)));
            row.AppendLine(string.Format("     - {0}", this.EnglishName));
            row.AppendLine(string.Format("     - {0}", Franchise.ToRstFranchise(this.FranchiseName, "pilots")));
            row.AppendLine(string.Format("     - {0}", FormatPersonality(this.Personality)));
            row.AppendLine(string.Format("     - {0}", this.NearAttack));
            row.AppendLine(string.Format("     - {0}", this.FarAttack));
            row.AppendLine(string.Format("     - {0}", this.Evasion));
            row.AppendLine(string.Format("     - {0}", this.Accuracy));
            row.AppendLine(string.Format("     - {0}", this.Intuition));
            row.AppendLine(string.Format("     - {0}", this.Skill));
            row.AppendLine(string.Format("     - {0}", this.NearAttack99()));
            row.AppendLine(string.Format("     - {0}", this.FarAttack99()));
            row.AppendLine(string.Format("     - {0}", this.Evasion99()));
            row.AppendLine(string.Format("     - {0}", this.Accuracy99()));
            row.AppendLine(string.Format("     - {0}", this.Intuition99()));
            row.AppendLine(string.Format("     - {0}", this.Skill99()));
            row.AppendLine(string.Format("     - {0}", this.StartSP));            
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionSpace)));
            row.AppendLine(string.Format("     - {0}", this.GetDoubleActLevel()));
            row.AppendLine(string.Format("     - {0}", FormatSpiritCommandsOrSkills(SpiritCommandsOrSkills, false, ',')));

            return row.ToString();
        }
        string FormatSpiritCommandsOrSkills(List<PilotSpiritCommandsOrSkill>? spiritCommandsOrSkills, bool withAddress, char seperator)
        {
            if (SpiritCommandsOrSkills == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            var validSpiritCommandsOrSkills = SpiritCommandsOrSkills.Where(s => s.SpiritCommandsOrSkill != 0);
            byte previousSpiritCommandsOrSkill = 0;
            foreach (var pilotSpiritCommandsOrSkill in validSpiritCommandsOrSkills)
            {
                string pilotSpiritCommandsOrSkillString = string.Format("{0} {1}", PilotSpiritCommandsOrSkill.Format(
                    pilotSpiritCommandsOrSkill.BaseOffset,
                    pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill, previousSpiritCommandsOrSkill, withAddress), pilotSpiritCommandsOrSkill.AcquireAtLevel);
                if (sb.Length != 0)
                {
                    sb.AppendFormat("{0} ", seperator);
                }
                sb.Append(pilotSpiritCommandsOrSkillString);
                previousSpiritCommandsOrSkill = pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill;
            }
            return sb.ToString();
        }

        public int Skill99()
        {
            int baseline = this.Skill + 99;
            switch (this.SkillGrowthType)
            {
                case 1: //セシリー
                    return baseline + 21;
                case 2: //兜甲児
                    return baseline + 10;
                case 3: //神勝平
                    return baseline + 5;
            }
            return baseline;
        }

        private string GetSkillGrowthType()
        {
            switch (this.SkillGrowthType)
            {
                case 1: //セシリー
                    return "+ 21";
                case 2: //兜甲児
                    return "+10";
                case 3: //神勝平
                    return "+5";
            }
            return string.Empty;
        }

        public int Accuracy99()
        {
            int baseline = this.Accuracy + 99;
            switch (this.AccuracyGrowthType)
            {
                case 1://マリア
                    return baseline + 12;
                case 2://ボス
                    return baseline + 10;
                case 3://兜甲児
                    return baseline + 5;
            }
            return baseline;
        }
        private string GetAccuracyGrowthType()
        {
            switch (this.AccuracyGrowthType)
            {
                case 1://マリア
                    return "+12";
                case 2://ボス
                    return "+10";
                case 3://兜甲児
                    return "+5";
            }
            return string.Empty;
        }
        public int Evasion99()
        {
            int baseline = this.Evasion + 99;
            switch (this.EvasionGrowthType)
            {
                case 1://エマ＝シーン,ファ＝ユイリィ
                    return baseline + 12;
                case 2://ボス
                    return baseline + 15;
                case 3://アムロ
                    return baseline + 10;
                case 4://神勝平
                    return baseline + 5;

            }
            return baseline;
        }

        private string GetEvasionGrowthType()
        {
            switch (this.EvasionGrowthType)
            {
                case 1://エマ＝シーン,ファ＝ユイリィ
                    return " + 12";
                case 2://ボス
                    return "+15";
                case 3://アムロ
                    return "+10";
                case 4://神勝平
                    return "+5";
            }
            return string.Empty;
        }


        public int Intuition99()
        {
            int baseline = this.Intuition + 99;
            switch (this.IntuitionGrowthType)
            {
                case 1://ファ＝ユイリィ
                    return baseline + 10;
                case 2://ショウ＝ザマ
                    return baseline + 5;
            }
            return baseline;
        }

        private string GetIntuitionGrowthType()
        {
            switch (this.IntuitionGrowthType)
            {
                case 1://ファ＝ユイリィ
                    return "+10";
                case 2://ショウ＝ザマ
                    return "+5";
            }
            return string.Empty;
        }
        public int GetDoubleActLevel()
        {
            switch (this.IntuitionGrowthType)
            {
                case 1://ファ＝ユイリィ, +10
                    return GetDoubleActLevelWithGrowth(this.Intuition, 
                        new int[] { 18, 25, 31, 32, 33, 34, 35, 36, 37,38 });
                case 2://ショウ＝ザマ,+5
                    return GetDoubleActLevelWithGrowth(this.Intuition,
                        new int[] { 18, 25, 31, 34, 36 });
            }

            return 130 - this.Intuition;
        }

        private int GetDoubleActLevelWithGrowth(byte intuition, int[] bonusLevels)
        {
            int bonus= 0;
            foreach (var bonusLevel in bonusLevels)
            {           
                if (intuition >= 130 - bonusLevel - bonus)
                {
                    return 130 - intuition - bonus;
                }
                bonus++;
            }
            return 130 - intuition - bonus;
        }

        public int NearAttack99()
        {
            int result = this.NearAttack + 52;
            switch (this.NearGrowthType)
            {
                case 1://バーニィ, スーパー男主人公
                    result += 20; break;
                case 2: //流竜馬
                    result += 10; break;
                case 3:
                    result += 5; break;
                case 4://藤原忍
                    result += 15; break;

            }
            return result > 255 ? 255 : result;
        }
        private string GetNearGrowthType()
        {
            string result = string.Empty;
            switch (this.NearGrowthType)
            {
                case 1://バーニィ, スーパー男主人公
                    result = "+20"; break;
                case 2: //流竜馬
                    result = "+10"; break;
                case 3:
                    result = "+5"; break;
                case 4://藤原忍
                    result = "+15"; break;
            }
            return result;
        }
        public int FarAttack99()
        {
            int result = this.FarAttack + 52;
            switch (this.FarGrowthType)
            {
                case 1://カツ＝コバヤシ/ハサウェイ＝ノア
                    result += 20; break;
                case 2://スーパー男主人公,キース
                    result += 10; break;
                case 3://夕月京四郎,流竜馬
                    result += 5; break;
                case 4://藤原忍
                    result += 15; break;
            }
            return result > 255 ? 255 : result;
        }
        private string GetFarGrowthType()
        {
            string result = string.Empty;
            switch (this.FarGrowthType)
            {
                case 1://カツ＝コバヤシ/ハサウェイ＝ノア
                    result = "+20"; break;
                case 2: //スーパー男主人公,キース
                    result = "+10"; break;
                case 3://夕月京四郎,流竜馬
                    result = "+5"; break;
                case 4://藤原忍
                    result = "+15"; break;
            }
            return result;
        }

        public static string RstGetPilotIcon(int id)
        {
            if (id >= 0xC8 && id <= 0xD0)
                return string.Empty;
            if (id >= 0xF8)
                return string.Empty;
            switch (id)
            {
                case 0x6C:
                case 0xED:
                case 0xEF:
                    return string.Empty;
                default:
                    return string.Format(".. image:: ../pilots/images/srw4_pilot_{0:X2}.png", id);
            }
        }

        internal static void RstAppendPilot(StringBuilder stringBuilder, int pilotId, List<PilotMetaData> affiliationsPilots, string empty, Rom snesRom, Rom playstationRom, Dictionary<string, string> comments, PilotTScoreParametersSet pilotTScoreParametersSet)
        {
            var pilotMetaData = affiliationsPilots.FirstOrDefault(p => p.Id == pilotId);
            if (pilotMetaData == null || pilotMetaData.Name == null)
            {
                throw new ArgumentNullException(nameof(pilotMetaData));
            }

            var snesPilot = snesRom.Pilots?.FirstOrDefault(p => p.Id == pilotId);
            var playstationPilot = playstationRom.Pilots?.FirstOrDefault(p => p.Id == pilotId);
            if (snesPilot == null)
                throw new ArgumentNullException(nameof(snesPilot));

            if (playstationPilot == null)
                throw new ArgumentNullException(nameof(playstationPilot));

            var pilotName = pilotMetaData.Name;
            if (!string.IsNullOrEmpty(pilotMetaData.ChineseName))
            {
                pilotName += string.Format("({0})", pilotMetaData.ChineseName);
            }

            RstHelper.AppendHeader(stringBuilder, pilotName, '^');
            stringBuilder.AppendLine();

            var pilotLabel = RstHelper.GetLabelName(pilotMetaData.EnglishName);
            Debug.Assert(!string.IsNullOrEmpty(pilotLabel));

            stringBuilder.AppendLine(string.Format(".. _srw4_pilot_{0}:", pilotLabel));
            stringBuilder.AppendLine();

            var fixedSeatUnits=snesRom.Units?.Where(u => u.FixedSeatPilotId == pilotId
            && !string.IsNullOrWhiteSpace(u.Name)).ToList();
            RstWritePilotMetaData(stringBuilder, pilotId, pilotTScoreParametersSet, pilotMetaData, snesPilot, playstationPilot, fixedSeatUnits);

            RstWritePilotSpiritCommandsAndSkills(stringBuilder, pilotMetaData, snesPilot, playstationPilot, delegate (PilotSpiritCommandsOrSkill pilotSpiritCommandsOrSkill) { return pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill < 0x20; });
            RstWritePilotSpiritCommandsAndSkills(stringBuilder, pilotMetaData, snesPilot, playstationPilot, delegate (PilotSpiritCommandsOrSkill pilotSpiritCommandsOrSkill) { return pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill >= 0x20; });
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(string.Format(".. _srw4_pilot_{0}_commentBegin:", pilotLabel));
            stringBuilder.AppendLine(RstHelper.GetComments(comments, string.Format("_srw4_pilot_{0}", pilotLabel)));
            stringBuilder.AppendLine(string.Format(".. _srw4_pilot_{0}_commentEnd:", pilotLabel));
            stringBuilder.AppendLine();
        }

        private static void RstWritePilotSpiritCommandsAndSkills(StringBuilder stringBuilder, PilotMetaData pilotMetaData, Pilot snesPilot, Pilot playstationPilot, Predicate<PilotSpiritCommandsOrSkill> filter)
        {
            List<PilotSpiritCommandsOrSkill> snesSpiritCommandsAndSkills = new List<PilotSpiritCommandsOrSkill>();
            List<PilotSpiritCommandsOrSkill> playstationSpiritCommandsAndSkills = new List<PilotSpiritCommandsOrSkill>();
            List<PilotSpiritCommandsOrSkill> playstationSpiritCommandsAndSkillsChanges = new List<PilotSpiritCommandsOrSkill>();
            if (snesPilot.SpiritCommandsOrSkills != null)
            {
                snesSpiritCommandsAndSkills = snesPilot.SpiritCommandsOrSkills.Where(s => filter(s)).ToList();
            }
            if (playstationPilot.SpiritCommandsOrSkills != null)
            {
                playstationSpiritCommandsAndSkills = playstationPilot.SpiritCommandsOrSkills.Where(s => filter(s)).ToList();
            }
            playstationSpiritCommandsAndSkillsChanges = playstationSpiritCommandsAndSkills.Where(psSc => !snesSpiritCommandsAndSkills.Any(snesSc => snesSc.SpiritCommandsOrSkill == psSc.SpiritCommandsOrSkill)).ToList();
            int changedSpiritCommand = 0;
            byte previousSpiritCommandOrSkill = 0;
            List<string> spiritCommandOrSkillTexts = new List<string>();

            foreach (var snesSpiritCommand in snesSpiritCommandsAndSkills)
            {
                if (snesSpiritCommand.SpiritCommandsOrSkill == 0) continue;//dead data
                var playstationMatch = playstationSpiritCommandsAndSkills.Where(psSc => psSc.SpiritCommandsOrSkill == snesSpiritCommand.SpiritCommandsOrSkill).FirstOrDefault();
                if (playstationMatch == null &&
                    changedSpiritCommand < playstationSpiritCommandsAndSkillsChanges.Count)
                {
                    playstationMatch = playstationSpiritCommandsAndSkillsChanges[changedSpiritCommand];
                    changedSpiritCommand++;
                }
                if (playstationMatch == null)
                {
                    spiritCommandOrSkillTexts.Add(string.Format("{0} (snes)",
                        PilotSpiritCommandsOrSkill.Format(0, snesSpiritCommand.SpiritCommandsOrSkill, previousSpiritCommandOrSkill, false)));
                }
                else
                {
                    spiritCommandOrSkillTexts.Add(string.Format("{0} {1}",
                        Rom.FormatValue(
                            PilotSpiritCommandsOrSkill.Format(
                                0, snesSpiritCommand.SpiritCommandsOrSkill,
                                previousSpiritCommandOrSkill, false),
                            PilotSpiritCommandsOrSkill.Format(
                                0, playstationMatch.SpiritCommandsOrSkill,
                                previousSpiritCommandOrSkill, false)
                        ),
                        Rom.FormatValue(
                            snesSpiritCommand.AcquireAtLevel,
                            playstationMatch.AcquireAtLevel)));
                }
                previousSpiritCommandOrSkill = snesSpiritCommand.SpiritCommandsOrSkill;
            }
            if (changedSpiritCommand < playstationSpiritCommandsAndSkillsChanges.Count)
            {
                for (int i = changedSpiritCommand; i < playstationSpiritCommandsAndSkillsChanges.Count; i++)
                {
                    var playstationMatch = playstationSpiritCommandsAndSkillsChanges[i];
                    spiritCommandOrSkillTexts.Add(
                        string.Format("{0} {1} (ps)",
                            PilotSpiritCommandsOrSkill.Format(
                                0, playstationMatch.SpiritCommandsOrSkill,
                                previousSpiritCommandOrSkill, false),
                            Rom.FormatValue(
                                playstationMatch.AcquireAtLevel,
                                playstationMatch.AcquireAtLevel)));
                    previousSpiritCommandOrSkill = playstationMatch.SpiritCommandsOrSkill;
                }
            }
            if (spiritCommandOrSkillTexts.Count > 0)
            {
                stringBuilder.AppendLine(Resource.RstUnitGridHeader);
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(string.Format("        {0}", string.Join(",", spiritCommandOrSkillTexts)));
            }
        }

        private static void RstWritePilotMetaData(StringBuilder stringBuilder, int pilotId, PilotTScoreParametersSet pilotTScoreParametersSet, PilotMetaData pilotMetaData, Pilot snesPilot, Pilot playstationPilot, List<Unit>? fixedSeatUnits)
        {
            stringBuilder.AppendLine(Resource.RstUnitGridHeader);
            stringBuilder.Append(Resource.RstUnitGridColumnAuto);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format("        {0}", RstGetPilotIcon(pilotId)));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("英文:{0}。", pilotMetaData.EnglishName)));

            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("二动等级:{0}。", Rom.FormatValue(snesPilot.GetDoubleActLevel(), playstationPilot.GetDoubleActLevel()))));

            var snesTScore = Math.Round(pilotTScoreParametersSet.SnesOwned.GetTScoreParameter(PilotTScoreParameterIndex.DoubleActLevel).CalculateTScore(snesPilot.GetDoubleActLevel()));
            var playStationTScore = Math.Round(pilotTScoreParametersSet.PlayStationOwned.GetTScoreParameter(PilotTScoreParameterIndex.DoubleActLevel).CalculateTScore(playstationPilot.GetDoubleActLevel()));

            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("偏差值:{0}。", Rom.FormatValue(snesTScore, playStationTScore))));

            if (pilotMetaData.FirstAppearance > 0)
            {
                stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("登场/加入:第{0}话。", pilotMetaData.FirstAppearance)));
            }
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("性格:{0}。", Rom.FormatValue(FormatPersonality(snesPilot.Personality), FormatPersonality(playstationPilot.Personality)))));

            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("SP:{0}。", Rom.FormatValue(snesPilot.StartSP, playstationPilot.StartSP))));

            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("EXP:{0}。", Rom.FormatValue(snesPilot.Experience, playstationPilot.Experience))));

            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("编码:{0:X2}。", pilotId)));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("地址:{0:X} ({1:X})。", snesPilot.BaseOffset, playstationPilot.BaseOffset)));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("精神地址:{0:X}({1:X})。", snesPilot.BaseOffset + 0x11, playstationPilot.BaseOffset + 0x12)));

            if (snesPilot.TerrainAdaptionSet == null) throw new ArgumentNullException("snesPilot.TerrainAdaptionSet ");
            if (playstationPilot.TerrainAdaptionSet == null) throw new ArgumentNullException("playstationPilot.TerrainAdaptionSet ");
            
            var terrainAdoptions = TerrainAdaptionSet.FormatEffectiveTerrainWeaponAdaption
                (snesPilot.TerrainAdaptionSet, playstationPilot.TerrainAdaptionSet,
                snesPilot.TerrainAdaptionSet, playstationPilot.TerrainAdaptionSet);
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("地形适应:{0}",
                string.Format("{0}。", string.Join(string.Empty, terrainAdoptions)))));

            if (fixedSeatUnits != null)
            { 
                List<string> fixedSeatUnitList=new List<string>();
                foreach ( var unit in fixedSeatUnits) 
                {
                    var fixedSeatUnit = string.Format("\\ :ref:`{0} <srw4_unit_{1}>`\\ ",
                        unit.Name, RstHelper.GetLabelName(unit.EnglishName));
                    if (!string.IsNullOrEmpty(unit.ChineseName))
                    {
                        fixedSeatUnit += string.Format("({0})", unit.ChineseName);
                    }
                    fixedSeatUnitList.Add(fixedSeatUnit);
                }
                if (fixedSeatUnitList.Count > 0)
                {
                    stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnAutoWithText, string.Format("搭乘机体:{0}",
                    string.Format("{0}。", string.Join("、", fixedSeatUnitList)))));
                }
            }
            if (snesPilot.NearAttack > 0)
            {
                stringBuilder.Append(Resource.RstUnitGridHeader);
                stringBuilder.AppendLine();
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "近攻击"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));

                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "回避"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));

                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "直感"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));
                stringBuilder.Append(Resource.RstUnitGridColumnBreak);

                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                    snesPilot.NearAttack, playstationPilot.NearAttack,
                    snesPilot.NearAttack99(), playstationPilot.NearAttack99(),
                    PilotTScoreParameterIndex.NearAttack, PilotTScoreParameterIndex.NearAttack99,
                    snesPilot.NearGrowthType, playstationPilot.NearGrowthType,
                    snesPilot.GetNearGrowthType(), playstationPilot.GetNearGrowthType()
                    );

                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                    snesPilot.Evasion, playstationPilot.Evasion,
                    snesPilot.Evasion99(), playstationPilot.Evasion99(),
                    PilotTScoreParameterIndex.Evasion, PilotTScoreParameterIndex.Evasion99,
                    snesPilot.EvasionGrowthType, playstationPilot.EvasionGrowthType,
                    snesPilot.GetEvasionGrowthType(), playstationPilot.GetEvasionGrowthType()
                    );


                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                    snesPilot.Intuition, playstationPilot.Intuition,
                    snesPilot.Intuition99(), playstationPilot.Intuition99(),
                    PilotTScoreParameterIndex.Intuition, PilotTScoreParameterIndex.Intuition99,
                    snesPilot.IntuitionGrowthType, playstationPilot.IntuitionGrowthType,
                    snesPilot.GetIntuitionGrowthType(), playstationPilot.GetIntuitionGrowthType()
                    );

                stringBuilder.Append(Resource.RstUnitGridColumnBreak);
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "远攻击"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));

                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "命中"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));

                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "技量"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "己偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "全偏差值"));
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "成长率"));

                stringBuilder.Append(Resource.RstUnitGridColumnBreak);
                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                     snesPilot.FarAttack, playstationPilot.FarAttack,
                     snesPilot.FarAttack99(), playstationPilot.FarAttack99(),
                     PilotTScoreParameterIndex.FarAttack, PilotTScoreParameterIndex.FarAttack99,
                     snesPilot.FarGrowthType, playstationPilot.FarGrowthType,
                     snesPilot.GetFarGrowthType(), playstationPilot.GetFarGrowthType()
                 );

                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                     snesPilot.Accuracy, playstationPilot.Accuracy,
                     snesPilot.Accuracy99(), playstationPilot.Accuracy99(),
                     PilotTScoreParameterIndex.Accuracy, PilotTScoreParameterIndex.Accuracy99,
                     snesPilot.AccuracyGrowthType, playstationPilot.AccuracyGrowthType,
                     snesPilot.GetAccuracyGrowthType(), playstationPilot.GetAccuracyGrowthType()
                 );

                WriteAttributeWithTScore(stringBuilder, pilotTScoreParametersSet,
                     snesPilot.Skill, playstationPilot.Skill,
                     snesPilot.Skill99(), playstationPilot.Skill99(),
                     PilotTScoreParameterIndex.Skill, PilotTScoreParameterIndex.Skill99,
                     snesPilot.SkillGrowthType, playstationPilot.SkillGrowthType,
                     snesPilot.GetSkillGrowthType(), playstationPilot.GetSkillGrowthType()
                 );
            }
        }

        private static void WriteAttributeWithTScore(StringBuilder stringBuilder, PilotTScoreParametersSet pilotTScoreParametersSet,
            int snesValue, int playstationValue,
            int snesValue99, int playstationValue99,
            PilotTScoreParameterIndex pilotTScoreParameterIndex, PilotTScoreParameterIndex pilotTScoreParameterIndex99,
            int snesNearGrowthType, int playstationGrowthType
            , string snesNearGrowthTypeText, string playstationNearGrowthTypeText)
        {            
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText,string.Empty));
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("        {0}→\r\n", Rom.FormatValue(snesValue, playstationValue));
            stringBuilder.AppendFormat("        {0}", Rom.FormatValue(snesValue99, playstationValue99));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            var snesTScore = Math.Round(pilotTScoreParametersSet.SnesOwned.GetTScoreParameter(pilotTScoreParameterIndex).CalculateTScore(snesValue));
            var playStationTScore = Math.Round(pilotTScoreParametersSet.PlayStationOwned.GetTScoreParameter(pilotTScoreParameterIndex).CalculateTScore(playstationValue));

            var snesTScore99 = Math.Round(pilotTScoreParametersSet.SnesOwned.GetTScoreParameter(pilotTScoreParameterIndex99).CalculateTScore(snesValue99));
            var playStationTScore99 = Math.Round(pilotTScoreParametersSet.PlayStationOwned.GetTScoreParameter(pilotTScoreParameterIndex99).CalculateTScore(playstationValue99));

            if (snesTScore == snesTScore99 && playStationTScore == playStationTScore99)
            {
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, 
                    Rom.FormatValue(snesTScore, playStationTScore)));
            }
            else
            {
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText,
                    string.Format("{0}→{1}",
                    Rom.FormatValue(snesTScore, playStationTScore),
                    Rom.FormatValue(snesTScore99, playStationTScore99))));
            }
            snesTScore = Math.Round(pilotTScoreParametersSet.SnesEncountered.GetTScoreParameter(pilotTScoreParameterIndex).CalculateTScore(snesValue));
            playStationTScore = Math.Round(pilotTScoreParametersSet.PlayStationEncountered.GetTScoreParameter(pilotTScoreParameterIndex).CalculateTScore(playstationValue));

            snesTScore99 = Math.Round(pilotTScoreParametersSet.SnesEncountered.GetTScoreParameter(pilotTScoreParameterIndex99).CalculateTScore(snesValue99));
            playStationTScore99 = Math.Round(pilotTScoreParametersSet.SnesEncountered.GetTScoreParameter(pilotTScoreParameterIndex99).CalculateTScore(playstationValue99));

            if (snesTScore == snesTScore99 && playStationTScore == playStationTScore99)
            {
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText,
                    Rom.FormatValue(snesTScore, playStationTScore)));
            }
            else
            {
                stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText,
                    string.Format("{0}→{1}",
                    Rom.FormatValue(snesTScore, playStationTScore),
                    Rom.FormatValue(snesTScore99, playStationTScore99))));
            }

            var growthType = string.Empty;
            if (snesNearGrowthType != 0 || playstationGrowthType != 0)
            {
                growthType = Rom.FormatValue(snesNearGrowthTypeText, playstationNearGrowthTypeText);
            }
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, growthType));
        }
    }
}
