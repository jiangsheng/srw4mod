using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class GroupInfo
    {
        static GroupInfo()
        {
            UnitGroups = new List<GroupInfo>();
            //主人公机
            UnitGroups.Add(new GroupInfo (1,new List<int> { 
                0x5E,0xB2,1,2,3,4
            } ));
            //GP 03
            var groupInfo = new GroupInfo(0x0E, 2);
            groupInfo.MemberIds.Add(0x123);
            UnitGroups.Add(groupInfo);
            //リ・ガズィ
            UnitGroups.Add(new GroupInfo(0X16, 2));
            //ゲッター
            groupInfo = new GroupInfo(0x18, 6);
            groupInfo.MemberIds.AddRange(new List<int> {0xFC,0xFD,0xFE});
            UnitGroups.Add(groupInfo);
            //マジンガーＺ
            UnitGroups.Add(new GroupInfo(0X1E, 15));
            //コンバトラーＶ
            UnitGroups.Add(new GroupInfo(0X2D, 5));
            //ダイターン３
            UnitGroups.Add(new GroupInfo(0X37, 3));
            //ザンボット３
            UnitGroups.Add(new GroupInfo(0X3A, 4));
            //ダンバイン
            UnitGroups.Add(new GroupInfo(0X3F,3));
            //ダンクーガ
            UnitGroups.Add(new GroupInfo(0X43, 13));
            //サイバスター
            UnitGroups.Add(new GroupInfo(0X53, new List<int> {
                0x53,0x54,0x57,0x58,0x59,0x5A,0x55,0x5B,0x5c
            }));
            //MS/MA ハンブラビ,アッシマー,サイコガンダム
            for (int i = 0x64; i < 0x6A; i+=2)
            {
                UnitGroups.Add(new GroupInfo(i, 2));
            }
            //MS/MA ガブスレイ(,バウンド・ドック,サイコガンダムmkII
            for (int i = 0x6B; i < 0x71; i += 2)
            {
                UnitGroups.Add(new GroupInfo(i, 2));
            }
            //MS/MA メタス
            UnitGroups.Add(new GroupInfo(0x72, 2));
            //MS/MA バウ
            UnitGroups.Add(new GroupInfo(0x80, 2));
            //青/赤 ギラ・ドーガ
            UnitGroups.Add(new GroupInfo(0x86, 2));
            //メカザウルス
            groupInfo = new GroupInfo(0x8F, 6);
            groupInfo.MemberIds.Add(0x11E);
            UnitGroups.Add(groupInfo);
            //機械獣
            groupInfo = new GroupInfo(0x95, 7);
            groupInfo.MemberIds.Remove(0x98);
            UnitGroups.Add(groupInfo);
            //円盤獣
            groupInfo = new GroupInfo(0xA2, 2);
            groupInfo.MemberIds.AddRange(new List<int> { 0x116, 0x117});
            UnitGroups.Add(groupInfo);
            //ギルギルガン
            UnitGroups.Add(new GroupInfo(0xA6, 2));
            //グラン・ガラン/ゴラオン
            UnitGroups.Add(new GroupInfo(0xB3, 2));
            //化石獣
            UnitGroups.Add(new GroupInfo(0xBA, 2));
            //メカブースト・ガビタン
            UnitGroups.Add(new GroupInfo(0xBE, 2));
            //赤騎士/青騎士
            UnitGroups.Add(new GroupInfo(0xC0, 2));
            //メカ戦士
            UnitGroups.Add(new GroupInfo(0xC3, 3));
            //ブンドル艦/カットナル艦/ケルナグール艦
            UnitGroups.Add(new GroupInfo(0xC9, 3));
            //トロイホース
            groupInfo = new GroupInfo(0xD3, 1);
            groupInfo.MemberIds.AddRange(new List<int> { 0xD9, 0xDD, 0xE1});
            UnitGroups.Add(groupInfo);
            //Sガンダム
            UnitGroups.Add(new GroupInfo(0xD5, 2));
            //ExSガンダム
            UnitGroups.Add(new GroupInfo(0xDB, 2));
            //エルガイム Mk-II
            UnitGroups.Add(new GroupInfo(0xEE, 2));
            //スーパーガンダム
            UnitGroups.Add(new GroupInfo(0x103, 3));
            //サーバイン
            UnitGroups.Add(new GroupInfo(0x108, 2));
            //Ｚガンダム
            UnitGroups.Add(new GroupInfo(0x112, 2));
            //ＺＺガンダム
            UnitGroups.Add(new GroupInfo(0x112, 2));
            //レプラカーン
            groupInfo = new GroupInfo(0xAB, 1);
            groupInfo.MemberIds.AddRange(new List<int> { 0x124, 0x125});
            UnitGroups.Add(groupInfo);
            //ライネック
            groupInfo = new GroupInfo(0xAE, 1);
            groupInfo.MemberIds.AddRange(new List<int> { 0x126, 0x127 });
            UnitGroups.Add(groupInfo);
            //ガラバ
            groupInfo = new GroupInfo(0xB1, 1);
            groupInfo.MemberIds.AddRange(new List<int> { 0x128, 0x129 });
            UnitGroups.Add(groupInfo);
        }

        public GroupInfo(int lead, int length)
        {
            LeadId = lead;
            MemberIds = new List<int>();
            for (int i = 0; i < length; i++)
            {
                MemberIds.Add(lead + i);
            }
        }

        public GroupInfo(int lead, List<int> list)
        {
            LeadId = lead;
            MemberIds = list;
        }

        public int LeadId { get; internal set; }
        public List<int> MemberIds{ get; internal set; }
        public static List<GroupInfo>? UnitGroups { get; set; }


        public static GroupInfo? GetUnitGroupInfo(int id)
        {
            foreach (var group in GroupInfo.UnitGroups)
            {
                if (group.MemberIds.Contains(id))
                {
                    return group;
                }
            }
            return null;
        }
    }
}
