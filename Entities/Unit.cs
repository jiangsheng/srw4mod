


using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Linq;

namespace Entities
{
    public class Unit : IRstFormatter, INamedItem
    {

        public int Id { get; set; }
        public string? Name { get; set; }
        public string? EnglishName { get; set; }
        public string? Affiliation { get; set; }
        public string? FranchiseName { get; set; }

        public ushort IconId { get; set; }
        public byte FranchiseId { get; set; }
        public ushort PortraitId { get; set; }
        public byte FixedSeatPilotId { get; set; }
        public byte TransferFranchiseId { get; set; }
        public bool Discardable { get; set; }
        public byte UnitSize { get; set; }
        public byte UnitSizeBit { get; set; }
        public byte BackgroundMusic { get; set; }
        public byte TransformOrCombineType { get; set; }
        public byte UnknownUnitSpecialSkill2 { get; set; }
        public bool HasSword { get; private set; }
        public int BeamCoatType { get; private set; }
        public bool HasAfterimage { get; private set; }
        public bool HasShield { get; private set; }
        public bool HasEnergyRecovery { get; private set; }
        public byte HPRecoveryType { get; private set; }
        public bool RageAndDetonateImmune { get; private set; }
        public bool IsAggressive { get; set; }
        public byte Team { get; set; }
        public byte Experience { get; set; }

        public ushort Gold { get; set; }

        public ushort RepairCost { get; set; }
        public byte MoveRange { get; set; }
        public byte MoveType { get; set; }

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
        public byte Armor { get; set; }
        public byte Mobility { get; set; }
        public byte Limit { get; set; }
        public short Energy { get; set; }
        public ushort HP { get; set; }
        public byte WeaponCount { get; set; }
        public byte AmmoWeaponCount { get; set; }
        public int BaseOffset { get; set; }
        public List<UnitWeapon>? Weapons { get; set; }
        [Ignore]
        public ushort FirstWeaponIndex { get; set; }
        [Ignore]
        public int FirstAppearance { get; set; }

        public int PreferredPilotId { get; set; }
        public static List<Unit>? Parse(byte[] unitData, int headerStartOffset, int offsetBase, int footerOffset, List<UnitMetaData> unitMetaData, List<Weapon> weapons)
        {
            var magicMark = BitConverter.ToUInt16(unitData, headerStartOffset);
            Debug.Assert(magicMark == 0);

            var firstUnitOffset = BitConverter.ToUInt16(unitData, headerStartOffset + 2);

            var unitList = new List<Unit>();

            for (int unitIndex = 1; unitIndex < firstUnitOffset / 2; unitIndex++)
            {
                var unitOffset = BitConverter.ToUInt16(unitData, headerStartOffset + unitIndex * 2);
                if (unitOffset == 0) continue;//no data at this address
                if (unitOffset >= footerOffset) break;//reached footer
                Unit unit = ParseUnit(unitData, offsetBase + unitOffset, unitIndex);
                FixUnitData(unit, unitMetaData, weapons);
                unitList.Add(unit);
            }
            return unitList.Where(u => (u.Affiliation != null && !u.Affiliation.Equals(""))).OrderBy(u => u.Id).ToList();
        }

