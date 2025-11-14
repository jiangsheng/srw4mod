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
            UnitGroups.Add(new GroupInfo(0X1E, 2));
            //コンバトラーＶ
            UnitGroups.Add(new GroupInfo(50, new List<int> {
                50,45,46,47,48,49
            }));
            //ダイターン３
            UnitGroups.Add(new GroupInfo(0X37, 3));
            //ザンボット３
            UnitGroups.Add(new GroupInfo(0X3A, 4));
            //ダンバイン
            UnitGroups.Add(new GroupInfo(0X3F,3));
            //ダンクーガ
            UnitGroups.Add(new GroupInfo(0X43, 13));
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
        public static List<GroupInfo> UnitGroups { get; set; }


        public static GroupInfo? GetUnitGroupInfo(int id)
        {
            foreach (var group in GroupInfo.UnitGroups)
            {
                if(group.LeadId==id)
                {
                    return group;
                }
                if (group.MemberIds.Contains(id))
                {
                    return group;
                }
            }
            return null;
        }
    }
}
