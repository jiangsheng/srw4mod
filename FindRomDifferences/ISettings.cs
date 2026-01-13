using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindRomDifferences
{
    public interface ISettings
    {
        public string FirstRomFile { get; set; }
        public string SecondRomFile { get; set; }

        public string HexValueInFirstRom { get; set; }
        public string HexValueInSecondRom { get; set; }
    }
}