        private static void FixUnitData(Unit unit, List<UnitMetaData> unitMetaData, List<Weapon> weapons)
        {
            var fixUnit = unitMetaData.Where(u => u.Id == unit.Id).FirstOrDefault();
            if (fixUnit == null)
            {
                Debug.WriteLine(string.Format("unable to find unit with id {0}", unit.Id));
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(fixUnit.Name);
            stringBuilder = stringBuilder.Replace("GMIII", "GM III");
            stringBuilder = stringBuilder.Replace("３", "3");
            stringBuilder = stringBuilder.Replace("(MS)", " (MS)");
            stringBuilder = stringBuilder.Replace("(MA)", " (MA)");
            stringBuilder = stringBuilder.Replace("  (MS)", " (MS)");
            stringBuilder = stringBuilder.Replace("  (MA)", " (MA)");
            stringBuilder = stringBuilder.Replace("mkII", " Mk-II");
            stringBuilder = stringBuilder.Replace("  MkII", " Mk-II");
            unit.Name = stringBuilder.ToString();
            unit.EnglishName = fixUnit.EnglishName;
            unit.Affiliation = fixUnit.Affiliation;
            unit.FranchiseName = fixUnit.FranchiseName;
            unit.FirstAppearance = fixUnit.FirstAppearance;

            unit.PreferredPilotId = fixUnit.PreferredPilotId;
            if (unit.FixedSeatPilotId != 0)
            {
                unit.PreferredPilotId = unit.FixedSeatPilotId;
            }

            if (string.Compare(unit.FranchiseName, "原创", StringComparison.Ordinal) == 0)
            {
                unit.FranchiseName = "オリジナル";
            }
            if (string.Compare(unit.FranchiseName, "マジンガーＺ", StringComparison.Ordinal) == 0)
            {
                unit.FranchiseName = "マジンガーZ";
            }

            if (unit.Weapons != null)
            {
                foreach (var unitWeapon in unit.Weapons)
                {
                    var fixWeapon = weapons.Where(w => w.Id == unitWeapon.WeaponIndex).First();
                    if (!fixWeapon.HasAssignedOwner)
                    {
                        fixWeapon.HasAssignedOwner = true;
                        fixWeapon.FirstOwner = unit;
                    }
                    unitWeapon.FirstOwner = fixWeapon.FirstOwner;

                    unitWeapon.Weapon = fixWeapon;
                    unitWeapon.Unit = unit;
                }
            }
        }

        private static Unit ParseUnit(byte[] unitData, int baseOffset, int unitIndex)
        {
            Unit unit = new Unit();
            unit.BaseOffset = baseOffset;
            int offset = baseOffset;
            //offset 0
            byte iconId1 = unitData[offset++];
            //offset 1
            byte fanchiseId = unitData[offset++];

            var iconId2 = fanchiseId & 0x01;

            unit.IconId = (ushort)(iconId2 * 256 + iconId1);

            unit.Id = unitIndex;
            unit.FranchiseId = (byte)(fanchiseId & 0xFE);
            //offset 2,3
            unit.PortraitId = BitConverter.ToUInt16(unitData, offset);
            offset += 2;
            //offset 4
            unit.FixedSeatPilotId = unitData[offset++];
            //offset 5
            unit.TransferFranchiseId = unitData[offset++];
            //offset 6
            var sizeAndBGM = unitData[offset++];

            unit.UnitSize = (byte)(sizeAndBGM & 0x60);
            unit.UnitSizeBit = (byte)(sizeAndBGM & 0x10);
            unit.Discardable = (sizeAndBGM & 0x80) == 0;
            unit.BackgroundMusic = (byte)(sizeAndBGM & 0x0F);
            //offset 7
            unit.TransformOrCombineType = unitData[offset++];
            //offset 8
            var unitSpecialSkill1 = unitData[offset++];
            //offset 9
            var unitSpecialSkill2 = unitData[offset++];
            unit.IsAggressive = (unitSpecialSkill1 & 0x1) != 0;
            unit.HasEnergyRecovery = (unitSpecialSkill1 & 0x02) != 0;
            unit.HPRecoveryType = (byte)(unitSpecialSkill1 & 0x0C);
            unit.HasSword = (unitSpecialSkill1 & 0x40) != 0;
            unit.RageAndDetonateImmune = (unitSpecialSkill1 & 0x80) != 0;

            unit.BeamCoatType = unitSpecialSkill2 & 0x0E;
            unit.HasAfterimage = (unitSpecialSkill2 & 0x10) != 0;
            unit.HasShield = (unitSpecialSkill2 & 0x20) != 0;
            unit.UnknownUnitSpecialSkill2 = (byte)(unitSpecialSkill1 & (~0x3F));
            //offset A
            unit.Team = unitData[offset++];
            //offset B,C,D,E
            offset += 4;
            //offset F
            unit.Experience = unitData[offset++];
            //offset 10,11
            unit.Gold = BitConverter.ToUInt16(unitData, offset);
            offset += 2;
            //offset 12,13
            unit.RepairCost = BitConverter.ToUInt16(unitData, offset);
            offset += 2;
            //offset 14
            unit.MoveRange = unitData[offset++];
            Debug.Assert(unit.MoveRange < 16);
            //offset 15
            unit.MoveType = unitData[offset++];
            Debug.Assert(unit.MoveType < 0xD);
            //offset 16
            var terrainAdaptionLow = unitData[offset++];
            var terrainAdaptionHigh = unitData[offset++];
            unit.TerrainAdaptionSet = TerrainAdaptionSet.FromPilotOrUnitAdaptions(
                terrainAdaptionLow, terrainAdaptionHigh);
            Debug.Assert(unit.TerrainAdaptionAir < 5);
            Debug.Assert(unit.TerrainAdaptionSea < 5);
            Debug.Assert(unit.TerrainAdaptionSpace < 5);
            Debug.Assert(unit.TerrainAdaptionLand < 5);
            //offset 18
            unit.Armor = unitData[offset++];
            //offset 19
            unit.Mobility = unitData[offset++];
            //offset 1A
            unit.Limit = unitData[offset++];
            //offset 1B
            unit.Energy = unitData[offset++];
            //offset 1C,1D
            unit.HP = BitConverter.ToUInt16(unitData, offset);
            offset += 2;
            //offset 1E
            unit.WeaponCount = unitData[offset++];
            Debug.Assert(unit.WeaponCount < 32);
            //offset 1F
            unit.AmmoWeaponCount = unitData[offset++];
            Debug.Assert(unit.AmmoWeaponCount < 32);
            Debug.Assert(unit.AmmoWeaponCount <= unit.WeaponCount);
            Debug.Assert(unit.WeaponCount < 0x20);

            unit.FirstWeaponIndex = (ushort)(BitConverter.ToUInt16(unitData, offset) & 0x03FF);

            //the last unit has no weapon 
            if (unit.WeaponCount > 0)
            {
                unit.Weapons = new List<UnitWeapon>();
                var weaponIndex = (ushort)(BitConverter.ToUInt16(unitData, offset));
                while (weaponIndex > 0)
                {
                    UnitWeapon unitWeapon = new UnitWeapon();
                    unitWeapon.BaseOffset = offset;
                    unitWeapon.WeaponIndex = (ushort)(weaponIndex & 0x03FF);
                    unitWeapon.AmmoSlot = (byte)((unitData[offset + 1] & 0xFC) / 4);
                    //Debug.Assert(unitWeapon.AmmoSlot <= unit.AmmoWeaponCount);
                    offset += 2;
                    var weaponOrderAndType = unitData[offset++];
                    unitWeapon.IsConditional = (weaponOrderAndType & 0x80) > 0;
                    unitWeapon.Order = (byte)(weaponOrderAndType & 0x1F);
                    Debug.Assert(unitWeapon.Order <= unit.WeaponCount);
                    if (unitWeapon.IsConditional)
                    {
                        unitWeapon.AvailableAtStage = unitData[offset++];
                    }
                    unit.Weapons.Add(unitWeapon);
                    weaponIndex = (ushort)((unitData[offset + 1] * 256 + unitData[offset]));
                }
            }
            else
            {
                var weaponIndex = (ushort)(unitData[offset + 1] * 256 + unitData[offset]);
                while (weaponIndex > 0)
                {
                    weaponIndex = (ushort)(unitData[offset + 1] * 256 + unitData[offset]);
                    if (weaponIndex > 0)
                        offset += 3;
                }
            }
            return unit;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.AppendFormat("Id: {0}", Id);
            stringBuilder.AppendFormat("\t名: {0}\t({1})", Name, EnglishName);
            stringBuilder.AppendFormat("\t属: {0}", Affiliation);
            stringBuilder.AppendFormat("\t作: {0}", FranchiseName);
            stringBuilder.AppendFormat("\t图标: {0:X}:{1:X}:", BaseOffset, IconId);
            stringBuilder.AppendFormat("\t游戏作:{0:X}:{1:X} ({2})", BaseOffset + 1, FranchiseId, Franchise.FormatFranchise(FranchiseId));
            stringBuilder.AppendFormat("\t图像: {0:X}:{1:X}", BaseOffset + 2, PortraitId);
            stringBuilder.AppendFormat("\t固定驾驶员: {0:X}:{1:X}", BaseOffset + 4, FixedSeatPilotId);
            stringBuilder.AppendFormat("\t换乘组: {0:X}:{1:X}", BaseOffset + 5, TransferFranchiseId);
            stringBuilder.AppendFormat("\t大小:{0:X}:{1:X} ({2})", BaseOffset + 6, UnitSize, FormatUnitSize(UnitSize));
            stringBuilder.AppendFormat("\t可废弃: {0}", Discardable ? "是" : "否");
            stringBuilder.AppendFormat("\tBGM: {0:X}", BackgroundMusic);
            stringBuilder.AppendFormat("\r\n合体分离变形种类: {0:X}:{1:X}", BaseOffset + 7, TransformOrCombineType);
            stringBuilder.AppendFormat("\t技能1: {0:X}", BaseOffset + 8);
            if (!IsAggressive)
            {
                stringBuilder.Append("\t不攻击");
            }
            if (HasSword)
            {
                stringBuilder.Append("\t剣装備");
            }
            if (HasEnergyRecovery)
            {
                stringBuilder.Append("\tEN恢復");
            }
            switch (HPRecoveryType)
            {
                case 0x04:
                    stringBuilder.Append("\tHP恢復(小)"); break;
                case 0x08:
                    stringBuilder.Append("\tHP恢復(大)"); break;
            }

            if (RageAndDetonateImmune)
            {
                stringBuilder.Append("\t激怒/自爆/てかげん無効");
            }
            stringBuilder.AppendFormat("\t技能2: {0:X}", BaseOffset + 9);
            switch (BeamCoatType)
            {
                case 0x02: stringBuilder.Append("\tビームコート"); break;
                case 0x04: stringBuilder.Append("\t Iフィールド"); break;
                case 0x06: stringBuilder.Append("\tオーラバリア"); break;
                case 0x08: stringBuilder.Append("\tビームバリア"); break;
            }
            if (HasAfterimage)
            {
                stringBuilder.Append("\t分身");
            }
            if (HasShield)
            {
                stringBuilder.Append("\t盾装備");
            }
            if (UnknownUnitSpecialSkill2 != 0)
            {
                stringBuilder.AppendFormat("\t未知技能 {0:X} ", UnknownUnitSpecialSkill2);
            }
            stringBuilder.AppendFormat("\t分队: {0:X}:{1:X}", BaseOffset + 0xa, Team);
            stringBuilder.AppendFormat("\t经验: {0:X}:{1}", BaseOffset + 0xf, Experience);
            stringBuilder.AppendFormat("\t获得资金: {0:X}:{1}", BaseOffset + 0x10, Gold);
            stringBuilder.AppendFormat("\t修理费 RepairCost: {0:X}:{1}", BaseOffset + 0x12, RepairCost);
            stringBuilder.AppendFormat("\t地形参照:{0:X}", PreferredPilotId);
            stringBuilder.AppendFormat("\r\n移动力: {0:X}:{1}", BaseOffset + 0x14, MoveRange);
            stringBuilder.AppendFormat("\t移动类型: {0:X}:{1:X}", BaseOffset + 0x15, MoveType);
            stringBuilder.AppendFormat("\t地形适应: {0:X}:{1}({2})", BaseOffset + 0x16,
                Convert.ToHexString(this.TerrainAdaptionSet != null ? this.TerrainAdaptionSet.ToPilotOrUnitAdaptions() : new byte[] { }),
                this.TerrainAdaptionSet?.ToString());
            stringBuilder.AppendFormat("\t甲: {0:X}:{1}", BaseOffset + 0x18, Armor * 10);
            stringBuilder.AppendFormat("\t运: {0:X}:{1}", BaseOffset + 0x19, Mobility);
            stringBuilder.AppendFormat("\t限: {0:X}:{1}", BaseOffset + 0x1a, Limit);
            stringBuilder.AppendFormat("\t能: {0:X}:{1}", BaseOffset + 0x1b, Energy);
            stringBuilder.AppendFormat("\tHP: {0:X}:{1}", BaseOffset + 0x1c, HP);
            stringBuilder.AppendFormat("\t武器数: {0:X}:{1}", BaseOffset + 0x1e, WeaponCount);
            stringBuilder.AppendFormat("\t弹药槽数: {0:X}:{1}", BaseOffset + 0x1f, AmmoWeaponCount);
            stringBuilder.AppendFormat("\t首武器地址: {0:X}", BaseOffset + 0x20);

            if (WeaponCount > 0 && Weapons != null)
            {
                stringBuilder.Append("\r\n");
                int weaponOrder = 1;
                foreach (var weapon in Weapons)
                {
                    stringBuilder.AppendFormat("{0}:{1}\r\n", weaponOrder++, weapon.ToString());
                }
            }
            return stringBuilder.ToString();
        }

        private string FormatUnitSize(int unitSize)
        {
            switch (unitSize)
            {
                case 0x00: return "S";
                case 0x20: return "M";
                case 0x40: return "L";
                case 0x60: return "LL";
                default: return "Unknown Unit Size " + unitSize.ToString();
            }
        }
        public int GetMaxDamage()
        {
            return int.Max(GetNearDamage(), GetFarDamage());
        }

        public int GetNearDamage()
        {
            var weapons = this.Weapons;
            if (weapons == null || weapons.Count == 0)
            {
                return 0;
            }
            var bestNearWeapon = weapons.Where(w => w.Weapon != null && w.Weapon.MaxRange == 1).OrderByDescending(w => w.Weapon == null ? 0 : w.Weapon.Damage).FirstOrDefault();
            if (bestNearWeapon == null) return 0;
            return bestNearWeapon.Weapon == null ? 0 : bestNearWeapon.Weapon.Damage;
        }
        public int GetFarDamage()
        {
            var weapons = this.Weapons;
            if (weapons == null || weapons.Count == 0)
            {
                return 0;
            }
            var bestFarWeapon = weapons.Where(w => w.Weapon != null && w.Weapon.MaxRange > 1 && w.Weapon.IsMap == false && w.Weapon.Damage > 0).OrderByDescending(w => w.Weapon == null ? 0 : w.Weapon.Damage).FirstOrDefault();
            if (bestFarWeapon == null) return 0;
            return bestFarWeapon.Weapon == null ? 0 : bestFarWeapon.Weapon.Damage;
        }
        string GetMaxRangeExceptMap(List<UnitWeapon>? weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return "🚫";
            }
            var bestFarWeapon = weapons.Where(w => w.Weapon != null && w.Weapon.MaxRange > 1 && w.Weapon.IsMap == false && w.Weapon.Damage > 0).OrderByDescending(w => w.Weapon == null ? 0 : w.Weapon.MaxRange).FirstOrDefault();
            if (bestFarWeapon == null) return "🚫";
            return bestFarWeapon.Weapon == null ? "0" : bestFarWeapon.Weapon.MaxRange.ToString();
        }
        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.AppendLine(string.Format("     - {0}", this.Affiliation));
            int unitIcon = GetUnitIcon(this.Id);
            if (unitIcon != 0)
                row.AppendLine(string.Format("     - .. image:: ../units/images/icon/srw4_units_icon_{0:X2}_B.png", unitIcon));
            else
                row.AppendLine("     - ");
            var unitLabel = RstHelper.GetLabelName(this.EnglishName);
            row.AppendLine(string.Format("     - \\ :ref:`{0} <srw4_unit_{1}>`\\ ", this.Name, unitLabel));
            row.AppendLine(string.Format("     - {0}", this.EnglishName));
            row.AppendLine(string.Format("     - {0}", Franchise.ToRstFranchise(this.FranchiseName, "units")));
            row.AppendLine(string.Format("     - {0}", this.HP));
            row.AppendLine(string.Format("     - {0}", this.Energy));
            row.AppendLine(string.Format("     - {0}", this.Mobility));
            row.AppendLine(string.Format("     - {0}", this.Armor * 10));
            row.AppendLine(string.Format("     - {0}", this.Limit));
            row.AppendLine(string.Format("     - {0}", this.MoveRange));
            var nearDamage = this.GetNearDamage();
            row.AppendLine(string.Format("     - {0}", nearDamage == 0 ? "🚫" : nearDamage.ToString()));
            var farDamage = this.GetFarDamage();
            row.AppendLine(string.Format("     - {0}", farDamage == 0 ? "🚫" : farDamage.ToString()));
            row.AppendLine(string.Format("     - {0}", this.GetMaxRangeExceptMap(this.Weapons)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatMovementType(this.MoveType)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet.FormatTerrainAdaption(this.TerrainAdaptionSpace)));
            return row.ToString();
        }

