using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class UnitWeapon
    {
        public int BaseOffset { get; set; }
        public byte Order { get; set; }
        public bool IsConditional { get; set; }
        public ushort WeaponIndex { get; set; }
        public byte AmmoSlot { get; set; }
        public byte AvailableAtStage { get; set; }
        public Unit? FirstOwner { get; set; }
        public Weapon? Weapon { get; set; }
        public Unit? Unit { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();

            stringBuilder.AppendFormat("地址: {0,6:X}", BaseOffset);
            stringBuilder.AppendFormat("\t序号:{0,2:X}", Order);
            stringBuilder.AppendFormat("\t武器代码:{0,3:X}", WeaponIndex);
            stringBuilder.AppendFormat("\t弹药槽:{0,2:X}", AmmoSlot == 0 ? string.Empty : AmmoSlot.ToString());
            stringBuilder.AppendFormat("\t伤害:{0,5}", Weapon?.Damage);
            stringBuilder.AppendFormat("\t程:{0,2}", Weapon?.MaxRange);
            stringBuilder.AppendFormat("\t地形适应:{0,10}", Weapon?.TerrainAdaptionSet?.ToString());
            if (Weapon?.MaxAmmo > 0)
                stringBuilder.AppendFormat("\t弹数:{0,3}", Weapon?.MaxAmmo);
            if (Weapon?.EnergyCost > 0)
                stringBuilder.AppendFormat("\t耗能:{0,3}", Weapon?.EnergyCost);
            if (Weapon?.MaxAmmo == 0 && Weapon?.EnergyCost == 0)
            {
                stringBuilder.Append("\t      ");
            }
            stringBuilder.AppendFormat("\t名:{0,-20}", Weapon?.GetNameWithAttributes());

            if(IsConditional)
                stringBuilder.AppendFormat("\t启用关卡:{0,3:X}", AvailableAtStage);
            if(FirstOwner!=null)
                stringBuilder.AppendFormat("\t首装备:{0,20}", FirstOwner?.Name);
            return stringBuilder.ToString();
        }
    }
}
