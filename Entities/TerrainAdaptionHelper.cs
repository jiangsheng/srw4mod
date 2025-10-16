using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public static class TerrainAdaptionHelper
    {
        public static string FormatTerrainAdaption(byte[] TerrainAdaptions)
        {
            StringBuilder stringBuilder
               = new StringBuilder();
            foreach (var item in TerrainAdaptions)
            {
                stringBuilder.Append(FormatTerrainAdaption(item));
            }
            return stringBuilder.ToString();
        }
        public static string FormatTerrainAdaption(byte TerrainAdaption)
        {
            switch (TerrainAdaption)
            { 
                case 0x00:
                    return "🚫";
                case 0x01:
                    return "D";
                case 0x02:
                    return "C";
                case 0x03:
                    return "B";
                case 0x04:
                    return "A";
                default:
                    return "?";
            }
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
        internal static int GetEffectiveAdaption(byte TerrainAdaptionUnit/*0-4*/, 
            int TerrainAdaptionPilot/*0-4*/, int TerrainAdaptionWeapon/*0-3*/)
        {
            //A+A=A,A+B=B,A+C=B, etc
            var effectiveAdaption = (TerrainAdaptionUnit + TerrainAdaptionPilot) / 2;
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
                damageCoefficientUnit[effectiveAdaption] * 
                damageCoefficientWeapon[TerrainAdaptionWeapon];
            //result would be a value between 0.0 and 1.2
            //        unit  A    B   C    D    -
            // weapon       1.2  1.0 0.8  0.6  0  
            // A     1.0    1.2  1.0 0.8  0.6  0  
            // B     0.7    0.84 0.7 0.56 0.42 0
            // C     0.4    0.48 0.4 0.32 0.24 0
            // -     0      0    0   0    0    0 
            if (result >1) 
                return 4;//A
            if (result> 0.8)
                return 3;//B
            if (result >0.5)
                return 2;//C
            if (result > 0.2)//C+D=D
                return 1;//D
            return 0;//-
        }
    }
}
