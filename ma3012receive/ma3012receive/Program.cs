using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Win32;
using MySql.Data.MySqlClient;

namespace ma3012receive
{
    public class Program
    {
        //설정 파라미터
        public string BindingIP = "";
        public ushort BindingPort = 0;
        public string RemoteIP = "";
        public string NETCode = "";
        public string StationCode = "";
        public string LocationCode = "";
        public string[] ChannelName = { "HGE", "HGN", "HGZ" };
        public double ScaleFactor = 0;
        public bool MSEEDRecordig = false;
        public bool DBRecordig = false;
        private ProcessConfig ProcessConfig = null;
        //public string EW_HOME = "";
        //public string EW_PARAMS = "";
        //public string TANKSavePath = "";
        //public string MSEEDSavePath = "";

        //public string DB_Host = "";
        //public string DB_Port = "";
        //public string DB_ID = "";
        //public string DB_PW = "";
        //public string PYTHON_HOME = "";
        //public string PY_HOME = "";

        private Socket socket;
        private Thread readerThread;
        private bool isConnected = false;
        private WorkerThread sendingDataBufferThread;
        private WorkerThread recordingDataBufferThread;
        private WorkerThread bufferReaderThread;
        public List<byte[]> receivebuf = new List<byte[]>();
        public List<EventData> recordingData = new List<EventData>();
        public List<EventData> sendingData = new List<EventData>();
        public double dayMaxTemp = 0.0;
        public DateTime preTime = new DateTime();
        MySqlConnection m_db;
        public string db_conString = "";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Program ServerSock = new Program();
                string config = args[0].ToString().Trim(); //"ma3012receive_NGA.d"; 
                                                           //string config = "ma3012receive_NGA.d"; 

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
                        Console.WriteLine(config + "RemoteIP 설정이 잘못되었습니다.");
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
                    if (configStream.ScaleFactor == 0)
                    {
                        Console.WriteLine(config + "ScaleFactor 설정이 누락되었습니다.");
                        Environment.Exit(0);
                        return;
                    }
                    //if (configStream.EW_HOME == "")
                    //{
                    //    Console.WriteLine(config + "EW_HOME 설정이 누락되었습니다.");
                    //    Environment.Exit(0);
                    //    return;
                    //}
                    //if (configStream.EW_PARAMS == "")
                    //{
                    //    Console.WriteLine(config + "EW_PARAMS 설정이 누락되었습니다.");
                    //    Environment.Exit(0);
                    //    return;
                    //}
                    //if (configStream.TANKSavePath == "")
                    //{
                    //    Console.WriteLine(config + "TANKSavePath 설정이 누락되었습니다.");
                    //    Environment.Exit(0);
                    //    return;
                    //}
                    //if (configStream.MSEEDSavePath == "")
                    //{
                    //    Console.WriteLine(config + "MSEEDSavePath 설정이 누락되었습니다.");
                    //    Environment.Exit(0);
                    //    return;
                    //}
                    //if (configStream.PY_HOME == "")
                    //{
                    //    Console.WriteLine(config + "PY_HOME 설정이 누락되었습니다.");
                    //    Environment.Exit(0);
                    //    return;
                    //}
                    //if (configStream.DBRecordig)
                    //{
                    //    if ((configStream.DB_Host == "") || (configStream.DB_Port == ""))
                    //    {
                    //        Console.WriteLine(config + "DB connection configuation(DB_Host, DB_Port) error!");
                    //        return;
                    //    }
                    //    if ((configStream.DB_ID == "") || (configStream.DB_PW == ""))
                    //    {
                    //        Console.WriteLine(config + "DB connection configuation(DB_ID, DB_PW) error!");
                    //        return;
                    //    }
                    //}

                    ServerSock.BindingIP = configStream.BindingIP;
                    ServerSock.BindingPort = configStream.BindingPort;
                    ServerSock.RemoteIP = configStream.RemoteIP;
                    ServerSock.NETCode = configStream.NETCode;
                    ServerSock.StationCode = configStream.StationCode;
                    ServerSock.LocationCode = configStream.LocationCode;
                    ServerSock.ChannelName = configStream.ChannelName;
                    ServerSock.ScaleFactor = configStream.ScaleFactor;
                    ServerSock.MSEEDRecordig = configStream.MSEEDRecordig;
                    ServerSock.DBRecordig = configStream.DBRecordig;
                    ServerSock.db_conString = "Server=127.0.0.1;Port=3306; Database=seis_monitoring;Uid=sa; Pwd=power88;CharSet=utf8";

