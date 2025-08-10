using CsvHelper;
using System;
using System.Collections.Generic;
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
        public List<Weapon>? Weapons { get; set; }
        bool IsPlayStation { get; set; }
        public static Rom Parse(byte[] romData, List<Weapon> weapons, List<Unit> units, List<Pilot> pilots,
            int weaponHeaderStartOffset, int weaponOffsetBase, int weaponFooterOffset, int unitHeaderStartOffset, int unitOffsetBase, int unitFooterOffset, int pilotHeaderOffset, int pilotDataOffset, bool isPlayStation)
        {
            var result=new Rom();
            result.IsPlayStation = isPlayStation;
            result.Weapons = Weapon.Parse(romData, weaponHeaderStartOffset, weaponOffsetBase, weaponFooterOffset, weapons);
            result.Units = Unit.Parse(romData, unitHeaderStartOffset, unitOffsetBase, unitFooterOffset,units, result.Weapons);
            result.Pilots = Pilot.Parse(romData, pilotHeaderOffset, pilotDataOffset,  pilots, isPlayStation);
            return result;
        }

        public void WriteCsv()
        {
            var prefix= IsPlayStation ? "psx" : "snes";
            WriteCsv(string.Format("{0}Weapons.csv", prefix), Weapons);
            WriteCsv(string.Format("{0}Units.csv", prefix), Units);
            WriteCsv(string.Format("{0}Pilots.csv", prefix), Pilots);
        }
        private static void WriteCsv<T>(string fileName, List<T>? data)
        {
            if (data == null) return;
            using (var writer = new StreamWriter(fileName, false, Encoding.UTF8))
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
                ,Units
                );
            WriteRst(string.Format("pilot_data_{0}.rst", postfix),
                IsPlayStation ? "https://jiangsheng.net/build/html/_sources/games/srw4/pilots/pilot_data_ps.rst.txt"
                : "https://jiangsheng.net/build/html/_sources/games/srw4/pilots/pilot_data_snes.rst.txt"
                , Pilots);
            WriteRst(string.Format("weapon_data_{0}.rst", postfix),
                 IsPlayStation ? "https://jiangsheng.net/build/html/_sources/games/srw4/units/weapon_data_ps.rst.txt"
                : "https://jiangsheng.net/build/html/_sources/games/srw4/units/weapon_data_snes.rst.txt"
                , Weapons);
        }
        void WriteRst<T>(string fileName, string rstUrl, List<T> data) where T : IRstFormatter
        {
            using (WebClient webClient = new WebClient())
            {
                var fileTemplate = webClient.DownloadString(rstUrl);
                var lines = fileTemplate.Split(new[] { '\n' });
                var header = string.Empty;
                var footer = string.Empty;
                int bodyLine = -1;
                int lastRawHtmlLine = -1;
                for (int i= 0; i<lines.Length; i++ )
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
                for (int i= 0; i<bodyLine; i++ )
                {
                    stringBuilder.AppendLine(lines[i]);
                }
                foreach (var item in data)
                {
                    stringBuilder.AppendLine(item.ToRstRow(this.IsPlayStation));
                }
                if (lastRawHtmlLine != -1)
                {
                    for(int i= lastRawHtmlLine; i< lines.Length; i++ )
                    {
                        stringBuilder.AppendLine(lines[i]);
                    }
                }
                File.WriteAllText(fileName, stringBuilder.ToString(), Encoding.UTF8);
            }
        }
    }
}
