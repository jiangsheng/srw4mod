using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Affiliation
    {
        static Affiliation()
        {
            Affiliations = new List<Affiliation>();
            Affiliations.Add(new Affiliation { Name = "我军", ShortName = "自", Label = "own" });
            Affiliations.Add(new Affiliation { Name = "敌军", ShortName = "敌", Label = "enemy" });
            Affiliations.Add(new Affiliation { Name = "盟军", ShortName = "盟", Label = "ally" });
            Affiliations.Add(new Affiliation { Name = "中立", ShortName = "中", Label = "neutral" });
        }

        public string? Name { get; set; }

        public string? ShortName { get; set; }

        public string? Label { get; set; }

        public static List<Affiliation>? Affiliations { get; set; }


        public void RstAppendUnits(
            StringBuilder stringBuilder,
            List<UnitMetaData> units,
            string franchise,
            string affiliationComment,
            List<Unit> unitsSnes,
            List<Unit>? unitsPlayStation,
            List<Pilot>? pilotsSnes,
            List<Pilot>? pilotsPlayStation,
            List<Weapon>? weaponSnes,
            List<Weapon>? weaponPlayStation)
        {
            RstHelper.AppendHeader(stringBuilder, $"{Name}机体", '-');
            stringBuilder.AppendLine($".. _srw4_units_{franchise}_{Label}_commentBegin:");
            stringBuilder.AppendLine(affiliationComment);
            stringBuilder.AppendLine($".. _srw4_units_{franchise}_{Label}_commentEnd:");
            stringBuilder.AppendLine();

            foreach (var unit in units)
            {
                var groupInfo = GroupInfo.GetUnitGroupInfo(unit.Id);
                if (groupInfo != null)//first unit in group
                {
                    if (groupInfo.LeadId==unit.Id)
                    {
                        foreach (int unitId in groupInfo.MemberIds)
                        {
                            Unit.RstAppendUnit(stringBuilder, unitId, units, string.Empty, unitsSnes,
                                unitsPlayStation,
                                pilotsSnes,
                                pilotsPlayStation, weaponSnes, weaponPlayStation);
                        }
                    }
                }
                else
                    Unit.RstAppendUnit(stringBuilder, unit.Id, units,string.Empty, unitsSnes,
                        unitsPlayStation,
                        pilotsSnes,
                        pilotsPlayStation, weaponSnes, weaponPlayStation);
            }
        }

    }
}
