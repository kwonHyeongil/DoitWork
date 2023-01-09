using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ma3012receive
{
    public class ExtendedSampleFrameBufferEntry
    {
        private byte[] packetData;
        public ExtendedSampleFrameBufferEntry(byte[] bytes)
        {
            packetData = bytes;
        }

        public SampleFrameBufferEntry GetSampleFrameBufferEntryNew(int count , int samplecount)
        {
            byte[] Transbuffer = new byte[samplecount];
            if (count < 2)
            {
                byte[] buffer = new byte[samplecount];
                buffer = this.GetBytes(((uint)((samplecount + 20) * 0) + 20), (uint)samplecount);
                Array.Copy(buffer, 0, Transbuffer, 0, samplecount);
            }
            else
            {
                byte[] buffer = new byte[samplecount];
                buffer = this.GetBytes(((uint)((samplecount + 20) * 1) + 20), (uint)samplecount);
                Array.Copy(buffer, 0, Transbuffer, 0, samplecount);
            }

            return new SampleFrameBufferEntry(Transbuffer);
        }

        public byte[] GetBytes(uint index, uint length)
        {
            byte[] destinationArray = new byte[length];
            Array.Copy(this.packetData, (long)index, destinationArray, 0L, (long)length);
            return destinationArray;
        }

        public string StationCode
        {
            get
            {
                byte[] strArray = new byte[2];
                Array.Copy(packetData, 1, strArray, 0, 2);
                string result = Encoding.ASCII.GetString(strArray);
                return result;
            }
        }
        public string SensorID
        {
            get
            {
                byte[] strArray = new byte[1];
                Array.Copy(packetData, 3, strArray, 0, 1);
                string result = Encoding.ASCII.GetString(strArray);
                return result;
            }
        }
        public DateTime TimeStamp
        {
            get
            {
                int YY = System.Convert.ToInt32(packetData[12]) + 2000;
                int MM = System.Convert.ToInt32(packetData[13]);
                int DD = System.Convert.ToInt32(packetData[14]);
                int HH = System.Convert.ToInt32(packetData[15]);
                int mm = System.Convert.ToInt32(packetData[16]);
                int ss = System.Convert.ToInt32(packetData[17]);
                DateTime measureTimes = new DateTime(YY, MM, DD, HH, mm, ss, DateTimeKind.Local);
                return measureTimes;
            }
        }

        public int SamplingRate
        {
            get
            {
                byte[] headerFrame = new byte[20];
                Array.Copy(this.packetData, 0, headerFrame, 0, 20);

                int CurrentSamplingRate = headerFrame[18];
                switch (CurrentSamplingRate)
                {
                    case 48:
                        CurrentSamplingRate = 20;
                        break;
                    case 49:
                        CurrentSamplingRate = 50;
                        break;
                    case 50:
                        CurrentSamplingRate = 100;
                        break;
                    case 51:
                        CurrentSamplingRate = 200;
                        break;
                }

                return CurrentSamplingRate;
            }
        }

    }
}
