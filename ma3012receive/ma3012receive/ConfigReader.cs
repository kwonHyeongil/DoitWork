using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ma3012receive
{
    public class ConfigReader
    {
        public string BindingIP = "";
        public ushort BindingPort = 0;
        public string RemoteIP = "";
        public string NETCode = "";
        public string StationCode = "";
        public string LocationCode = "";
        public string[] ChannelName = {"HGE", "HGN", "HGZ"};
        public double ScaleFactor = 0;
        public bool MSEEDRecordig = false;
        //public string EW_HOME = "";
        //public string EW_PARAMS = "";
        //public string TANKSavePath = "";
        //public string MSEEDSavePath = "";
        public bool DBRecordig = false;
        //public string DB_Host = "";
        //public string DB_Port = "";
        //public string DB_ID = "";
        //public string DB_PW = "";
        //public string PY_HOME = "";


        public bool StreamReader(string path)
        {
            bool Flag = false;
            try
            {
                FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.Default);

                Char[] delimiters = { ' ', '\t', '\n' };
                while (sr.Peek() > -1)
                {
                    string context = sr.ReadLine().Trim();
                    if ((context != "") && (context.Substring(0, 1).ToString() != "#"))
                    {
                        string[] wordsSplit = context.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                        switch (wordsSplit[0])
                        {
                            case "BindingIP":
                                this.BindingIP = wordsSplit[1];
                                break;
                            case "BindingPort":
                                this.BindingPort = System.Convert.ToUInt16(wordsSplit[1]);
                                break;
                            case "RemoteIP":
                                this.RemoteIP = wordsSplit[1];
                                break;
                            case "NETCode":
                                this.NETCode = wordsSplit[1];
                                break;
                            case "StationCode":
                                this.StationCode = wordsSplit[1];
                                break;
                            case "LocationCode":
                                this.LocationCode = wordsSplit[1];
                                break;
                            case "ChannelName":
                                this.ChannelName[0] = wordsSplit[1];
                                this.ChannelName[1] = wordsSplit[2];
                                this.ChannelName[2] = wordsSplit[3];
                                break;
                            case "ScaleFactor":
                                this.ScaleFactor = System.Convert.ToDouble(wordsSplit[1]);
                                break;
                            case "MSEEDRecordig":
                                this.MSEEDRecordig = System.Convert.ToBoolean(wordsSplit[1]);
                                break;
                            //case "EW_HOME":
                            //    this.EW_HOME = wordsSplit[1];
                            //    break;
                            //case "EW_PARAMS":
                            //    this.EW_PARAMS = wordsSplit[1];
                            //    break;
                            //case "TANKSavePath":
                            //    this.TANKSavePath = wordsSplit[1];
                            //    break;
                            //case "MSEEDSavePath":
                            //    this.MSEEDSavePath = wordsSplit[1];
                            //    break;
                            //case "PY_HOME":
                            //    this.PY_HOME = wordsSplit[1];
                            //    break;

                            case "DBRecordig":
                                this.DBRecordig = System.Convert.ToBoolean(wordsSplit[1]);
                                break;
                            //case "DB_Host":
                            //    this.DB_Host = wordsSplit[1];
                            //    break;
                            //case "DB_Port":
                            //    this.DB_Port = wordsSplit[1];
                            //    break;
                            //case "DB_ID":
                            //    this.DB_ID = wordsSplit[1];
                            //    break;
                            //case "DB_PW":
                            //    this.DB_PW = wordsSplit[1];
                            //    break;
                        }
                    }
                }
                sr.Close();
                fs.Close();
                Flag = true;
            }
            catch
            {
                Flag = false;
            }
            return Flag;
        }


    }
}
