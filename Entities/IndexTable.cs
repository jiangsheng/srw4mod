using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class IndexTable
    {
        public IndexTable(int headerOffset, int dataOffset, int dataLength, int indexBase)
        {
            HeaderOffset = headerOffset;
            DataOffset = dataOffset;
            DataLength = dataLength;
            IndexBase = indexBase;
        }

        public int HeaderOffset { get; set; }
        public int DataOffset { get; set; }
        public int DataLength{ get; set; }
        public int IndexBase { get; set; }
        /// <summary>
        /// return a list of indexed locations stored in the data table
        /// </summary>
        /// <param name="data"></param>
        /// <param name="headerOffset">start of the index table</param>
        /// <param name="dataOffset">start of indexed data</param>
        /// <param name="dataLength">length of indexed data</param>
        /// <param name="indexBase">base offset of index values</param>
        /// <returns></returns>
        public List<int> Read(ReadOnlySpan<byte> data)
        {
            List<int> indexedLocations=new List<int> ();
            //do not read header past data
            var doNotReadPastThis = DataOffset + DataLength;
            for (int currentPosition = 0; currentPosition + HeaderOffset< DataOffset; currentPosition += 2)
            {
                var indexValue= BitConverter.ToUInt16(data.Slice(HeaderOffset+ currentPosition,2));
                if (indexValue == 0) {
                    //no data at this address
                    indexedLocations.Add(0);
                }
                else
                {
                    var indexedOffset = indexValue + IndexBase;
                    if (indexedOffset >= doNotReadPastThis)
                        break;
                    else
                        indexedLocations.Add(indexedOffset);
                }
            }
            return indexedLocations;
        }
    }
}
