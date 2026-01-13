using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class RomOffsets
    {
        public RomOffsets(bool isPlayStation)
        {
            IsPlayStation = isPlayStation;
            if (IsPlayStation)
            {
                Weapons = new IndexTable(0x2E800,0x2ED20,0x315a0- 0x2ED20, 0x2E800);
                WeaponNames = new IndexTable(0x528e4, 0x52e04, 0x5400C - 0x52e04, 0x49800);
                Units = new IndexTable(0x26000, 0x26260, 0x29640 - 0x26260, 0x26000);
                UnitNames = new IndexTable(0x50a30, 0x50c90, 0x514ea - 0x50c90, 0x49800);
                Pilots = new IndexTable(0x2a800, 0x2ab00,0x2ca24-0x2ab00, 0x2a800);
                
                PilotNames = new IndexTable(0x51514, 0x51794, 0x51FC8 - 0x51794, 0x49800);
                PilotCallSigns = new IndexTable(0x5210c, 0x5238c, 0x527F5 - 0x5238c, 0x49800);
            }
            else {
                Weapons = new IndexTable(0xbc950, 0xbce70, 0xBF6F0 - 0xbce70,0xb0000);
                WeaponNames = new IndexTable(0xc7760, 0xc7c80, 0xc8e88 - 0xc7c80, 0xc0000);
                Units = new IndexTable(0xb9311, 0xb9571, 0xBC92E - 0xb9571, 0xb0000);                
                UnitNames=new IndexTable(0x126050, 0x1262B0, 0x126b0a- 0x1262B0, 0x120000);
                Pilots = new IndexTable(0xb7012, 0xB7312, 0xb8f8b - 0xB7312, 0xb0000);
                PilotNames = new IndexTable(0x126b34,0x126db4,0x1275e7- 0x126db4, 0x120000);
                PilotCallSigns = new IndexTable(0x12772b,0x1279ab,0x127e14- 0x1279ab, 0x120000);
            }
        }

        public bool IsPlayStation { get; }
        public IndexTable Weapons { get; set; }
        public IndexTable WeaponNames { get; set; }
        public IndexTable Units { get; set; }
        public IndexTable UnitNames { get; set; }

        public IndexTable Pilots{ get; set; }

        public IndexTable PilotNames { get; set; }
        public IndexTable PilotCallSigns{ get; set; }
    }
}
