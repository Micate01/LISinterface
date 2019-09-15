using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;

namespace ZLCHSLisComm
{
    public class Init_TCP
    {
        //TCP
        public string strInstrumentID;
        public string strError;

        TcpListener tlTcpListen;
        Socket socket;
        string ip;         //仪器IP
        string host_ip;    //主机IP
        string port;       //接收端口
        string role;       //角色
        string dataType;
        string databegin;
        string dataend;
        string inBegin;
        string inEnd;
        string decode_mode;
        string SubBegin;
        string SubEnd;
        string ACK_term;
        string ACK_all;
        string recvStr;
        string strDeviceName;//仪器名称
        string RemarkContent;//设备备注
        string strResult = "";

        DataSet ds = new DataSet();

        DataSetHandle dsHandle = new DataSetHandle();
        GetResultString resString = new GetResultString();
        Write_Log writeLog = new Write_Log();

        List<Thread> threadUpp = new List<Thread>();          //线程数组
        Thread thread;
        string CommProgramName;             //通讯程序名称 
        public object obj = null;                                             //定义反射对象
        public IDataResolve IResolve;                            //定义数据解析接口

        string inifile = "SOLVESET.INI";

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        public void Init()
        {
            //string host = "127.0.0.1";
            //   port = "6000";
            ds = dsHandle.GetDataSet(@"Extractvalue(通讯参数, '/root/ip') As 仪器IP, Extractvalue(通讯参数, '/root/port') As 端口, Extractvalue(通讯参数, '/root/role') As 接收角色,
                                    Extractvalue(通讯参数, '/root/host') As 主机IP", "检验仪器", "id = '" + strInstrumentID + "'");
            ip = ds.Tables[0].Rows[0]["仪器IP"].ToString();
            port = ds.Tables[0].Rows[0]["端口"].ToString();
            role = ds.Tables[0].Rows[0]["接收角色"].ToString();
            host_ip = ds.Tables[0].Rows[0]["主机IP"].ToString();
            role = role == "" ? "S" : (role.Replace("1", "S").Replace("2", "C"));
            ds = dsHandle.GetDataSet(@"解析类型,通讯程序名,名称,备注,Extractvalue(接收规则, '/root/data_type') As data_type, Extractvalue(接收规则, '/root/data_begin') As data_begin, 
                                       Extractvalue(接收规则, '/root/data_end') As data_end, Extractvalue(接收规则, '/root/start_cmd') As start_cmd, 
                                       Extractvalue(接收规则, '/root/end_cmd') As end_cmd, Extractvalue(接收规则, '/root/ack_all') As ack_all, 
                                       Extractvalue(接收规则, '/root/ack_term') As ack_term, Extractvalue(接收规则, '/root/decode_mode') As decode_mode, 
                                       Extractvalue(接收规则, '/root/begin_bits') As begin_bits, Extractvalue(接收规则, '/root/end_bits') As end_bits", "检验仪器", "id = '" + strInstrumentID + "'");
            dataType = ds.Tables[0].Rows[0]["data_type"].ToString();
            databegin = ds.Tables[0].Rows[0]["data_begin"].ToString();
            dataend = ds.Tables[0].Rows[0]["data_end"].ToString();
            //inBegin = ds.Tables[0].Rows[0][3].ToString();
            //inEnd = ds.Tables[0].Rows[0][4].ToString();
            decode_mode = ds.Tables[0].Rows[0]["decode_mode"].ToString();
            SubBegin = ds.Tables[0].Rows[0]["begin_bits"].ToString();
            SubEnd = ds.Tables[0].Rows[0]["end_bits"].ToString();
            ACK_term = ds.Tables[0].Rows[0]["ack_term"].ToString();
            ACK_all = ds.Tables[0].Rows[0]["ack_all"].ToString();
            CommProgramName = ds.Tables[0].Rows[0]["通讯程序名"].ToString();
            strDeviceName = ds.Tables[0].Rows[0]["名称"].ToString();
            RemarkContent = ds.Tables[0].Rows[0]["备注"].ToString();
            try
            {
                obj = ObjectReflection.CreateObject(CommProgramName.Substring(0, CommProgramName.IndexOf(".dll")));
                IResolve = obj as IDataResolve;
                IResolve.GetRules(strInstrumentID);
            }
            catch (Exception exobj)
            {
                writeLog.Write(strDeviceName, exobj.Message, "log");
                return;
            }

            //通过线程去提取数据
            System.Threading.ParameterizedThreadStart ParStart = new System.Threading.ParameterizedThreadStart(Start);
            System.Threading.Thread threadSocket = new System.Threading.Thread(ParStart);
            object socketListen = strError;
            threadSocket.Start(socketListen);

            //Start();
        }

        /// <summary>
        /// 回收线程
        /// </summary>
        public void ListenThread(object s)
        {
            while (threadUpp.Count > 0)
            {
                for (int i = 0; i < threadUpp.Count; i++)
                {
                    if (threadUpp[i].IsAlive) threadUpp[i].Join();
                }
            }
        }

        public void Stop()
        {
            if (role == "S")
            {
                for (int i = 0; i < threadUpp.Count; i++)
                {
                    threadUpp[i].Join();
                }
                //tlTcpListen.Stop();
                tlTcpListen = null;
            }
        }
        /// <summary>
        /// TCP监听运行  作为client监听
        /// </summary>
        public void Start(object a)
        {
            //host_ip = "192.168.0.188";
            IPAddress ipAdd = null;
            if (host_ip.Length > 0)
            {
                ipAdd = IPAddress.Parse(host_ip);

            }
            else
            {
                ipAdd = IPAddress.Parse(ip);
            }

            IPEndPoint ipe = null;
            EndPoint ep = null;
            try
            {
                ipe = new IPEndPoint(ipAdd, int.Parse(port));
                ep = (EndPoint)ipe;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString());
            }
            if (role == "C")
            {
                //socket通常也称作"套接字"，用于描述IP地址和端口
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    socket.Connect(ipe);
                }
                catch (Exception exs)
                {
                    writeLog.Write(exs.Message, "log");
                    return;
                }
                while (true)
                {
                    try
                    {
                        TcpListenClient(socket);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Forms.MessageBox.Show(ex.ToString());
                    }
                }
            }
            else
            {
                try
                {
                  
                    if (!String.IsNullOrEmpty(host_ip))
                    {
                        tlTcpListen = new TcpListener(IPAddress.Parse(host_ip), int.Parse(port));
                    }
                    else
                    {
                        string ip1 = Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
                        tlTcpListen = new TcpListener(IPAddress.Parse(Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString()), int.Parse(port));
                    }
                    try
                    {
                        tlTcpListen.Start(); //开始监听
                    }
                    catch (Exception ex1)
                    {
                        System.Windows.Forms.MessageBox.Show(ex1.ToString());
                        return;
                    }
                }

                catch (System.Security.SecurityException ex)
                {
                    writeLog.Write(ex.Message, "log");
                    return;
                }

                try
                {

                    TcpListen();

                }
                catch (Exception ex)
                {
                    socket.Close();
                    socket = tlTcpListen.AcceptSocket();
                    TcpListen();

                }
            }
        }

