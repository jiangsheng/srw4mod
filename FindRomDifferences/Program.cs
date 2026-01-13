using Config.Net;
using System.Diagnostics;
using System.Text;

namespace FindRomDifferences
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var appdataPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var settingFilePath = Path.Combine(appdataPath, "srw4mod"); 
            if (!Directory.Exists(settingFilePath))
            {
                Directory.CreateDirectory(settingFilePath);
            }
            settingFilePath = Path.Combine(settingFilePath, "config.json");
            ISettings settings = new ConfigurationBuilder<ISettings>().UseJsonFile(settingFilePath).Build();

            settings.FirstRomFile = PromptForText(string.Format("Please enter location of the first Rom file, enter for default ({0})", settings.FirstRomFile), settings.FirstRomFile);

            if (!File.Exists(settings.FirstRomFile))
            {
                ReportError(string.Format("Specified file {0} does not exist", settings.FirstRomFile));
                return;
            }
            settings.SecondRomFile = PromptForText(string.Format("Please enter location of the second Rom file, enter for default ({0})", settings.SecondRomFile), settings.SecondRomFile);

            if (!File.Exists(settings.SecondRomFile))
            {
                ReportError(string.Format("Specified file {0} does not exist", settings.SecondRomFile));
                return;
            }
            settings.HexValueInFirstRom= PromptForText(string.Format("Please enter hex data to search in the first Rom file, enter for default ({0})", settings.HexValueInFirstRom), settings.HexValueInFirstRom);
            var dataToSearchInFirstRom = ParseHexString(settings.HexValueInFirstRom);
            if (dataToSearchInFirstRom==null|| dataToSearchInFirstRom.Length==0)
             {
                ReportError(string.Format("Specified text {0} is not a hex string", settings.HexValueInFirstRom));
                return;
            }
            settings.HexValueInSecondRom = PromptForText(string.Format("Please enter hex data to search in the second Rom file, enter for default ({0})", settings.HexValueInSecondRom), settings.HexValueInSecondRom);
            var dataToSearchInSecondRom = ParseHexString(settings.HexValueInSecondRom);
            if (dataToSearchInSecondRom==null|| dataToSearchInSecondRom.Length==0)
            {
                ReportError(string.Format("Specified text {0} is not a hex string", settings.HexValueInSecondRom));
                return;
            }

            var firstRomData=File.ReadAllBytes(settings.FirstRomFile);
            var locationsOfInterest = SearchBytes(firstRomData, dataToSearchInFirstRom);
            var secoondRomData = File.ReadAllBytes(settings.SecondRomFile);
            List<int> matchedLocations=new List<int>();
            foreach (
                var location in locationsOfInterest)
            {
                Debug.Assert(firstRomData[location] == dataToSearchInFirstRom[0]);
                if (location > secoondRomData.Length)
                {
                    ReportError(string.Format("location {0:X} does not exist in second file"));
                    break;
                }
                var k = 0;
                var matchResult = new StringBuilder();                
                for (; k < dataToSearchInSecondRom.Length; k++)
                {
                    if (dataToSearchInSecondRom[k] != secoondRomData[location + k]) 
                        break;
                }
                if (k == dataToSearchInSecondRom.Length)
                {
                    ReportError(string.Format("found match at location {0:X}", location));
                    matchedLocations.Add(location);
                }
                else
                { 
                    var foundBytes=new ReadOnlySpan<byte>(secoondRomData,location,dataToSearchInFirstRom.Length);
                    var expectedBytes = new ReadOnlySpan<byte>(dataToSearchInSecondRom);
                    //ReportError(string.Format("Expected {0} at location {1:X}, found {2}", Convert.ToHexString(expectedBytes), location,Convert.ToHexString(foundBytes)));
                }
            }
            var result = new StringBuilder();
            if (matchedLocations.Count == 0)
            {
                ReportError("No match found between the two files");
                return;
            }
            foreach (var item in matchedLocations)
            {
                ReportError(string.Format("match found at {0:x}",item));
            }
        }

        private static byte[]? ParseHexString(string hexString)
        {
            try
            {
                byte[] byteArray = Convert.FromHexString(hexString);
                // byteArray will contain { 10, 1, 2, 72 } (decimal representation)
                // or { 0x0A, 0x01, 0x02, 0x48 } (hexadecimal representation)
                return byteArray;
            }
            catch (FormatException e)
            {
                Console.WriteLine($"Invalid hex string: {e.Message}");
                return null;
            }
        }

        private static void ReportError(string errorMessage)
        {
            Console.WriteLine(errorMessage);
            Debug.WriteLine(errorMessage);
        }

        private static string PromptForText(string prompt, string defaultValue)
        {
            Console.WriteLine(prompt);
            var result = Console.ReadLine();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            return defaultValue;
        }
        static List<int> SearchBytes(byte[] haystack, byte[] needle)
        {
            List<int> result=new List<int>();
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len)
                {
                    result.Add(i);
                }
            }
            return result;
        }
    }
}
