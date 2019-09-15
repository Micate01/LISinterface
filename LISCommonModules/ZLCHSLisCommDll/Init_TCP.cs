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
        string ip;         //����IP
        string host_ip;    //����IP
        string port;       //���ն˿�
        string role;       //��ɫ
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
        string strDeviceName;//��������
        string RemarkContent;//�豸��ע
        string strResult = "";

        DataSet ds = new DataSet();

        DataSetHandle dsHandle = new DataSetHandle();
        GetResultString resString = new GetResultString();
        Write_Log writeLog = new Write_Log();

        List<Thread> threadUpp = new List<Thread>();          //�߳�����
        Thread thread;
        string CommProgramName;             //ͨѶ�������� 
        public object obj = null;                                             //���巴�����
        public IDataResolve IResolve;                            //�������ݽ����ӿ�

        string inifile = "SOLVESET.INI";

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        public void Init()
        {
            //string host = "127.0.0.1";
            //   port = "6000";
            ds = dsHandle.GetDataSet(@"Extractvalue(ͨѶ����, '/root/ip') As ����IP, Extractvalue(ͨѶ����, '/root/port') As �˿�, Extractvalue(ͨѶ����, '/root/role') As ���ս�ɫ,
                                    Extractvalue(ͨѶ����, '/root/host') As ����IP", "��������", "id = '" + strInstrumentID + "'");
            ip = ds.Tables[0].Rows[0]["����IP"].ToString();
            port = ds.Tables[0].Rows[0]["�˿�"].ToString();
            role = ds.Tables[0].Rows[0]["���ս�ɫ"].ToString();
            host_ip = ds.Tables[0].Rows[0]["����IP"].ToString();
            role = role == "" ? "S" : (role.Replace("1", "S").Replace("2", "C"));
            ds = dsHandle.GetDataSet(@"��������,ͨѶ������,����,��ע,Extractvalue(���չ���, '/root/data_type') As data_type, Extractvalue(���չ���, '/root/data_begin') As data_begin, 
                                       Extractvalue(���չ���, '/root/data_end') As data_end, Extractvalue(���չ���, '/root/start_cmd') As start_cmd, 
                                       Extractvalue(���չ���, '/root/end_cmd') As end_cmd, Extractvalue(���չ���, '/root/ack_all') As ack_all, 
                                       Extractvalue(���չ���, '/root/ack_term') As ack_term, Extractvalue(���չ���, '/root/decode_mode') As decode_mode, 
                                       Extractvalue(���չ���, '/root/begin_bits') As begin_bits, Extractvalue(���չ���, '/root/end_bits') As end_bits", "��������", "id = '" + strInstrumentID + "'");
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
            CommProgramName = ds.Tables[0].Rows[0]["ͨѶ������"].ToString();
            strDeviceName = ds.Tables[0].Rows[0]["����"].ToString();
            RemarkContent = ds.Tables[0].Rows[0]["��ע"].ToString();
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

            //ͨ���߳�ȥ��ȡ����
            System.Threading.ParameterizedThreadStart ParStart = new System.Threading.ParameterizedThreadStart(Start);
            System.Threading.Thread threadSocket = new System.Threading.Thread(ParStart);
            object socketListen = strError;
            threadSocket.Start(socketListen);

            //Start();
        }

        /// <summary>
        /// �����߳�
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
        /// TCP��������  ��Ϊclient����
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
                //socketͨ��Ҳ����"�׽���"����������IP��ַ�Ͷ˿�
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
                        tlTcpListen.Start(); //��ʼ����
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
        /// �ͻ��˼���
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
        /// �������˼���
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
                    #region ͨ��Э��,ͨ���ӿ������������Ƿ�������                     
                    IResolve.ParseResult(recString, ref strResult, ref recvStr, ref strCmd); 
                    //������ڷ���ָ��,�򷵻�ָ��
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
        /// ͨѶ������Ϊ�ͻ���ȥ����
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
            //��������ָ��򷵻�ָ��
            if (!String.IsNullOrEmpty(strCmd))
            {
                socketRetrieve.Send(Encoding.GetEncoding(encode).GetBytes(strCmd));
            }       
            }

        }

    }
}
