using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pilot : IRstFormatter, INamedItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EnglishName { get; set; }
        public string? Affiliation { get; set; }
        public string? FranchiseName { get; set; }
        public int BaseOffset { get; private set; }
        public int FaceId { get; set; }
        public byte FranchiseId { get; set; }

        public byte PlayStationFranchise2 { get; private set; }
        public byte TransferFranchiseId { get; private set; }
        public bool IsFemale { get; private set; }
        public bool IsFixedSeat { get; private set; }
        public byte Personality { get; private set; }
        public byte ExperienceAward { get; private set; }
        public byte AccuracyGrowthType { get; private set; }
        public byte SkillGrowthType { get; private set; }
        public byte NearGrowthType { get; set; }
        public byte FarGrowthType { get; set; }
        public byte SPGrowthType { get; private set; }
        public byte EvasionGrowthType { get; private set; }
        public byte IntuitionGrowthType { get; private set; }
        public byte TerrainAdaptionAir { get; private set; }
        public byte TerrainAdaptionSea { get; private set; }
        public byte TerrainAdaptionSpace { get; private set; }
        public byte TerrainAdaptionLand { get; private set; }
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
        public static List<Pilot>? Parse(byte[] pilotData, int headerOffset, int dataOffset, List<PilotMetaData> pilotMetaData, bool isPlayStation)
        {
            var magicMark = BitConverter.ToUInt16(pilotData, headerOffset);
            Debug.Assert(magicMark == 0);

            var pilotList = new List<Pilot>();

            for (int pilotIndex = 1; pilotIndex < 256; pilotIndex++)
            {
                var unitOffset = BitConverter.ToUInt16(pilotData, headerOffset + pilotIndex * 2);
                if (unitOffset == 0) continue;//no data at this address
                Pilot pilot = ParsePilot(pilotData, dataOffset + unitOffset, pilotIndex, isPlayStation);
                FixPilotData(pilot, pilotMetaData);
                pilotList.Add(pilot);
            }
            return pilotList.Where(u => (u.Affiliation != null && !u.Affiliation.Equals(""))).OrderBy(p => p.Id).ToList();
        }

        private static void FixPilotData(Pilot pilot, List<PilotMetaData> pilotMetaData)
        {
            var fixPilot = pilotMetaData.Where(p => p.Id == pilot.Id).FirstOrDefault();
            if (fixPilot == null)
            {
                Debug.WriteLine(string.Format("unable to find pilot with id {0}", pilot.Id));
                return;
            }
            pilot.Name = fixPilot.Name;
            pilot.EnglishName = fixPilot.EnglishName;
            pilot.Affiliation = fixPilot.Affiliation;
            pilot.FranchiseName = fixPilot.FranchiseName;
            if (string.Compare(pilot.FranchiseName, "原创", StringComparison.Ordinal) == 0)
            {
                pilot.FranchiseName = "オリジナル";
            }
            if (string.Compare(pilot.FranchiseName, "マジンガーＺ", StringComparison.Ordinal) == 0)
            {
                pilot.FranchiseName = "マジンガーZ";
            }
        }

        private static Pilot ParsePilot(byte[] playStationUnitData, int baseOffset, int pilotIndex, bool isPlayStation)
        {
            Pilot pilot = new Pilot();
            pilot.IsPlayStation=isPlayStation;
            pilot.Id = pilotIndex;
            pilot.BaseOffset = baseOffset;
            int offset = baseOffset;
            //offset 0
            pilot.FaceId = playStationUnitData[offset++];
            //offset 1
            var inGameFranchise = playStationUnitData[offset++];
            pilot.FranchiseId = (byte)(inGameFranchise & 0xFE);
            if ((inGameFranchise & 0x01) != 0)
            {
                pilot.FaceId += 256;
            }
            if (isPlayStation)
            {
                //offset 2
                pilot.PlayStationFranchise2 = playStationUnitData[offset++];
            }
            //offset 2/3
            byte transferFranchiseAndPersonality = playStationUnitData[offset++];
            pilot.TransferFranchiseId = (byte)(transferFranchiseAndPersonality & 0xF);
            pilot.IsFemale = (transferFranchiseAndPersonality & 0x80) != 0;
            pilot.IsFixedSeat = (transferFranchiseAndPersonality & 0x40) != 0;
            pilot.Personality = (byte)((transferFranchiseAndPersonality & 0x30) / 16);
            //offset 3/4
            pilot.ExperienceAward = playStationUnitData[offset++];
            //offset 4/5
            byte growthType = playStationUnitData[offset++];
            pilot.AccuracyGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.SkillGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.SkillGrowthType < 4);
            Debug.Assert(pilot.AccuracyGrowthType < 4);
            //offset 5/6
            growthType = playStationUnitData[offset++];
            pilot.NearGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.FarGrowthType = (byte)(growthType & 0xF);
            //Debug.Assert(pilot.NearGrowthType <= 4);
            //Debug.Assert(pilot.FarGrowthType <= 4);
            //offset 6/7
            pilot.SPGrowthType = (byte)((playStationUnitData[offset++]) / 16);
            Debug.Assert(pilot.SPGrowthType < 4);
            //offset 7/8
            growthType = playStationUnitData[offset++];
            pilot.EvasionGrowthType = (byte)((growthType & 0xF0) / 16);
            Debug.Assert(pilot.EvasionGrowthType < 5);
            pilot.IntuitionGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.IntuitionGrowthType < 3);
            //offset 8/9
            var TerrainAdaption = playStationUnitData[offset++];
            pilot.TerrainAdaptionAir = (byte)((TerrainAdaption & 0xF0) / 16);
            pilot.TerrainAdaptionSea = (byte)(TerrainAdaption & 0x0F);
            Debug.Assert(pilot.TerrainAdaptionAir < 5);
            Debug.Assert(pilot.TerrainAdaptionSea < 5);
            //offset 9/a
            TerrainAdaption = playStationUnitData[offset++];
            pilot.TerrainAdaptionSpace = (byte)((TerrainAdaption & 0xF0) / 16);
            pilot.TerrainAdaptionLand = (byte)(TerrainAdaption & 0x0F);
            Debug.Assert(pilot.TerrainAdaptionSpace < 5);
            Debug.Assert(pilot.TerrainAdaptionLand < 5);
            //offset a/b
            pilot.NearAttack = playStationUnitData[offset++];
            //offset b/c
            pilot.FarAttack = playStationUnitData[offset++];
            //offset c/d
            pilot.Accuracy = playStationUnitData[offset++];
            //offset d/e
            pilot.Skill = playStationUnitData[offset++];
            //offset e/f
            pilot.Evasion = playStationUnitData[offset++];
            //offset f/10
            pilot.Intuition = playStationUnitData[offset++];
            //offset 10/11
            pilot.StartSP = playStationUnitData[offset++];
            pilot.SpiritCommandsOrSkills = new List<PilotSpiritCommandsOrSkill>();
            ushort testData = BitConverter.ToUInt16(playStationUnitData, offset);
            while (testData != 0)
            {
                var baseAddress = offset;
                var pilotSpiritCommandsOrSkill = playStationUnitData[offset++];
                var acquireAtLevel = playStationUnitData[offset++];
                Debug.Assert(pilotSpiritCommandsOrSkill < 0x40);
                //Debug.Assert(acquireAtLevel < 64);
                PilotSpiritCommandsOrSkill spiritCommandsOrSkill = new PilotSpiritCommandsOrSkill
                {
                    BaseAddress = baseAddress,
                    AcquireAtLevel = acquireAtLevel,
                    SpiritCommandsOrSkill = pilotSpiritCommandsOrSkill
                };
                pilot.SpiritCommandsOrSkills.Add(spiritCommandsOrSkill);
                testData = BitConverter.ToUInt16(playStationUnitData, offset);
            }
            return pilot;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.AppendFormat(" Id: {0:X}", Id);
            stringBuilder.AppendFormat("\t 名: {0}\t ({1})", Name, EnglishName);
            stringBuilder.AppendFormat("\t 属: {0:X}", Affiliation);
            stringBuilder.AppendFormat("\t 作: {0:X}", FranchiseName);
            stringBuilder.AppendFormat("\t 颜: {0:X}:{1}", BaseOffset, FaceId);
            stringBuilder.AppendFormat("\t 游戏作: {0:X}:{1:X}({2})", BaseOffset + 0x01, FranchiseId, Franchise.FormatFranchise(FranchiseId));
            if (PlayStationFranchise2 != 0)
                stringBuilder.AppendFormat("\t PS游戏作: {0:X}:{1}", BaseOffset + 0x02, Franchise.FormatPlayStationFranchise2(PlayStationFranchise2));
            stringBuilder.AppendFormat("\t 换乘: {0:X}:{1:X}", BaseOffset + (this.IsPlayStation ? 3 : 2), TransferFranchiseId);
            if (IsFemale)
                stringBuilder.Append("\t 女");
            if (IsFixedSeat)
                stringBuilder.Append("\t 換乘不可");
            stringBuilder.AppendFormat("\t 性格: {0}", FormatPersonality(Personality));
            stringBuilder.AppendFormat("\t 经验: {0}", ExperienceAward);
            stringBuilder.AppendFormat("\t 能力增长模式: {0},{1},{2},{3},{4},{5},{6}",
                NearGrowthType, FarGrowthType, AccuracyGrowthType, SkillGrowthType, EvasionGrowthType, IntuitionGrowthType, SPGrowthType);

            stringBuilder.AppendFormat("\r\n 地形适应: {0:X}:{1}(空{2}陆{3}海{4}宇{5})", BaseOffset + (this.IsPlayStation ? 9 : 8), Convert.ToHexString(
                new byte[] {
                (byte)(TerrainAdaptionAir*16+TerrainAdaptionSea), (byte)(TerrainAdaptionLand*16+TerrainAdaptionSpace)})

                ,TerrainAdaptionHelper.FormatTerrainAdaption(TerrainAdaptionAir)
                ,TerrainAdaptionHelper.FormatTerrainAdaption(TerrainAdaptionLand)
                , TerrainAdaptionHelper.FormatTerrainAdaption(TerrainAdaptionSea)
                , TerrainAdaptionHelper.FormatTerrainAdaption(TerrainAdaptionSpace));
            stringBuilder.AppendFormat("\t 近: {0:X}:{1}", BaseOffset + (this.IsPlayStation ? 0xb : 0xa), NearAttack);
            stringBuilder.AppendFormat("\t 远: {0}", FarAttack);
            stringBuilder.AppendFormat("\t 命: {0}", Accuracy);
            stringBuilder.AppendFormat("\t 技: {0}", Skill);
            stringBuilder.AppendFormat("\t 回: {0}", Evasion);
            stringBuilder.AppendFormat("\t 直: {0}", Intuition);
            stringBuilder.AppendFormat("\t SP: {0}", StartSP);
            stringBuilder.Append("\r\n 精神/技能:");
            stringBuilder.AppendLine(FormatSpiritCommandsOrSkills(SpiritCommandsOrSkills));
            return stringBuilder.ToString();
        }


        private string FormatPersonality(byte personality)
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
            row.AppendLine(string.Format("     - {0:X2}", GetPilotIcon(this.Id)));
            row.AppendLine(string.Format("     - {0}", this.Name));
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
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionSpace)));
            row.AppendLine(string.Format("     - {0}", FormatSpiritCommandsOrSkills(SpiritCommandsOrSkills)));
           
            return row.ToString();
        }
        string FormatSpiritCommandsOrSkills(List<PilotSpiritCommandsOrSkill>? spiritCommandsOrSkills)
        {
            if (SpiritCommandsOrSkills == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            var validSpiritCommandsOrSkills = SpiritCommandsOrSkills.Where(s => s.SpiritCommandsOrSkill != 0);
            byte previousSpiritCommandsOrSkill = 0;
            foreach (var pilotSpiritCommandsOrSkill in validSpiritCommandsOrSkills)
            {
                string pilotSpiritCommandsOrSkillString = string.Format("{0} {1}", PilotSpiritCommandsOrSkill.Format(
                    pilotSpiritCommandsOrSkill.BaseAddress,
                    pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill, previousSpiritCommandsOrSkill), pilotSpiritCommandsOrSkill.AcquireAtLevel);
                if (sb.Length != 0)
                {
                    sb.Append("\t ");
                }
                sb.Append(pilotSpiritCommandsOrSkillString);
                previousSpiritCommandsOrSkill = pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill;
            }
            return sb.ToString();
        }

        private int Skill99()
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

        private int Accuracy99()
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
        private int Evasion99()
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


        private int Intuition99()
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

        private int NearAttack99()
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
        private int FarAttack99()
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

        private string GetPilotIcon(int id)
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
    }
}
