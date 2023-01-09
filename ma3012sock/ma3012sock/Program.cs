using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;

namespace ma3012sock
{
    public class Program
    {
        //설정 파라미터
        private string BindingIP = "";
        private ushort BindingPort = 0;
        private string RemoteIP = "";
        public bool DirectConncetion = false;
        public string NETCode = "";
        public string StationCode = "";
        public string LocationCode = "";
        public List<string> DestinationIP = new List<string>();
        public List<int> DestinationPort = new List<int>();
        public List<string> EvtDestinationIP = new List<string>();
        public List<int> EvtDestinationPort = new List<int>();

        private Socket socket;
        private Thread readerThread;
        private bool isConnected = false;
        private WorkerThread bufferReaderThread;
        public List<byte[]> receivebuf = new List<byte[]>();
        public List<EventData> sendingData = new List<EventData>();

        private Socket[] _sockets;
        private bool[] _socketsFlag;

        private Socket[] _evt_sockets;
        private bool[] _evt_socketsFlag;

        private byte[] startBytes;
        private byte[] endBytes;


        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Program ServerSock = new Program();
                string config = args[0].ToString().Trim(); //"ma3012sock_NGA.d"; 
                                                           //string config = "ma3012sock_NGA.d";

                if (!File.Exists(config))
                {
                    Console.WriteLine(config + " 설정파일이 존재하지 않습니다. 확인바랍니다");
                    Environment.Exit(0);
                    return;
                }

