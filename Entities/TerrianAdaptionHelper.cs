using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public static class TerrianAdaptionHelper
    {
        public static string FormatTerrianAdaption(byte[] terrianAdaptions)
        {
            StringBuilder stringBuilder
               = new StringBuilder();
            foreach (var item in terrianAdaptions)
            {
                stringBuilder.Append(item);
            }
            return stringBuilder.ToString();
        }
        public static string FormatTerrianAdaption(byte terrianAdaption)
        {
            switch (terrianAdaption)
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
    }
}
