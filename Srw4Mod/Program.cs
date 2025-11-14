using Config.Net;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

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
            settings.SnesDataLocation = PromptForPath(string.Format("Please enter location for srw4 rom, enter for default ({0})", settings.SnesDataLocation), settings.SnesDataLocation);
            if (!Directory.Exists(settings.SnesDataLocation))
            {
                ReportError(string.Format("Specified SnesDataLocation location {0} is invalid", settings.SnesDataLocation));
                return;
            }
            string playStationDataPath = Path.Combine(settings.PlayStationDataLocation, "STAYDAT.BIN");
            if (!File.Exists(playStationDataPath))
            {
                ReportError(string.Format("Specified folder does not have a file named STAYDAT.BIN"));
                return;
            }

            string snesDataPath = Path.Combine(settings.SnesDataLocation, "Dai 4 Ji Super Robot Taisen (V1.1) (J).smc");
            if (!File.Exists(snesDataPath))
            {
                ReportError(string.Format("Specified folder does not have a file named Dai 4 Ji Super Robot Taisen (V1.1) (J).smc"));
                return;
            }
            byte[] snesData = File.ReadAllBytes(snesDataPath);
            byte[] playStationData=File.ReadAllBytes(playStationDataPath);
            Franchise.Franchises= LoadFranchises();

            var comments = DownloadComments(Franchise.Franchises);

            Rom playstationRom = Rom.Parse(playStationData, weapons, units, pilots, 
                0x2E800, 0x2E800, 0x2D90
                , 0x26000, 0x26000, 0x361A
                , 0x2a800, 0x2a800, true
                );
            Rom snesRom = Rom.Parse(snesData, weapons, units, pilots,
                0xbc950, 0xb0000, 0xf6f0
                , 0xb9311, 0xb0000, 0xc92e
                ,0xb7012, 0xb0000, false
                );
            playstationRom.WriteCsv();
            snesRom.WriteCsv();
            playstationRom.WriteRst();
            snesRom.WriteRst();
            var snesRomUnits = snesRom.Units;
            var playstationRomUnits = playstationRom.Units;
            if (snesRomUnits == null)
            {
                throw new ArgumentNullException(nameof(snesRomUnits));
            }
            if (playstationRomUnits == null)
            {
                throw new ArgumentNullException(nameof(playstationRomUnits));
            }

            Debug.Assert(snesRomUnits.Where(u => u.Id == 1).First().PreferredPilotId == 250);
            Debug.Assert(playstationRomUnits.Where(u => u.Id == 1).First().PreferredPilotId == 250);
            var unitsFolder = Path.Combine(Environment.CurrentDirectory, "units");
            var pilotsFolder = Path.Combine(Environment.CurrentDirectory, "pilots");
            if(!Directory.Exists(unitsFolder))
            {
                Directory.CreateDirectory(unitsFolder);
            }
            if (!Directory.Exists(pilotsFolder))
            {
                Directory.CreateDirectory(pilotsFolder);
            }

            Franchise.WriteUnitsRst(unitsFolder, units,
                    snesRom, playstationRom, comments);

            Franchise.WritePilotRst(pilotsFolder, pilots, snesRom, playstationRom, comments);

           
            DumpData(playstationRom.Pilots);
            //DumpData(playstationRom.Units);
            //DumpData(playstationRom.Weapons);
        }


        private static Dictionary<string, string> DownloadComments(List<Franchise> franchises)
        {
            
            Dictionary<string, string> result = new Dictionary<string,string>();
            if (File.Exists("comments.csv"))
            {
                using (var reader = new StreamReader("comments.csv", Encoding.UTF8))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var rstCommentsFromCsv = csv.GetRecords<RstComments>();
                    foreach (var rstComment in rstCommentsFromCsv)
                    {
                        if(rstComment.Label!=null && rstComment.Content!=null)
                            result.Add(rstComment.Label, rstComment.Content);
                    }
                    if (result.Keys.Count > 0)
                    {
                        return result;
                    }
                }
            }
            
            var baseUrl = "https://jiangsheng.net/build/html/_sources/games/srw4/units/";
            var franchiseNames= franchises.Select(x => x.FileName).Distinct().ToList();
            DownloadComments(result, baseUrl, franchiseNames);
            baseUrl = "https://jiangsheng.net/build/html/_sources/games/srw4/pilots/";
            franchiseNames = franchises.Where(f => f.FileName != "mobile_suit_gundam_sentinel"
            && f.FileName != "mobile_suit_gundam_hathaway").Select(x => x.FileName).Distinct().ToList();
            DownloadComments(result, baseUrl, franchiseNames);
            List<RstComments> rstComments = new List<RstComments>();
            foreach (var label in result.Keys)
            {
                rstComments.Add(new RstComments
                {
                    Label = label,
                    Content = result[label]
                });
            }
            using (var writer = new StreamWriter("comments.csv", false, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rstComments);
            }
            return result;
        }

        private static void DownloadComments(Dictionary<string, string> result, string baseUrl, List<string> franchiseLabels)
        {
            foreach (var franchiseLabel in franchiseLabels)
            {
                DownloadCommentsFromUrl(result, string.Format("{0}{1}.rst.txt", baseUrl, franchiseLabel));
            }
        }

        private static void DownloadCommentsFromUrl(Dictionary<string, string> result, string address)
        {
            Debug.WriteLine("Downloading " + address);
            using (WebClient webClient = new WebClient())
            {
                var text = webClient.DownloadString(address);
                string[] lines = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                string currentLabel = string.Empty;
                List<string> currentComment = new List<string>();
                bool inComment = false;
                foreach (var line in lines)
                {
                    if (line.StartsWith(".. _srw4_"))
                    {
                        if (line.EndsWith("_commentBegin:"))
                        {
                            inComment = true;
                            currentComment.Clear();
                            currentLabel = line.Substring(3, line.Length - 17);
                            continue;
                        }
                        if (line.EndsWith("_commentEnd:"))
                        {
                            var commentContent = string.Join("<BR>", currentComment);
                            result.Add(currentLabel, commentContent);
                            currentLabel = string.Empty;
                            inComment = false;
                            continue;
                        }
                    }
                    if (inComment)
                    {
                        currentComment.Add(line);
                    }
                }
            }
        }

        private static List<Franchise> LoadFranchises()
        {
            using (var reader = new StreamReader("Franchise.csv", Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var pilots = csv.GetRecords<Franchise>();
                return pilots.ToList();
            }
        }
        private static void DumpData<T>(List<T>? data)
        {
            if(data!= null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    if(item!=null)
                        Debug.WriteLine(item.ToString());
                }
            }
        }

        private static List<WeaponMetaData> DownloadWeaponData()
        {
            /*
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
            }*/
            using (var reader = new StreamReader("WeaponMetaData.csv", Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var weapons = csv.GetRecords<WeaponMetaData>();
                return weapons.ToList();
            }
        }

        private static List<PilotMetaData> DownloadPilotData()
        {
            /*
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
                                pilot.FranchiseName = WebUtility.HtmlDecode(cells[4].InnerText);
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
            */
            using (var reader = new StreamReader("PilotMetaData.csv", Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var pilots = csv.GetRecords<PilotMetaData>();
                return pilots.ToList();
            }
        }

        private static List<UnitMetaData> DownloadUnitData()
        {
            /*
          
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
                                unit.FranchiseName = WebUtility.HtmlDecode(cells[4].InnerText);
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
            }*/

            using (var reader = new StreamReader("UnitMetaData.csv", Encoding.UTF8))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var pilots = csv.GetRecords<UnitMetaData>();
                return pilots.ToList();
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
