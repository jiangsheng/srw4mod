using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Srw4Mod
{
    public interface ISettings
    {
        //location for data file from srw4s cd
        string PlayStationDataLocation { get; set; }
        string SnesDataLocation { get; set; }
        
    }
}