                    //1.DB로부터 프로세스 설정값 불러오기
                    object[] alData_pro = ServerSock.ProcessConfigLoad();
                    if ("00" != alData_pro[0].ToString() || "0" == alData_pro[3].ToString())
                    {
                        Console.WriteLine("ma3012evtproc error: DB에서프로세스 설정값을 불러올 수 없습니다. 시스템 설정>> 프로세스 환경설정이 등록되었는지 확인 바랍니다");
                        Environment.Exit(0);
                    }
                    else
                    {
                        ProcessConfig processConfig = new ProcessConfig
                        {
                            DBHOST = alData_pro[4].ToString(),
                            DBPORT = alData_pro[5].ToString(),
                            DBNAME = alData_pro[6].ToString(),
                            USERID = alData_pro[7].ToString(),
                            USERPW = alData_pro[8].ToString(),
                            EW_HOME = alData_pro[9].ToString(),
                            EW_PARAMS = alData_pro[10].ToString(),
                            PY_HOME = alData_pro[11].ToString(),
                            RESERVE1 = alData_pro[12].ToString(),
                            RESERVE2 = alData_pro[13].ToString(),
                            RESERVE3 = alData_pro[14].ToString(),
                            RESERVE4 = alData_pro[15].ToString(),
                            RESERVE5 = alData_pro[16].ToString()
                        };
                        ServerSock.ProcessConfig = processConfig;
                    }
                    if (ServerSock.ProcessConfig == null)
                    {
                        Console.WriteLine("ma3012evtproc error: DB에서 프로세스 설정값을 불러올 수 없습니다. 시스템 설정>> 프로세스 환경설정이 등록되었는지 확인 바랍니다");
                        Environment.Exit(0);
                    }
                    else
                    {
                        if (!Directory.Exists(ServerSock.ProcessConfig.EW_HOME))
                        {
                            Console.WriteLine("ma3012receive: 시스템 설정>> 프로세스 환경설정의 EW_HOME 경로 정보가 정확하지 않습니다. 확인바랍니다. ");
                            Environment.Exit(0);
                        }
                        if (!Directory.Exists(ServerSock.ProcessConfig.EW_PARAMS))
                        {
                            Console.WriteLine("ma3012receive: 시스템 설정>> 프로세스 환경설정의 EW_PARAMS 경로 정보가 정확하지 않습니다. 확인바랍니다. ");
                            Environment.Exit(0);
                        }

                        if ((ServerSock.ProcessConfig.RESERVE4 == "") || (ServerSock.ProcessConfig.RESERVE5 == ""))
                        {
                            Console.WriteLine("ma3012receive: 시스템 설정>> 프로세스 환경설정의 초당 미니시드 혹은 Tank파일 저장 경로 정보가 정확하지 않습니다. 확인바랍니다. ");
                            Environment.Exit(0);
                        }

                        // 2. 파이썬 경로 확인하기
                        string pythonRoot = ServerSock.ProcessConfig.PY_HOME;
                        if (File.Exists(pythonRoot))
                        {
                            if ((pythonRoot != "") || (pythonRoot != null))
                            {
                                string pythonSrc = Path.GetDirectoryName(pythonRoot);
                                string PYTHON_HOME = Environment.ExpandEnvironmentVariables(pythonSrc);

                                AddEnvPath(PYTHON_HOME, Path.Combine(PYTHON_HOME, @"Library\bin"));
                                // Python 홈 설정.
                                PythonEngine.PythonHome = PYTHON_HOME;
                                // 모듈 패키지 패스 설정.
                                PythonEngine.PythonPath = string.Join(
                                Path.PathSeparator.ToString(),
                                new string[] {
                            PythonEngine.PythonPath,
                            // pip하면 설치되는 패키지 폴더.
                            Path.Combine(PYTHON_HOME, @"Lib\site-packages"),
                            Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory),
                            ServerSock.ProcessConfig.EW_HOME
                                }
                                );
                                if (!PythonEngine.IsInitialized)
                                {
                                    PythonEngine.Initialize();
                                    PythonEngine.BeginAllowThreads();
                                }
                            }
                            else
                            {
                                Console.WriteLine("ma3012receive: Python 설치경로 정보를 확인할 수 없습니다!");
                                Environment.Exit(0);
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("ma3012receive: 시스템 설정>> 프로세스 환경설정의 Python 설치경로 정보가 정확하지 않습니다. 확인바랍니다. ");
                            Environment.Exit(0);
                        }
                    }


