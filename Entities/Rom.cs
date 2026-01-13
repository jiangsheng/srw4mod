using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Rom
    {
        public List<Pilot>? Pilots { get; set; }
        public List<Unit>? Units { get; set; }
        public List<EntityName> WeaponNames { get; private set; }
        public List<EntityName>? UnitNames { get; private set; }
        public List<EntityName> PilotNames { get; private set; }
        public List<EntityName> PilotCallSigns { get; private set; }
        public List<Weapon>? Weapons { get; set; }
        bool IsPlayStation { get; set; }

        public static SRW4Encoding srw4Encoding = new SRW4Encoding();
        public RomOffsets RomOffsets { get; set; }
        public Rom(bool isPlayStation)
        {
            IsPlayStation = isPlayStation;
            RomOffsets=new RomOffsets(IsPlayStation);
        }

        public static Rom Parse(ReadOnlySpan<byte> romData, List<UnitMetaData> unitMetaData, List<PilotMetaData> pilotMetaData,
           bool isPlayStation)
        {
            var result = new Rom(isPlayStation);
            result.WeaponNames = EntityName.Parse(romData, result.RomOffsets.WeaponNames);
            result.UnitNames= EntityName.Parse(romData, result.RomOffsets.UnitNames);
            result.PilotNames = EntityName.Parse(romData, result.RomOffsets.PilotNames);
            Debug.Assert(result.WeaponNames.Count > 0);
            Debug.Assert(result.UnitNames.Count > 0);
            Debug.Assert(result.PilotNames.Count > 0);


            result.PilotCallSigns= EntityName.Parse(romData, result.RomOffsets.PilotCallSigns);
            result.Weapons = Weapon.Parse(romData, result.RomOffsets.Weapons, result.WeaponNames);
            if (result.Weapons == null) throw new ArgumentNullException("Rom.Weapons is null");
            result.Units = Unit.Parse(romData, result.RomOffsets.Units, unitMetaData, result.Weapons, result.UnitNames);
           
            
            result.Pilots = Pilot.Parse(romData, result.RomOffsets.Pilots, pilotMetaData, isPlayStation, result.PilotNames, result.PilotCallSigns);
            return result;
        }

        public static List<int> FindByteSequence(ReadOnlySpan<byte> haystack,
            int startOffset, byte[] needle)
        {
            List<int> result = new List<int>();
            // Validate inputs
            if (haystack == null || needle == null)
                throw new ArgumentNullException("Arrays cannot be null.");
            if (needle.Length == 0)
                throw new ArgumentException("Needle cannot be empty.");
            if (needle.Length > haystack.Length)
                return result;

            // Loop through haystack
            for (int i = startOffset; i <= haystack.Length - needle.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    result.Add(i); // Found match at index i
            }

            return result;
        }
        public static List<string> ParseNames(ReadOnlySpan<byte> romData,int startOffset, int length)
        {
            List<string> result = new List<string>();
            byte[] terminator = new byte[1];
            terminator[0] = 0xff;
            var terminatorLocations = FindByteSequence(romData, startOffset, terminator);
            int currentPosition = startOffset;
            int previousterminatorLocation = 0;
            foreach (var terminatorLocation in terminatorLocations)
            {
                if (previousterminatorLocation + 1 == terminatorLocation)
                {
                    if (romData[terminatorLocation + 1] == 0x11)
                        break;//double FF+11   
                    else
                    {
                        Debug.WriteLine(string.Format("{0:X}:", terminatorLocation));
                        result.Add(string.Empty);
                    }
                }
                else
                {

                    ReadOnlySpan<byte> encoded = romData.Slice(currentPosition, terminatorLocation - currentPosition);
                    try
                    {
                        var decoded = srw4Encoding.GetString(encoded);
                        if (decoded != null)
                        {
                            Debug.WriteLine(string.Format("{0:X}:{1}", currentPosition, decoded));
                            result.Add(decoded);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                currentPosition = terminatorLocation + 1;
                previousterminatorLocation = terminatorLocation;
            }
            return result;
        }

        public void WriteCsv()
        {
            var prefix = IsPlayStation ? "psx" : "snes";
            WriteCsv(string.Format("{0}Weapons.csv", prefix), Weapons);
            WriteCsv(string.Format("{0}Units.csv", prefix), Units);
            WriteCsv(string.Format("{0}Pilots.csv", prefix), Pilots);
            if (!IsPlayStation)
            {
                //write fixed metadata
                WriteCsv(string.Format("Franchise.csv"), Franchise.Franchises);
                var units = this.Units?.Select(u => new UnitMetaData
                {
                    Id = u.Id,
                    Affiliation = u.Affiliation,
                    FranchiseName = u.FranchiseName,
                    Name = u.Name,
                    EnglishName = u.EnglishName,
                    ChineseName = u.ChineseName,
                    PreferredPilotId = u.PreferredPilotId
                    , FirstAppearance = u.FirstAppearance
                }).ToList();
                WriteCsv("UnitMetaData.csv", units);
                var pilots = this.Pilots?.Select(p => new PilotMetaData
                {
                    Id = p.Id,
                    Name = p.Name,
                    FranchiseName = p.FranchiseName,
                    Affiliation = p.Affiliation,
                    EnglishName = p.EnglishName,
                    ChineseName = p.ChineseName,
                    FirstAppearance = p.FirstAppearance,                    
                }).ToList();
                WriteCsv("PilotMetaData.csv", pilots);
                var weapons = this.Weapons?.Select(p => new EntityName
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList();
                WriteCsv("WeaponMetaData.csv", weapons);
            }
        }
        private static void WriteCsv<T>(string fileName, List<T>? data)
        {
            if (data == null) return;
            using (var fileStream=new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }

        public void WriteRst()
        {
            var postfix = IsPlayStation ? "ps" : "snes";
            WriteRst(string.Format("unit_data_{0}.rst", postfix),
                IsPlayStation ? "https://jiangsheng.net/build/html/_sources/games/srw4/units/unit_data_ps.rst.txt"
                : "https://jiangsheng.net/build/html/_sources/games/srw4/units/unit_data_snes.rst.txt"
                , Units?.Where(u => !string.IsNullOrEmpty(u.Name))?.ToList()
                );
            WriteRst(string.Format("pilot_data_{0}.rst", postfix),
                IsPlayStation ? "https://jiangsheng.net/build/html/_sources/games/srw4/pilots/pilot_data_ps.rst.txt"
                : "https://jiangsheng.net/build/html/_sources/games/srw4/pilots/pilot_data_snes.rst.txt"
                , Pilots?.Where(p=>!string.IsNullOrEmpty(p.Name))?.ToList());
            WriteRst(string.Format("weapon_data_{0}.rst", postfix),
                 IsPlayStation ? "https://jiangsheng.net/build/html/_sources/games/srw4/units/weapon_data_ps.rst.txt"
                : "https://jiangsheng.net/build/html/_sources/games/srw4/units/weapon_data_snes.rst.txt"
                , Weapons?.Where(w=>!string.IsNullOrEmpty(w.Name))?.ToList());
        }

        void WriteRst<T>(string fileName, string rstUrl, List<T>? data) where T : IRstFormatter
        {
            if (data == null) return;
            using (WebClient webClient = new WebClient())
            {
                var fileTemplate = webClient.DownloadString(rstUrl);
                var lines = fileTemplate.Split(new[] { '\n' });
                int bodyLine = -1;
                int lastRawHtmlLine = -1;
                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.Compare(line, "   * - 01", StringComparison.Ordinal) == 0)
                    {
                        bodyLine = i;
                    }
                    if (string.Compare(line, ".. raw:: html", StringComparison.Ordinal) == 0)
                    {
                        lastRawHtmlLine = i;
                    }
                }
                var stringBuilder = new StringBuilder();
                for (int i = 0; i < bodyLine; i++)
                {
                    stringBuilder.AppendLine(lines[i]);
                }
                foreach (var item in data)
                {
                    if (data != null)
                    {
                        stringBuilder.AppendLine(item.ToRstRow(this.IsPlayStation));
                    }
                }
                if (lastRawHtmlLine != -1)
                {
                    for (int i = lastRawHtmlLine; i < lines.Length; i++)
                    {
                        if (i < lines.Length - 1)
                        {
                            stringBuilder.AppendLine(lines[i]);
                        }
                        else
                        {
                            stringBuilder.Append(lines[i]);
                        }
                    }
                }
                File.WriteAllText(fileName, stringBuilder.ToString(), Encoding.UTF8);
            }
        }

        public static string FormatValue<T>(T snesValue, T playstationValue)
        {
            if (EqualityComparer<T>.Default.Equals(snesValue, playstationValue))
            {
                return snesValue?.ToString() ?? string.Empty;
            }
            else
            {
                return string.Format("{0} ({1})", snesValue?.ToString() ?? string.Empty, playstationValue?.ToString() ?? string.Empty);

            }
        }
    }
}
