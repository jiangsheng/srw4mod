


using System;
using System.Diagnostics;
using System.Text;

namespace Entities
{
    public class Unit
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
        public bool Discardable{ get; set; }
        public byte UnitSize { get; set; }
        public byte UnitSizeBit { get; set; }
        public byte BackgroundMusic { get; set; }
        public byte TransformOrCombineType { get; set; }
        public byte UnknownUnitSpecialSkill1 { get; set; }
        public byte UnknownUnitSpecialSkill2 { get; set; }
        public bool HasSword { get; private set; }
        public int BeamCoatType { get; private set; }
        public bool HasAfterimage { get; private set; }
        public bool HasShield { get; private set; }
        public byte EnergyRecoveryType { get; private set; }
        public byte HPRecoveryType { get; private set; }
        public bool RageAndDetonateImmune { get; private set; }
        public bool IsAggressive { get; set; }
        public byte Team { get; set; }
        public byte Experience { get; set; }

        public ushort Gold { get; set; }

        public ushort RepairCost { get; set; }
        public byte MoveRange{ get; set; }
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
        
        public static List<Unit>? Parse(byte[] playStationUnitData, int startOffset,List<Unit> units, List<Weapon> weapons)
        {
            var magicMark= BitConverter.ToUInt16(playStationUnitData, startOffset);
            Debug.Assert(magicMark == 0);

            var firstUnitOffset= BitConverter.ToUInt16(playStationUnitData, startOffset+2);
            var footerAddressOffset = BitConverter.ToUInt16(playStationUnitData, startOffset+ firstUnitOffset - 2);

            var unitList = new List<Unit>();

            for (int  unitIndex= 1; unitIndex < firstUnitOffset/2; unitIndex++)
            {
                var unitOffset = BitConverter.ToUInt16(playStationUnitData, startOffset + unitIndex*2);
                if (unitOffset == 0|| unitOffset == footerAddressOffset) continue;//no data at this address
                Unit unit = ParseUnit(playStationUnitData, startOffset+unitOffset, unitIndex);
                FixUnitData(unit, units, weapons);
                Debug.WriteLine(unit);
                unitList.Add(unit);
            }
            return unitList;
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
                        fixWeapon.FirstOwner=unit.Name;                        
                    }
                    unitWeapon.FirstOwner = fixWeapon.FirstOwner;
                    unitWeapon.Damage=fixWeapon.Damage;
                    unitWeapon.Range = fixWeapon.MaxRange;
                    unitWeapon.Ammo=fixWeapon.MaxAmmo;
                    unitWeapon.En = fixWeapon.EnergyCost;
                }
            }
        }

        private static Unit ParseUnit(byte[] playStationUnitData, int baseOffset, int unitIndex)
        {
            Unit unit=new Unit();
            unit.BaseOffset= baseOffset;
            int offset = baseOffset;

            byte iconId1= playStationUnitData[offset++];
            byte inGameFranchise = playStationUnitData[offset++];

            var iconId2 = inGameFranchise & 0x01;

            unit.IconId = (ushort)(iconId2*256+iconId1);

            unit.Id = unitIndex;
            unit.InGameFranchise = (byte)( inGameFranchise & 0xFE);
            unit.PortraitId = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;

            unit.FixedSeatPilotId= playStationUnitData[offset++];
            unit.TransferFranchiseId = playStationUnitData[offset++];
            var sizeAndBGM= playStationUnitData[offset++];

            unit.UnitSize =(byte) (sizeAndBGM&0x60);
            unit.UnitSizeBit = (byte)(sizeAndBGM & 0x10);
            unit.Discardable = (sizeAndBGM & 0x80)==0; 
            unit.BackgroundMusic= (byte)(sizeAndBGM & 0x0F);
            unit.TransformOrCombineType = playStationUnitData[offset++];

            var unitSpecialSkill1 = playStationUnitData[offset++];
            var unitSpecialSkill2 = playStationUnitData[offset++];
            unit.HasSword= (unitSpecialSkill1 & 0x40)!=0;
            unit.EnergyRecoveryType = (byte)(unitSpecialSkill1 & 0x03);
            unit.HPRecoveryType = (byte)(unitSpecialSkill1 & 0x0C);
            unit.RageAndDetonateImmune = (unitSpecialSkill1 & 0x80)!=0;
            unit.UnknownUnitSpecialSkill1 = (byte) (unitSpecialSkill1 & (~0xCF));
            

            unit.BeamCoatType = unitSpecialSkill2 & 0x0E;
            unit.HasAfterimage = (unitSpecialSkill2 & 0x10)!=0;
            unit.HasShield = (unitSpecialSkill2 & 0x20) != 0;
            unit.IsAggressive = (unitSpecialSkill2 & 0x1) != 0;
            unit.UnknownUnitSpecialSkill2 = (byte)(unitSpecialSkill1 & (~0x3F));
            unit.Team = playStationUnitData[offset++];
            offset += 4;
            unit.Experience = playStationUnitData[offset++];
            unit.Gold = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;
            unit.RepairCost = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;
            unit.MoveRange = playStationUnitData[offset++];
            unit.MoveType = playStationUnitData[offset++];
            var terrianAdaption = playStationUnitData[offset++];
            unit.TerrianAdaptionAir = (byte)(terrianAdaption & 0xF0);
            unit.TerrianAdaptionSea = (byte)(terrianAdaption & 0x0F);

            terrianAdaption = playStationUnitData[offset++];
            unit.TerrianAdaptionSpace = (byte)(terrianAdaption & 0xF0);
            unit.TerrianAdaptionLand = (byte)(terrianAdaption & 0x0F);

            unit.Armor = playStationUnitData[offset++];
            unit.Mobility = playStationUnitData[offset++];
            unit.Limit = playStationUnitData[offset++];

            unit.Energy = playStationUnitData[offset++];
            unit.HP = BitConverter.ToUInt16(playStationUnitData, offset);
            offset += 2;

            unit.WeaponCount = playStationUnitData[offset++];
            unit.AmmoWeaponCount = playStationUnitData[offset++];
            Debug.Assert(unit.AmmoWeaponCount<= unit.WeaponCount);
            Debug.Assert(unit.WeaponCount <0x20);
            unit.FirstWeaponIndex=(ushort)((playStationUnitData[offset]+ playStationUnitData[offset+1]*256) & 0x03FF);

            //the last unit has no weapon 
            if (unit.WeaponCount>0 )
            {
                unit.Weapons = new List<UnitWeapon>();
                var weaponIndex = (ushort)(playStationUnitData[offset + 1] * 256 + playStationUnitData[offset]);
                while (weaponIndex>0) {
                    UnitWeapon unitWeapon = new UnitWeapon();
                    unitWeapon.BaseOffset = offset;
                    unitWeapon.WeaponIndex = (ushort)(weaponIndex & 0x03FF);
                    unitWeapon.AmmoSlot = (byte)((playStationUnitData[offset + 1] & 0xFC)/4);
                    offset += 2;
                    var weaponOrderAndType = playStationUnitData[offset++];
                    unitWeapon.IsConditional = (weaponOrderAndType & 0x80) > 0;
                    unitWeapon.Order = (byte)(weaponOrderAndType &0x1F);
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
                while(weaponIndex>0)
                {
                    weaponIndex = (ushort)(playStationUnitData[offset + 1] * 256 + playStationUnitData[offset]);
                    if(weaponIndex>0)
                        offset +=3;
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
            stringBuilder.AppendFormat(", TransformOrCombineTypeAddress: {0:X}", BaseOffset+7);
            stringBuilder.AppendFormat(", WeaponCountAddress: {0:X}", BaseOffset + 0x1E);
            stringBuilder.AppendFormat(", AmmoWeaponCountAddress: {0:X}", BaseOffset + 0x1F);
            stringBuilder.AppendFormat(", TerrianAdaption: {0:X}", BaseOffset + 0x16); 
            stringBuilder.AppendFormat(", FirstWeaponAddress: {0:X}", BaseOffset + 0x20);

            stringBuilder.AppendFormat(", Affiliation: {0:X}", Affiliation); 
            stringBuilder.AppendFormat(", Franchise: {0:X}", Franchise);
            stringBuilder.AppendFormat(", IconId: {0:X}", IconId);
            stringBuilder.AppendFormat(", InGameFranchise:{0:X} ({1})", InGameFranchise, FormatInGameFranchise(InGameFranchise));
            stringBuilder.AppendFormat("\r\n, PortraitId: {0:X}", PortraitId);
            stringBuilder.AppendFormat(", FixedSeatPilotId: {0:X}", FixedSeatPilotId); 
            stringBuilder.AppendFormat(", TransferFranchiseId: {0:X}", TransferFranchiseId);
            stringBuilder.AppendFormat(", Discardable: {0}", Discardable); 
            stringBuilder.AppendFormat(", UnitSize:{0:X} ({1})", UnitSize, FormatUnitSize(UnitSize));
            if(UnitSizeBit!=0)
                stringBuilder.AppendFormat(", Unknown UnitSize Bit:{0:X}", UnitSizeBit);
            stringBuilder.AppendFormat(", BackgroundMusic: {0:X}", BackgroundMusic);
            stringBuilder.AppendFormat(", TransformOrCombineType: {0:X}", TransformOrCombineType);
            stringBuilder.Append("\r\n");
            if (HasSword)
            {
                stringBuilder.Append("剣装備");
            }
            switch (EnergyRecoveryType)
            {
                case 0x02:
                    stringBuilder.Append(", EN恢復(小)"); break;
                case 0x03:
                    stringBuilder.Append(", EN恢復(大)"); break;
                default:
                    stringBuilder.Append(string.Format(" Unknown EnergyRecoveryType: {0}", EnergyRecoveryType)); break;
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
            if (UnknownUnitSpecialSkill1 != 0)
            {
                stringBuilder.AppendFormat(", Unknown unitSpecialSkill1 {0} ", FormatHex(UnknownUnitSpecialSkill1));
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
                stringBuilder.AppendFormat(", Unknown unitSpecialSkill2 {0} ", FormatHex(UnknownUnitSpecialSkill2));
            }
            stringBuilder.AppendFormat(", Team: {0:X}", Team);
            stringBuilder.AppendFormat(", Experience: {0:X}", Experience);
            stringBuilder.AppendFormat(", Gold: {0:X}", Gold);
            stringBuilder.AppendFormat(", RepairCost: {0:X}", RepairCost);

            stringBuilder.AppendFormat(", MoveRange: {0:X}", MoveRange);
            stringBuilder.AppendFormat(", MoveType: {0:X}", MoveType);

            stringBuilder.AppendFormat(", TerrianAdaption: {0}", FormatTerrianAdaption(
                new byte[] {
                TerrianAdaptionAir,TerrianAdaptionLand, TerrianAdaptionSea, TerrianAdaptionSpace}));

            stringBuilder.AppendFormat("\r\n, Armor: {0}", Armor*10);
            stringBuilder.AppendFormat(", Mobility: {0}", Mobility);
            stringBuilder.AppendFormat(", Limit: {0}", Limit);

            stringBuilder.AppendFormat(", Energy: {0}", Energy);
            stringBuilder.AppendFormat(", HP: {0}", HP);

            stringBuilder.AppendFormat(", WeaponCount: {0:X}", WeaponCount);
            stringBuilder.AppendFormat(", AmmoWeaponCount: {0:X}", AmmoWeaponCount);

            if (WeaponCount > 0 && Weapons!=null)
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

        private string FormatTerrianAdaption(byte[] terrianAdaptions)
        {
            StringBuilder stringBuilder
               = new StringBuilder();
            foreach (var item in terrianAdaptions)
            {
                if (item == 0x04 || item == 0x40)
                    stringBuilder.Append("A");
                if (item == 0x03 || item == 0x30)
                    stringBuilder.Append("B");
                if (item == 0x02 || item == 0x20)
                    stringBuilder.Append("C");
                if (item == 0x01 || item == 0x10)
                    stringBuilder.Append("D");
                if (item == 0x00)
                    stringBuilder.Append("-");
            }
            return stringBuilder.ToString();
        }

        private string FormatHex(int value)
        {
            return value.ToString("X");
        }
        private string FormatUnitSize(int unitSize)
        {
            switch (unitSize)
            {
                case 0x00: return "S";
                case 0x20: return "M";
                case 0x40: return "L";
                case 0x60: return "LL";
                case 0xB0: return "M";
                default: return "Unknown Unit Size "+unitSize.ToString();
            }
        }

        private string FormatInGameFranchise(int inGameFranchise)
        {
            switch (inGameFranchise)
            {
                case 0x00:
                    return "マジンガーＺ";
                case 0x02:
                    return "劇場版マジンガーＺ";
                case 0x04:
                    return "グレートマジンガー";
                case 0x06:
                    return "グレンダイザー";
                case 0x08:
                    return "ゲッターロボ";
                case 0x0A:
                    return "ゲッターロボG";
                case 0x0C:
                    return "コンバトラーＶ";
                case 0x0E:
                    return "ダイモス";
                case 0x10:
                    return "ザンボット３";
                case 0x12:
                    return "ダイターン";
                case 0x14:
                    return "ダンバイン";
                case 0x16:
                    return "ダンバインOVA";
                case 0x18:
                    return "エルガイム";
                case 0x1A:
                    return "機動戦士ガンダム";
                case 0x1C:
                    return "機動戦士Zガンダム";
                case 0x1E:
                    return "機動戦士ZZガンダム";
                case 0x20:
                    return "機動戦士ガンダム0080";
                case 0x22:
                    return "機動戦士ガンダム0083";
                case 0x24:
                    return "ガンダムセンチネル";
                case 0x26:
                    return "逆襲のシャア";
                case 0x28:
                    return "F91";
                case 0x2A:
                    return "ライディーン";
                case 0x2C:
                    return "ゴーショーグン";
                case 0x2E:
                    return "ダンクーガ";
                case 0x30:
                    return "オリジナル";
                default:
                    return (string.Format("Unknown inGameFranchise {0:x}", inGameFranchise));
            }
        }
    }

}
