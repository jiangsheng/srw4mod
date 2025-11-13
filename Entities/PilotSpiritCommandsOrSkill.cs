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
        public int BaseAddress { get; set; }
        static List<string> SpiritCommandNames = new List<string>();
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Format(this.BaseAddress, this.SpiritCommandsOrSkill, 0, false));
            sb.Append(string.Format(" {0}", this.AcquireAtLevel));
            return sb.ToString();
        }
        public static string Format(int baseAddress,byte spiritCommandsOrSkill, byte previousSpiritCommandsOrSkill, bool withAddress)
        {
            if (spiritCommandsOrSkill >= 0x01 && spiritCommandsOrSkill <= 0x1E)
            {
                if(withAddress)
                    return string.Format("{0:X}: {1}", baseAddress, SpiritCommandNames[spiritCommandsOrSkill]);
                else
                    return SpiritCommandNames[spiritCommandsOrSkill];
            }
            if (spiritCommandsOrSkill >= 0x20 && spiritCommandsOrSkill <= 0x27)
            {
                if (previousSpiritCommandsOrSkill >= 0x20 && previousSpiritCommandsOrSkill <= 0x27)
                {
                    if (withAddress)
                        return string.Format("{0:X}:Ｌ{1}", baseAddress, spiritCommandsOrSkill - 0x20 + 1);
                    else
                        return string.Format("Ｌ{0}", spiritCommandsOrSkill - 0x20 + 1);
                }
                else
                {
                    if (withAddress)
                        return string.Format("{0:X}:シールド防御Ｌ{1}", baseAddress, spiritCommandsOrSkill - 0x20 + 1);
                    else
                        return string.Format("シールド防御Ｌ{0}", spiritCommandsOrSkill - 0x20 + 1);

                }
            }
            if (spiritCommandsOrSkill >= 0x28 && spiritCommandsOrSkill <= 0x2F)
            {
                if (previousSpiritCommandsOrSkill >= 0x28 && previousSpiritCommandsOrSkill <= 0x2f)
                {
                    if (withAddress)
                        return string.Format("{0:X}:Ｌ{1}", baseAddress, spiritCommandsOrSkill - 0x28 + 1);
                    else
                        return string.Format("Ｌ{0}", spiritCommandsOrSkill - 0x28 + 1);
                }
                else
                {
                    if (withAddress)
                        return string.Format("{0:X}:切り払いＬ{1}", baseAddress, spiritCommandsOrSkill - 0x28 + 1);
                    else
                        return string.Format("切り払いＬ{0}", spiritCommandsOrSkill - 0x28 + 1);
                }
            }
            string spiritCommandsOrSkillString = string.Empty;
            switch (spiritCommandsOrSkill)
            {
                case 0x30:
                    spiritCommandsOrSkillString ="底力";break;
                case 0x31:
                    spiritCommandsOrSkillString = "野性化"; break;
                case 0x32:
                    spiritCommandsOrSkillString = "聖戦士"; break;
                case 0x3E:
                    spiritCommandsOrSkillString = "ニュータイプ"; break;
                case 0x3F:
                    spiritCommandsOrSkillString = "強化人間"; break;
                default:
                    spiritCommandsOrSkillString = spiritCommandsOrSkill.ToString(); break;
            }
            if (withAddress)
                return string.Format("{0:X}: {1}", baseAddress, spiritCommandsOrSkillString);
            else
                return spiritCommandsOrSkillString;

        }
    }
}
