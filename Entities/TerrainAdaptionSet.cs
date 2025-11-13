using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class TerrainAdaptionSet
    {
        //-=0,D=1,C=2,B=3,A=4
        byte[] adaptions = new byte[(int)TerrainAdaptionSetIndex.Count];

        public static string[] adaptionNames = { "空", "陆", "海", "宇" };
        public static string[] adaptionRatings = { "🚫", "D", "C", "B", "A" };
        public TerrainAdaptionSet(byte[] newAdaption) {
            this.adaptions = newAdaption; }

        public static string FormatTerrainAdaption(byte TerrainAdaption)
        {
            return adaptionRatings[TerrainAdaption];
        }
        public static TerrainAdaptionSet FromPilotOrUnitAdaptions(
            byte pilotAdaptionsByteLow,
            byte pilotAdaptionsByteHigh)
        {
            byte[] adaptions = new byte[4];
            adaptions[0] = (byte)(pilotAdaptionsByteLow / 16);//air
            adaptions[1] = (byte)(pilotAdaptionsByteHigh & 0x0F );//land
            adaptions[2] = (byte)(pilotAdaptionsByteLow & 0x0F);//sea
            adaptions[3] = (byte)(pilotAdaptionsByteHigh / 16);//space
            return new TerrainAdaptionSet(adaptions);
        }

        public byte[] ToPilotOrUnitAdaptions()
        {
            var result = new byte[2];
            result[0] = (byte)(this.adaptions[0] * 16 + this.adaptions[2]);
            result[1] = (byte)(this.adaptions[1] * 16 + this.adaptions[3]);
            return result;
        }

        public static TerrainAdaptionSet FromWeaponAdaptions(byte adaptionByte)
        {
            byte[] adaptions = new byte[4];
            adaptions[0] = (byte)((adaptionByte / 4) & 0x03);//air
            adaptions[1] = (byte)((adaptionByte / 16) & 0x03);//land
            adaptions[2] = (byte)(adaptionByte / 64);//sea
            adaptions[3] = (byte)(adaptionByte & 0x03);//space
            for(int i = 0; i < 4; i++)
            {
                var adaption = adaptions[i];
                //there is no D rating in weapons
                if(adaption != 0)
                    adaptions[i] = (byte)(adaption + 1);
            }
            return new TerrainAdaptionSet(adaptions);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < 4; i++)
            {
                var adaption = adaptions[i];
                sb.AppendFormat("{0}{1}", adaptionNames[i], adaptionRatings[adaption]);
            }
            return sb.ToString();
        }

        internal byte GetTerrainAdaptionByIndex(TerrainAdaptionSetIndex index) { return adaptions[(int)index]; }
        internal byte GetTerrainAdaptionByIndex(int index) { return adaptions[index]; }

        internal static TerrainAdaptionSet Combine(
            TerrainAdaptionSet terrainAdaptionSetUnit,
            TerrainAdaptionSet terrainAdaptionSetPilot)
        {
            byte[] adaptions = new byte[(int)TerrainAdaptionSetIndex.Count];
            for (int i = 0; i < (int)TerrainAdaptionSetIndex.Count; i++)
            {
                adaptions[i] = (byte)((terrainAdaptionSetUnit.adaptions[i] + terrainAdaptionSetPilot.adaptions[i]) / 2);
            }
            return new TerrainAdaptionSet(adaptions);
        }

        public static string FormatEffectiveTerrainAdaption(
            byte snesTerrainAdaption,
            byte playStationTerrainAdaption,
            byte effectiveSnesTerrainAdaption,
            byte effectivePlayStationTerrainAdaption)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if(snesTerrainAdaption == playStationTerrainAdaption)
            {
                stringBuilder.AppendFormat("{0}", TerrainAdaptionSet.FormatTerrainAdaption(snesTerrainAdaption));
            } else
                stringBuilder.AppendFormat(
                    "{0} ({1})",
                    TerrainAdaptionSet.FormatTerrainAdaption(snesTerrainAdaption),
                    TerrainAdaptionSet.FormatTerrainAdaption(playStationTerrainAdaption));
            if(effectiveSnesTerrainAdaption != snesTerrainAdaption ||
                effectivePlayStationTerrainAdaption != playStationTerrainAdaption)
            {
                stringBuilder.Append("→");

                if(effectiveSnesTerrainAdaption == effectivePlayStationTerrainAdaption)
                {
                    stringBuilder.AppendFormat(
                        "{0}",
                        TerrainAdaptionSet.FormatTerrainAdaption(effectiveSnesTerrainAdaption));
                } else
                {
                    stringBuilder.AppendFormat(
                        "{0} ({1})",
                        TerrainAdaptionSet.FormatTerrainAdaption(effectiveSnesTerrainAdaption),
                        TerrainAdaptionSet.FormatTerrainAdaption(effectivePlayStationTerrainAdaption));
                }
            }
            return stringBuilder.ToString();
        }

        public static TerrainAdaptionSet GetEffectiveTerrainWeaponAdaptions(
            TerrainAdaptionSet? effectiveUnitTerrainAdoptions,
            TerrainAdaptionSet? weaponTerrainAdoptions,
            bool IsMelee,
            int moveType)
        {
            if (effectiveUnitTerrainAdoptions == null) throw new ArgumentNullException(nameof(effectiveUnitTerrainAdoptions));
            if (weaponTerrainAdoptions == null) throw new ArgumentNullException(nameof(weaponTerrainAdoptions));

            if (!IsMelee) return weaponTerrainAdoptions;
            byte[] adaptions = new byte[(int)TerrainAdaptionSetIndex.Count];
            for(int i = 0; i < (int)TerrainAdaptionSetIndex.Count; i++)
            {
                adaptions[i] =
                    GetEffectiveWeaponAdaption(
                    effectiveUnitTerrainAdoptions.GetTerrainAdaptionByIndex(i),
                    weaponTerrainAdoptions.GetTerrainAdaptionByIndex(i));

                if (!CanMoveTo(moveType, i))
                {
                    adaptions[i] = 0;
                }
            }
            return new TerrainAdaptionSet(adaptions);
        }

        public static string FormatMovementType(byte moveType)
        {
            switch (moveType)
            {
                case 0x00: return "陸宇";
                case 0x01: return "空陸";
                case 0x02: return "空";
                case 0x03: return "水陸";
                case 0x04: return "水陸空";
                case 0x05: return "陸地中";
                case 0x06: return "陸空地中";
                case 0x07: return "海";
                case 0x08: return "陸";
                case 0x09: return "宇宙";
                case 0x0A: return "空地中";
                case 0x0B: return "空海";
                case 0x0C: return "空（陸可）";
                default:
                    return "?";
            }
        }
        public static bool CanMoveTo(int moveType, int terrain)
        {
            switch (terrain)
            {
                case 0:
                    switch (moveType)
                    {
                        case 0x01: //"空陸";
                        case 0x02: //"空";
                        case 0x04: //"水陸空";
                        case 0x06: //"陸空地中";";
                        case 0x0A: //"空地中";
                        case 0x0B: //"空海";
                        case 0x0C: //"空（陸可）";
                            return true;
                        default: return false;
                    }
                case 1:
                    switch (moveType)
                    {
                        case 0x00: //"陸宇";
                        case 0x01: //"空陸";
                        case 0x03: //"水陸";
                        case 0x04: //"水陸空";
                        case 0x05: //"陸地中";
                        case 0x06: //"陸空地中";
                        case 0x08: //"陸";
                        case 0x0C: //"空（陸可）";
                            return true;
                        default: return false;
                    }
                case 2:
                    switch (moveType)
                    {
                        case 0x00: //"陸宇";
                        case 0x01: //"空陸";
                        case 0x03: //"水陸";
                        case 0x04: //"水陸空";
                        case 0x05: //"陸地中";
                        case 0x06: //"陸空地中";
                        case 0x07: //"海";
                        case 0x08: //"陸";
                        case 0x0B: //"空海";
                        case 0x0C: //"空（陸可）";
                            return true;
                        default: return false;
                    }
                case 3:
                    switch (moveType)
                    {
                        case 0x00: //"陸宇";
                        case 0x01: //"空陸";
                        case 0x02: //"空";
                        case 0x03: //"水陸";
                        case 0x04: //"水陸空";
                        case 0x05: //"陸地中";
                        case 0x06: //"陸空地中";
                        case 0x07: //"海";
                        case 0x09: //"宇宙";
                        case 0x0A: //"空地中";
                        case 0x0B: //"空海";
                        case 0x0C: //"空（陸可）";
                            return true;
                        default: return false;
                    }
                default: return false;
            }
        }

        internal static byte GetEffectiveWeaponAdaption(
            byte effectiveUnitTerrainAdaption/*0-4*/,
            int terrainAdaptionWeapon/*0-3*/)
        {
            var terrainAdaptionWeaponRating = terrainAdaptionWeapon;
            //there is no d terrain rating for weapons
            if (terrainAdaptionWeaponRating>0)
                terrainAdaptionWeaponRating = terrainAdaptionWeaponRating - 1;

            //weapon adaption is an coefficent, not additive
                //e.g. for 0 weapon adaption the damage would be 0%, not 50% when effectiveAdaption is A 
                var damageCoefficientUnit = new float[]
            {
                0.0f, //0=-
                0.6f, //1=D
                0.8f, //2=C
                1.0f, //3=B
                1.2f //4=A
            };
            var damageCoefficientWeapon = new float[]
            {
                0.0f, //0=-
                0.4f, //1=C
                0.7f, //2=B
                1.0f //3=A
            };
            var result =
                damageCoefficientUnit[effectiveUnitTerrainAdaption] * damageCoefficientWeapon[terrainAdaptionWeaponRating];
            //result would be a value between 0.0 and 1.2
            //        unit  A    B   C    D    -
            // weapon       1.2  1.0 0.8  0.6  0  
            // A     1.0    1.2  1.0 0.8  0.6  0  
            // B     0.7    0.84 0.7 0.56 0.42 0
            // C     0.4    0.48 0.4 0.32 0.24 0
            // -     0      0    0   0    0    0 
            if(result > 1)
                return 4;//A+A=A
            if(result >= 0.7)
                return 3;//B+B=B
            if(result >= 0.3)
                return 2;//C+C=C
            if(result > 0.2)//C+D=D
                return 1;//D
            return 0;//-
        }

        public static List<string> FormatEffectiveTerrainWeaponAdaption(TerrainAdaptionSet snesTerrainAdaptionSet, TerrainAdaptionSet playstationTerrainAdaptionSet, TerrainAdaptionSet snesWeaponEffectiveTerrainAdaptionSet, TerrainAdaptionSet playstationWeaponEffectiveTerrainAdaptionSet)
        {

            List<string> result=new List<string>();

            for (int i = 0; i < (int)TerrainAdaptionSetIndex.Count; i++)
            {
                var snesUnitTerrainAdoption = snesTerrainAdaptionSet.GetTerrainAdaptionByIndex((TerrainAdaptionSetIndex)i);
                var playStationUnitTerrainAdoption = playstationTerrainAdaptionSet.GetTerrainAdaptionByIndex((TerrainAdaptionSetIndex)i);
                var effectiveSnesUnitTerrainAdoption = snesWeaponEffectiveTerrainAdaptionSet.GetTerrainAdaptionByIndex((TerrainAdaptionSetIndex)i);
                var effectivePlayStationUnitTerrainAdoption = playstationWeaponEffectiveTerrainAdaptionSet.GetTerrainAdaptionByIndex((TerrainAdaptionSetIndex)i);
                result.Add(TerrainAdaptionSet.adaptionNames[i]+TerrainAdaptionSet.FormatEffectiveTerrainAdaption(snesUnitTerrainAdoption,
                    playStationUnitTerrainAdoption, effectiveSnesUnitTerrainAdoption, effectivePlayStationUnitTerrainAdoption));                
            }
            return result;
        }
    }

    enum TerrainAdaptionSetIndex
    {
        Air,
        Land,
        Sea,
        Space,
        Count
    }
}
