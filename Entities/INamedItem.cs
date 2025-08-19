using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public interface INamedItem
    {
        public string? Name { get; set; }
        static string GetNames(List<INamedItem>? namedItems)
        {
            if (namedItems == null || namedItems.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var item in namedItems)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(", ");
                    }
                    stringBuilder.Append(item.Name);
                }
            }
            return stringBuilder.ToString();
        }
    }
}
