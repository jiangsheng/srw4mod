using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitTScoreParameters
    {
        public UnitTScoreParameters(List<Unit> samples)
        {
            TScoreParametersList=new List<TScoreParameters>();
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Experience).ToList()
                )); 
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Gold).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.RepairCost).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.MoveRange).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Armor).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
                samples.Select(sample => (int)sample.Mobility).ToList()
                ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Limit).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.Energy).ToList()
               ));
            TScoreParametersList.Add(new TScoreParameters(
               samples.Select(sample => (int)sample.HP).ToList()
               )); 
        }

        public List<TScoreParameters> TScoreParametersList { get; private set; }

        public TScoreParameters GetTScoreParameter(UnitTScoreParameterIndex index)
        {
            return GetTScoreParameter((int)index);
        }
        public TScoreParameters GetTScoreParameter(int index)
        {
            return this.TScoreParametersList[index];
        }
    }
    public enum UnitTScoreParameterIndex
    {
        Experience,
        Gold,
        RepairCost,
        MoveRange,
        Armor,
        Mobility,
        Limit,
        Energy,
        HP,
        Count
    }
}