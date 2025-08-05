using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Rom
    {
        public List<Pilot>? Pilots { get; set; }
        public List<Unit>? Units { get; set; }    
        public List<Weapon>? Weapons { get; set; }   
        
    }
}
