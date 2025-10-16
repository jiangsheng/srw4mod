using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Weapon : IRstFormatter, INamedItem
    {
        public int BaseOffset { get; set; }
        public int Id { get; set; }
        public byte TypeCode1 { get; set; }
        public byte TypeCode2 { get; set; }
        public bool IsDeflectable { get; set; }
        public bool IsBeam { get; set; }
        public bool IsPortable { get; set; }
        public byte TypeCode2LowerHalf { get; set; }
        public byte PilotQuote { get; set; }
        public ushort BattleAnimation { get; set; }
        public string? Name { get; set; }
        public bool IsRepair { get; set; }
        public bool IsResupply { get; set; }
        public bool IsMap { get; set; }
        public bool IsMelee { get; set; }
        public ushort Damage { get; set; }
        public sbyte AccuracyBonus { get; set; }
        public byte CriticalHitRateBonusAndUpgradeCostType { get; set; }
        public sbyte CriticalHitRateBonus { get; set; }
        public byte UpgradeCostType { get; set; }
        public byte MinRange { get; set; }
        public byte MaxRange { get; set; }
        public byte TerrainAdaption { get; set; }
        public byte MaxAmmo { get; set; }
        public byte EnergyCost { get; set; }
        public byte RequiredWill { get; set; }
        public byte RequiredSkill { get; set; }
        [Ignore]
        public bool HasAssignedOwner { get; set; }
        public string? FirstOwner { get; set; }

        public static List<Weapon>? Parse(byte[] weaponData, int headerStartOffset, int offsetBase, int footerOffset, List<WeaponMetaData> weaponMetaData)
        {
            var magicMark = BitConverter.ToUInt16(weaponData, headerStartOffset);
            Debug.Assert(magicMark == 0);

            var firstWeaponAddressOffset = BitConverter.ToUInt16(weaponData, headerStartOffset + 2);

            var weaponList = new List<Weapon>();

            for (int weaponIndex = 1; weaponIndex < firstWeaponAddressOffset; weaponIndex++)
            {
                var weaponOffset = BitConverter.ToUInt16(weaponData, headerStartOffset + weaponIndex * 2);
                if (weaponOffset == 0) continue;//no data at this address
                if (weaponOffset >= footerOffset) break;//reached footer
                Weapon weapon = ParseWeapon(weaponData, offsetBase + weaponOffset, weaponIndex);
                FixWeaponData(weapon, weaponMetaData);
                weaponList.Add(weapon);
            }
            return weaponList;
        }

        private static void FixWeaponData(Weapon weapon, List<WeaponMetaData> weaponMetaData)
        {
            var fixUnit = weaponMetaData.Where(u => u.Id == weapon.Id).FirstOrDefault();
            if (fixUnit == null)
            {
                Debug.WriteLine(string.Format("unable to find unit with id {0}", weapon.Id));
                return;
            }
            if(fixUnit.Name!=null)
                weapon.Name = fixUnit.Name.Trim()
                    ;
        }

        private static Weapon ParseWeapon(byte[] weaponData, int baseOffset, int unitIndex)
        {
            int offset = baseOffset;
            Weapon weapon = new Weapon();
            weapon.BaseOffset = baseOffset;
            weapon.Id = unitIndex;
            //offset 0
            weapon.TypeCode1 = weaponData[offset++];
            switch (weapon.TypeCode1)
            {
                case 0xDE:
                    weapon.IsRepair = true;
                    break;
                case 0xDF:
                    weapon.IsResupply = true;
                    break;
                default:
                    if (weapon.TypeCode1 >= 0x80 && weapon.TypeCode1 <= 0x8f)
                    {
                        weapon.IsMap = true;
                    }
                    else
                        weapon.IsMelee = (weapon.TypeCode1 & 0x40) != 0;
                    break;
            }
            //offset 1
            weapon.TypeCode2 = weaponData[offset++];
            weapon.IsDeflectable = (weapon.TypeCode2 & 0x20) != 0;
            weapon.IsBeam = (weapon.TypeCode2 & 0x80) != 0;
            weapon.IsPortable = (weapon.TypeCode2 & 0x40) == 0;
            weapon.TypeCode2LowerHalf = (byte)(weapon.TypeCode2 & 0x0f);
            //offset 2
            weapon.PilotQuote = weaponData[offset++];
            //offset 3,4
            weapon.BattleAnimation = BitConverter.ToUInt16(weaponData, offset);
            offset += 2;
            //offset 5,6
            weapon.Damage = BitConverter.ToUInt16(weaponData, offset);
            Debug.Assert(weapon.Damage <= 18000);
            offset += 2;
            //offset 7
            weapon.AccuracyBonus = (sbyte)weaponData[offset++];
            //offset 8
            weapon.CriticalHitRateBonusAndUpgradeCostType = weaponData[offset++];
            weapon.CriticalHitRateBonus = (sbyte)(((weapon.CriticalHitRateBonusAndUpgradeCostType & 0xF0) / 16) * 10 - 10);
            weapon.UpgradeCostType = (byte)(weapon.CriticalHitRateBonusAndUpgradeCostType & 0x0F);
            //offset 9
            weapon.MinRange = weaponData[offset++];
            Debug.Assert(weapon.MinRange < 15);
            //offset a
            weapon.MaxRange = weaponData[offset++];
            Debug.Assert(weapon.MaxRange < 15);
            //offset b
            weapon.TerrainAdaption = weaponData[offset++];
            //offset c
            weapon.MaxAmmo = weaponData[offset++];
            Debug.Assert(weapon.MaxAmmo <= 50);
            //offset d
            weapon.EnergyCost = weaponData[offset++];
            Debug.Assert(weapon.EnergyCost <= 150);
            //offset e
            weapon.RequiredWill = weaponData[offset++];
            Debug.Assert(weapon.RequiredWill <= 150);
            //offset f
            weapon.RequiredSkill = weaponData[offset++];
            Debug.Assert(weapon.RequiredSkill < 0x40);
            return weapon;
        }
        private static string FormatTerrainAdaption(byte terrainAdaption)
        {
            StringBuilder sb = new StringBuilder();
            byte[] terrainAdaptions = new byte[4];
            terrainAdaptions[2] = (byte)(terrainAdaption / 64);//sea
            terrainAdaptions[1] = (byte)((terrainAdaption / 16) & 0x03);//land
            terrainAdaptions[0] = (byte)((terrainAdaption / 4) & 0x03);//air
            terrainAdaptions[3] = (byte)(terrainAdaption & 0x03);//space
            for (int i = 0; i < 4; i++)
            {
                switch (terrainAdaptions[i])
                {
                    case 3: sb.Append("A"); break;
                    case 2: sb.Append("B"); break;
                    case 1: sb.Append("C"); break;
                    case 0: sb.Append("-"); break;
                }
            }
            return sb.ToString();
        }
        private static string FormatPilotQuote(byte pilotQuote)
        {
            switch (pilotQuote)
            {
                default:
                    return pilotQuote.ToString("X");
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.AppendFormat("Id: {0:X}", Id);
            stringBuilder.AppendFormat(", 名: {0}", Name);
            if (IsMelee)
                stringBuilder.Append("🤛");
            if (IsMap)
                stringBuilder.Append("🗺️");
            if (IsPortable)
                stringBuilder.Append("Ⓟ");
            if (IsRepair)
                stringBuilder.Append("🔧");
            if (IsResupply)
                stringBuilder.Append("🔄");
            if (IsDeflectable)
                stringBuilder.Append("⚔");
            if (IsBeam)
                stringBuilder.Append("Ⓑ");
            stringBuilder.AppendFormat(", 地址: {0:X}", BaseOffset);
            //stringBuilder.AppendFormat(", 类型1: {0:X2}{1:X2}", TypeCode1, TypeCode2);
            //stringBuilder.AppendFormat(", 类型2未知字节: {0:X}", TypeCode2LowerHalf);
            stringBuilder.AppendFormat(", 台词: {0:X}:{1:X}", BaseOffset + 2, FormatPilotQuote(PilotQuote));
            stringBuilder.AppendFormat(", 动画: {0:X}:{1:X}", BaseOffset + 3, BattleAnimation);
            stringBuilder.AppendFormat(", 伤害: {0:X}:{1}", BaseOffset + 5, Damage);
            stringBuilder.AppendFormat(", 程: {0}~{1}", MinRange, MaxRange);
            stringBuilder.AppendFormat(", 命中补正: {0}", AccuracyBonus);
            stringBuilder.AppendFormat(", 改造价格类型: {0}", UpgradeCostType);
            stringBuilder.AppendFormat(", 地形适应: {0:X}:{1}", BaseOffset + 0xb, FormatTerrainAdaption(TerrainAdaption));
            stringBuilder.AppendFormat(", 暴击补正: {0}", CriticalHitRateBonus);

            if (MaxAmmo > 0)
                stringBuilder.AppendFormat("\t弹数: {0:X}:{1}", BaseOffset + 0xc, MaxAmmo);
            if (EnergyCost > 0)
                stringBuilder.AppendFormat("\t耗能: {0:X}:{1}", BaseOffset + 0xd, EnergyCost);
            if (RequiredWill > 0)
                stringBuilder.AppendFormat("\t必要气力: {0:X}:{1}", BaseOffset + 0xe, RequiredWill);
            if (RequiredSkill > 0)
                stringBuilder.AppendFormat("\t必要技能: {0:X}:{1}:{0:X}", BaseOffset + 0xf, RequiredSkill);
            

            if (HasAssignedOwner)
                stringBuilder.AppendFormat("\t首装备: {0:X}", FirstOwner);
            return stringBuilder.ToString();
        }

        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.Append(string.Format("     - {0}", this.Name));
            if (IsMelee)
                row.Append("🤛");
            if (IsMap)
                row.Append("🗺️");
            if(IsPortable)
                row.Append("Ⓟ");
            if (IsRepair)
                row.Append("🔧");
            if (IsResupply)
                row.Append("🔄");
            if (IsDeflectable)
                row.Append("⚔");
            if (IsBeam)
                row.Append("Ⓑ");

            row.AppendLine();
            row.AppendLine(string.Format("     - {0}", Damage));
            if(MaxRange== MinRange)
                row.AppendLine(string.Format("     - {0}", MinRange));
            else
                row.AppendLine(string.Format("     - {0}~{1}", MinRange, MaxRange));
            row.AppendLine(string.Format("     - {0}", AccuracyBonus));
            row.AppendLine(string.Format("     - {0}", CriticalHitRateBonus));
            row.AppendLine(string.Format("     - {0}", FormatTerrainAdaption(TerrainAdaption)));
            if(MaxAmmo>0)
                row.AppendLine(string.Format("     - {0}", MaxAmmo));
            else
                row.AppendLine("     - ");
            if(EnergyCost>0)
                row.AppendLine(string.Format("     - {0}", EnergyCost));
            else
                row.AppendLine("     - ");
            if (RequiredWill>0)
                row.AppendLine(string.Format("     - {0}", RequiredWill));
            else
                row.AppendLine("     - ");
            if(RequiredSkill>0)
                row.AppendLine(string.Format("     - {0}", PilotSpiritCommandsOrSkill.Format(0,RequiredSkill,0)));
            else
                row.AppendLine("     - ");
            row.AppendLine(string.Format("     - {0:X}", UpgradeCostType));
            row.AppendLine(string.Format("     - {0}", FormatPilotQuote(PilotQuote)));  
            if (string.IsNullOrEmpty(FirstOwner))
            {
                row.AppendLine("     - ");
            }
            else
            {
                row.AppendLine(string.Format("     - {0}", FirstOwner));
            }

            return row.ToString();
        }
    }
}
