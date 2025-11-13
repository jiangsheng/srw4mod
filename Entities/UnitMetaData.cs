using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitMetaData: INamedItem
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Affiliation { get; set; }
        public string? FranchiseName { get; set; }
        public string? EnglishName { get; set; }
        [Optional]
        public int PreferredPilotId { get; set; }
        [Optional]
        public int FirstAppearance { get; set; }
        public int GetFirstAppearanceOrder()
        {
            if (FirstAppearance == 0)
                return 255;
            return FirstAppearance;
        }
    }
}
