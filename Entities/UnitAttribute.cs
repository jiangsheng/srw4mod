using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitAttribute
    {
        public int Address { get; set; }
        public string? Name { get; set; }
        public string? Owner{ get; set; }
        public bool? BoolValue { get; set; }
        public int? IntValue { get; set; }
        public string? StringValue { get; set; }

    }
}
