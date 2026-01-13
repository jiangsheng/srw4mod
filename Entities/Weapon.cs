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
        public int NameOffset { get; private set; }
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
        public byte MaxAmmo { get; set; }
        public byte EnergyCost { get; set; }
        public byte RequiredWill { get; set; }
        public byte RequiredSkill { get; set; }
        [Ignore]
        public bool HasAssignedOwner { get; set; }
        public Unit? FirstOwner { get; set; }

        public static List<Weapon>? Parse(ReadOnlySpan<byte> romData, IndexTable indexTable, List<EntityName> weaponNames)
        {
            var weaponList = new List<Weapon>();
            var indexedLocations = indexTable.Read(romData);
            int weaponId = 0;
            foreach (var location in indexedLocations) {
                if (location != 0)
                {
                    var weapon = ParseWeapon(romData.Slice(location, 16), location, weaponId);
                    if (weapon != null)
                    {
                        FixWeaponData(weapon, weaponNames);
                        weaponList.Add(weapon);
                    }
                    else
                        break;
                }
                weaponId++;
            }
            return weaponList;
        }

        private static void FixWeaponData(Weapon weapon, List<EntityName> weaponNames)
        {
            var fixWeaponMetaData = weaponNames.Where(u => u.Id == weapon.Id).FirstOrDefault();
            if (fixWeaponMetaData == null)
            {
                Debug.WriteLine(string.Format("unable to find weapon with id {0}", weapon.Id));
                return;
            }
            if (fixWeaponMetaData.Name != null)
            {
                weapon.Name = fixWeaponMetaData.Name.Trim().Replace("[0]", "[グルンガスト]");
                weapon.NameOffset = fixWeaponMetaData.Offset;
            }
        }

        private static Weapon? ParseWeapon(ReadOnlySpan<byte> data, int baseOffset, int weaponId)
        {
            if (data[15] == 0xFF)
            { 
                //some buggy rom has a full row ff
                return null;
            }
            int offset = 0;
            Weapon weapon = new Weapon();
            weapon.BaseOffset = baseOffset;
            weapon.Id = weaponId;
            //offset 0
            weapon.TypeCode1 = data[offset++];
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
            weapon.TypeCode2 = data[offset++];
            weapon.IsDeflectable = (weapon.TypeCode2 & 0x20) != 0;
            weapon.IsBeam = (weapon.TypeCode2 & 0x80) != 0;
            weapon.IsPortable = (weapon.TypeCode2 & 0x40) == 0;
            weapon.TypeCode2LowerHalf = (byte)(weapon.TypeCode2 & 0x0f);
            //offset 2
            weapon.PilotQuote = data[offset++];
            //offset 3,4
            weapon.BattleAnimation = BitConverter.ToUInt16(
                data.Slice(offset,2));
            offset += 2;
            //offset 5,6
            weapon.Damage = BitConverter.ToUInt16(data.Slice(offset, 2));
            Debug.Assert(weapon.Damage <= 18000);
            offset += 2;
            //offset 7
            weapon.AccuracyBonus = (sbyte)data[offset++];
            //offset 8
            weapon.CriticalHitRateBonusAndUpgradeCostType = data[offset++];
            weapon.CriticalHitRateBonus = (sbyte)(((weapon.CriticalHitRateBonusAndUpgradeCostType & 0xF0) / 16) * 10 - 10);
            weapon.UpgradeCostType = (byte)(weapon.CriticalHitRateBonusAndUpgradeCostType & 0x0F);
            //offset 9
            weapon.MinRange = data[offset++];
            Debug.Assert(weapon.MinRange < 15);
            //offset a
            weapon.MaxRange = data[offset++];
            Debug.Assert(weapon.MaxRange < 15);
            //offset b
            byte terrainAdaptionByte= data[offset++];
            weapon.TerrainAdaptionSet= TerrainAdaptionSet.FromWeaponAdaptions(terrainAdaptionByte);
            //offset c
            weapon.MaxAmmo = data[offset++];
            Debug.Assert(weapon.MaxAmmo <= 50);
            //offset d
            weapon.EnergyCost = data[offset++];
            Debug.Assert(weapon.EnergyCost <= 150);
            //offset e
            weapon.RequiredWill = data[offset++];
            Debug.Assert(weapon.RequiredWill <= 150);
            //offset f
            weapon.RequiredSkill = data[offset++];
            Debug.Assert(weapon.RequiredSkill < 0x40);
            return weapon;
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
            stringBuilder.AppendFormat("\t地址:{0,6:X}", BaseOffset);
            stringBuilder.AppendFormat("\t伤害:{0,6:X}:{1,5}", BaseOffset + 5, Damage);
            stringBuilder.AppendFormat("\t程:{0,2}~{1,2}", MinRange, MaxRange);
            stringBuilder.AppendFormat("\t命中补正:{0,3}", AccuracyBonus);
            stringBuilder.AppendFormat("\t暴击补正:{0,3}", CriticalHitRateBonus);
            stringBuilder.AppendFormat("\t地形适应:{0,6:X}:{1}", BaseOffset + 0xb, TerrainAdaptionSet?.ToString());
            if (MaxAmmo > 0)
                stringBuilder.AppendFormat("\t弹数:{0,3:X}:{1}", BaseOffset + 0xc, MaxAmmo);
            if (EnergyCost > 0)
                stringBuilder.AppendFormat("\t耗能:{0,3:X}:{1}", BaseOffset + 0xd, EnergyCost);
            if (MaxAmmo == 0 && EnergyCost == 0)
            {
                stringBuilder.AppendFormat("\t      ");
            }
            stringBuilder.AppendFormat("\t名:{0:X}:{1}", NameOffset, GetNameWithAttributes());

            if (RequiredWill > 0)
                stringBuilder.AppendFormat("\t必要气力: {0,6:X}:{1,2}", BaseOffset + 0xe, RequiredWill);
            if (RequiredSkill > 0)
                stringBuilder.AppendFormat("\t必要技能: {0,6:X}:{1,2}", BaseOffset + 0xf, RequiredSkill);
            
            stringBuilder.AppendFormat("\t 台词: {0:X}:{1:X}", BaseOffset + 2, FormatPilotQuote(PilotQuote));
            stringBuilder.AppendFormat("\t 动画: {0:X}:{1:X}", BaseOffset + 3, BattleAnimation);
            stringBuilder.AppendFormat("\t 改造价格类型: {0}", UpgradeCostType);
            //stringBuilder.AppendFormat("\t类型1: {0:X2}{1:X2}", TypeCode1, TypeCode2);
            //stringBuilder.AppendFormat("\t类型2未知字节: {0:X}", TypeCode2LowerHalf);
            if (HasAssignedOwner)
                stringBuilder.AppendFormat("\t首装备: {0:X}", FirstOwner?.Name);
            return stringBuilder.ToString();
        }

        public string? ToRstRow(bool isPlayStation)
        {
            var row = new StringBuilder();
            row.AppendLine(string.Format("   * - {0:X2}", this.Id));
            row.Append(string.Format("     - {0}", Name));
            row.AppendLine();
            row.AppendLine(string.Format("     - {0}", Damage));
            if(MaxRange== MinRange)
                row.AppendLine(string.Format("     - {0}", MinRange));
            else
                row.AppendLine(string.Format("     - {0}~{1}", MinRange, MaxRange));
            row.AppendLine(string.Format("     - {0}", AccuracyBonus));
            row.AppendLine(string.Format("     - {0}", CriticalHitRateBonus));
            row.AppendLine(string.Format("     - {0}", TerrainAdaptionSet?.ToString()));
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
                row.AppendLine(string.Format("     - {0}", PilotSpiritCommandsOrSkill.Format(0,RequiredSkill,0,false)));
            else
                row.AppendLine("     - ");
            row.AppendLine(string.Format("     - {0:X}", UpgradeCostType));
            row.AppendLine(string.Format("     - {0}", FormatPilotQuote(PilotQuote)));  
            if (FirstOwner==null)
            {
                row.AppendLine("     - ");
            }
            else
            {
                row.AppendLine(string.Format("     - \\ :ref:`{0} <srw4_unit_{1}>`\\ {2}", FirstOwner.Name, RstHelper.GetLabelName(FirstOwner.EnglishName),
                    
                    string.IsNullOrEmpty(FirstOwner.ChineseName)?string.Empty: string.Format(" ({0})", FirstOwner.ChineseName)));
            }

            return row.ToString();
        }

        public string GetNameWithAttributes()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.Append(Name);
            if (IsMelee)
                stringBuilder.Append("🤛");
            if (IsMap)
            {
                if (!string.IsNullOrWhiteSpace(Name) && !Name.Contains("🗺"))
                {
                    stringBuilder.Append("🗺");
                }
            }
            if (IsPortable)
            {
                if (!string.IsNullOrWhiteSpace(Name) && !Name.Contains("Ⓟ"))
                    stringBuilder.Append("Ⓟ");
            }
            if (IsRepair)
                stringBuilder.Append("🔧");
            if (IsResupply)
                stringBuilder.Append("🔄");
            if (IsDeflectable)
                stringBuilder.Append("⚔");
            if (IsBeam)
            {
                if (!string.IsNullOrWhiteSpace(Name) && !Name.Contains("Ⓑ"))
                    stringBuilder.Append("Ⓑ");
            }
            return stringBuilder.ToString();
        }
    }
}