                try
                {
                    ConfigReader configStream = new ConfigReader();
                    bool ckFlag = configStream.StreamReader(config);
                    if (!ckFlag)
                    {
                        Console.WriteLine(config + " 프로세스 설정파일 로드에 오류가 발생하였습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.BindingIP == "")
                    {
                        Console.WriteLine(config + "BindingIP 설정이 잘못되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.BindingPort == 0)
                    {
                        Console.WriteLine(config + "BindingPort 설정이 잘못되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.RemoteIP == "")
                    {
                        Console.WriteLine(config + "RemoteIP 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.NETCode == "")
                    {
                        Console.WriteLine(config + "NETCode 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.StationCode == "")
                    {
                        Console.WriteLine(config + "StationCode 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.LocationCode == "")
                    {
                        Console.WriteLine(config + "LocationCode 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.DestinationIP.Count == 0)
                    {
                        Console.WriteLine(config + "DestinationIP 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.DestinationPort.Count == 0)
                    {
                        Console.WriteLine(config + "DestinationPort 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    if (configStream.DestinationIP.Count != configStream.DestinationPort.Count)
                    {
                        Console.WriteLine(config + "프로세스 설정파일 중 데이터 전송목적지 로드에 오류가 발생하였습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    ServerSock.socket = null;
                    ServerSock.BindingIP = configStream.BindingIP;
                    ServerSock.BindingPort = configStream.BindingPort;
                    ServerSock.RemoteIP = configStream.RemoteIP;
                    ServerSock.DirectConncetion = configStream.DirectConncetion;
                    ServerSock.NETCode = configStream.NETCode;
                    ServerSock.StationCode = configStream.StationCode;
                    ServerSock.LocationCode = configStream.LocationCode;
                    ServerSock.DestinationIP = configStream.DestinationIP;
                    ServerSock.DestinationPort = configStream.DestinationPort;
                    ServerSock.EvtDestinationIP = configStream.EvtDestinationIP;
                    ServerSock.EvtDestinationPort = configStream.EvtDestinationPort;
                    ServerSock._sockets = new Socket[ServerSock.DestinationIP.Count];
                    ServerSock._socketsFlag = new bool[ServerSock.DestinationIP.Count];
                    ServerSock._evt_sockets = new Socket[ServerSock.EvtDestinationIP.Count];
                    ServerSock._evt_socketsFlag = new bool[ServerSock.EvtDestinationIP.Count];
                    ServerSock.startBytes = configStream.startBytes;
                    ServerSock.endBytes = configStream.endBytes;

                    for (int i = 0; i < ServerSock.DestinationIP.Count; i++)
                    {
                        ServerSock._socketsFlag[i] = ServerSock.Server_ConnectSocket(i, ServerSock.DestinationIP[i], ServerSock.DestinationPort[i]);
                    }

                    if (ServerSock.ConnectSilent())
                    {
                        ServerSock.bufferReaderThread = new WorkerThread(new ThreadStart(ServerSock.BufferReaderThreadStart));
                        ServerSock.receivebuf = new List<byte[]>();
                        ServerSock.sendingData = new List<EventData>();
                        ServerSock.bufferReaderThread.Start();

                        while (ServerSock.readerThread != null && ServerSock.readerThread.IsAlive && ServerSock.bufferReaderThread.KeepRunning)
                        {
                            Thread.Sleep(1000);
                        }
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("ma3012sock : IPAddress : " + configStream.BindingIP + " PORT : " + System.Convert.ToString(configStream.BindingPort) + " RemoteIP : " + configStream.RemoteIP + " 연결 오류 ");
                        ServerSock.Disconnect();
                        Environment.Exit(0);
                    }

                }
                catch (Exception o)
                {
                    Console.WriteLine(o.Message);
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("ma3012sock process 설정파일 누락되었습니다");
                Environment.Exit(0);
            }


        }
        private bool Server_ConnectSocket(int index, string si_ip, int si_port)
        {
            bool flag = false;
            try
            {
                _sockets[index] = null;
                _sockets[index] = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _sockets[index].Connect(IPAddress.Parse(si_ip), si_port);
                flag = true;
            }
            catch (Exception o)
            {
                Console.WriteLine("ma3012sock: Destination Socket connection error! " + o.Message);
                Environment.Exit(0);
                flag = false;
            }
            return flag;
        }
        private bool Event_ConnectSocket(int index, string si_ip, int si_port)
        {
            bool flag = false;
            try
            {
                _evt_sockets[index] = null;
                _evt_sockets[index] = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _evt_sockets[index].Connect(IPAddress.Parse(si_ip), si_port);
                flag = true;
            }
            catch (Exception o)
            {
                Console.WriteLine("ma3012sock: Event Destination Socket connection error! " + o.Message);
                flag = false;
            }
            return flag;
        }
        
        private bool Event_DisConnectSocket(int index)
        {
            bool flag = false;
            try
            {
                if (_evt_sockets[index] != null)
                {
                    _evt_sockets[index].Close();
                    flag = true;
                }
                
            }
            catch (Exception o)
            {
                Console.WriteLine("ma3012sock: Event Destination Socket disconnection error! " + o.Message);
                flag = false;
            }
            return flag;
        }

        public bool ConnectSilent()
        {
            bool socket_ck = false;
            try
            {
                socket_ck = ConnectSocket();
                if (socket_ck)
                {
                    this.readerThread = new Thread(new ThreadStart(this.ReaderThread));
                    this.readerThread.Name = string.Format("{0}-udpBrodcastReader", this);
                    this.readerThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
                    this.readerThread.IsBackground = false;
                    this.isConnected = true;
                    this.readerThread.Start();
                }
            }
            catch(Exception o)
            {
                Console.WriteLine(o.Message);
                return socket_ck;
            }
            return socket_ck;
        }

        public bool ConnectSocket()
        {
            try
            {
                this.socket = new Socket(IPAddress.Parse(this.RemoteIP).AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                this.socket.DontFragment = true;
                this.socket.EnableBroadcast = true;
                this.socket.MulticastLoopback = false;
                IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(this.BindingIP), this.BindingPort);
                this.socket.Bind(localEP);
            }
            catch (Exception o)
            {
                Console.WriteLine("ma3012sock(ConnectSock) error:" + o.Message);
                return false;
            }
            return true;
        }



        public void Disconnect()
        {
            try
            {
                this.isConnected = false;
                if(this.readerThread.IsAlive)
                {
                    this.readerThread.Abort();
                }
                this.socket.Close();
            }
            catch (Exception)
            {
            }
        }
        
        private void ReaderThread()
        {
            byte[] buffer = new byte[1220];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, this.BindingPort);
            byte[] destinationArray = new byte[1220];
            this.socket.ReceiveTimeout = 5000;
            try
            {
                while (this.isConnected)
                {
                    try
                    {
                        if (this.socket != null)
                        {
                            int length = this.socket.ReceiveFrom(buffer, SocketFlags.None, ref remoteEP);
                            if (length == 1220)  //실시간 데이터 처리
                            {
                                if (this.bufferReaderThread != null)
                                {
                                    if (buffer[18] != 51)
                                    {
                                        destinationArray = new byte[length];
                                        Array.Copy(buffer, destinationArray, length);
                                        receivebuf.Add(destinationArray);
                                    }
                                    else
                                    {
                                        if (buffer[19] == 48)
                                        {
                                            destinationArray = new byte[length * 2];
                                            Array.Copy(buffer, 0, destinationArray, 0, length);
                                        }
                                        else
                                        {
                                            Array.Copy(buffer, 0, destinationArray, length, length);
                                            receivebuf.Add(destinationArray);
                                        }
                                    }
                                }
                                else
                                {
                                    this.bufferReaderThread = new WorkerThread(new ThreadStart(this.BufferReaderThreadStart));
                                    this.bufferReaderThread.Start();
                                }
                            }
                            else if (length == 21)
                            {
                                destinationArray = new byte[length];
                                Array.Copy(buffer, destinationArray, length);
                                EventDataReply event_reply = new EventDataReply(destinationArray);
                                if (event_reply != null)
                                {
                                    Thread senderingDataBufferThread = new Thread(new ParameterizedThreadStart(SenderingThreadStart));
                                    senderingDataBufferThread.Start(event_reply);
                                }
                            }
                        }
                        else
                        {
                            bool ck = ConnectSocket();
                            if (!ck)
                            {
                                Console.WriteLine("ma3012sock: Destination Socket reconnection error! ");
                                Environment.Exit(0);
                            }
                        }
                    }
                    catch (SocketException o)
                    {
                        try
                        {
                            if (o.SocketErrorCode == SocketError.TimedOut)
                            {
                                if (this.socket != null)
                                {
                                    if (this.DirectConncetion)
                                    {
                                        DeviceSend(this.endBytes);
                                        Thread.Sleep(100);
                                        DeviceSend(this.startBytes);
                                    }
                                }
                                else
                                {
                                    this.readerThread.Abort();
                                    Environment.Exit(0);
                                    return;
                                }
                                //this.socket.Close();
                                //Thread.Sleep(10);
                                //bool re_ck = ConnectSocket();
                                //if (!re_ck)
                                //{
                                //    Environment.Exit(0);
                                //    return;
                                //}
                                //else
                                //{
                                //    if (this.DirectConncetion)
                                //    {
                                //        if (this.socket != null)
                                //        {
                                //            DeviceSend(this.endBytes);
                                //            Thread.Sleep(100);
                                //            DeviceSend(this.startBytes);
                                //        }
                                //    }
                                //}
                            }
                            else
                            {
                                //Environment.Exit(0);
                                //return;
                                if (this.DirectConncetion)
                                {
                                    DeviceSend(this.endBytes);
                                    Thread.Sleep(100);
                                    DeviceSend(this.startBytes);
                                }
                            }
                        }
                        catch
                        {
                            Environment.Exit(0);
                        }
                    }
                    finally
                    {
                        while (this.receivebuf.Count > 100)
                        {
                            this.receivebuf.RemoveAt(0);
                        }
                    }
                }
            }
            catch
            {
                Environment.Exit(0);
            }
        }

        private void SenderingThreadStart(object realData)
        {
            try
            {
                EventDataReply eventData = (EventDataReply)realData;
                if (eventData != null)
                {
                    for (int i = 0; i < this.EvtDestinationIP.Count; i++)
                    {
                        this._evt_socketsFlag[i] = this.Event_ConnectSocket(i, this.EvtDestinationIP[i], this.EvtDestinationPort[i]);
                        if (this._evt_socketsFlag[i])
                        {
                            this.evt_Send(i, eventData.Data);
                            Thread.Sleep(10);
                            this.Event_DisConnectSocket(i);
                        }
                    }
                    eventData = null;
                }
            }
            catch (Exception o)
            {
                Console.WriteLine("ma3012sock: ma3012evtdetect sender error ! : " + o.Message);
            }
        }

        private void BufferReaderThreadStart()
        {
            this.bufferReaderThread.IsRunning = true;
            try
            {
                while (this.bufferReaderThread.KeepRunning)
                {
                    int num = this.receivebuf.Count;
                    try
                    {
                        if (num > 0)
                        {
                            try
                            {
                                if (this._sockets == null)
                                {
                                    for (int i = 0; i < this.DestinationIP.Count; i++)
                                    {
                                        this._socketsFlag[i] = this.Server_ConnectSocket(i, this.DestinationIP[i], this.DestinationPort[i]);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < num; i++)
                                    {
                                        for (int j = 0; j < this._sockets.Length; j++)
                                        {
                                            if (this._socketsFlag[j])
                                            {
                                                this.Send(j, this.receivebuf[i]);
                                            }
                                        }
                                        //this.receivebuf.RemoveAt(0);
                                        this.bufferReaderThread.Thread.Join(10);
                                    }
                                    this.receivebuf.RemoveRange(0, num);
                                }

                            }
                            catch (Exception o)
                            {
                                Console.WriteLine("ma3012sock: BufferSenderThread Error " + o.Message);
                                Environment.Exit(0); // 신규 추가
                            }
                            finally
                            {
                                this.bufferReaderThread.Thread.Join(100);
                            }
                        }
                        else
                        {
                            this.bufferReaderThread.Thread.Join(100);
                        }
                    }
                    catch (Exception b)
                    {
                        Console.WriteLine("ma3012sock: BufferSenderThread Error1 " + b.Message);
                    }

                }
                this.bufferReaderThread.IsRunning = false;
            }
            catch
            {
                Environment.Exit(0); // 신규 추가
            }
        }

        public void DeviceSend(byte[] bytes)
        {
            try
            {
                IPEndPoint devicePoint = new IPEndPoint(IPAddress.Parse(this.RemoteIP), this.BindingPort);
                this.socket.SendTo(bytes, SocketFlags.None, devicePoint);
            }
            catch (Exception)
            {
                Thread.Sleep(10);
            }
        }

        public void Send(int index, byte[] bytes)
        {
            try
            {
                if (this._sockets[index] != null)
                {
                    this._sockets[index].Send(bytes, bytes.Length, SocketFlags.None);
                }
            }
            catch (Exception)
            {
                this._sockets[index].Close();
                Thread.Sleep(10);
                this._socketsFlag[index] = this.Server_ConnectSocket(index, this.DestinationIP[index], this.DestinationPort[index]);
            }
        }

        public void evt_Send(int index, byte[] bytes)
        {
            try
            {
                if (this._evt_sockets[index] != null)
                {
                    this._evt_sockets[index].Send(bytes, bytes.Length, SocketFlags.None);
                }
            }
            catch (Exception)
            {
                this._evt_sockets[index].Close();
                Thread.Sleep(10);
                this._evt_socketsFlag[index] = this.Event_ConnectSocket(index, this.EvtDestinationIP[index], this.EvtDestinationPort[index]);
            }
        }

    }
}