        /// <summary>
        /// 客户端监听
        /// </summary>
        private void receiveThread()
        {
            byte[] buffer = new byte[1024];
            string recString = "";

            buffer = new byte[1024];
            int bytes = socket.Receive(buffer, buffer.Length, SocketFlags.None);
            recString = Encoding.UTF8.GetString(buffer, 0, bytes);
            //writeLog.Write(recString, "log");
            //-----------Begin
            string strCmd = "";
            IResolve.ParseResult(recString, ref strResult, ref recString, ref strCmd);
            //----------END
            buffer = new byte[1];
            buffer[0] = Convert.ToByte((char)6);
            socket.Send(buffer);
        }
        /// <summary>
        /// 服务器端监听
        /// </summary>
        /// 
        public void TcpListen()
        {
            Socket socketRetrieve = null;
            Mindray mry = new Mindray();
            IniFile ConfigIni = new IniFile("SOLVESET.INI");
            string encode = ConfigIni.IniReadValue("EQUIPMENT", "Encode");
            try
            {
                socket = tlTcpListen.AcceptSocket();
                socketRetrieve = socket;
                byte[] buffer = new byte[10240 * 50];
                string recString = "";
                recvStr = "";
                strResult = "";
                string strCmd = "";
                while (true)
                {
                    int bytes = socketRetrieve.Receive(buffer, buffer.Length, SocketFlags.None);
                    Thread.Sleep(20);                   
                    recString = Encoding.GetEncoding(encode).GetString(buffer, 0, bytes);
                    writeLog.Write(recString, "log");                  
                    #region 通用协议,通过接口来处理数据是否接受完成                     
                    IResolve.ParseResult(recString, ref strResult, ref recvStr, ref strCmd); 
                    //如果存在返回指令,则返回指令
                    if (!string.IsNullOrEmpty(strCmd))
                    {                     
                        socketRetrieve.Send(Encoding.GetEncoding(encode).GetBytes(strCmd));
                    }
                    #endregion

                }
            }
            catch (Exception e)
            {
                if (socketRetrieve != null)
                {
                    socketRetrieve.Close();
                }
                TcpListen();
            }
        }
        /// <summary>
        /// 通讯程序作为客户端去连接
        /// </summary>
        /// 
        public void TcpListenClient(object socketConn)
        {
            IniFile ConfigIni = new IniFile("SOLVESET.INI");
            string encode = ConfigIni.IniReadValue("EQUIPMENT", "Encode");
            Socket socketRetrieve;
            socketRetrieve = (Socket)socketConn;
            byte[] buffer = new byte[10240 * 50];
            string recString = "";          
            while (true)
            {
                int bytes = socketRetrieve.Receive(buffer, buffer.Length, SocketFlags.None);
                recString = Encoding.GetEncoding(encode).GetString(buffer, 0, bytes);
                writeLog.Write(recString, "log");               
            
            string strCmd = "";
            IResolve.ParseResult(recString, ref strResult, ref recvStr, ref strCmd);
            //包含返回指令，则返回指令
            if (!String.IsNullOrEmpty(strCmd))
            {
                socketRetrieve.Send(Encoding.GetEncoding(encode).GetBytes(strCmd));
            }       
            }

        }

    }
}
