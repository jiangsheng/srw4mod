using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitTScoreParametersSet
    {
        public UnitTScoreParametersSet(List<Unit> snesUnits,List<Unit> playStationUnits) {
            SnesEncountered = new UnitTScoreParameters(
                snesUnits.Where(unit => unit.FirstAppearance > 0).ToList());

            PlayStationEncountered = new UnitTScoreParameters(
                playStationUnits.Where(unit => unit.FirstAppearance> 0).ToList());

            SnesOwned= new UnitTScoreParameters(
                snesUnits.Where(unit => unit.Affiliation=="自").ToList());

            PlayStationOwned = new UnitTScoreParameters(
                playStationUnits.Where(unit => unit.Affiliation == "自").ToList());
        }
        public UnitTScoreParameters SnesEncountered { get; set; }
        public UnitTScoreParameters PlayStationEncountered { get; set; }

        public UnitTScoreParameters SnesOwned { get; set; }
        public UnitTScoreParameters PlayStationOwned { get; set; }

    }
}
