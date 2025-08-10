using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Pilot : IRstFormatter
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Affiliation { get; set; }
        public string? Franchise { get; set; }
        public int BaseOffset { get; private set; }
        public int FaceId { get; set; }
        public byte InGameFranchise { get; set; }

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
        public byte TerrianAdaptionAir { get; private set; }
        public byte TerrianAdaptionSea { get; private set; }
        public byte TerrianAdaptionSpace { get; private set; }
        public byte TerrianAdaptionLand { get; private set; }
        public byte NearAttack { get; private set; }
        public byte FarAttack { get; private set; }
        public byte Accuracy { get; private set; }
        public byte Skill { get; private set; }
        public byte Evasion { get; private set; }
        public byte Intuition { get; private set; }
        public byte StartSP { get; private set; }
        public List<PilotSpiritCommandsOrSkill>? SpiritCommandsOrSkills { get;  set; }

        public static List<Pilot>? Parse(byte[] pilotData, int headerOffset, int dataOffset, List<Pilot> pilots, bool isPlayStation)
        {
            var magicMark = BitConverter.ToUInt16(pilotData, headerOffset);
            Debug.Assert(magicMark == 0);

            var firstPilotOffset = BitConverter.ToUInt16(pilotData, headerOffset + 2);

            var pilotList = new List<Pilot>();

            for (int pilotIndex = 1; pilotIndex < 256 ; pilotIndex++)
            {
                var unitOffset = BitConverter.ToUInt16(pilotData, headerOffset + pilotIndex * 2);
                if (unitOffset == 0 ) continue;//no data at this address
                Pilot pilot = ParsePilot(pilotData, dataOffset + unitOffset, pilotIndex, isPlayStation);
                FixPilotData(pilot, pilots);
                pilotList.Add(pilot);
            }
            return pilotList.Where(u => (u.Affiliation != null && !u.Affiliation.Equals(""))).OrderBy(p => p.Id).ToList();
        }

        private static void FixPilotData(Pilot pilot, List<Pilot> pilots)
        {
            var fixPilot = pilots.Where(p => p.Id == pilot.Id).FirstOrDefault();
            if (fixPilot == null)
            {
                Debug.WriteLine(string.Format("unable to find pilot with id {0}", pilot.Id));
                return;
            }
            pilot.Name = fixPilot.Name;
            pilot.Affiliation = fixPilot.Affiliation;
            pilot.Franchise = fixPilot.Franchise;
        }

        private static Pilot ParsePilot(byte[] playStationUnitData, int baseOffset, int pilotIndex, bool isPlayStation)
        {
            Pilot pilot = new Pilot();
            pilot.Id = pilotIndex;
            pilot.BaseOffset = baseOffset;            
            int offset = baseOffset;
            pilot.FaceId= playStationUnitData[offset++];
            var inGameFranchise= playStationUnitData[offset++];
            pilot.InGameFranchise = (byte)(inGameFranchise & 0xFE);
            if ((inGameFranchise & 0x01) != 0)
            {
                pilot.FaceId += 256;
            }
            if (isPlayStation)
            {
                pilot.PlayStationFranchise2 = playStationUnitData[offset++];
            }
            byte transferFranchiseAndPersonality=playStationUnitData[offset++];
            pilot.TransferFranchiseId = (byte)(transferFranchiseAndPersonality & 0xF);
            pilot.IsFemale= (transferFranchiseAndPersonality & 0x80)  != 0;
            pilot.IsFixedSeat= (transferFranchiseAndPersonality & 0x40) != 0;
            pilot.Personality = (byte)((transferFranchiseAndPersonality & 0x30)/10);
            pilot.ExperienceAward= playStationUnitData[offset++];
            byte growthType = playStationUnitData[offset++];
            pilot.AccuracyGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.SkillGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.SkillGrowthType < 4);
            Debug.Assert(pilot.AccuracyGrowthType < 4);

            growthType = playStationUnitData[offset++];
            pilot.NearGrowthType = (byte)((growthType & 0xF0) / 16);
            pilot.FarGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.NearGrowthType <= 4);
            Debug.Assert(pilot.FarGrowthType <= 4);

            pilot.SPGrowthType = (byte)((playStationUnitData[offset++])/16);
            Debug.Assert(pilot.SPGrowthType < 4);
            growthType = playStationUnitData[offset++];
            pilot.EvasionGrowthType = (byte)((growthType & 0xF0) / 16);
            Debug.Assert(pilot.EvasionGrowthType < 5);
            pilot.IntuitionGrowthType = (byte)(growthType & 0xF);
            Debug.Assert(pilot.IntuitionGrowthType < 3);
            var terrianAdaption = playStationUnitData[offset++];
            pilot.TerrianAdaptionAir = (byte)((terrianAdaption & 0xF0) / 16);
            pilot.TerrianAdaptionSea = (byte)(terrianAdaption & 0x0F);
            Debug.Assert(pilot.TerrianAdaptionAir < 5);
            Debug.Assert(pilot.TerrianAdaptionSea < 5);

            terrianAdaption = playStationUnitData[offset++];
            pilot.TerrianAdaptionSpace = (byte)((terrianAdaption & 0xF0)/16);
            pilot.TerrianAdaptionLand = (byte)(terrianAdaption & 0x0F);
            Debug.Assert(pilot.TerrianAdaptionSpace < 5);
            Debug.Assert(pilot.TerrianAdaptionLand < 5);
            pilot.NearAttack=playStationUnitData[offset++];
            pilot.FarAttack = playStationUnitData[offset++];
            pilot.Accuracy = playStationUnitData[offset++];
            pilot.Skill = playStationUnitData[offset++];
            pilot.Evasion = playStationUnitData[offset++];
            pilot.Intuition = playStationUnitData[offset++];
            pilot.StartSP = playStationUnitData[offset++];
            pilot.SpiritCommandsOrSkills=new List<PilotSpiritCommandsOrSkill>();
            ushort testData=BitConverter.ToUInt16(playStationUnitData, offset);
            while (testData != 0)
            {
                var pilotSpiritCommandsOrSkill = playStationUnitData[offset++];
                var acquireAtLevel = playStationUnitData[offset++];
                Debug.Assert(pilotSpiritCommandsOrSkill < 0x40);
                Debug.Assert(acquireAtLevel < 64);
                PilotSpiritCommandsOrSkill spiritCommandsOrSkill = new PilotSpiritCommandsOrSkill
                {
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
            stringBuilder.AppendFormat("Id: {0}", Id);
            stringBuilder.AppendFormat(", 名: {0}", Name);
            stringBuilder.AppendFormat(", BaseOffset: {0:X}", BaseOffset);
            stringBuilder.AppendFormat(", SpiritCommandOffset: {0:X}", BaseOffset+0x12);
            stringBuilder.AppendFormat(", TerrianAdaption: {0:X}", BaseOffset + 0x09);
            stringBuilder.AppendFormat(", 属: {0:X}", Affiliation);
            stringBuilder.AppendFormat(", 作: {0:X}", Franchise);
            stringBuilder.AppendFormat(", 颜: {0:X}", FaceId);
            if(PlayStationFranchise2 != 0)
                stringBuilder.AppendFormat(", PlayStationFranchise2: {0}", FranchiseHelper.FormatPlayStationFranchise2(PlayStationFranchise2));
            stringBuilder.AppendFormat(", 游戏作: {0:X}({1})", InGameFranchise,FranchiseHelper.FormatInGameFranchise(InGameFranchise));
            stringBuilder.AppendFormat(", 换乘: {0:X}", TransferFranchiseId);
            if(IsFemale)
                stringBuilder.Append(", 女");
            if (IsFixedSeat)
                stringBuilder.Append(", 換乘不可");
            stringBuilder.AppendFormat(", Personality: {0}", FormatPersonality(Personality));
            stringBuilder.AppendFormat(", ExperienceAward: {0}\r\n", ExperienceAward);
            stringBuilder.AppendFormat(", Growth Type: {0},{1},{2},{3},{4},{5},{6}\r\n",
                NearGrowthType, FarGrowthType, AccuracyGrowthType, SkillGrowthType, EvasionGrowthType, IntuitionGrowthType, SPGrowthType); 
            
            stringBuilder.AppendFormat(", TerrianAdaption: {0}\r\n", TerrianAdaptionHelper.FormatTerrianAdaption(
                new byte[] {
                TerrianAdaptionAir,TerrianAdaptionLand, TerrianAdaptionSea, TerrianAdaptionSpace}));
            stringBuilder.AppendFormat(", 近: {0}", NearAttack);
            stringBuilder.AppendFormat(", 远: {0}", FarAttack);
            stringBuilder.AppendFormat(", 命: {0}", Accuracy);
            stringBuilder.AppendFormat(", 技: {0}", Skill); 
            stringBuilder.AppendFormat(", 回: {0}", Evasion);
            stringBuilder.AppendFormat(", 直: {0}", Intuition);
            stringBuilder.AppendFormat(", SP: {0}", StartSP);
            stringBuilder.Append("\r\n 精神");
            if (SpiritCommandsOrSkills != null)
            {
                foreach (var pilotSpiritCommandsOrSkill in SpiritCommandsOrSkills)
                {
                    stringBuilder.AppendFormat(", {0}({1})", PilotSpiritCommandsOrSkill.Format(pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill), pilotSpiritCommandsOrSkill.AcquireAtLevel);
                }
            }
            return stringBuilder.ToString();
        }


        private string FormatPersonality(byte personality)
        {
            switch (personality)
            {
                case 3: return "超強気";
                case 2:return "強気";
                case 1:return "普通";
                case 0:return "弱気";
                default:return string.Empty;
            }
        }
        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.AppendLine(string.Format("     - {0}", this.Affiliation));
            row.AppendLine(string.Format("     - {0:X2}", GetPilotIcon(this.Id)));
            row.AppendLine(string.Format("     - {0}", this.Name));
            row.AppendLine(string.Format("     - {0}", FranchiseHelper.ToRstFranchise(this.Franchise, "pilots")));
            row.AppendLine(string.Format("     - {0}", FormatPersonality(this.Personality)));
            row.AppendLine(string.Format("     - {0}", this.NearAttack));
            row.AppendLine(string.Format("     - {0}", this.FarAttack));
            row.AppendLine(string.Format("     - {0}", this.Evasion));
            row.AppendLine(string.Format("     - {0}", this.Accuracy));
            row.AppendLine(string.Format("     - {0}", this.Intuition));
            row.AppendLine(string.Format("     - {0}", this.Skill));
            row.AppendLine(string.Format("     - {0}", this.NearAttack99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.FarAttack99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.Evasion99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.Accuracy99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.Intuition99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.Skill99(isPlayStation)));
            row.AppendLine(string.Format("     - {0}", this.StartSP));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionSpace)));
            if (SpiritCommandsOrSkills != null)
            {
                var validSpiritCommandsOrSkills = SpiritCommandsOrSkills.Where(s => s.SpiritCommandsOrSkill != 0);
                var first6= validSpiritCommandsOrSkills.Take(6).ToList();
                var rest = validSpiritCommandsOrSkills.Skip(6).ToList();
                foreach (var pilotSpiritCommandsOrSkill in first6)
                {
                    row.AppendLine(string.Format("     - {0} {1}", PilotSpiritCommandsOrSkill.Format(pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill), pilotSpiritCommandsOrSkill.AcquireAtLevel));
                }
                if(rest.Count > 1)
                {
                    bool isFirst = true;
                    foreach (var pilotSpiritCommandsOrSkill in rest)
                    {
                        if(isFirst)
                        {
                            row.Append("     - | ");
                            isFirst = false;
                        }
                        else
                        {
                            row.Append("       | ");
                        }
                        row.AppendLine(string.Format("{0} {1}", PilotSpiritCommandsOrSkill.Format(pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill), pilotSpiritCommandsOrSkill.AcquireAtLevel));
                    }
                }
                else if(rest.Count == 1)
                {
                    var pilotSpiritCommandsOrSkill = rest.First();
                    row.AppendLine(string.Format("     - {0} {1}", PilotSpiritCommandsOrSkill.Format(pilotSpiritCommandsOrSkill.SpiritCommandsOrSkill), pilotSpiritCommandsOrSkill.AcquireAtLevel));
                }
            }
            return row.ToString();
        }

        private int Skill99(bool isPlayStation)
        {
            int baseline = this.Skill+ 99;
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

        private int Accuracy99(bool isPlayStation)
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
        private int Evasion99(bool isPlayStation)
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
        

        private int Intuition99(bool isPlayStation)
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

        private int NearAttack99(bool isPlayStation)
        {
            int result = this.NearAttack + 52;
            switch (this.NearGrowthType)
            {
                case 1://バーニィ, スーパー男主人公
                    result+= 20;break;
                case 2: //流竜馬
                    result +=  10; break;
                case 3:
                    result +=  5; break;
                case 4://藤原忍
                    result += 15; break;

            }
            return result>255 ? 255 : result;
        }
        private int FarAttack99(bool isPlayStation)
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
            if(id>=0xC8 && id <=0xD0)
                return string.Empty;
            if (id >= 0xF8)
                return string.Empty;
            switch (id)
            {
                case 0xED:
                case 0xEF:
                    return string.Empty;
                default:
                    return string.Format(".. image:: ../pilots/images/srw4_pilot_{0:X2}.png", id);

            }
        }
    }
}
