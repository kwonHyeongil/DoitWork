using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ma3012sock
{
    public class EventDataReply : Packet
    {
        public EventDataReply(byte[] bytes)
            : base(bytes)
        {
        }

        // Properties
        public string EventType
        {
            get
            {
                return base.GetString(0);
            }
        }
        public string StationID
        {
            get
            {
                return base.GetString(1, 2);
            }

        }
        public string SensorID
        {
            get
            {
                return base.GetString(3);
            }
        }
        public DateTime TimeStamp
        {
            get
            {
                byte[] timeStamp = base.GetBytes(4, 7);
                int YY = System.Convert.ToInt32(timeStamp[0]) + 2000;
                int MM = System.Convert.ToInt32(timeStamp[1]);
                int DD = System.Convert.ToInt32(timeStamp[2]);
                int HH = System.Convert.ToInt32(timeStamp[3]);
                int mm = System.Convert.ToInt32(timeStamp[4]);
                int ss = System.Convert.ToInt32(timeStamp[5]);
                int ms = System.Convert.ToInt32(timeStamp[6]) * 10;
                DateTime eventTimes = new DateTime(YY, MM, DD, HH, mm, ss, ms, DateTimeKind.Local);
                return eventTimes;
            }
        }
        public byte PGACoordinate
        {
            get
            {
                return base.GetByte(11);
            }
        }
        public float PGAMaxValue
        {
            get
            {
                return GetFloat_NONESwap(12);
            }
        }
        public byte PDCoordinate
        {
            get
            {
                return base.GetByte(16);
            }
        }
        public float PDMaxValue
        {
            get
            {
                return GetFloat_NONESwap(17);
            }
        }

    }
}
