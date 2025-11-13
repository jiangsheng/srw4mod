
namespace Entities
{
    public class PilotTScoreParameters
    {
        public PilotTScoreParameters(List<Pilot> samples)
        {
            TScoreParametersList = new List<TScoreParameters>();
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Experience).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.NearAttack).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.NearAttack99()).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.FarAttack).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.FarAttack99()).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Accuracy).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Accuracy99()).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Evasion).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Evasion99()).ToList()
               ));

            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Intuition).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Intuition99()).ToList()
               ));


            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Skill).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Skill99()).ToList()
               ));

            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.StartSP).ToList()
               ));
        }

        public List<TScoreParameters> TScoreParametersList { get; private set; }

        public TScoreParameters GetTScoreParameter(PilotTScoreParameterIndex index)
        {
            return GetTScoreParameter((int)index);
        }
        public TScoreParameters GetTScoreParameter(int index)
        {
            return this.TScoreParametersList[index];
        }
    }

    public enum PilotTScoreParameterIndex
    {
        Experience, NearAttack, NearAttack99, FarAttack, FarAttack99, Accuracy, Accuracy99,
        Evasion, Evasion99, Intuition, Intuition99, Skill, Skill99, StartSP,Count
    }
}