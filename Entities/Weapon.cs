using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Weapon : IRstFormatter
    {
        public static int BaseOffset { get; set; }
        public int Id { get; set; }
        public byte TypeCode1 { get; set; }
        public byte TypeCode2 { get; set; }
        public bool IsCountercutable { get; set; }
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
        public byte CriticalHitRateBonusType { get; set; }
        public byte UpgradeCostType { get; set; }
        public byte MinRange { get; set; }
        public byte MaxRange { get; set; }
        public byte TerrainAdaption { get; set; }
        public byte MaxAmmo { get; set; }
        public byte EnergyCost { get; set; }
        public byte RequiredWill { get; set; }
        public byte RequiredSkill { get; set; }
        public bool HasAssigneOwner { get; set; }
        public string? FirstOwner { get; set; }

        public static List<Weapon>? Parse(byte[] weaponData, int headerStartOffset, int offsetBase, int footerOffset, List<Weapon> weapons)
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
                FixWeaponData(weapon, weapons);
                weaponList.Add(weapon);
            }
            return weaponList;
        }

        private static void FixWeaponData(Weapon weapon, List<Weapon> weapons)
        {
            var fixUnit = weapons.Where(u => u.Id == weapon.Id).FirstOrDefault();
            if (fixUnit == null)
            {
                Debug.WriteLine(string.Format("unable to find unit with id {0}", weapon.Id));
                return;
            }
            if(fixUnit.Name!=null)
                weapon.Name = fixUnit.Name.Trim()
                    ;
        }

        private static Weapon ParseWeapon(byte[] playStationWeaponData, int baseOffset, int unitIndex)
        {
            int offset = baseOffset;
            Weapon weapon = new Weapon();
            Weapon.BaseOffset = baseOffset;
            weapon.Id = unitIndex;
            weapon.TypeCode1 = playStationWeaponData[offset++];
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
            weapon.TypeCode2 = playStationWeaponData[offset++];
            weapon.IsCountercutable = (weapon.TypeCode2 & 0x20) != 0;
            weapon.IsBeam = (weapon.TypeCode2 & 0x80) != 0;
            weapon.IsPortable = (weapon.TypeCode2 & 0x40) == 0;
            weapon.TypeCode2LowerHalf = (byte)(weapon.TypeCode2 & 0x0f);
            weapon.PilotQuote = playStationWeaponData[offset++];
            weapon.BattleAnimation = BitConverter.ToUInt16(playStationWeaponData, offset);
            offset += 2;
            weapon.Damage = BitConverter.ToUInt16(playStationWeaponData, offset);
            Debug.Assert(weapon.Damage <= 18000);
            offset += 2;

            weapon.AccuracyBonus = (sbyte)playStationWeaponData[offset++];
            weapon.CriticalHitRateBonusAndUpgradeCostType = playStationWeaponData[offset++];
            weapon.CriticalHitRateBonusType = (byte)(weapon.CriticalHitRateBonusAndUpgradeCostType & 0xF0);
            weapon.UpgradeCostType = (byte)(weapon.CriticalHitRateBonusAndUpgradeCostType & 0x0F);
            weapon.MinRange = playStationWeaponData[offset++];
            Debug.Assert(weapon.MinRange < 15);
            weapon.MaxRange = playStationWeaponData[offset++];
            Debug.Assert(weapon.MaxRange < 15);
            weapon.TerrainAdaption = playStationWeaponData[offset++];
            weapon.MaxAmmo = playStationWeaponData[offset++];
            Debug.Assert(weapon.MaxAmmo <= 50);
            weapon.EnergyCost = playStationWeaponData[offset++];
            Debug.Assert(weapon.EnergyCost <= 150);
            weapon.RequiredWill = playStationWeaponData[offset++];
            Debug.Assert(weapon.RequiredWill <= 150);
            weapon.RequiredSkill = playStationWeaponData[offset++];
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
        private static string FormatCriticalHitRateBonusType(byte criticalHitBonusType)
        {
            int criticalHitRateBonus = criticalHitBonusType / 16 * 10 - 10;
            return criticalHitRateBonus.ToString();
        }
        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();
            stringBuilder.AppendFormat("Id: {0:X}", Id);
            stringBuilder.AppendFormat(", Name: {0}", Name);
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
            if (IsCountercutable)
                stringBuilder.Append("⚔");
            if (IsBeam)
                stringBuilder.Append("Ⓑ");
            stringBuilder.AppendFormat(", BaseOffset: {0:X}", BaseOffset);
            stringBuilder.AppendFormat(", TypeCode: {0:X2}{1:X2}", TypeCode1, TypeCode2);
            stringBuilder.AppendFormat(", TypeCode2LowerHalf: {0:X}", TypeCode2LowerHalf);
            stringBuilder.AppendFormat(", Quote: {0}", FormatPilotQuote(PilotQuote));
            stringBuilder.AppendFormat(", Animation: {0}", BattleAnimation);
            stringBuilder.AppendFormat(", Damage: {0}", Damage);
            stringBuilder.AppendFormat(", Range: {0}~{1}", MinRange, MaxRange);
            stringBuilder.AppendFormat(", AccuracyBonus: {0}", AccuracyBonus);
            stringBuilder.AppendFormat(", Terrain: {0}", FormatTerrainAdaption(TerrainAdaption));
            stringBuilder.AppendFormat(", CriticalHitRateBonus: {0}", FormatCriticalHitRateBonusType(CriticalHitRateBonusType));

            if (MaxAmmo > 0)
                stringBuilder.AppendFormat(", Ammo: {0}", MaxAmmo);
            if (EnergyCost > 0)
                stringBuilder.AppendFormat(", Energy: {0}", EnergyCost);
            if (RequiredWill > 0)
                stringBuilder.AppendFormat(", Will: {0}", RequiredWill);
            if (RequiredSkill > 0)
                stringBuilder.AppendFormat(", Skill: {0:X}", RequiredSkill);
            stringBuilder.AppendFormat(", UpgradeCostType: {0}", UpgradeCostType);

            if (HasAssigneOwner)
                stringBuilder.AppendFormat(", FirstOwner: {0:X}", FirstOwner);
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
            if (IsCountercutable)
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
            row.AppendLine(string.Format("     - {0}", FormatCriticalHitRateBonusType(CriticalHitRateBonusType)));
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
                row.AppendLine(string.Format("     - {0}", PilotSpiritCommandsOrSkill.Format(RequiredSkill)));
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
