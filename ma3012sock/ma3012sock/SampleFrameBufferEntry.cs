using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ma3012sock
{
    public class SampleFrameBufferEntry
    {
        byte[] Bufferbytes;

        public SampleFrameBufferEntry()
        {
        }
        public SampleFrameBufferEntry(byte[] bytes)
        {
            Bufferbytes = bytes;
        }

        // Properties



        public byte[] ActualDataBlock
        {
            get
            {
                byte[] destinationArray = new byte[Bufferbytes.Length];

                Array.Copy(Bufferbytes, 0, destinationArray, 0, 1200);
                return destinationArray;
            }
        }
    }
}
