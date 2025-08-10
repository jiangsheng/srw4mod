


using System;
using System.Diagnostics;
using System.Text;

namespace Entities
{
    public class Unit: IRstFormatter
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Affiliation { get; set; }
        public string? Franchise { get; set; }

        public ushort IconId { get; set; }
        public byte InGameFranchise { get; set; }
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
        public byte TerrianAdaptionAir { get; set; }
        public byte TerrianAdaptionSea { get; set; }
        public byte TerrianAdaptionSpace { get; set; }
        public byte TerrianAdaptionLand { get; set; }
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

        public static List<Unit>? Parse(byte[] unitData, int headerStartOffset, int offsetBase, int footerOffset, List<Unit> units, List<Weapon> weapons)
        {
            var magicMark = BitConverter.ToUInt16(unitData, headerStartOffset);
            Debug.Assert(magicMark == 0);

            var firstUnitOffset = BitConverter.ToUInt16(unitData, headerStartOffset + 2);

            var unitList = new List<Unit>();

            for (int unitIndex = 1; unitIndex < firstUnitOffset / 2; unitIndex++)
            {
                var unitOffset = BitConverter.ToUInt16(unitData, headerStartOffset + unitIndex * 2);
                if (unitOffset == 0 ) continue;//no data at this address
                if (unitOffset >= footerOffset) break;//reached footer
                Unit unit = ParseUnit(unitData, offsetBase + unitOffset, unitIndex);
                FixUnitData(unit, units, weapons);
                unitList.Add(unit);
            }
            return unitList.Where(u => (u.Affiliation != null && !u.Affiliation.Equals(""))).OrderBy(u => u.Id).ToList();
        }

        private static void FixUnitData(Unit unit, List<Unit> units, List<Weapon> weapons)
        {
            var fixUnit = units.Where(u => u.Id == unit.Id).FirstOrDefault();
            if (fixUnit == null)
            {
                Debug.WriteLine(string.Format("unable to find unit with id {0}", unit.Id));
                return;
            }
            unit.Name = fixUnit.Name;
            unit.Affiliation = fixUnit.Affiliation;
            unit.Franchise = fixUnit.Franchise;
            if (unit.Weapons != null)
            {
                foreach (var unitWeapon in unit.Weapons)
                {
                    var fixWeapon = weapons.Where(w => w.Id == unitWeapon.WeaponIndex).First();
                    unitWeapon.Name = fixWeapon.Name;
                    if (!fixWeapon.HasAssigneOwner)
                    {
                        fixWeapon.HasAssigneOwner = true;
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
            byte inGameFranchise = playStationUnitData[offset++];

            var iconId2 = inGameFranchise & 0x01;

            unit.IconId = (ushort)(iconId2 * 256 + iconId1);

            unit.Id = unitIndex;
            unit.InGameFranchise = (byte)(inGameFranchise & 0xFE);
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
            var terrianAdaption = playStationUnitData[offset++];
            unit.TerrianAdaptionAir = (byte)((terrianAdaption & 0xF0) / 16);
            Debug.Assert(unit.TerrianAdaptionAir < 5);
            unit.TerrianAdaptionSea = (byte)(terrianAdaption & 0x0F);
            Debug.Assert(unit.TerrianAdaptionSea < 5);
            terrianAdaption = playStationUnitData[offset++];
            unit.TerrianAdaptionSpace = (byte)((terrianAdaption & 0xF0) / 16);
            Debug.Assert(unit.TerrianAdaptionSpace < 5);
            unit.TerrianAdaptionLand = (byte)(terrianAdaption & 0x0F);
            Debug.Assert(unit.TerrianAdaptionLand < 5);

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
                        unitWeapon.AvailabilAtStage = playStationUnitData[offset++];
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
            stringBuilder.AppendFormat(", TerrianAdaption: {0:X}", BaseOffset + 0x16);
            stringBuilder.AppendFormat(", FirstWeaponAddress: {0:X}", BaseOffset + 0x20);

            stringBuilder.AppendFormat(", Affiliation: {0:X}", Affiliation);
            stringBuilder.AppendFormat(", Franchise: {0:X}", Franchise);
            stringBuilder.AppendFormat(", IconId: {0:X}", IconId);
            stringBuilder.AppendFormat(", InGameFranchise:{0:X} ({1})", InGameFranchise, FranchiseHelper.FormatInGameFranchise(InGameFranchise));
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

            stringBuilder.AppendFormat(", TerrianAdaption: {0}", TerrianAdaptionHelper.FormatTerrianAdaption(
                new byte[] {
                TerrianAdaptionAir,TerrianAdaptionLand, TerrianAdaptionSea, TerrianAdaptionSpace}));

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
        string GetNearDamage(List<UnitWeapon>? weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return "🚫";
            }
            var bestNearWeapon = weapons.Where(w => w.Range == 1).OrderByDescending(w => w.Damage).FirstOrDefault();
            if (bestNearWeapon == null) return "🚫";
            return bestNearWeapon.Damage.ToString();
        }
        string GetFarDamage(List<UnitWeapon>? weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return "🚫";
            }
            var bestFarWeapon = weapons.Where(w => w.Range > 1 && w.IsMap == false&& w.Damage>0).OrderByDescending(w => w.Damage).FirstOrDefault();
            if (bestFarWeapon == null) return "🚫";
            return bestFarWeapon.Damage.ToString();
        }
        string GetMaxRangeExceptMap(List<UnitWeapon>? weapons)
        {
            if (weapons == null || weapons.Count == 0)
            {
                return "🚫";
            }
            var bestFarWeapon = weapons.Where(w => w.Range > 1 && w.IsMap == false&& w.Damage>0).OrderByDescending(w => w.Range).FirstOrDefault();
            if (bestFarWeapon == null) return "🚫";
            return bestFarWeapon.Range.ToString();
        }
        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.AppendLine(string.Format("     - {0}", this.Affiliation));
            row.AppendLine(string.Format("     - {0}", GetUnitIcon(this.Id)));
            row.AppendLine(string.Format("     - {0}", this.Name));
            row.AppendLine(string.Format("     - {0}", FranchiseHelper.ToRstFranchise(this.Franchise, "units")));
            row.AppendLine(string.Format("     - {0}", this.HP));
            row.AppendLine(string.Format("     - {0}", this.Energy));
            row.AppendLine(string.Format("     - {0}", this.Mobility));
            row.AppendLine(string.Format("     - {0}", this.Armor * 10));
            row.AppendLine(string.Format("     - {0}", this.Limit));
            row.AppendLine(string.Format("     - {0}", this.MoveRange));
            row.AppendLine(string.Format("     - {0}", this.GetNearDamage(this.Weapons)));
            row.AppendLine(string.Format("     - {0}", this.GetFarDamage(this.Weapons)));
            row.AppendLine(string.Format("     - {0}", this.GetMaxRangeExceptMap(this.Weapons)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatMovementType(this.MoveType)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionAir)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionLand)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionSea)));
            row.AppendLine(string.Format("     - {0}", TerrianAdaptionHelper.FormatTerrianAdaption(this.TerrianAdaptionSpace)));
            return row.ToString();
        }

        private string GetUnitIcon(int id)
        {
            switch (id) {
                case 0x51:
                case 0xF8:
                case 0x101:
                case 0x106:
                case 0x10D:
                    return string.Empty;
                default:
                    return string.Format(".. image:: ../units/images/icon/srw4_units_icon_{0:X2}_B.png", id);

            }
        }
    }

}
