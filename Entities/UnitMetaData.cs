using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitMetaData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Affiliation { get; set; }
        public string? FranchiseName { get; set; }
        public string? EnglishName { get; set; }
        [Optional]
        public int? PreferredPilotId { get; set; }
    }
}
