
namespace Entities
{
    public class PilotTScoreParametersSet
    {
        public PilotTScoreParametersSet(List<Pilot>? snesPilots, List<Pilot>? playstationPilots)
        {
            if(snesPilots==null) 
                throw new ArgumentNullException(nameof(snesPilots));
            if(playstationPilots==null)
                throw new ArgumentNullException(nameof(playstationPilots));

            SnesEncountered = new PilotTScoreParameters(
                snesPilots.Where(p => 
                p.FirstAppearance > 0
                && p.NearAttack>0).ToList());

            PlayStationEncountered = new PilotTScoreParameters(
                playstationPilots.Where(p =>p.FirstAppearance > 0
                && p.NearAttack > 0).ToList());

            SnesOwned = new PilotTScoreParameters(
                snesPilots.Where(p => p.Affiliation == "自"
                 && p.NearAttack > 0).ToList());

            PlayStationOwned = new PilotTScoreParameters(
                playstationPilots.Where(p => p.Affiliation == "自"
                 && p.NearAttack > 0).ToList());
        }
        public PilotTScoreParameters SnesEncountered { get; set; }
        public PilotTScoreParameters PlayStationEncountered { get; set; }

        public PilotTScoreParameters SnesOwned { get; set; }
        public PilotTScoreParameters PlayStationOwned { get; set; }
    }
}