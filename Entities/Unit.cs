


using System;
using System.Diagnostics;
using System.Text;

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
        public byte TerrainAdaptionAir { get; set; }
        public byte TerrainAdaptionSea { get; set; }
        public byte TerrainAdaptionSpace { get; set; }
        public byte TerrainAdaptionLand { get; set; }
        public byte Armor { get; set; }
        public byte Mobility { get; set; }
        public byte Limit { get; set; }
        public short Energy { get; set; }
        public ushort HP { get; set; }
        public byte WeaponCount { get; set; }
        public byte AmmoWeaponCount { get; set; }
        public int BaseOffset { get; set; }
        public List<UnitWeapon>? Weapons { get; set; }
        public ushort FirstWeaponIndex { get; private set; }

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
                    unitWeapon.Name = fixWeapon.Name;
                    if (!fixWeapon.HasAssignedOwner)
                    {
                        fixWeapon.HasAssignedOwner = true;
                        fixWeapon.FirstOwner = unit.Name;
                    }
                    unitWeapon.FirstOwner = fixWeapon.FirstOwner;
                    unitWeapon.Damage = fixWeapon.Damage;
                    unitWeapon.Range = fixWeapon.MaxRange;
                    unitWeapon.Ammo = fixWeapon.MaxAmmo;
                    unitWeapon.En = fixWeapon.EnergyCost;
                    unitWeapon.IsMap = fixWeapon.IsMap;
                }
            }
        }

        private static Unit ParseUnit(byte[] playStationUnitData, int baseOffset, int unitIndex)
        {
            Unit unit = new Unit();
            unit.BaseOffset = baseOffset;
            int offset = baseOffset;

            byte iconId1 = playStationUnitData[offset++];
            byte fanchiseId = playStationUnitData[offset++];

            var iconId2 = fanchiseId & 0x01;

            unit.IconId = (ushort)(iconId2 * 256 + iconId1);

            unit.Id = unitIndex;
            unit.FranchiseId = (byte)(fanchiseId & 0xFE);
            unit.PortraitId = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;

            unit.FixedSeatPilotId = playStationUnitData[offset++];
            unit.TransferFranchiseId = playStationUnitData[offset++];
            var sizeAndBGM = playStationUnitData[offset++];

            unit.UnitSize = (byte)(sizeAndBGM & 0x60);
            unit.UnitSizeBit = (byte)(sizeAndBGM & 0x10);
            unit.Discardable = (sizeAndBGM & 0x80) == 0;
            unit.BackgroundMusic = (byte)(sizeAndBGM & 0x0F);
            unit.TransformOrCombineType = playStationUnitData[offset++];

            var unitSpecialSkill1 = playStationUnitData[offset++];
            var unitSpecialSkill2 = playStationUnitData[offset++];
            unit.IsAggressive = (unitSpecialSkill1 & 0x1) != 0;
            unit.HasEnergyRecovery = (unitSpecialSkill1 & 0x02) != 0;
            unit.HPRecoveryType = (byte)(unitSpecialSkill1 & 0x0C);
            unit.HasSword = (unitSpecialSkill1 & 0x40) != 0;
            unit.RageAndDetonateImmune = (unitSpecialSkill1 & 0x80) != 0;

            unit.BeamCoatType = unitSpecialSkill2 & 0x0E;
            unit.HasAfterimage = (unitSpecialSkill2 & 0x10) != 0;
            unit.HasShield = (unitSpecialSkill2 & 0x20) != 0;
            unit.UnknownUnitSpecialSkill2 = (byte)(unitSpecialSkill1 & (~0x3F));
            unit.Team = playStationUnitData[offset++];
            offset += 4;
            unit.Experience = playStationUnitData[offset++];
            unit.Gold = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;
            unit.RepairCost = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;
            unit.MoveRange = playStationUnitData[offset++];
            Debug.Assert(unit.MoveRange < 16);
            unit.MoveType = playStationUnitData[offset++];
            Debug.Assert(unit.MoveType < 0xD);
            var TerrainAdaption = playStationUnitData[offset++];
            unit.TerrainAdaptionAir = (byte)((TerrainAdaption & 0xF0) / 16);
            Debug.Assert(unit.TerrainAdaptionAir < 5);
            unit.TerrainAdaptionSea = (byte)(TerrainAdaption & 0x0F);
            Debug.Assert(unit.TerrainAdaptionSea < 5);
            TerrainAdaption = playStationUnitData[offset++];
            unit.TerrainAdaptionSpace = (byte)((TerrainAdaption & 0xF0) / 16);
            Debug.Assert(unit.TerrainAdaptionSpace < 5);
            unit.TerrainAdaptionLand = (byte)(TerrainAdaption & 0x0F);
            Debug.Assert(unit.TerrainAdaptionLand < 5);

            unit.Armor = playStationUnitData[offset++];
            unit.Mobility = playStationUnitData[offset++];
            unit.Limit = playStationUnitData[offset++];

            unit.Energy = playStationUnitData[offset++];
            unit.HP = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;

            unit.WeaponCount = playStationUnitData[offset++];
            Debug.Assert(unit.WeaponCount < 32);
            unit.AmmoWeaponCount = playStationUnitData[offset++];
            Debug.Assert(unit.AmmoWeaponCount < 32);
            Debug.Assert(unit.AmmoWeaponCount <= unit.WeaponCount);
            Debug.Assert(unit.WeaponCount < 0x20);

            unit.FirstWeaponIndex = (ushort)(BitConverter.ToUInt16(playStationUnitData, offset) & 0x03FF);

            //the last unit has no weapon 
            if (unit.WeaponCount > 0)
            {
                unit.Weapons = new List<UnitWeapon>();
                var weaponIndex = (ushort)(BitConverter.ToUInt16(playStationUnitData, offset));
                while (weaponIndex > 0)
                {
                    UnitWeapon unitWeapon = new UnitWeapon();
                    unitWeapon.BaseOffset = offset;
                    unitWeapon.WeaponIndex = (ushort)(weaponIndex & 0x03FF);
                    unitWeapon.AmmoSlot = (byte)((playStationUnitData[offset + 1] & 0xFC) / 4);
                    //Debug.Assert(unitWeapon.AmmoSlot <= unit.AmmoWeaponCount);
                    offset += 2;
                    var weaponOrderAndType = playStationUnitData[offset++];
                    unitWeapon.IsConditional = (weaponOrderAndType & 0x80) > 0;
                    unitWeapon.Order = (byte)(weaponOrderAndType & 0x1F);
                    Debug.Assert(unitWeapon.Order <= unit.WeaponCount);
                    if (unitWeapon.IsConditional)
                    {
                        unitWeapon.AvailableAtStage = playStationUnitData[offset++];
                    }
                    unit.Weapons.Add(unitWeapon);
                    weaponIndex = (ushort)((playStationUnitData[offset + 1] * 256 + playStationUnitData[offset]));
                }
            }
            else
            {
                var weaponIndex = (ushort)(playStationUnitData[offset + 1] * 256 + playStationUnitData[offset]);
                while (weaponIndex > 0)
                {
                    weaponIndex = (ushort)(playStationUnitData[offset + 1] * 256 + playStationUnitData[offset]);
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
            stringBuilder.AppendFormat(", Name: {0}", Name);
            stringBuilder.AppendFormat(", BaseOffset: {0:X}", BaseOffset);
            stringBuilder.AppendFormat(", TransformOrCombineTypeAddress: {0:X}", BaseOffset + 7);
            stringBuilder.AppendFormat(", WeaponCountAddress: {0:X}", BaseOffset + 0x1E);
            stringBuilder.AppendFormat(", AmmoWeaponCountAddress: {0:X}", BaseOffset + 0x1F);
            stringBuilder.AppendFormat(", TerrainAdaption: {0:X}", BaseOffset + 0x16);
            stringBuilder.AppendFormat(", FirstWeaponAddress: {0:X}", BaseOffset + 0x20);

            stringBuilder.AppendFormat(", Affiliation: {0:X}", Affiliation);
            stringBuilder.AppendFormat(", FranchiseName: {0:X}", FranchiseName);
            stringBuilder.AppendFormat(", IconId: {0:X}", IconId);
            stringBuilder.AppendFormat(", FranchiseId:{0:X} ({1})", FranchiseId, Franchise.FormatFranchise(FranchiseId));
            stringBuilder.AppendFormat("\r\n, PortraitId: {0:X}", PortraitId);
            stringBuilder.AppendFormat(", FixedSeatPilotId: {0:X}", FixedSeatPilotId);
            stringBuilder.AppendFormat(", TransferFranchiseId: {0:X}", TransferFranchiseId);
            stringBuilder.AppendFormat(", Discardable: {0}", Discardable);
            stringBuilder.AppendFormat(", UnitSize:{0:X} ({1})", UnitSize, FormatUnitSize(UnitSize));
            if (UnitSizeBit != 0)
            {
                //stringBuilder.AppendFormat(", Unknown UnitSize Bit:{0:X}", UnitSizeBit);
            }
            stringBuilder.AppendFormat(", BackgroundMusic: {0:X}", BackgroundMusic);
            stringBuilder.AppendFormat(", TransformOrCombineType: {0:X}", TransformOrCombineType);
            stringBuilder.Append("\r\n");
            if (HasSword)
            {
                stringBuilder.Append("剣装備");
            }
            if (HasEnergyRecovery)
            {
                stringBuilder.Append(", EN恢復");
            }
            switch (HPRecoveryType)
            {
                case 0x04:
                    stringBuilder.Append(", HP恢復(小)"); break;
                case 0x08:
                    stringBuilder.Append(", HP恢復(大)"); break;
            }

            if (RageAndDetonateImmune)
            {
                stringBuilder.Append(",激怒/自爆/てかげん無効");
            }

            switch (BeamCoatType)
            {
                case 0x02: stringBuilder.Append(", ビームコート"); break;
                case 0x04: stringBuilder.Append(", Iフィールド"); break;
                case 0x06: stringBuilder.Append(", オーラバリア"); break;
                case 0x08: stringBuilder.Append(", ビームバリア"); break;
            }
            if (HasAfterimage)
            {
                stringBuilder.Append(", 分身");
            }
            if (HasShield)
            {
                stringBuilder.Append(", 盾装備");
            }
            if (!IsAggressive)
            {
                stringBuilder.Append(", 不攻击");
            }
            if (UnknownUnitSpecialSkill2 != 0)
            {
                //stringBuilder.AppendFormat(", Unknown unitSpecialSkill2 {0:X} ", FormatHex(UnknownUnitSpecialSkill2));
            }
            stringBuilder.AppendFormat(", Team: {0:X}", Team);
            stringBuilder.AppendFormat(", Experience: {0:X}", Experience);
            stringBuilder.AppendFormat(", Gold: {0:X}", Gold);
            stringBuilder.AppendFormat(", RepairCost: {0:X}", RepairCost);

            stringBuilder.AppendFormat(", MoveRange: {0:X}", MoveRange);
            stringBuilder.AppendFormat(", MoveType: {0:X}", MoveType);

            stringBuilder.AppendFormat(", TerrainAdaption: {0}", TerrainAdaptionHelper.FormatTerrainAdaption(
                new byte[] {
                TerrainAdaptionAir,TerrainAdaptionLand, TerrainAdaptionSea, TerrainAdaptionSpace}));

            stringBuilder.AppendFormat("\r\n, Armor: {0}", Armor * 10);
            stringBuilder.AppendFormat(", Mobility: {0}", Mobility);
            stringBuilder.AppendFormat(", Limit: {0}", Limit);

            stringBuilder.AppendFormat(", Energy: {0}", Energy);
            stringBuilder.AppendFormat(", HP: {0}", HP);

            stringBuilder.AppendFormat(", WeaponCount: {0:X}", WeaponCount);
            stringBuilder.AppendFormat(", AmmoWeaponCount: {0:X}", AmmoWeaponCount);

            if (WeaponCount > 0 && Weapons != null)
            {
                stringBuilder.Append("\r\nWeapons:\r\n");
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
            var bestNearWeapon = weapons.Where(w => w.Range == 1).OrderByDescending(w => w.Damage).FirstOrDefault();
            if (bestNearWeapon == null) return 0;
            return bestNearWeapon.Damage;
        }
        public int GetFarDamage()
        {
            var weapons = this.Weapons;
            if (weapons == null || weapons.Count == 0)
            {
                return 0;
            }
            var bestFarWeapon = weapons.Where(w => w.Range > 1 && w.IsMap == false && w.Damage > 0).OrderByDescending(w => w.Damage).FirstOrDefault();
            if (bestFarWeapon == null) return 0;
            return bestFarWeapon.Damage;
        }
        string GetMaxRangeExceptMap(List<UnitWeapon>? weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return "🚫";
            }
            var bestFarWeapon = weapons.Where(w => w.Range > 1 && w.IsMap == false && w.Damage > 0).OrderByDescending(w => w.Range).FirstOrDefault();
            if (bestFarWeapon == null) return "🚫";
            return bestFarWeapon.Range.ToString();
        }
        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.AppendLine(string.Format("     - {0}", this.Affiliation));
            int unitIcon = GetUnitIcon(this.Id);
            if(unitIcon!=0)
                row.AppendLine(string.Format("     - .. image:: ../units/images/icon/srw4_units_icon_{0:X2}_B.png", unitIcon));
            else
                row.AppendLine("     - ");
            row.AppendLine(string.Format("     - {0}", this.Name));
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
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatMovementType(this.MoveType)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionHelper.FormatTerrainAdaption(this.TerrainAdaptionSpace)));
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

        internal static void RstAppendUnit(StringBuilder stringBuilder, int unitId, List<UnitMetaData> units, string unitComment, List<Unit> unitsSnes, List<Unit>? unitsPlayStation, List<Pilot>? pilotsSnes, List<Pilot>? pilotsPlayStation, List<Weapon>? weaponSnes, List<Weapon>? weaponPlayStation)
        {
            var unitMetaData = units.FirstOrDefault(u => u.Id == unitId);
            if (unitMetaData == null)
            {
                return;
            }
            var snesUnit = unitsSnes.FirstOrDefault(u => u.Id == unitId);
            var playstationUnit = unitsPlayStation?.FirstOrDefault(u => u.Id == unitId);

            int preferedPilotId = snesUnit.FixedSeatPilotId;
            if (preferedPilotId <= 0&& unitMetaData.PreferredPilotId.HasValue)
            {
                preferedPilotId = unitMetaData.PreferredPilotId.Value;
            }
            List<byte> snesUnitTerrainAdoptions = new List<byte>();
            List<byte> playStationUnitTerrainAdoptions = new List<byte>();
            List<byte> snesUnitTerrainAdoptionsWithPreferdPilot = new List<byte>();
            List<byte> playStationUnitTerrainAdoptionsWithPreferdPilot = new List<byte>();
            RstHelper.AppendHeader(stringBuilder, unitMetaData.Name, '^');
            RstAppendUnitMetaData(stringBuilder, unitId, pilotsSnes, pilotsPlayStation, snesUnit, playstationUnit, preferedPilotId
                , snesUnitTerrainAdoptions, playStationUnitTerrainAdoptions, snesUnitTerrainAdoptionsWithPreferdPilot, playStationUnitTerrainAdoptionsWithPreferdPilot);
            RstAppendUnitWeapons(stringBuilder, weaponSnes, weaponPlayStation, snesUnit, snesUnitTerrainAdoptions, playStationUnitTerrainAdoptions, snesUnitTerrainAdoptionsWithPreferdPilot, playStationUnitTerrainAdoptionsWithPreferdPilot);

            var unitLabel = RstHelper.GetLabelName(unitMetaData.EnglishName);
            Debug.Assert(!string.IsNullOrEmpty(unitLabel));
            stringBuilder.AppendLine(string.Format(".. _srw4_units_{0}_commentBegin", unitLabel));
            stringBuilder.AppendLine(unitComment);
            stringBuilder.AppendLine(string.Format(".. _srw4_units_{0}_commentEnd:", unitLabel));
            stringBuilder.AppendLine();

        }

        private static void RstAppendUnitWeapons(StringBuilder stringBuilder, List<Weapon>? weaponSnes, List<Weapon>? weaponPlayStation, Unit snesUnit, List<byte> snesUnitTerrainAdoptions, List<byte> playStationUnitTerrainAdoptions, List<byte> snesUnitTerrainAdoptionsWithPreferdPilot, List<byte> playStationUnitTerrainAdoptionsWithPreferdPilot)
        {
            stringBuilder.Append(Resource.RstUnitGridHeader);
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, "名字", 3));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "攻击"));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "射程"));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "命中"));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "暴击"));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, "地形空陆海宇", 3));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "残弹/EN"));
            stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithText, "条件"));
            stringBuilder.AppendLine(Resource.RstUnitGridColumnBreak);
            var snesUnitWeapons = snesUnit.Weapons.OrderBy(w => w.Damage).ToList();
            foreach (var unitWeapon in snesUnitWeapons)
            {
                var weaponId = unitWeapon.WeaponIndex;
                var snesWeapon = weaponSnes.FirstOrDefault(w => w.Id == weaponId);
                var playStationWeapon = weaponPlayStation?.FirstOrDefault(w => w.Id == weaponId);
                if (weaponId == 0x275)
                    playStationWeapon = null;//in ps the weapon is not available
                if (playStationWeapon != null)
                {
                    stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, snesWeapon.Name, 3));
                }
                else
                {
                    stringBuilder.AppendLine(string.Format(Resource.RstUnitGridColumnWithTextAndSpan, snesWeapon.Name + " (仅Snes)", 3));
                }
                if (playStationWeapon == null || snesWeapon.Damage == playStationWeapon.Damage)
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, snesWeapon.Damage);
                }
                else
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText,
                        string.Format("{0}({1})", snesWeapon.Damage, playStationWeapon.Damage));
                }
                stringBuilder.AppendLine();
                StringBuilder rangeBuilder = new StringBuilder();
                if (playStationWeapon == null || snesWeapon.MinRange == playStationWeapon.MinRange)
                {
                    rangeBuilder.Append(snesWeapon.MinRange);
                }
                else
                {
                    rangeBuilder.AppendFormat("{0}({1})", snesWeapon.MinRange, playStationWeapon.MinRange);
                }
                if (playStationWeapon == null || snesWeapon.MaxRange == playStationWeapon.MaxRange)
                {
                    if (snesWeapon.MaxRange > 1)
                    {
                        rangeBuilder.AppendFormat("~{0}", snesWeapon.MaxRange);
                    }
                }
                else
                {
                    if (snesWeapon.MaxRange > 1)
                    {
                        rangeBuilder.AppendFormat("~{0}", snesWeapon.MaxRange);
                    }
                    else
                    {
                        rangeBuilder.AppendFormat("~{0}({1})", snesWeapon.MaxRange, playStationWeapon.MaxRange);
                    }
                }
                stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, rangeBuilder.ToString());
                stringBuilder.AppendLine();


                stringBuilder.AppendLine();
                if (playStationWeapon == null || snesWeapon.AccuracyBonus == 0)
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
                }
                else
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, snesWeapon.AccuracyBonus);
                }
                stringBuilder.AppendLine();

                if (playStationWeapon == null || snesWeapon.CriticalHitRateBonus == 0)
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, string.Empty);
                }
                else
                {
                    stringBuilder.AppendFormat(Resource.RstUnitGridColumnWithText, snesWeapon.CriticalHitRateBonus);
                }
                stringBuilder.AppendLine();
                //List<byte> snesWeaponTerrainAdaptions = GetWeaponTerrainAdaptions(snesWeapon,)


            }
        }

        private static void RstAppendUnitMetaData(StringBuilder stringBuilder, int unitId, List<Pilot>? pilotsSnes, List<Pilot>? pilotsPlayStation, Unit snesUnit, Unit? playstationUnit, int preferedPilotId, List<byte> snesUnitTerrainAdoptions, List<byte> playStationUnitTerrainAdoptions, List<byte> snesUnitTerrainAdoptionsWithPreferedPilot, List<byte> playStationUnitTerrainAdoptionsWithPreferedPilot)
        {
            stringBuilder.AppendLine();
            stringBuilder.AppendLine(Resource.RstUnitGridHeader);
            stringBuilder.AppendLine();
            switch (unitId)
            {
                case 0x51:
                case 0xf8:
                case 0x101:
                case 0x106:
                case 0x10d:
                    break;
                default:
                    stringBuilder.AppendFormat("        .. image:: ../units/images/portrait/srw4_units_portrait_{0}.png"
                , unitId);
                    break;
            }
            stringBuilder.AppendLine();
            stringBuilder.Append(Resource.RstUnitGridHeader);
            stringBuilder.AppendLine();
            stringBuilder.Append(Resource.RstUnitGridColumnAuto);
            stringBuilder.AppendLine();
            if (snesUnit.HP == playstationUnit.HP)
                stringBuilder.AppendFormat("        | HP {0}\r\n", snesUnit.HP);
            else
                stringBuilder.AppendFormat("        | HP {0} ({1})\r\n", snesUnit.HP, playstationUnit.HP);
            if (snesUnit.Energy == playstationUnit.Energy)
                stringBuilder.AppendFormat("        | EN {0}\r\n", snesUnit.Energy);
            else
                stringBuilder.AppendFormat("        | EN {0} ({1})\r\n", snesUnit.Energy, playstationUnit.Energy);
            if (snesUnit.Armor == playstationUnit.Armor)
            {
                stringBuilder.AppendFormat("        | Armor {0}\r\n", snesUnit.Armor * 10);
            }
            else
            {
                stringBuilder.AppendFormat("        | Armor {0} ({1})\r\n", snesUnit.Armor * 10, playstationUnit.Armor * 10);
            }
            if (snesUnit.Mobility == playstationUnit.Mobility)
            {
                stringBuilder.AppendFormat("        | Mobility {0}\r\n", snesUnit.Mobility);
            }
            else
            {
                stringBuilder.AppendFormat("        | Mobility {0} ({1})\r\n", snesUnit.Mobility, playstationUnit.Mobility);
            }
            if (snesUnit.Limit == playstationUnit.Limit)
            {
                stringBuilder.AppendFormat("        | Limit {0}\r\n", snesUnit.Limit);
            }
            else
            {
                stringBuilder.AppendFormat("        | Limit {0} ({1})\r\n", snesUnit.Limit, playstationUnit.Limit);
            }

            stringBuilder.Append(Resource.RstUnitGridColumnAuto);
            stringBuilder.AppendLine();
            stringBuilder.AppendFormat("        | 编码 {0}\r\n", snesUnit.Id);
            if (snesUnit.MoveType == playstationUnit.MoveType)
            {
                stringBuilder.AppendFormat("        | 类型 {0}\r\n", TerrainAdaptionHelper.FormatMovementType(snesUnit.MoveType));

            }
            else
            {
                stringBuilder.AppendFormat("        | 类型 {0} ({1})\r\n", TerrainAdaptionHelper.FormatMovementType(snesUnit.MoveType), TerrainAdaptionHelper.FormatMovementType(playstationUnit.MoveType));
            }
            if (snesUnit.MoveRange == playstationUnit.MoveRange)
            {
                stringBuilder.AppendFormat("        | 移动力 {0}\r\n", snesUnit.MoveRange);
            }
            else
            {
                stringBuilder.AppendFormat("        | 移动力 {0} ({1})\r\n", snesUnit.MoveRange, playstationUnit.MoveRange);
            }
            stringBuilder.AppendFormat("        | 大小 {0}\r\n", snesUnit.FormatUnitSize(snesUnit.UnitSize));

            stringBuilder.Append(Resource.RstUnitGridColumnAuto);
            stringBuilder.AppendLine();

            var preferedPilotSnes = pilotsSnes?.FirstOrDefault(p => p.Id == preferedPilotId);
            var preferedPilotPlayStation = pilotsPlayStation?.FirstOrDefault(p => p.Id == preferedPilotId);
            string[] terrainAdaptionNames = new string[] { "空", "陆", "海", "宇" };
            snesUnitTerrainAdoptions.AddRange(new byte[] {
                    snesUnit.TerrainAdaptionAir,
                    snesUnit.TerrainAdaptionLand,
                    snesUnit.TerrainAdaptionSea,
                    snesUnit.TerrainAdaptionSpace
                });
            playStationUnitTerrainAdoptions.AddRange(new byte[] {
                    playstationUnit.TerrainAdaptionAir,
                    playstationUnit.TerrainAdaptionLand,
                    playstationUnit.TerrainAdaptionSea,
                    playstationUnit.TerrainAdaptionSpace
                });
            if (preferedPilotSnes != null)
            {
                snesUnitTerrainAdoptionsWithPreferedPilot.AddRange(new byte[] {
                    (byte)((snesUnit.TerrainAdaptionAir + preferedPilotSnes.TerrainAdaptionAir)/2),
                    (byte)((snesUnit.TerrainAdaptionLand+ preferedPilotSnes.TerrainAdaptionLand)/2),
                    (byte)((snesUnit.TerrainAdaptionSea+ preferedPilotSnes.TerrainAdaptionSea)/2),
                    (byte)((snesUnit.TerrainAdaptionSpace+ preferedPilotSnes.TerrainAdaptionAir)/2),
                });
            }
            if (preferedPilotPlayStation != null)
            {
                playStationUnitTerrainAdoptionsWithPreferedPilot.AddRange(new byte[] {
                    (byte)((playstationUnit.TerrainAdaptionAir + preferedPilotPlayStation.TerrainAdaptionAir)/2),
                    (byte)((playstationUnit.TerrainAdaptionLand+ preferedPilotPlayStation.TerrainAdaptionLand)/2),
                    (byte)((playstationUnit.TerrainAdaptionSea+ preferedPilotPlayStation.TerrainAdaptionSea)/2),
                    (byte)((playstationUnit.TerrainAdaptionSpace+ preferedPilotPlayStation.TerrainAdaptionAir)/2)
                });
            }
            for (int i = 0; i < terrainAdaptionNames.Length; i++)
            {
                stringBuilder.AppendFormat("        | {0}", terrainAdaptionNames[i]);
                var snesUnitTerrainAdoption = snesUnitTerrainAdoptions[i];
                var playStationUnitTerrainAdoption = playStationUnitTerrainAdoptions[i];
                var effectiveSnesUnitTerrainAdoption = snesUnitTerrainAdoption;
                var effectivePlayStationUnitTerrainAdoption = playStationUnitTerrainAdoption;
                if (preferedPilotSnes != null)
                {
                    effectiveSnesUnitTerrainAdoption = snesUnitTerrainAdoptionsWithPreferedPilot[i];
                }
                if (preferedPilotPlayStation != null)
                {
                    effectivePlayStationUnitTerrainAdoption = playStationUnitTerrainAdoptionsWithPreferedPilot[i];
                }
                if (snesUnitTerrainAdoption == playStationUnitTerrainAdoption)
                {
                    stringBuilder.AppendFormat("{0}", TerrainAdaptionHelper.FormatTerrainAdaption(snesUnitTerrainAdoption));
                }
                else
                {
                    stringBuilder.AppendFormat("{0} ({1})", TerrainAdaptionHelper.FormatTerrainAdaption(snesUnitTerrainAdoption),
                        TerrainAdaptionHelper.FormatTerrainAdaption(playStationUnitTerrainAdoption));
                }
                if (effectiveSnesUnitTerrainAdoption != snesUnitTerrainAdoption || effectivePlayStationUnitTerrainAdoption != playStationUnitTerrainAdoption)
                {
                    stringBuilder.Append("→");
                    if (effectiveSnesUnitTerrainAdoption == effectivePlayStationUnitTerrainAdoption)
                    {
                        stringBuilder.AppendFormat("{0}", TerrainAdaptionHelper.FormatTerrainAdaption(effectiveSnesUnitTerrainAdoption));
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{0} ({1})", TerrainAdaptionHelper.FormatTerrainAdaption(effectiveSnesUnitTerrainAdoption),
                        TerrainAdaptionHelper.FormatTerrainAdaption(effectivePlayStationUnitTerrainAdoption));
                    }
                }
                stringBuilder.AppendLine();
            }
            Debug.Assert(snesUnit.HasAfterimage == playstationUnit.HasAfterimage);
            if (snesUnit.HasAfterimage)
            {
                stringBuilder.AppendLine("        | 分身");
            }
            Debug.Assert(snesUnit.HasShield == playstationUnit.HasShield);
            if (snesUnit.HasShield)
            {
                stringBuilder.AppendLine("        | 盾装備");
            }
            Debug.Assert(snesUnit.HasSword == playstationUnit.HasSword);
            if (snesUnit.HasSword || playstationUnit.HasSword)
            {
                stringBuilder.AppendLine("        | 剣装備");
            }
            Debug.Assert(snesUnit.HasEnergyRecovery == playstationUnit.HasEnergyRecovery);
            if (snesUnit.HasEnergyRecovery)
            {
                switch (snesUnit.HPRecoveryType)
                {
                    case 0x04:
                        stringBuilder.AppendLine("        | HP恢復(小)");
                        break;
                    case 0x08:
                        stringBuilder.AppendLine("        | HP恢復(大)");
                        break;
                }
            }
            Debug.Assert(snesUnit.RageAndDetonateImmune == playstationUnit.RageAndDetonateImmune);
            if (snesUnit.RageAndDetonateImmune)
            {
                stringBuilder.AppendLine("        | 激怒/自爆/てかげん無効");
            }
            Debug.Assert(snesUnit.BeamCoatType == playstationUnit.BeamCoatType);
            switch (snesUnit.BeamCoatType)
            {
                case 0x02:
                    stringBuilder.AppendLine("        | ビームコート");
                    break;
                case 0x04:
                    stringBuilder.AppendLine("        | Iフィールド");
                    break;
                case 0x06:
                    stringBuilder.AppendLine("        | オーラバリア");
                    break;
                case 0x08:
                    stringBuilder.AppendLine("        | ビームバリア");
                    break;
            }
            stringBuilder.AppendLine();

            stringBuilder.Append(Resource.RstUnitGridColumnAuto);
            stringBuilder.AppendLine();

        }
    }

}
