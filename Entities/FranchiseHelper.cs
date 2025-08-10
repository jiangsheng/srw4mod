using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public static class FranchiseHelper
    {
        public static string FormatInGameFranchise(int inGameFranchise)
        {
            switch (inGameFranchise)
            {
                case 0x00:
                    return "マジンガーＺ";
                case 0x02:
                    return "劇場版マジンガーＺ";
                case 0x04:
                    return "グレートマジンガー";
                case 0x06:
                    return "グレンダイザー";
                case 0x08:
                    return "ゲッターロボ";
                case 0x0A:
                    return "ゲッターロボG";
                case 0x0C:
                    return "コンバトラーＶ";
                case 0x0E:
                    return "ダイモス";
                case 0x10:
                    return "ザンボット３";
                case 0x12:
                    return "ダイターン";
                case 0x14:
                    return "ダンバイン";
                case 0x16:
                    return "ダンバインOVA";
                case 0x18:
                    return "エルガイム";
                case 0x1A:
                    return "機動戦士ガンダム";
                case 0x1C:
                    return "機動戦士Zガンダム";
                case 0x1E:
                    return "機動戦士ZZガンダム";
                case 0x20:
                    return "機動戦士ガンダム0080";
                case 0x22:
                    return "機動戦士ガンダム0083";
                case 0x24:
                    return "ガンダムセンチネル";
                case 0x26:
                    return "逆襲のシャア";
                case 0x28:
                    return "ガンダムF91";
                case 0x2A:
                    return "ライディーン";
                case 0x2C:
                    return "ゴーショーグン";
                case 0x2E:
                    return "ダンクーガ";
                case 0x30:
                    return "オリジナル";
                default:
                    return (string.Format("Unknown inGameFranchise {0:x}", inGameFranchise));
            }
        }
        public static string FormatPlayStationFranchise2(byte playStationFranchise2)
        {
            switch (playStationFranchise2)
            {
                case 0:
                    return "マジンガーＺ&兵士";
                case 1:
                    return "劇場版マジンガーＺ&グレートマジンガー";
                case 2:
                    return "グレンダイザー";
                case 3:
                    return "ゲッターロボ&ゲッターロボG";

                case 4:
                    return "コンバトラーＶ";
                case 5:
                    return "ダイモス";
                case 6:
                    return "ザンボット３";
                case 7:
                    return "ダイターン３";
                case 8:
                    return "ダンバイン&ダンバインOVA";
                case 9:
                    return "エルガイム";
                case 10:
                    return "機動戦士ガンダムF91&0080&0079";
                case 11:
                    return "機動戦士Zガンダム";
                case 12:
                    return "機動戦士ZZガンダム";
                case 13:
                    return "機動戦士ガンダム0083";
                case 14:
                    return "逆襲のシャア";
                case 15:
                    return "ライディーン";
                case 16:
                    return "ゴーショーグン";
                case 17:
                    return "ダンクーガ";
                case 18:
                    return "ヒーロー戦記&魔装機神";
                case 19:
                    return "オリジナル";
                default:
                    return string.Format("Unknown playStationFranchise2 {0:x}", playStationFranchise2);
            }
        }
        public  static string ToRstFranchise(string? franchise, string type)
        {
            switch (franchise)
            {
                case "オリジナル":
                    return string.Format(":ref:`原创 <srw4_{0}_banpresto_originals>`", type);
                case "逆襲のシャア":
                    return string.Format(":ref:`逆襲のシャア <srw4_{0}_ms_gundam_char_s_counterattack>`", type);
                case "ガンダムF91":
                    return string.Format(":ref:`ガンダムF91 <srw4_{0}_ms_gundam_f91>`", type);
                case "ガンダム0080":
                    return string.Format(":ref:`ガンダム0080 <srw4_{0}_ms_gundam_0080>`", type);
                case "ガンダム0083":
                    return string.Format(":ref:`ガンダム0083 <srw4_{0}_ms_gundam_0083>`", type);
                case "ガンダム0079":
                    return string.Format(":ref:`ガンダム0079 <srw4_{0}_ms_gundam>`", type);
                case "Zガンダム":
                    return string.Format(":ref:`Zガンダム <srw4_{0}_ms_z_gundam>`", type);
                case "ZZガンダム":
                    return string.Format(":ref:`ZZガンダム <srw4_{0}_ms_gundam_zz>`", type);
                case "ガンダムセンチネル":
                    return string.Format(":ref:`ガンダムセンチネル <srw4_{0}_ms_gundam_sentinel>`", type);
                case "ダンバイン":
                case "ダンバインOVA":
                    return string.Format(":ref:`ダンバイン <srw4_{0}_dunbine>`", type);
                case "エルガイム":
                    return string.Format(":ref:`エルガイム <srw4_{0}_heavy_metal_l_gaim>`", type);
                case "ゲッター":
                case "ゲッターG":
                case "真ゲッター":
                    return string.Format(":ref:`ゲッター <srw4_{0}_getter_robo>`", type);
                case "コンバトラーV":
                    return string.Format(":ref:`コンバトラーV <srw4_{0}_combattler_v>`", type);
                case "マジンガーＺ":
                    return string.Format(":ref:`マジンガーＺ <srw4_{0}_mazinger_z>`", type);
                case "劇場版マジンガーＺ":
                    return string.Format(":ref:`劇場版マジンガーＺ <srw4_{0}_mazinger_z_the_movie>`", type);
                case "グレートマジンガー":
                    return string.Format(":ref:`グレートマジンガー <srw4_{0}_great_mazinger>`", type);
                case "グレンダイザー":
                    return string.Format(":ref:`グレンダイザー <srw4_{0}_grendizer>`", type);
                case "ダイターン３":
                    return string.Format(":ref:`ダイターン３ <srw4_{0}_daitarn_3>`", type);
                case "ダイモス":
                    return string.Format(":ref:`ダイモス <srw4_{0}_daimos>`", type);
                case "ザンボット３":
                    return string.Format(":ref:`ザンボット３ <srw4_{0}_zambot_3>`", type);
                case "ゴーショーグン":
                    return string.Format(":ref:`ゴーショーグン <srw4_{0}_goshogun>`", type);
                case "ライディーン":
                    return ":ref:`ライディーン <srw4_" + type + "_reideen_the_brave>`";
                case "ダンクーガ":
                    return string.Format(":ref:`ダンクーガ <srw4_{0}_dancouga>`", type);
                case "閃光のハサウェイ":
                    return string.Format(":ref:`閃光のハサウェイ <srw4_{0}_ms_gundam_hathaway>`", type);
                default:
                    return string.Empty;
            }
        }
    }
}