        private int GetUnitIcon(int id)
        {
            switch (id)
            {
                case 0x51:
                case 0xF8:
                case 0x101:
                case 0x106:
                case 0x10D:
                    return 0;
                default:
                    return id;

            }
        }

        internal static void RstAppendUnit(StringBuilder stringBuilder, int unitId, List<UnitMetaData> units, string unitComment, Rom snesRom, Rom playstationRom, Dictionary<string, string> comments, UnitTScoreParametersSet unitTScoreParametersSet)
        {
            var unitMetaData = units.FirstOrDefault(u => u.Id == unitId);
            if (unitMetaData == null || unitMetaData.Name == null)
            {
                throw new ArgumentNullException(nameof(unitMetaData));
            }


            var snesUnit = snesRom.Units?.FirstOrDefault(u => u.Id == unitId);
            if (snesUnit == null || snesUnit.TerrainAdaptionSet == null)
                throw new ArgumentNullException(nameof(snesUnit));

            var playstationUnit = playstationRom.Units?.FirstOrDefault(u => u.Id == unitId);

            if (playstationUnit == null || playstationUnit.TerrainAdaptionSet == null)
                throw new ArgumentNullException(nameof(playstationUnit));

            RstHelper.AppendHeader(stringBuilder, unitMetaData.Name, '^');
            stringBuilder.AppendLine();
            var unitLabel = RstHelper.GetLabelName(unitMetaData.EnglishName);
            Debug.Assert(!string.IsNullOrEmpty(unitLabel));

            stringBuilder.AppendLine(string.Format(".. _srw4_unit_{0}:", unitLabel));
            stringBuilder.AppendLine();

            var snesPreferredPilot = unitMetaData.PreferredPilotId == 0 ? null : snesRom.Pilots?.FirstOrDefault(p => p.Id == snesUnit.PreferredPilotId);
            //default to the unit terrain adaption
            TerrainAdaptionSet snesPreferredPilotTerrainAdaptionSet = snesUnit.TerrainAdaptionSet;

            if (snesPreferredPilot != null)
            {
                if (snesPreferredPilot.TerrainAdaptionSet != null)
                    snesPreferredPilotTerrainAdaptionSet = snesPreferredPilot.TerrainAdaptionSet;
            }
            var playstationPreferredPilot = unitMetaData.PreferredPilotId == 0 ? null : playstationRom.Pilots?.FirstOrDefault(p => p.Id == playstationUnit.PreferredPilotId);
            //default to the unit terrain adaption
            TerrainAdaptionSet playstationPreferredPilotTerrainAdaptionSet = playstationUnit.TerrainAdaptionSet;

            if (playstationPreferredPilot != null)
            {
                if (playstationPreferredPilot.TerrainAdaptionSet != null)
                    playstationPreferredPilotTerrainAdaptionSet = playstationPreferredPilot.TerrainAdaptionSet;
            }

            TerrainAdaptionSet snesEffectiveUnitTerrainAdoptions = TerrainAdaptionSet.Combine(snesUnit.TerrainAdaptionSet, snesPreferredPilotTerrainAdaptionSet);
            TerrainAdaptionSet playstationEffectiveUnitTerrainAdoptions = TerrainAdaptionSet.Combine(playstationUnit.TerrainAdaptionSet, playstationPreferredPilotTerrainAdaptionSet);

            RstAppendUnitMetaData(stringBuilder, unitMetaData, unitId, snesRom, playstationRom, snesUnit, playstationUnit
                , snesPreferredPilot, playstationPreferredPilot,
                snesEffectiveUnitTerrainAdoptions, playstationEffectiveUnitTerrainAdoptions, unitTScoreParametersSet);
            if (snesUnit.Weapons != null)
            {
                RstAppendUnitWeapons(stringBuilder, snesRom, playstationRom, snesUnit, playstationUnit, snesPreferredPilot, playstationPreferredPilot,
                    snesEffectiveUnitTerrainAdoptions, playstationEffectiveUnitTerrainAdoptions);
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format(".. _srw4_unit_{0}_commentBegin:", unitLabel));
            stringBuilder.AppendLine(RstHelper.GetComments(comments, string.Format("_srw4_unit_{0}", unitLabel)));
            stringBuilder.AppendLine(string.Format(".. _srw4_unit_{0}_commentEnd:", unitLabel));
            stringBuilder.AppendLine();
        }

