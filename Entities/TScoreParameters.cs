using System.Runtime.Intrinsics.X86;

namespace Entities
{
    public class TScoreParameters
    {
        public double PopulationMean { get; set; }
        double StandardDeviation { get; set; }
        public int SampleSize { get; set; }
        public TScoreParameters(List<int> samples) 
        {
            PopulationMean= samples.Average();
            SampleSize= samples.Count();

            StandardDeviation =
                Math.Sqrt(
                samples.Sum(d => (d - PopulationMean) * (d - PopulationMean))
                / SampleSize
                );
        }

        public double CalculateTScore(int sample)
        {
            return (10 * (sample - PopulationMean) / StandardDeviation) + 50;
        }
    }
}
