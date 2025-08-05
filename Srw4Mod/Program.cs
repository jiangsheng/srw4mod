using Config.Net;
using CsvHelper;
using Entities;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;

namespace Srw4Mod
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var units=DownloadUnitData();
            var pilots=DownloadPilotData();
            var weapons = DownloadWeaponData();
            var appdataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingFilePath = Path.Combine(appdataPath, "srw4mod");
            if (!Directory.Exists(settingFilePath)) {
                Directory.CreateDirectory(settingFilePath);
            }
            settingFilePath = Path.Combine(settingFilePath, "config.json");
            ISettings settings = new ConfigurationBuilder<ISettings>().UseJsonFile(settingFilePath).Build();
            settings.PlayStationDataLocation = PromptForPath(string.Format("Please enter location for data folder from srw4s cd, enter for default ({0})", settings.PlayStationDataLocation), settings.PlayStationDataLocation);
            if (!Directory.Exists(settings.PlayStationDataLocation))
            {
                ReportError(string.Format("Specified PlayStationDataLocation location {0} is invalid", settings.PlayStationDataLocation));
                return;
            }
            string playStationUnitDataPath = Path.Combine(settings.PlayStationDataLocation, "STAYDAT.BIN");
            if (!File.Exists(playStationUnitDataPath))
            {
                ReportError(string.Format("Specified folder does not have a file named STAYDAT.BIN"));
                return;
            }
            string playStationPilotDataPath = Path.Combine(settings.PlayStationDataLocation, "D_PILOT.BIN");
            if (!File.Exists(playStationPilotDataPath))
            {
                ReportError(string.Format("Specified folder does not have a file named D_PILOT.BIN"));
                return;
            }
            string playStationWeaponDataPath = Path.Combine(settings.PlayStationDataLocation, "STAYDAT.BIN");
            if (!File.Exists(playStationWeaponDataPath))
            {
                ReportError(string.Format("Specified folder does not have a file named STAYDAT.BIN"));
                return;
            }
            byte[] playStationUnitData=File.ReadAllBytes(playStationUnitDataPath);
            byte[] playStationPilotData = File.ReadAllBytes(playStationPilotDataPath);
            byte[] playStationWeaponData=File.ReadAllBytes(playStationWeaponDataPath);

            Rom playstationRom = new Rom();
            playstationRom.Weapons=Weapon.Parse(playStationWeaponData, 0x2E800, weapons);
            using (var writer = new StreamWriter("weapons.csv", false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(playstationRom.Weapons);
            }
            playstationRom.Units = Unit.Parse(playStationUnitData,0x26000, units, playstationRom.Weapons);

            units = playstationRom.Units.OrderBy(u => u.Id).ToList();

            using (var writer = new StreamWriter("units.csv", false,Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(units);
            }
        }

        private static List<Weapon> DownloadWeaponData()
        {
            if (!File.Exists("weapons.csv"))
            {
                List<Weapon> weapons= new List<Weapon>();
                string Url = "https://wikiwiki.jp/snes007/%E7%AC%AC4%E6%AC%A1%E3%82%B9%E3%83%BC%E3%83%91%E3%83%BC%E3%83%AD%E3%83%9C%E3%83%83%E3%83%88%E5%A4%A7%E6%88%A6%E3%80%80%E5%90%84%E7%A8%AE%E3%83%AA%E3%82%B9%E3%83%88";
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(Url);
                var weaponTableLead = doc.DocumentNode.SelectSingleNode("//td[contains(text(), '7E0EACff')]");
                if (weaponTableLead != null)
                {
                    var nextDiv = weaponTableLead.ParentNode.ParentNode.ParentNode.ParentNode.NextSibling;
                    while (nextDiv.Name != "div")
                    {
                        nextDiv = nextDiv.NextSibling;
                    }
                    var node = nextDiv.SelectSingleNode("table");
                    if (node!=null)
                    {
                        var rows = node.SelectNodes("tbody/tr");
                        if (rows != null)
                        {
                            foreach (var row in rows)
                            {
                                var cells = row.SelectNodes("td");
                                foreach (var cell in cells)
                                {
                                    var cellText = cell.InnerText;
                                    if (cellText.Contains("="))
                                    { 
                                        var index=cellText.IndexOf('=');
                                        var weaponCode = cellText.Substring(0, index);
                                        int? weaponIndex = ParseHex(weaponCode);
                                        if (weaponIndex.HasValue)
                                        {
                                            string? weaponName;
                                            switch(weaponIndex.Value)
                                            {
                                                case 0x0203:
                                                    weaponName = "ハンドビーム"; break;
                                                case 0x0204:
                                                    weaponName = "ビームランチャー";break;                                                
                                                default:
                                                    weaponName = WebUtility.HtmlDecode(cellText.Substring(index + 1));                                                   
                                                    break;
                                            }
                                            weapons.Add(new Weapon
                                            {
                                                Id = (ushort)weaponIndex.Value,
                                                Name = WebUtility.HtmlDecode(weaponName)
                                            });

                                        }

                                    }
                                }
                            }
                        }

                    }
                }
                using (var writer = new StreamWriter("weapons.csv", false, Encoding.UTF8))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(weapons);
                }
                return weapons;
            }
            else
            {
                using (var reader = new StreamReader("weapons.csv", Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var weapons = csv.GetRecords<Weapon>();
                    return weapons.ToList();
                }
            }
        }

        private static int? ParseHex(string hexCode)
        {
            int result = 0;
            if (int.TryParse(hexCode, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out result))
            {
                return result;
            }
            return null;
        }

        private static List<Pilot> DownloadPilotData()
        {
            if (!File.Exists("pilots.csv"))
            {
                List<Pilot> pilots = new List<Pilot>();
                string Url = "https://jiangsheng.net/build/html/games/srw4/pilots/pilot_data_snes.html";
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(Url);
                var node = doc.DocumentNode.SelectSingleNode("//table[@id='srw4-pilots-snes-table']");
                var rows = node.SelectNodes("tbody/tr");
                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count > 4)
                        {
                            var pilot = new Pilot();
                            int? pilotId = ParseHex(cells[0].InnerText);
                            if(pilotId.HasValue)
                            {
                                pilot.Id = pilotId.Value;
                                pilot.Affiliation = WebUtility.HtmlDecode(cells[1].InnerText);
                                pilot.Franchise = WebUtility.HtmlDecode(cells[4].InnerText);
                                pilot.Name = WebUtility.HtmlDecode(cells[3].InnerText);
                                pilots.Add(pilot);
                            }
                        }
                    }
                    using (var writer = new StreamWriter("pilots.csv", false, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(pilots);
                    }
                }
                return pilots;
            }
            else {
                using (var reader = new StreamReader("pilots.csv", Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var pilots = csv.GetRecords<Pilot>();
                    return pilots.ToList();
                }
            }
        }

        private static List<Unit> DownloadUnitData()
        {
            if (!File.Exists("units.csv"))
            {
                List<Unit> units = new List<Unit>();

                string Url = "https://jiangsheng.net/build/html/games/srw4/units/unit_data_snes.html";
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(Url);
                var node = doc.DocumentNode.SelectSingleNode("//table[@id='srw4-units-snes-table']");
                var rows = node.SelectNodes("tbody/tr");
                if (rows != null)
                {

                    foreach (var row in rows)
                    {
                        var cells = row.SelectNodes("td");
                        if (cells != null && cells.Count > 4)
                        {
                            var unit = new Unit();
                            int? unitId = ParseHex(cells[0].InnerText);
                            if (unitId.HasValue)
                            {
                                unit.Id = unitId.Value;
                                unit.Affiliation = WebUtility.HtmlDecode(cells[1].InnerText);
                                unit.Franchise = WebUtility.HtmlDecode(cells[4].InnerText);
                                unit.Name = WebUtility.HtmlDecode(cells[3].InnerText);
                                units.Add(unit);
                            }
                        }
                    }
                    using (var writer = new StreamWriter("units.csv", false, Encoding.UTF8))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(units);
                    }
                }
                return units;
            }
            else
            {
                using (var reader = new StreamReader("units.csv", Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var units=csv.GetRecords<Unit>(); 
                    return units.ToList();
                }
            }
            
        }

        private static void ReportError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Debug.WriteLine(errorMessage);
        }

        private static string PromptForPath(string prompt, string defaultValue)
        {
            Console.WriteLine(prompt);
            var result=Console.ReadLine();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            return defaultValue;
        }
    }
}
