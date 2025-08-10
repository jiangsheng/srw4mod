using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class PilotSpiritCommandsOrSkill
    {
        static PilotSpiritCommandsOrSkill()
        {
            SpiritCommandNames.AddRange(
                new string[]
                {
                    "無し","根性","ド根性","補給","友情","信頼","愛","激怒","気合","加速","熱血",
                    "必中","ひらめき","幸運","覚醒","威圧","てかげん","集中","激励","再動","復活","隠れ身"
                    ,"脱力","自爆","探索","足かせ","かく乱","偵察","鉄壁","魂","奇跡"
                }
            );
        }
           
        public byte SpiritCommandsOrSkill {  get; set; }
        public int AcquireAtLevel{ get; set; }
        static List<string> SpiritCommandNames = new List<string>();
        public static string Format(byte spiritCommandsOrSkill)
        {
            if (spiritCommandsOrSkill >= 0x01 && spiritCommandsOrSkill <= 0x1E)
            {
                return SpiritCommandNames[spiritCommandsOrSkill];
            }
            if (spiritCommandsOrSkill >= 0x20 && spiritCommandsOrSkill <= 0x27)
            {
                return string.Format("シールド防御Ｌ{0}", spiritCommandsOrSkill - 0x20+1);
            }
            if (spiritCommandsOrSkill >= 0x28 && spiritCommandsOrSkill <= 0x2F)
            {
                return string.Format("切り払いＬ{0}", spiritCommandsOrSkill - 0x28+1);
            }
            switch (spiritCommandsOrSkill)
            {
                case 0x30:
                    return "底力";
                case 0x31:
                    return "野性化";
                case 0x32:
                    return "聖戦士";
                case 0x3E:
                    return "ニュータイプ"; 
                case 0x3F: 
                    return "強化人間";
            }
            return spiritCommandsOrSkill.ToString();
        }
    }
}
