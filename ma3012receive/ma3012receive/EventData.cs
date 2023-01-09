using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ma3012receive
{
    [Serializable]
    public class EventData : SerializationBase
    {
        // Fields
        private uint eventId;
        private long firstSampleNumber;
        private long lastSampleNumber;
        private uint samplingRateinHz;
        private DateTime startTime;
        private List<Waveform> waveformList;
        private string stationCode;
        private string sensorID;

        // Methods
        public void AddWaveform(Waveform waveform)
        {
            lock (this)
            {
                foreach (Waveform waveform2 in this.waveformList)
                {
                    if (waveform2.ChannelNumber == waveform.ChannelNumber)
                    {
                        throw new ArgumentException("ChannelNumber already in collection");
                    }
                    if (waveform2.NumberOfSamples != waveform.NumberOfSamples)
                    {
                        throw new ArgumentException("Number of samples different");
                    }
                }
                this.waveformList.Add(waveform);
            }
        }

        public Waveform GetWaveform(int index)
        {
            return this.waveformList[index];
        }

        protected override void SetDefaults()
        {
            this.waveformList = new List<Waveform>();
        }

        // Properties

        public uint EventId
        {
            get
            {
                return this.eventId;
            }
            set
            {
                this.eventId = value;
            }
        }

        public long FirstSampleNumber
        {
            get
            {
                return this.firstSampleNumber;
            }
            set
            {
                this.firstSampleNumber = value;
            }
        }

        public long LastSampleNumber
        {
            get
            {
                return this.lastSampleNumber;
            }
            set
            {
                this.lastSampleNumber = value;
            }
        }

        public int NumberOfWaveforms
        {
            get
            {
                return this.waveformList.Count;
            }
        }

        public string StationCode
        {
            get
            {
                return this.stationCode;
            }
            set
            {
                this.stationCode = value;
            }
        }
        public string SensorID
        {
            get
            {
                return this.sensorID;
            }
            set
            {
                this.sensorID = value;
            }
        }

        public uint SamplingRateinHz
        {
            get
            {
                return this.samplingRateinHz;
            }
            set
            {
                this.samplingRateinHz = value;
            }
        }


        public DateTime Starttime
        {
            get
            {
                return this.startTime;
            }
            set
            {
                this.startTime = value;
            }
        }
    }
}
