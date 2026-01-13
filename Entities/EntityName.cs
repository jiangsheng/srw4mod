using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class EntityName
    {
        public string? Name { get; set; }
        [Ignore]
        public int Offset { get; set; }
        public int Id { get; set; }

        internal static List<EntityName> Parse(ReadOnlySpan<byte> romData, IndexTable indexTable)
        {
            var result = new List<EntityName>();
            var indexedLocations = indexTable.Read(romData);
            int nameId = 0;
            foreach (var location in indexedLocations)
            {
                if (location != 0)
                {
                    EntityName? entityName = ParseName(romData, location, nameId);
                    if (entityName != null)
                    {
                        result.Add(entityName);
                    }
                }
                nameId++;
            }
            return result;
        }

        private static EntityName? ParseName(ReadOnlySpan<byte> romData, int location, int nameId)
        {
            ReadOnlySpan<byte> bytes = romData.Slice(location, 256);
            var terminator = bytes.IndexOf((byte)0xff);
            if (terminator == -1) 
                return null;
            EntityName entityName = new EntityName();
            entityName.Id = nameId;
            entityName.Offset = location;
            entityName.Name=Rom.srw4Encoding.GetString(romData.Slice(location, terminator));
            return entityName;
        }
    }
}
