using Config.Net;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Srw4Mod
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var units = DownloadUnitData();
            var pilots = DownloadPilotData();
            var appdataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingFilePath = Path.Combine(appdataPath, "srw4mod");
            if (!Directory.Exists(settingFilePath))
            {
                Directory.CreateDirectory(settingFilePath);
            }
            settingFilePath = Path.Combine(settingFilePath, "config.json");
            ISettings settings = new ConfigurationBuilder<ISettings>().UseJsonFile(settingFilePath).Build();
            settings.PlayStationDataPath = PromptForPath(string.Format("Please enter location of the data folder from srw4s cd, enter for default ({0})", settings.PlayStationDataPath), settings.PlayStationDataPath);

            if (!Directory.Exists(settings.PlayStationDataPath))
            {
                ReportError(string.Format("Specified location {0} does not exist", settings.PlayStationDataPath));
                return;
            }
            if (string.IsNullOrWhiteSpace(settings.PlayStationDataFileName))
            {
                settings.PlayStationDataFileName = "STAYDAT.BIN";
            }
            settings.PlayStationDataFileName = PromptForPath(string.Format("Please enter file name of the data file in the data folder, enter for default ({0})",settings.PlayStationDataFileName), settings.PlayStationDataFileName);
            string playStationDataPath = Path.Combine(settings.PlayStationDataPath, settings.PlayStationDataFileName);
            if (!File.Exists(playStationDataPath))
            {
                ReportError(string.Format("Specified file name {0} is invalid", playStationDataPath));
                return;
            }

            settings.SnesDataPath = PromptForPath(string.Format("Please enter path for srw4 rom, enter for default ({0})", settings.SnesDataPath), settings.SnesDataPath);
            if (!Directory.Exists(settings.SnesDataPath))
            {
                ReportError(string.Format("Specified location {0} does not exist", settings.SnesDataPath));
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.SnesDataFileName))
            {
                settings.SnesDataFileName = "Dai 4 Ji Super Robot Taisen (V1.1) (J).smc";
            }
            settings.SnesDataFileName = PromptForPath(string.Format("Please enter file name of the snes ROM, enter for default ({0})",
               settings.SnesDataFileName), settings.SnesDataFileName);

            string snesDataPath = Path.Combine(settings.SnesDataPath, settings.SnesDataFileName);
            if (!File.Exists(snesDataPath))
            {
                ReportError(string.Format("Specified file does not have exist :{0}", snesDataPath));
                return;
            }

            var snesData = new ReadOnlySpan<byte>(File.ReadAllBytes(snesDataPath));
            var playStationData = new ReadOnlySpan<byte>(File.ReadAllBytes(playStationDataPath));
            //TestEncoding(snesData,playStationData);

           
            Franchise.Franchises = LoadFranchises();

            var comments = DownloadComments(Franchise.Franchises);

            Rom playstationRom = Rom.Parse(playStationData,  units, pilots,true);
            Rom snesRom = Rom.Parse(snesData, units, pilots, false);
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
            if (!Directory.Exists(unitsFolder))
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

            var snes9xCheatFileName = "snes9x.cht";
            var bsnesCheatFileName = "bsnes.cht";
            var duckstationCheatFileName = "duckstation.cht";
            if (!File.Exists(snes9xCheatFileName)|| File.Exists(bsnesCheatFileName) || !File.Exists(duckstationCheatFileName))
            {
            
                WriterCheats(snes9xCheatFileName, bsnesCheatFileName,duckstationCheatFileName);
            }
            
            //DumpData(snesRom.Pilots);
            //DumpData(snesRom.Units);
            //DumpData(snesRom.Weapons);
            //DumpData(playstationRom.Pilots);
            //DumpData(playstationRom.Units);
            //DumpData(playstationRom.Weapons);
           
        }

        private static void WriterCheats(string snes9xCheatFileName, string bsnesCheatFileName, string duckstationCheatFileName)
        {
            CheatFile cheatFile = CheatFile.CreateFromUrl("https://jiangsheng.net/build/html/games/srw4/mechanics/cheat");
            cheatFile.WriteToFile(snes9xCheatFileName, bsnesCheatFileName, duckstationCheatFileName);
            
        }


        private static void TestEncoding(ReadOnlySpan<byte> snesData, ReadOnlySpan<byte> playStationData)
        {
            var names = new List<string>();
            names.Add("毛林");
            names.Add("曹　操　猛徳");
            names.Add("司馬　意　仲達");
            names.Add("江　維　伯約");
            names.Add("孫　権　仲謀");
            names.Add("猿　紹　本初");
            names.Add("尉　延　文長");
            names.Add("劉　備　旋徳");
            names.Add("張　飛　翼徳");
            names.Add("関　羽　雲長");
            names.Add("許　楚　仲康");
            names.Add("太史　磁　子義");
            names.Add("孫　策　伯付");
            names.Add("馬　超　猛起");
            names.Add("文　醜");
            names.Add("顔　良");
            names.Add("黄　忠　漢省"); ;
            names.Add("周　太　幼平");
            names.Add("曹　障　子文");
            names.Add("法　正　肖直");
            names.Add("諸葛　亮　孔明");
            names.Add("程　立　仲徳");
            names.Add("曹　植　子建");
            names.Add("張　招　子布");
            names.Add("陸　抗　幼節");
            names.Add("馬　良　祭常");
            names.Add("張　角");
            names.Add("歩練師");
            names.Add("黄月英");
            names.Add("曹憲");
            names.Add("曹節");            

            foreach (var name in names)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(name);
                sb.Append(": ");
                foreach (var nameChar in name)
                {
                    var charBytes = Rom.srw4Encoding.GetBytes(nameChar.ToString());
                    var hexNameBytes = BitConverter.ToString(charBytes).Replace("-", string.Empty);
                    if (charBytes.Length == 2)
                    {
                        if (charBytes[0] == 0xFE && charBytes[1] == 0xFF)
                        {
                            var homophones = Rom.srw4Encoding.FindHomophones(nameChar);
                            sb.Append("{");
                            foreach (var homophon in homophones)
                            {
                                sb.Append(homophon);
                            }
                            sb.Append("}");
                        }
                        else
                        {
                            sb.Append(hexNameBytes);
                        }
                    }
                    else if (charBytes.Length == 1)
                    {
                        if (charBytes[0] == 0)
                            sb.Append("\t");
                        else
                            sb.Append(hexNameBytes);
                    }
                }
                Debug.WriteLine(sb.ToString());
            }
            SRW4Encoding srw4Encoding = new SRW4Encoding();

            var bytes = srw4Encoding.GetBytes("グレ－ス");


            var findByteSequence = Rom.FindByteSequence(playStationData, 0, bytes);
            foreach (var item in findByteSequence)
            {
                Debug.WriteLine(string.Format("Find Name at {0:x}", item));
                var decoded = Rom.ParseNames(playStationData, item, 20000);
                foreach (var item1 in decoded)
                {
                    Debug.WriteLine(item1);
                }
            }

            var address = 0x54a27;//0x54594
            Debug.WriteLine(string.Format("Find Name at {0:x}", address));
            var decoded1 = Rom.ParseNames(playStationData, address, 20000);
            foreach (var item1 in decoded1)
            {
                Debug.WriteLine(item1);
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(" ", string.Empty);
            return Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();
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
            using (var fileStream = new FileStream("PilotMetaData.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
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

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);
            csvConfiguration.MissingFieldFound = null;
            using(var fileStream=new FileStream("UnitMetaData.csv",FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
            using (var reader = new StreamReader(fileStream, Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfiguration))
            {
                var units = csv.GetRecords<UnitMetaData>();
                return units.ToList();
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
