using System;
using System.Collections.Generic;
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
        public string? Name { get; set; }
        public string? FirstOwner { get; set; }
        public ushort Damage { get; set; }
        public byte Range{ get; set; }
        public byte Ammo { get; set; }
        public byte En { get; set; }
        public bool IsMap { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder
                = new StringBuilder();

            stringBuilder.AppendFormat("地址: {0:X}", BaseOffset);
            stringBuilder.AppendFormat("\t名: {0}", Name);
            if ((IsMap))
            {
                stringBuilder.AppendFormat(" (Map)");
            }
            stringBuilder.AppendFormat("\t伤害: {0}", Damage);
            stringBuilder.AppendFormat("\t程: {0}", Range);

            if (Ammo > 0)
            {
                stringBuilder.AppendFormat("\t弹数: {0}", Ammo);
            }

            if (En > 0)
            {
                stringBuilder.AppendFormat("\t耗能: {0}", En);
            }
            stringBuilder.AppendFormat("\t武器代码: {0:X}", WeaponIndex);
            if (AmmoSlot != 0)
                stringBuilder.AppendFormat("\t弹药槽: {0:X}", AmmoSlot);
            stringBuilder.AppendFormat("\t序号: {0:X}", Order);

            if (IsConditional)
            {
                stringBuilder.AppendFormat("\t启用关卡: {0:X}", AvailableAtStage);
            }
            if (!string.IsNullOrEmpty(FirstOwner))
            {
                stringBuilder.AppendFormat("\t首装备: {0}", FirstOwner);
            }
            
            return stringBuilder.ToString();
        }
    }
}
