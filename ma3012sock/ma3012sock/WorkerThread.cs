using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ma3012sock
{
    public class WorkerThread
    {
        // Fields
        private bool isRunning;
        private bool keepRunning;
        private object parameter;
        private ParameterizedThreadStart parameterizedThreadStart;
        private Thread thread;
        private ThreadStart threadStart;

        // Methods
        public WorkerThread(ThreadStart threadStart)
        {
            this.threadStart = threadStart;
        }

        public WorkerThread(ParameterizedThreadStart parameterizedThreadStart, object parameter)
        {
            this.parameterizedThreadStart = parameterizedThreadStart;
            this.parameter = parameter;
        }

        public void Start()
        {
            if (!this.IsRunning)
            {
                if (this.parameterizedThreadStart != null)
                {
                    this.thread = new Thread(this.parameterizedThreadStart);
                    this.thread.Name = this.parameterizedThreadStart.Method.ToString();
                }
                else
                {
                    this.thread = new Thread(this.threadStart);
                    this.thread.Name = this.threadStart.Method.ToString();
                }
                this.thread.IsBackground = true;
                this.keepRunning = true;
                this.isRunning = true;
                if (this.parameterizedThreadStart != null)
                {
                    this.thread.Start(this.parameter);
                }
                else
                {
                    this.thread.Start();
                }
            }
        }

        public void Stop()
        {
            this.keepRunning = false;
            while (this.isRunning)
            {
                Thread.Sleep(1000);
            }
            this.isRunning = false;
        }

        // Properties
        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
            set
            {
                this.isRunning = value;
            }
        }

        public bool KeepRunning
        {
            get
            {
                return this.keepRunning;
            }
            set
            {
                this.keepRunning = value;
            }
        }

        public Thread Thread
        {
            get
            {
                return this.thread;
            }
        }
    }


}
