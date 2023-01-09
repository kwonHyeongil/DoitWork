using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ma3012receive
{
    [Serializable]
    public class Waveform : SerializationBase
    {
        // Fields
        private string channelName;
        private int channelNumber;
        private string channelUnit;
        private double[] data;
        private double maximum;
        private double minimum;
        private bool mustComputeAttributes;
        private double offset;
        private double rangeInUnits;
        public int position;

        // Methods
        private void ComputeAttributes()
        {
            lock (this)
            {
                if (this.mustComputeAttributes)
                {
                    if (this.data.Length > 0)
                    {
                        this.offset = 0.0;
                        this.maximum = double.MinValue;
                        this.minimum = double.MaxValue;
                        for (int i = 0; i < this.data.Length; i++)
                        {
                            this.offset += this.data[i];
                            if (this.data[i] > this.maximum)
                            {
                                this.maximum = this.data[i];
                               
                            }
                            if (this.data[i] < this.minimum)
                            {
                                this.minimum = this.data[i];
                          
                            }
                        }
                        this.offset /= (double)this.data.Length;
                    }
                    else
                    {
                        this.maximum = double.NaN;
                        this.minimum = double.NaN;
                        this.offset = double.NaN;
                    }
                    this.mustComputeAttributes = false;
                }
            }
        }

        public double[] GetData()
        {
            lock (this)
            {
                return this.data;
            }
        }

        public void SetData(double[] samples)
        {
            lock (this)
            {
                this.data = samples;
                this.mustComputeAttributes = true;
            }
        }
        
        protected override void SetDefaults()
        {
            this.mustComputeAttributes = true;
        }

        // Properties

        public double AbsoluteMaximum
        {
            get
            {
                return Math.Max(Math.Abs(this.Maximum), Math.Abs(this.Minimum));
            }
        }

        public double AbsoluteMaximumWithOutOffset
        {
            get
            {
                return Math.Max(Math.Abs(this.MaximumWithOutOffset), Math.Abs(this.MinimumWithOutOffset));
            }
        }

        public double AbsoluteMinimum
        {
            get
            {
                return Math.Min(Math.Abs(this.Maximum), Math.Abs(this.Minimum));
            }
        }

        public double AbsoluteMinimumWithOutOffset
        {
            get
            {
                return Math.Min(Math.Abs(this.MaximumWithOutOffset), Math.Abs(this.MinimumWithOutOffset));
            }
        }

        public string ChannelName
        {
            get
            {
                return this.channelName;
            }
            set
            {
                this.channelName = value;
            }
        }

        public int ChannelNumber
        {
            get
            {
                return this.channelNumber;
            }
            set
            {
                this.channelNumber = value;
            }
        }

        public string ChannelUnit
        {
            get
            {
                return this.channelUnit;
            }
            set
            {
                this.channelUnit = value;
            }
        }

        public double Maximum
        {
            get
            {
                this.ComputeAttributes();
                return this.maximum;
            }
        }

        public double MaximumWithOutOffset
        {
            get
            {
                return (this.Maximum - this.Offset);
            }
        }

        public double Minimum
        {
            get
            {
                this.ComputeAttributes();
                return this.minimum;
            }
        }

        public double MinimumWithOutOffset
        {
            get
            {
                return (this.Minimum - this.Offset);
            }
        }

        public int NumberOfSamples
        {
            get
            {
                lock (this)
                {
                    return this.data.Length;
                }
            }
        }

        public double Offset
        {
            get
            {
                this.ComputeAttributes();
                return this.offset;
            }
        }

        public double RangeInUnits
        {
            get
            {
                return this.rangeInUnits;
            }
            set
            {
                this.rangeInUnits = value;
            }
        }
        public int Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
            }
        }
    }
}