                    //PythonEngine.Shutdown();
                    //ServerSock.BindingIP = configStream.BindingIP;
                    //ServerSock.BindingPort = configStream.BindingPort;
                    //ServerSock.RemoteIP = configStream.RemoteIP;
                    //ServerSock.NETCode = configStream.NETCode;
                    //ServerSock.StationCode = configStream.StationCode;
                    //ServerSock.LocationCode = configStream.LocationCode;
                    //ServerSock.ChannelName = configStream.ChannelName;
                    //ServerSock.ScaleFactor = configStream.ScaleFactor;
                    //ServerSock.MSEEDSavePath = configStream.MSEEDSavePath;
                    //ServerSock.EW_HOME = configStream.EW_HOME;
                    //ServerSock.EW_PARAMS = configStream.EW_PARAMS;
                    //ServerSock.TANKSavePath = configStream.TANKSavePath;
                    //ServerSock.MSEEDRecordig = configStream.MSEEDRecordig;
                    //ServerSock.DBRecordig = configStream.DBRecordig;
                    //ServerSock.DB_Host = configStream.DB_Host;
                    //ServerSock.DB_Port = configStream.DB_Port;
                    //ServerSock.DB_ID = configStream.DB_ID;
                    //ServerSock.DB_PW = configStream.DB_PW;
                    //if ((ServerSock.DB_Host != "") && (ServerSock.DB_Port != ""))
                    //{
                    //    ServerSock.db_conString = "Server=" + ServerSock.DB_Host + ";Port=" + ServerSock.DB_Port + ";Database=seis_monitoring;Uid=" + ServerSock.DB_ID + ";Pwd=" + ServerSock.DB_PW + ";CharSet=utf8";
                    //}