        private static void RstAppendUnitWeapons(StringBuilder stringBuilder, Rom snesRom, Rom playstationRom, Unit snesUnit, Unit playstationUnit, Pilot? snesPreferedPilot, Pilot? playstationPreferredPilot, TerrainAdaptionSet effectiveSnesUnitTerrainAdoptions, TerrainAdaptionSet effectivePlayStationUnitTerrainAdoptions)
        {
            if (snesUnit.Weapons == null)
                throw new ArgumentNullException(nameof(snesUnit));

            if (playstationUnit.Weapons == null)
                throw new ArgumentNullException(nameof(playstationUnit));

            stringBuilder.Append(Resource.RstUnitGridHeader);
            stringBuilder.AppendLine();
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, "名字", 3));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "攻击"));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "射程"));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "命中"));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "暴击"));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, "地形空陆海宇", 3));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "残弹/EN"));
            stringBuilder.Append(string.Format(Resource.RstUnitGridColumnWithText, "条件"));
            stringBuilder.Append(Resource.RstUnitGridColumnBreak);

            var snesUnitWeapons = snesUnit.Weapons.OrderBy(w => w.Weapon == null ? 0 : w.Weapon.Damage).ToList();
            var playStationUnitWeapons = playstationUnit.Weapons.OrderBy(w => w.Weapon == null ? 0 : w.Weapon.Damage).ToList();
            var snesUnitWeaponDictionary = new Dictionary<int, UnitWeapon>();
            var playstationUnitWeaponDictionary = new Dictionary<int, UnitWeapon>();

            foreach (var snesUnitWeapon in snesUnitWeapons)
            {
                snesUnitWeaponDictionary.Add(snesUnitWeapon.WeaponIndex, snesUnitWeapon);
            }
            foreach (var playStationUnitWeapon in playStationUnitWeapons)
            {
                playstationUnitWeaponDictionary.Add(playStationUnitWeapon.WeaponIndex, playStationUnitWeapon);
            }

            foreach (var unitWeapon in snesUnitWeapons)
            {
                var weaponId = unitWeapon.WeaponIndex;
                var snesWeapon = unitWeapon.Weapon;
                if (snesWeapon == null) throw new ArgumentNullException(nameof(snesWeapon));

                var snesWeaponName = snesWeapon.GetNameWithAttributes();

                Weapon playstationWeapon = snesWeapon;
                string playstationWeaponName = snesWeaponName;
                if (playstationUnitWeaponDictionary.ContainsKey(weaponId))
                {
                    var playstationUnitWeaponInDictionary = playstationUnitWeaponDictionary[weaponId];
                    if (playstationUnitWeaponInDictionary.Weapon != null)
                    {
                        playstationWeapon = playstationUnitWeaponInDictionary.Weapon;
                        if (playstationWeapon != null)
                            playstationWeaponName = playstationWeapon.GetNameWithAttributes();
                    }
                }
                else
                {
                    snesWeaponName = string.Format("{0} (仅Snes)", snesWeaponName);
                    playstationWeapon = snesWeapon;
                    playstationWeaponName = snesWeaponName;
                }
                if (playstationWeapon == null) throw new ArgumentNullException(nameof(playstationWeapon));
                RstAppendUnitWeapon(stringBuilder, snesUnit, playstationUnit, effectiveSnesUnitTerrainAdoptions, effectivePlayStationUnitTerrainAdoptions, snesWeapon, playstationWeapon, snesWeaponName, playstationWeaponName);

                playstationUnitWeaponDictionary.Remove(weaponId);
            }
            var playStationOnlyWeaponIndicess = playstationUnitWeaponDictionary.Keys.ToList();
            foreach (var playStationOnlyWeaponIndex in playStationOnlyWeaponIndicess)
            {
                var playStationUnitWeapon = playstationUnitWeaponDictionary[playStationOnlyWeaponIndex];
                var weapon = playStationUnitWeapon.Weapon;
                if (weapon == null) throw new ArgumentNullException(nameof(playStationUnitWeapon));
                var weaponName = string.Format("{0} (仅PlayStation)", weapon.GetNameWithAttributes());
                RstAppendUnitWeapon(stringBuilder, snesUnit, playstationUnit, effectiveSnesUnitTerrainAdoptions, effectivePlayStationUnitTerrainAdoptions, weapon, weapon, weaponName, weaponName);
            }

        }

        private static void RstAppendUnitWeapon(StringBuilder stringBuilder, Unit snesUnit, Unit playstationUnit, TerrainAdaptionSet effectiveSnesUnitTerrainAdoptions,
            TerrainAdaptionSet effectivePlayStationUnitTerrainAdoptions, Weapon snesWeapon, Weapon playstationWeapon, string snesWeaponName, string playStationWeaponName)
        {
            if (string.Compare(snesWeaponName, playStationWeaponName) == 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithTextAndSpan, snesWeaponName, 3);
            }
            else
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithTextAndSpan, string.Empty, 3);
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(string.Format("        | {0} (Snes)", snesWeaponName));
                stringBuilder.AppendLine(string.Format("        | {0} (PlayStation)", playStationWeaponName));
                stringBuilder.AppendLine();
            }

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, 
                Rom.FormatValue(snesWeapon.Damage, playstationWeapon.Damage));

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                Rom.FormatValue(snesWeapon.Damage, playstationWeapon.Damage));

            StringBuilder rangeBuilder = new StringBuilder();
            if (snesWeapon.MinRange == playstationWeapon.MinRange)
            {
                rangeBuilder.Append(snesWeapon.MinRange);
            }
            else
            {
                rangeBuilder.AppendFormat("{0}({1})", snesWeapon.MinRange, playstationWeapon.MinRange);
            }
            if (snesWeapon.MaxRange == playstationWeapon.MaxRange)
            {
                if (snesWeapon.MaxRange > 1)
                {
                    rangeBuilder.AppendFormat("~{0}", snesWeapon.MaxRange);
                }
            }
            else
            {
                rangeBuilder.AppendFormat("~{0}({1})", snesWeapon.MaxRange, playstationWeapon.MaxRange);
            }
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, rangeBuilder.ToString());

            if (snesWeapon.AccuracyBonus == 0 &&
                playstationWeapon.AccuracyBonus == 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
            }
            else
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                Rom.FormatValue(snesWeapon.AccuracyBonus.ToString("+##;-##;0"), playstationWeapon.AccuracyBonus.ToString("+##;-##;0")));
            }

            if (snesWeapon.CriticalHitRateBonus == 0 &&
                playstationWeapon.AccuracyBonus == 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
            }
            else
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                Rom.FormatValue(snesWeapon.CriticalHitRateBonus.ToString("+#;-#;0"), playstationWeapon.CriticalHitRateBonus.ToString("+#;-#;0")));
            }
            if (snesWeapon.TerrainAdaptionSet == null)
            {
                throw new ArgumentNullException("snesWeapon.TerrainAdaptionSet");
            }
            if (playstationWeapon.TerrainAdaptionSet == null)
            {
                throw new ArgumentNullException("snesWeapon.TerrainAdaptionSet");
            }

            var snesWeaponEffectiveTerrainAdaptionSet = TerrainAdaptionSet.GetEffectiveTerrainWeaponAdaptions
                (effectiveSnesUnitTerrainAdoptions, snesWeapon.TerrainAdaptionSet, snesWeapon.IsMelee,
                snesUnit.MoveType);

            var playStationWeaponEffectiveTerrainAdaptionSet = TerrainAdaptionSet.GetEffectiveTerrainWeaponAdaptions
                (effectivePlayStationUnitTerrainAdoptions, playstationWeapon == null ? snesWeapon.TerrainAdaptionSet : playstationWeapon.TerrainAdaptionSet, playstationWeapon == null ? snesWeapon.IsMelee : playstationWeapon.IsMelee, playstationUnit.MoveType);
            var formattedEffectiveTerrainWeaponAdaption = TerrainAdaptionSet.FormatEffectiveTerrainWeaponAdaption(snesWeapon.TerrainAdaptionSet, playstationWeapon == null ? snesWeapon.TerrainAdaptionSet : playstationWeapon.TerrainAdaptionSet, snesWeaponEffectiveTerrainAdaptionSet, playStationWeaponEffectiveTerrainAdaptionSet);

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithTextAndSpan, string.Join(string.Empty, formattedEffectiveTerrainWeaponAdaption), 3);
            WriteUnitConsumption(stringBuilder, snesWeapon, playstationWeapon);
            List<String> requirements = new List<string>();

            if (snesWeapon.RequiredWill > 0 || playstationWeapon.RequiredWill > 0)
            {
                if (snesWeapon.RequiredWill == playstationWeapon.RequiredWill)
                {
                    requirements.Add(string.Format("{0}气力", snesWeapon.RequiredWill));
                }
                else
                {
                    requirements.Add(string.Format("{0} 气力 ({1})", snesWeapon.RequiredWill, playstationWeapon.RequiredWill));
                }
            }

            if (snesWeapon.RequiredSkill > 0 || playstationWeapon.RequiredSkill > 0)
            {
                if (snesWeapon.RequiredSkill == playstationWeapon.RequiredSkill)
                {
                    requirements.Add(PilotSpiritCommandsOrSkill.Format(
                        0, snesWeapon.RequiredSkill, 0, false));
                }
                else
                {
                    requirements.Add(string.Format("{0} ({1}", PilotSpiritCommandsOrSkill.Format(
                        0, snesWeapon.RequiredSkill, 0, false)
                        , PilotSpiritCommandsOrSkill.Format(
                        0, playstationWeapon.RequiredSkill, 0, false)));
                }
            }
            if (requirements.Count > 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Join("", requirements));
            }
            else
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
            }
            stringBuilder.Append(Resource.RstUnitGridColumnBreak);
        }

        private static void WriteUnitConsumption(StringBuilder stringBuilder, Weapon snesWeapon, Weapon playstationWeapon)
        {
            if (snesWeapon.MaxAmmo > 0 || playstationWeapon.MaxAmmo > 0)
            {
                if (snesWeapon.MaxAmmo == playstationWeapon.MaxAmmo)
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                        string.Format("残弹 {0}", snesWeapon.MaxAmmo));
                }
                else
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                        string.Format("残弹 {0} ({1})", snesWeapon.MaxAmmo, playstationWeapon.MaxAmmo));
                }
            }
            else if (snesWeapon.EnergyCost > 0 || playstationWeapon.EnergyCost > 0)
            {
                if (snesWeapon.EnergyCost == playstationWeapon.EnergyCost)
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                        string.Format("EN {0}", snesWeapon.EnergyCost));
                }
                else
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                        string.Format("EN {0} ({1})", snesWeapon.EnergyCost, playstationWeapon.EnergyCost));
                }
            }
            else
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
        }

        private static void RstAppendUnitMetaData(StringBuilder stringBuilder, UnitMetaData unitMetaData, int unitId, Rom snesRom, Rom playstationRom, Unit snesUnit, Unit playstationUnit, Pilot? snesPreferredPilot, Pilot? playstationPreferredPilot, TerrainAdaptionSet effectiveSnesUnitTerrainAdoptions, TerrainAdaptionSet effectivePlayStationUnitTerrainAdoptions, UnitTScoreParametersSet unitTScoreParametersSet)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Resource.RstUnitGridHeader);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithTextAndSpan,string.Empty,3);
            switch (unitId)
            {
                case 0xf8:
                case 0x101:
                case 0x106:
                case 0x10d:
                    break;
                default:

                    stringBuilder.AppendLine();
                    stringBuilder.AppendFormat("        .. image:: ../units/images/portrait/srw4_units_portrait_{0:X2}.png\r\n"
                , unitId);
                    break;
            }
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithTextAndSpan, string.Empty, 9);
            stringBuilder.AppendLine();

            stringBuilder.AppendLine(Resource.RstUnitGridHeader2);
            if (snesUnit.FirstAppearance > 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("登场/加入:第{0}话", snesUnit.FirstAppearance));
            }

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("编码 {0:X2}", snesUnit.Id));
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("地址 {0:X} ({1:X})", snesUnit.BaseOffset, playstationUnit.BaseOffset));
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("武器首地址 {0:X} ({1:X})", snesUnit.BaseOffset + 0x20, playstationUnit.BaseOffset + 0x20));
            
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("移动类型 {0}", Rom.FormatValue
                (TerrainAdaptionSet.FormatMovementType(snesUnit.MoveType), TerrainAdaptionSet.FormatMovementType(playstationUnit.MoveType))));
            stringBuilder.AppendLine();

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("大小 {0}", snesUnit.FormatUnitSize(snesUnit.UnitSize)));

            if (snesUnit.TerrainAdaptionSet != null && playstationUnit.TerrainAdaptionSet != null)
            {
                var formattedEffectiveTerrainWeaponAdaption = TerrainAdaptionSet.FormatEffectiveTerrainWeaponAdaption(snesUnit.TerrainAdaptionSet
                    , playstationUnit.TerrainAdaptionSet, effectiveSnesUnitTerrainAdoptions, effectivePlayStationUnitTerrainAdoptions);

                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("地形适应 {0}", string.Join(string.Empty, formattedEffectiveTerrainWeaponAdaption)));
                stringBuilder.AppendLine();
            }
            if (snesPreferredPilot != null)
            {
                var preferedPilotName = snesPreferredPilot.Name;
                var preferedPilotLabel = string.Format("srw4_pilot_{0}", snesPreferredPilot.GetLabel());

                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Format("地形参照  \\ :ref:`{0} <{1}>`\\ ", snesPreferredPilot.Name, preferedPilotLabel));
                stringBuilder.AppendLine();

                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,Pilot.RstGetPilotIcon(snesPreferredPilot.Id));  
            }
            List<string> unitSkills = GetUnitSkillDescriptions(snesUnit, playstationUnit);
            if (unitSkills.Count > 0)
            {
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnAuto2WithText,
                    string.Join(", ", unitSkills));
                stringBuilder.AppendLine();
            }
            stringBuilder.AppendLine(Resource.RstUnitGridHeader);

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "属性");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "己偏差值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "全偏差值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "属性");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "己偏差值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "全偏差值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "属性");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "己偏差值");
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, "全偏差值");
            stringBuilder.Append(Resource.RstUnitGridColumnBreak);           

            WriteAttributeWithTScore(stringBuilder,"HP", unitTScoreParametersSet,
                UnitTScoreParameterIndex.HP,
                snesUnit.HP,
                playstationUnit.HP
                );

            WriteAttributeWithTScore(stringBuilder, "EN", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Energy,
                snesUnit.Energy,
                playstationUnit.Energy
                );

            WriteAttributeWithTScore(stringBuilder, "装甲", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Armor,
                snesUnit.Armor ,
                playstationUnit.Armor
                );

            stringBuilder.Append(Resource.RstUnitGridColumnBreak);

            WriteAttributeWithTScore(stringBuilder, "运动性", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Mobility,
                snesUnit.Mobility ,
                playstationUnit.Mobility
                );

            WriteAttributeWithTScore(stringBuilder, "限界", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Limit,
                snesUnit.Limit,
                playstationUnit.Limit
                );

            WriteAttributeWithTScore(stringBuilder, "移动力", unitTScoreParametersSet,
                UnitTScoreParameterIndex.MoveRange,
                snesUnit.MoveRange,
                playstationUnit.MoveRange
                );

            stringBuilder.Append(Resource.RstUnitGridColumnBreak);

            WriteAttributeWithTScore(stringBuilder, "经验值", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Experience,
                snesUnit.Experience,
                playstationUnit.Experience
                );

            WriteAttributeWithTScore(stringBuilder, "价值", unitTScoreParametersSet,
                UnitTScoreParameterIndex.Gold,
                snesUnit.Gold,
                playstationUnit.Gold
                );

            WriteAttributeWithTScore(stringBuilder, "修理费", unitTScoreParametersSet,
                UnitTScoreParameterIndex.RepairCost,
                snesUnit.RepairCost,
                playstationUnit.RepairCost
                );
        }

        private static List<string> GetUnitSkillDescriptions(Unit snesUnit, Unit playstationUnit)
        {
            var result= new List<string>();
            if (snesUnit.HasAfterimage == playstationUnit.HasAfterimage)
            {
                if (snesUnit.HasAfterimage)
                    result.Add("分身");
            }
            else
            {
                if (snesUnit.HasAfterimage)
                    result.Add("分身 (仅Snes)");
                else
                    result.Add("分身 (仅PlayStation)");

            }
            //Debug.Assert(snesUnit.HasShield == playstationUnit.HasShield);
            if (snesUnit.HasShield)
            {
                result.Add("盾装備");
            }
            Debug.Assert(snesUnit.HasShield == playstationUnit.HasShield);
            //Debug.Assert(snesUnit.HasSword == playstationUnit.HasSword);
            if (snesUnit.HasSword || playstationUnit.HasSword)
            {
                result.Add("剣装備");
            }
            Debug.Assert(snesUnit.HasSword == playstationUnit.HasSword);
            if (snesUnit.HasEnergyRecovery)
            {
                result.Add("EN恢復");

            }
            switch (snesUnit.HPRecoveryType)
            {
                case 0x04:
                    result.Add("HP恢復(小)");
                    break;
                case 0x08:
                    result.Add("HP恢復(大)");
                    break;
            }
            Debug.Assert(snesUnit.HasEnergyRecovery == playstationUnit.HasEnergyRecovery);
            Debug.Assert(snesUnit.HPRecoveryType == playstationUnit.HPRecoveryType);
            if (snesUnit.RageAndDetonateImmune)
            {
                result.Add("激怒/自爆/てかげん無効");
            }
            Debug.Assert(snesUnit.RageAndDetonateImmune == playstationUnit.RageAndDetonateImmune);
            switch (snesUnit.BeamCoatType)
            {
                case 0x02:
                    result.Add("ビームコート");
                    break;
                case 0x04:
                    result.Add("Iフィールド");
                    break;
                case 0x06:
                    result.Add("オーラバリア");
                    break;
                case 0x08:
                    result.Add("ビームバリア");
                    break;
            }
            Debug.Assert(snesUnit.BeamCoatType == playstationUnit.BeamCoatType);
            return result;
        }

        private static void WriteAttributeWithTScore(StringBuilder stringBuilder, string dataType, UnitTScoreParametersSet unitTScoreParametersSet,
            UnitTScoreParameterIndex attributeIndex, int snesAttribute, int playStationAttribute)
        {
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, dataType);
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, Rom.FormatValue(
                attributeIndex== UnitTScoreParameterIndex.Armor?snesAttribute * 10 : snesAttribute
                , attributeIndex == UnitTScoreParameterIndex.Armor ? playStationAttribute * 10: playStationAttribute));


            var snesTScore = Math.Round(unitTScoreParametersSet.SnesOwned.GetTScoreParameter(attributeIndex).CalculateTScore(snesAttribute));
            var playStationTScore = Math.Round(unitTScoreParametersSet.PlayStationOwned.GetTScoreParameter(attributeIndex).CalculateTScore(playStationAttribute));

            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, Rom.FormatValue(snesTScore, playStationTScore));


            snesTScore = Math.Round(unitTScoreParametersSet.SnesEncountered.GetTScoreParameter(attributeIndex).CalculateTScore(snesAttribute));
            playStationTScore = Math.Round(unitTScoreParametersSet.PlayStationEncountered.GetTScoreParameter(attributeIndex).CalculateTScore(playStationAttribute));
            stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, Rom.FormatValue(snesTScore, playStationTScore));
        }

    }

}
