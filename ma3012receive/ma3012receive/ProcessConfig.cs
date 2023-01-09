using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ma3012receive
{
    public class ProcessConfig
    {
        private string dBHOST = "";
        private string dBPORT = "";
        private string dBNAME = "";
        private string uSERID = "";
        private string uSERPW = "";
        private string eW_HOME = "";
        private string eW_PARAMS = "";
        private string pY_HOME = "";
        private string rESERVE1 = "";
        private string rESERVE2 = "";
        private string rESERVE3 = "";
        private string rESERVE4 = "";
        private string rESERVE5 = "";

        public string DBHOST
        {
            get
            {
                return this.dBHOST;
            }
            set
            {
                this.dBHOST = value;
            }
        }
        public string DBPORT
        {
            get
            {
                return this.dBPORT;
            }
            set
            {
                this.dBPORT = value;
            }
        }
        public string DBNAME
        {
            get
            {
                return this.dBNAME;
            }
            set
            {
                this.dBNAME = value;
            }
        }
        public string USERID
        {
            get
            {
                return this.uSERID;
            }
            set
            {
                this.uSERID = value;
            }
        }
        public string USERPW
        {
            get
            {
                return this.uSERPW;
            }
            set
            {
                this.uSERPW = value;
            }
        }
        public string EW_HOME
        {
            get
            {
                return this.eW_HOME;
            }
            set
            {
                this.eW_HOME = value;
            }
        }
        public string EW_PARAMS
        {
            get
            {
                return this.eW_PARAMS;
            }
            set
            {
                this.eW_PARAMS = value;
            }
        }
        public string PY_HOME
        {
            get
            {
                return this.pY_HOME;
            }
            set
            {
                this.pY_HOME = value;
            }
        }
        public string RESERVE1
        {
            get
            {
                return this.rESERVE1;
            }
            set
            {
                this.rESERVE1 = value;
            }
        }
        public string RESERVE2
        {
            get
            {
                return this.rESERVE2;
            }
            set
            {
                this.rESERVE2 = value;
            }
        }
        public string RESERVE3
        {
            get
            {
                return this.rESERVE3;
            }
            set
            {
                this.rESERVE3 = value;
            }
        }
        public string RESERVE4
        {
            get
            {
                return this.rESERVE4;
            }
            set
            {
                this.rESERVE4 = value;
            }
        }
        public string RESERVE5
        {
            get
            {
                return this.rESERVE5;
            }
            set
            {
                this.rESERVE5 = value;
            }
        }
    }
}