                    if (ServerSock.ConnectSilent())
                    {
                        ServerSock.receivebuf = new List<byte[]>();
                        ServerSock.recordingData = new List<EventData>();
                        ServerSock.sendingData = new List<EventData>();

                        ServerSock.bufferReaderThread = new WorkerThread(new ThreadStart(ServerSock.BufferReaderThreadStart));
                        ServerSock.recordingDataBufferThread = new WorkerThread(new ThreadStart(ServerSock.RecordingThreadStart));
                        ServerSock.sendingDataBufferThread = new WorkerThread(new ThreadStart(ServerSock.SendingThreadStart));

                        //파리미터값 읽어서 ReCording enable이면 스레드 시작
                        if (ServerSock.MSEEDRecordig)
                        {
                            try
                            {
                                ServerSock.recordingDataBufferThread.Start();
                            }
                            catch (Exception m)
                            {
                                Console.WriteLine("ma3012receive: MSEEDThread start error : " + m.Message);
                                Environment.Exit(0);
                            }
                        }
                        //파리미터값 읽어서 서버(DB) 저장이 enable이면 스레드 시작
                        if (ServerSock.DBRecordig)
                        {
                            try
                            {
                                ServerSock.sendingDataBufferThread.Start();
                            }
                            catch (Exception s)
                            {
                                Console.WriteLine("ma3012receive: DBRecordigThread start error : " + s.Message);
                                Environment.Exit(0);
                            }
                        }
                        ServerSock.bufferReaderThread.Start();

                        while (ServerSock.readerThread != null && ServerSock.readerThread.IsAlive)
                        {
                            Thread.Sleep(1000);
                        }
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("ma3012receive:  BindingIP : " + configStream.BindingIP + " PORT : " + System.Convert.ToString(configStream.BindingPort) + " RemoteIP:" + configStream.RemoteIP + " 연결 오류 ");
                        ServerSock.Disconnect();
                        Environment.Exit(0);

                    }
                }
                catch (Exception o)
                {
                    Console.WriteLine("ma3012receive: " + o.Message);
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("ma3012receive process 설정파일이 누락되었습니다: ");
                Environment.Exit(0);
            }


        }
        static string GetPythonExecutablePath(int major = 3)
        {

            var software = "SOFTWARE";
            var key = Registry.CurrentUser.OpenSubKey(software);
            if (key == null)
                key = Registry.LocalMachine.OpenSubKey(software);
            if (key == null)
                return null;

            var pythonCoreKey = key.OpenSubKey(@"Python\PythonCore");
            if (pythonCoreKey == null)
                pythonCoreKey = key.OpenSubKey(@"Wow6432Node\Python\PythonCore");
            if (pythonCoreKey == null)
                return null;

            var targetVersion = pythonCoreKey.GetSubKeyNames()[0];

            var installPathKey = pythonCoreKey.OpenSubKey(targetVersion + @"\InstallPath");
            if (installPathKey == null)
                return null;

            return (string)installPathKey.GetValue("ExecutablePath");
            
        }
        public bool ConnectSilent()
        {
            bool socket_ck = false;
            try
            {
                socket_ck = ConnectSocket(); //서버소켓 연결
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
            catch (Exception o)
            {
                Console.WriteLine("ma3012receive(ConnectSilent) error : " + o.Message);
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
                Console.WriteLine("ma3012receive(ConnectSock) error:" + o.Message);
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            try
            {
                this.isConnected = false;
                this.readerThread.Abort();
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
                            if (length >= 1220)  //실시간 데이터 처리
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
                        }
                        else
                        {
                            bool ck = ConnectSocket();
                            if (!ck)
                            {
                                Console.WriteLine("ma3012receive: Socket reconnection error! ");
                                Environment.Exit(0);
                            }
                        }
                    }
                    catch (SocketException o)
                    {
                        if (o.SocketErrorCode == SocketError.TimedOut)
                        {
                            Console.WriteLine("ma3012receive: 5s Time Delay occured!! retry connection  ");
                            Thread.Sleep(10);
                            
                            //this.socket.Close();
                            //Thread.Sleep(10);
                            //bool re_ck = ConnectSocket();
                            //if (!re_ck)
                            //{
                            //    Environment.Exit(0);
                            //}
                        }
                        else 
                        {
                            Console.WriteLine("ma3012receive error: " + o.Message);
                            this.readerThread.Abort();
                            Environment.Exit(0); // 신규추가
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
            catch(Exception u)
            {
                Console.WriteLine("ma3012receive error :" + u.Message );
                Environment.Exit(0);
            }
        }

        private void BufferReaderThreadStart()
        {
            this.bufferReaderThread.IsRunning = true;
            try
            {
                TransferBuffer buffer = new TransferBuffer();
                while (this.bufferReaderThread.KeepRunning)
                {

                    int num = this.receivebuf.Count;

                    if (num > 0)
                    {
                        try
                        {
                            for (int i = 0; i < num; i++)
                            {
                                buffer = new TransferBuffer();
                                buffer.Push(this.receivebuf[i]);
                                ParseOnlineTraceBuffer(buffer);   //  버퍼에 있는 데이타 파싱 작업
                            }
                            this.receivebuf.RemoveRange(0, num);
                            //this.bufferReaderThread.Thread.Join(100);
                        }
                        catch (Exception o)
                        {
                            Console.WriteLine("ma3012receive: BufferSenderThread Error " + o.Message);
                        }
                        finally
                        {
                            //this.bufferReaderThread.Thread.Join(100); 
                        }
                    }
                    else
                    {
                        this.bufferReaderThread.Thread.Join(200);
                    }

                }
                this.bufferReaderThread.IsRunning = false;
            }
            catch
            {
                this.bufferReaderThread.IsRunning = false;
                Environment.Exit(0); // 신규추가
            }
        }

        private void ParseOnlineTraceBuffer(TransferBuffer buffer)
        {
            try
            {
                if (buffer.Length >= 260) // 최소 20Hz  (header(20byte) + sample data (240byte) 
                {
                    ExtendedSampleFrameBufferEntry extendedSampleFrameBufferEntry = new ExtendedSampleFrameBufferEntry(buffer.Pop(buffer.Length));
                    if ((extendedSampleFrameBufferEntry.StationCode == StationCode) && (extendedSampleFrameBufferEntry.SensorID == LocationCode))
                    {
                        this.ExtraceOnlineTraceData(extendedSampleFrameBufferEntry);
                    }
                    else
                    {
                        Console.WriteLine("ma3012receive: 수신되는 데이터 StationCode : " + extendedSampleFrameBufferEntry.StationCode + "  LocationCode :" + extendedSampleFrameBufferEntry.SensorID);
                        Console.WriteLine("ma3012receive: 설정 파라미터 StationCode : " + StationCode + "  LocationCode :" + LocationCode);
                        Console.WriteLine("ma3012receive: 수신되는 데이터의 StationCode와 LocationCode 가 설정파라미터와 일치하지 않습니다. 확인바랍니다");
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("ma3012receive: Pasing Error " + exception.Message);
            }
        }

        //실시간 데이터 파싱 및 MSEED 및 DB 버퍼에 데이터 저장
        private void ExtraceOnlineTraceData(ExtendedSampleFrameBufferEntry extendedSampleFrameBufferEntry)
        {
            try
            {
                double[][] numArray = new double[3][]; //x,y,z 센서 3개
                int CurrentSamplingRate = extendedSampleFrameBufferEntry.SamplingRate;
                int componentsample = CurrentSamplingRate * 4; // 샘플링 * 4byte
                int totalsample = componentsample * 3; // componentsample * 3축
                int count = 1;
                if (CurrentSamplingRate == 200)
                {
                    count = 2;
                    totalsample = totalsample / 2;
                }

                for (int i = 0; i < numArray.Length; i++)
                {
                    numArray[i] = null;
                    numArray[i] = new double[CurrentSamplingRate];
                }
                SampleFrameBufferEntry sampleFrameBufferEntry = new SampleFrameBufferEntry();
                for (int c = 0; c < count; c++)
                {
                    sampleFrameBufferEntry = extendedSampleFrameBufferEntry.GetSampleFrameBufferEntryNew(c + 1, totalsample);
                    for (int j = 0; j < numArray.Length; j++)
                    {
                        try
                        {
                            DecodeSampleFrameBufferEntryNew(sampleFrameBufferEntry, ref numArray[j], j, c * 100);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine("ma3012receive: Pasing(Decoding) Error " + exception.Message);
                        }
                    }
                }

                EventData eventData = new EventData
                {
                    StationCode = extendedSampleFrameBufferEntry.StationCode,
                    SensorID = extendedSampleFrameBufferEntry.SensorID,
                    Starttime = extendedSampleFrameBufferEntry.TimeStamp,
                    SamplingRateinHz = (uint)extendedSampleFrameBufferEntry.SamplingRate,
                };
                for (int k = 0; k < numArray.Length; k++)
                {
                    if (numArray[k] != null)
                    {
                        Waveform waveform = new Waveform
                        {
                            ChannelName = ChannelName[k],
                            ChannelNumber = k,
                            ChannelUnit = "gal"
                        };
                        waveform.SetData(numArray[k]);
                        eventData.AddWaveform(waveform);
                    }
                }
                if (this.MSEEDRecordig)
                {
                    if (recordingDataBufferThread != null)
                    {
                        if (recordingDataBufferThread.IsRunning)
                        {
                            recordingData.Add(eventData);
                        }
                        else
                        {
                            recordingDataBufferThread.Start();
                        }
                    }
                    else
                    {
                        recordingDataBufferThread = new WorkerThread(new ThreadStart(this.RecordingThreadStart));
                        recordingDataBufferThread.Start();
                    }
                }
                if (this.DBRecordig)
                {
                    if (sendingDataBufferThread != null)
                    {
                        if (sendingDataBufferThread.IsRunning)
                        {
                            sendingData.Add(eventData);
                        }
                        else
                        {
                            sendingDataBufferThread.Start();
                        }
                    }
                    else
                    {
                        sendingDataBufferThread = new WorkerThread(new ThreadStart(this.SendingThreadStart));
                        sendingDataBufferThread.Start();
                    }
                }
            }
            catch (Exception re)
            {
                Console.WriteLine("ma3012receive: Pasing(Extrace) Error " + re.Message);
            }

        }

        //파싱 함수
        private static void DecodeSampleFrameBufferEntryNew(SampleFrameBufferEntry sampleFrameBufferEntry, ref double[] dataArrayChannel1, int channel, int index)
        {
            byte[] dspDataBlock = sampleFrameBufferEntry.ActualDataBlock;
            int count = dspDataBlock.Length / 3; //3은 성분의 수

            byte[] conv_data = new byte[4];
            for (int i = 0; i < count / 4; i++) //4는 한샘플의 byte
            {

                conv_data[0] = dspDataBlock[(count * channel) + (4 * i) + 3];
                conv_data[1] = dspDataBlock[(count * channel) + (4 * i) + 2];
                conv_data[2] = dspDataBlock[(count * channel) + (4 * i) + 1];
                conv_data[3] = dspDataBlock[(count * channel) + (4 * i) + 0];

                if (dataArrayChannel1 != null)
                {
                    dataArrayChannel1[index] = BitConverter.ToUInt32(conv_data, 0);
                }
                index++;
            }

        }

        // 미니시드 저장 쓰레드
        private void RecordingThreadStart()
        {
            int errorNum = 0;
            this.recordingDataBufferThread.IsRunning = true;
            try
            {
                while (this.recordingDataBufferThread.KeepRunning)
                {
                    try
                    {
                        int num = recordingData.Count;
                        if (num > 0)
                        {
                            try
                            {
                                EventData eventData = recordingData[0];
                                string strStartTime = eventData.Starttime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"); //UTC
                                int strSamplingRateHz = (int)eventData.SamplingRateinHz;
                                using (Py.GIL())
                                {

                                    dynamic pywrite = Py.Import("ms2convert");
                                    dynamic strNet = NETCode; // 기관코드
                                    dynamic strStation = StationCode; // 스테이션코드
                                    dynamic strLocation = LocationCode; // 위치코드 (센서번호)
                                    dynamic strChannel = ChannelName; // 파라미터에서 읽어서 처리

                                    List<Int32>[] datas = new List<Int32>[3];
                                    for (int i = 0; i < datas.Length; i++)
                                    {
                                        datas[i] = new List<int>();
                                        for (int j = 0; j < num; j++)
                                        {
                                            double[] getData = this.recordingData[j].GetWaveform(i).GetData();
                                            for (int k = 0; k < getData.Length; k++)
                                            {
                                                datas[i].Add((Int32)getData[k]);
                                            }
                                        }
                                    }
                                    dynamic strNpts = datas[0].Count;
                                    dynamic strSamplingHz = strSamplingRateHz;
                                    dynamic strStartime = strStartTime;  //MSSEED 기록은 UTC 시간으로
                                    dynamic strXData = datas[0];
                                    dynamic strYData = datas[1];
                                    dynamic strZData = datas[2];
                                    dynamic strHome = ProcessConfig.EW_HOME;
                                    dynamic strParams = ProcessConfig.EW_PARAMS;
                                    dynamic strMSHome = ProcessConfig.RESERVE5;
                                    dynamic strTANKHome = ProcessConfig.RESERVE4;

                                    dynamic saveCall = pywrite.MSEED(strNet, strStation, strLocation, strChannel, strNpts, strSamplingHz, strStartime, strXData, strYData, strZData, strHome, strParams, strMSHome, strTANKHome);
                                    bool ck = saveCall.MSWrite();
                                    bool ck_flag = false;
                                    if (!ck)
                                    {
                                        ck_flag = true;
                                        errorNum++;
                                    }
                                    else
                                    {
                                        this.recordingData.RemoveRange(0, num);
                                        //for (int i = 0; i < num; i++)
                                        //{
                                        //    this.recordingData.RemoveAt(0);
                                        //}
                                    }

                                    if (!ck_flag)
                                    {
                                        errorNum = 0;
                                    }

                                    if (errorNum > 5)
                                    {
                                        Environment.Exit(0);
                                    }
                                }
                            }
                            catch (Exception o)
                            {
                                Environment.Exit(0);
                            }
                        }
                        else
                        {
                            this.recordingDataBufferThread.Thread.Join(100);
                        }
                    }
                    catch
                    {
                        if (recordingData == null)
                        {
                            this.recordingData = new List<EventData>();
                        }
                        Environment.Exit(0); //신규추가
                    }
                    finally
                    {
                        while (this.recordingData.Count > 100)
                        {
                            this.recordingData.RemoveAt(0);
                        }
                    }
                }

                this.recordingDataBufferThread.IsRunning = false;
            }
            catch
            {
                this.recordingDataBufferThread.IsRunning = false;
                Environment.Exit(0);
            }
        }

        private void SendingThreadStart()
        {
            this.sendingDataBufferThread.IsRunning = true;
            try
            {
                while (this.sendingDataBufferThread.KeepRunning)
                {
                    try
                    {
                        int sendingCount = sendingData.Count;
                        if (sendingCount > 0)
                        {
                            double x_PGA = 0;
                            double y_PGA = 0;
                            double z_PGA = 0;
                            double vector = 0;
                            double vector2d = 0;
                            double vector3d = 0;

                            EventData eventData = sendingData[0];
                            sendingData.Clear(); // 모든  데이터 삭제
                            string strStartTime = eventData.Starttime.ToString("yyyy-MM-dd HH:mm:ss");
                            int strSamplingHz = (int)eventData.SamplingRateinHz;

                            double[][] numArray = new double[3][];
                            for (int i = 0; i < numArray.Length; i++)
                            {
                                numArray[i] = null;
                                numArray[i] = new double[strSamplingHz];
                            }

                            double[] offset = new double[3];
                            double[] AbsoluteMaximum = new double[3];
                            for (int i = 0; i < numArray.Length; i++)
                            {
                                Waveform waveform = eventData.GetWaveform(i);
                                numArray[i] = waveform.GetData();
                                offset[i] = waveform.Offset;
                                AbsoluteMaximum[i] = waveform.AbsoluteMaximumWithOutOffset;
                            }

                            x_PGA = AbsoluteMaximum[0] / ScaleFactor;
                            y_PGA = AbsoluteMaximum[1] / ScaleFactor;
                            z_PGA = AbsoluteMaximum[2] / ScaleFactor;
                            vector = Math.Max(Math.Max(x_PGA, y_PGA), z_PGA);
                            vector2d = Math.Sqrt(Math.Pow(x_PGA, 2) + Math.Pow(y_PGA, 2));
                            vector3d = Math.Sqrt(Math.Pow(x_PGA, 2) + Math.Pow(y_PGA, 2) + Math.Pow(z_PGA, 2));


                            string strOBSCode = StationCode + LocationCode;
                            string strGMTtime = strStartTime;
                            string strUTCtime = eventData.Starttime.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
                            string strEpochtime = "";

                            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            strEpochtime = Convert.ToInt64((eventData.Starttime.ToUniversalTime() - epoch).TotalSeconds).ToString();

                            string strPGA_E = System.Convert.ToString(x_PGA);
                            string strPGA_N = System.Convert.ToString(y_PGA);
                            string strPGA_Z = System.Convert.ToString(z_PGA);
                            string strPGA_Vector = System.Convert.ToString(vector);
                            string strPGA2D_Vector = System.Convert.ToString(vector2d);
                            string strPGA3D_Vector = System.Convert.ToString(vector3d);
                            dayMaxTemp = Math.Max(dayMaxTemp, vector);


                            if (preTime.Day != eventData.Starttime.Day)
                            {
                                dayMaxTemp = 0.0;
                            }
                            preTime = eventData.Starttime;
                            string strDayMax = System.Convert.ToString(dayMaxTemp);

                            object[] alData = SeisWaveSave(NETCode, StationCode, LocationCode, strOBSCode, strGMTtime, strUTCtime, strEpochtime,
                                strPGA_E, strPGA_N, strPGA_Z, strPGA_Vector, strPGA2D_Vector, strPGA3D_Vector, strDayMax);

                            this.sendingDataBufferThread.Thread.Join(500);
                        }
                        else
                        {
                            this.sendingDataBufferThread.Thread.Join(100);
                        }
                    }
                    catch (Exception b)
                    {
                        if (sendingData == null)
                        {
                            this.sendingData = new List<EventData>();
                        }

                    }
                    finally
                    {
                        while (sendingData.Count > 1)
                        {
                            sendingData.RemoveAt(0);
                        }
                    }
                }
                this.sendingDataBufferThread.IsRunning = false;
            }
            catch
            {
                Environment.Exit(0); //신규추가
            }
        }

        public object[] SeisWaveSave(string strNetCode, string strStationCode, string strLocation, string strOBSCode, string strGMTtime, string strUTCtime, string strEpochtime,
            string strPGA_E, string strPGA_N, string strPGA_Z, string strPGA_Vector, string strPGA2D_Vector, string strPGA3D_Vector, string strDayMax)
        {
            m_db = this.getCon();
            try
            {
                this.DbOpen();
            }
            catch
            {
                this.DbClose();
                return this.TYEncodeData("01", "에러");
            }

            string strQuery = @"
Update SeisWaveMaster 
Set Time_GMT = " + this.Q(strGMTtime) + @",
Time_UTC = " + this.Q(strUTCtime) + @",
EpochTime = " + this.Toi(strEpochtime) + @",
PGA_E = " + this.Tod(strPGA_E) + @",
PGA_N = " + this.Tod(strPGA_N) + @",
PGA_Z = " + this.Tod(strPGA_Z) + @",
PGA_Vector = " + this.Tod(strPGA_Vector) + @",
PGA2D_Vector = " + this.Tod(strPGA2D_Vector) + @",
PGA3D_Vector = " + this.Tod(strPGA3D_Vector) + @",
DayPGAMaxValue = " + this.Tod(strDayMax) + @"
Where SiteCode ='N001' And NetCode = " + this.Q(strNetCode) + @" And StationCode = " + this.Q(strStationCode) + @" And LocationCode = " + this.Q(strLocation) + @" And OBSCode = " + this.Q(strOBSCode) + @"
";

            MySqlCommand cm = new MySqlCommand(strQuery, m_db);
            MySqlDataReader dr;
            try
            {
                dr = cm.ExecuteReader();
            }
            catch (Exception e)
            {
                this.DbClose();
                return this.TYEncodeData("01", "에러");
            }

            object[] alData = this.TYEncodeData("00", "", dr);

            dr.Close();
            cm = null;
            this.DbClose();

            return alData;

        }

        public static void AddEnvPath(params string[] paths)
        {
            // PC에 설정되어 있는 환경 변수를 가져온다.
            var envPaths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator).ToList();
            // 중복 환경 변수가 없으면 list에 넣는다.
            envPaths.InsertRange(0, paths.Where(x => x.Length > 0 && !envPaths.Contains(x)).ToArray());
            // 환경 변수를 다시 설정한다.
            Environment.SetEnvironmentVariable("PATH", string.Join(Path.PathSeparator.ToString(), envPaths), EnvironmentVariableTarget.Process);
        }

        public MySqlConnection getCon()
        {
            if (m_db == null)
            {
                m_db = new MySqlConnection(db_conString);
            }
            return m_db;
        }
        /// <summary>
        /// 연결되면 오픈 
        /// </summary>
        public void DbOpen()
        {
            m_db.Open();
        }

        /// <summary>
        /// 연결종료 
        /// </summary>
        public void DbClose()
        {
            m_db.Close();
        }
        public string[] TYEncodeData(string strErrCode, string strErrMsg)
        {
            string[] alData = new string[4];
            // alData 기본 등록
            // 첫째자리:에러코드, 두번째자리:에러메세지, 세번째자리:열(필드수), 네번째자리:행(레코드수)
            alData[0] = strErrCode;
            alData[1] = strErrMsg;
            alData[2] = "0";
            alData[3] = "0";

            return alData;
        }
        public string[] TYEncodeData(string strErrCode, string strErrMsg, MySqlDataReader dr)
        {
            ArrayList alResult = new ArrayList();

            int iRows = dr.FieldCount;
            int iCols = 0;
            int iCnt = 0;
            while (dr.Read())
            {
                for (int i = 0; i < iRows; i++)
                {
                    alResult.Add(dr[i].ToString());
                }
                iCols++;
            }
            dr.Close();
            int iNum = iRows * iCols + 4;
            string[] alData = new string[iNum];
            // alData 기본 등록
            // 첫째자리:에러코드, 두번째자리:에러메세지, 세번째자리:열(필드수), 네번째자리:행(레코드수)
            alData[0] = "00";
            alData[1] = "";
            alData[2] = iRows.ToString();
            alData[3] = iCols.ToString();

            iCnt = iRows * iCols;
            if (iNum > 4)
            {
                for (int i = 0; i < iCnt; i++)
                {
                    alData[i + 4] = alResult[i].ToString();
                }
            }
            return alData;

        }
        /// <summary>
        /// 사용자ID 확인 
        /// </summary>
        public string Q(string strText)
        {
            if (strText != null)
            {
                strText = "N'" + strText.ToString().Trim() + "'";
            }
            else
            {
                strText = "N'" + "" + "'";
            }
            return strText;
        }
        public double Tod(string strText)
        {
            double strNumeric = System.Convert.ToDouble(strText);
            return strNumeric;
        }
        public double Toi(string strText)
        {
            int strNumeric = System.Convert.ToInt32(strText);
            return strNumeric;
        }

        public object[] ProcessConfigLoad()
        {
            m_db = this.getCon();
            try
            {
                this.DbOpen();
            }
            catch
            {
                this.DbClose();
                return this.TYEncodeData("01", "에러");
            }

            string strQuery = @"
Select DBHOST, DBPORT, DBNAME, USERID, USERPW, EW_HOME, EW_PARAMS, PY_HOME, 
RESERVE1, RESERVE2, RESERVE3, RESERVE4, RESERVE5
From seis_monitoring.processconfig
Where SiteCode = 'N001';
";

            MySqlCommand cm = new MySqlCommand(strQuery, m_db);
            MySqlDataReader dr;
            try
            {
                dr = cm.ExecuteReader();
            }
            catch (Exception e)
            {
                this.DbClose();
                return this.TYEncodeData("01", "에러");
            }

            object[] alData = this.TYEncodeData("00", "", dr);

            dr.Close();
            cm = null;
            this.DbClose();

            return alData;

        }

    }
}
