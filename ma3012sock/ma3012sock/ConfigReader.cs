using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ma3012sock
{
    public class ConfigReader
    {
        public string BindingIP = "";
        public ushort BindingPort = 0;
        public string RemoteIP = "";
        public bool DirectConncetion = false;
        public string NETCode = "";
        public string StationCode = "";
        public string LocationCode = "";
        public List<string> DestinationIP = new List<string>();
        public List<int> DestinationPort = new List<int>();
        public List<string> EvtDestinationIP = new List<string>();
        public List<int> EvtDestinationPort = new List<int>();
        public byte[] startBytes = { 0x53, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };
        public byte[] endBytes = { 0x45, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30, 0x30 };

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
                            case "DirectConncetion":
                                this.DirectConncetion = System.Convert.ToBoolean(wordsSplit[1]);
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
                            case "Destination":
                                DestinationIP.Add(wordsSplit[2]);
                                DestinationPort.Add(System.Convert.ToInt32(wordsSplit[3]));
                                break;
                            case "EvtDestination":
                                EvtDestinationIP.Add(wordsSplit[2]);
                                EvtDestinationPort.Add(System.Convert.ToInt32(wordsSplit[3]));
                                break;

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
