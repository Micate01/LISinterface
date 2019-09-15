using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;
using System.Data.OracleClient;
namespace ZLCHSLisComm
{
    public partial class LisCommForm : Form
    {
        ZLCHSLisComm.CallInterFaceDll CallDll = new ZLCHSLisComm.CallInterFaceDll();
        public string inipath;
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        private byte[] Keys =　{ 0xEF, 0xAB, 0x56, 0x78, 0x90, 0x34, 0xCD, 0x12 };
        string FileName;
        string instrument_id;
        string strError;
        bool bolLongin=false;
        bool bolCon = false;
        string inifile = "SOLVESET.INI";
        DataTable tmpTable =new  DataTable();

        //接收消息
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)]
            public string lpData;
        };
        const int WM_COPYDATA = 0x004A;
        //const int WM_GETTEXT = 0x000D;
        const int WM_GETTEXT = 13;
        public string jgid = "";
        public LisCommForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string Return_Value;
            System.Windows.Forms.Form.CheckForIllegalCrossThreadCalls = false;//线程处理后的错误处理控件任然可以赋值
            Return_Value = INIExists(inifile);
            if (!String.IsNullOrEmpty(Return_Value)) MessageBox.Show(Return_Value);
            StringBuilder objStrBd = new StringBuilder(256);
            GetPrivateProfileString("EQUIPMENT", "INSTRUMENTID", "", objStrBd, 256, FileName);
            instrument_id = objStrBd.ToString();
          
            textBox2.Text = INIHelper.getInstance().IniReadValue("EQUIPMENT", "Encode");
            string orgID = INIHelper.getInstance().IniReadValue("EQUIPMENT", "Agencies");
            string sql = "select 资源id,简称 from 机构 where 资源id='in_机构id'";
            sql = sql.Replace("in_机构id", orgID);
             DataTable orgName= OracleHelper.GetDataTable(sql);
             if (orgName.Rows.Count > 0)
                 textBox1.Text = orgName.Rows[0]["简称"].ToString();
            using (OracleConnection _connection = new OracleConnection(OracleHelper.GetConnectionstring()))
            {
                DataTable dt = new DataTable();
                ListViewItem item = new ListViewItem();
                try
                {
                    _connection.Open();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = new OracleCommand("select id,名称 from 检验仪器 where instr('" + instrument_id + "',id)>0", _connection);
                    da.Fill(dt);
                    bolLongin = true;
                }
                catch (Exception exs)
                {
                    LoginForm log = new LoginForm();
                    if (log.ShowDialog() == DialogResult.OK) bolLongin = true;
                    else Application.Exit();
                }
                finally { _connection.Close(); }

                lv_Device.Items.Clear();
                foreach (DataRow i in dt.Rows)
                {
                    item = lv_Device.Items.Add(i["名称"].ToString(), 1);
                    item.Tag = i["id"].ToString();
                    lv_Device.Columns[2].Text = "0";
                }
            }

            if (lv_Device.Items.Count > 0)
            {
                but_begin.Enabled = false;
                but_end.Enabled = true;
                bolCon = true;
                backgroundWorker1.RunWorkerAsync();
            }

            this.WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.notifyIcon1.Visible = true;
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (!string.IsNullOrEmpty(instrument_id))
            {
                try
                {
                    CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=0");
                }
                catch
                { 
                
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string strServer, strUser, strPassword;
            //string Return_Value;
            //System.Data.OracleClient.OracleConnection con = new System.Data.OracleClient.OracleConnection();

            //strServer = textBoxServer.Text;
            //strUser = textBoxLog.Text;
            //strPassword = textBoxPass.Text;

            //Return_Value = INIExists(inifile);
            //if (!String.IsNullOrEmpty(Return_Value))
            //{
            //    MessageBox.Show(Return_Value);
            //    return;
            //}
            //string connStr = "Data Source=" + strServer + ";User Id=" + strUser + ";Password=" + strPassword + ";Integrated Security=no;";
            //con.ConnectionString = connStr;
            //try
            //{
            //    con.Open();
            //}
            //catch (Exception ex)
            //{
            //    con.Close();
            //    MessageBox.Show("数据库连接失败！" + ex.Message);
            //}

            //strServer = EncryptDES(strServer, "zlsofthis");
            //strUser = EncryptDES(strUser, "zlsofthis");
            //strPassword = EncryptDES(strPassword, "zlsofthis");

            //long OpStation = WritePrivateProfileString("EQUIPMENT", "DBSERVER", strServer, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            //if (OpStation == 0)
            //{
            //    MessageBox.Show("未找到段落【EQUIPMENT】或关键字【DBSERVER】!");
            //}
            //OpStation = WritePrivateProfileString("EQUIPMENT", "LOGID", strUser, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            //if (OpStation == 0)
            //{
            //    MessageBox.Show("未找到段落【EQUIPMENT】或关键字【LOGID】!");
            //}
            //OpStation = WritePrivateProfileString("EQUIPMENT", "LOGPASS", strPassword, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            //if (OpStation == 0)
            //{
            //    MessageBox.Show("未找到段落【EQUIPMENT】或关键字【LOGPASS】!");
            //}
            //CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=9");
            //CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=0");
        }
        private string GetCheckCode(string strIn)
        {
            int lngAsc = 0;

            string strCode;
            byte[] b1 = System.Text.Encoding.Default.GetBytes(strIn);
            foreach (byte b in b1)
            {
                lngAsc += Convert.ToInt32(b.ToString(""));
            }
            strCode = Convert.ToString(Convert.ToInt32(lngAsc % 256), 16).ToUpper().PadLeft(2, '0');
            return strCode.Substring(strCode.Length - 2, 2);
        }
        /// <summary>
        /// 判断配置文件是否存在
        /// </summary>
        /// <param name="AFileName"></param>
        public string INIExists(string AFileName)
        {
            AFileName = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + AFileName;
            FileInfo fileInfo = new FileInfo(AFileName);
            if ((!fileInfo.Exists))
            {   //文件不存在
                return "未找到配置文件[" + AFileName + "]";
            }
            //必须是完全路径，不能是相对路径
            FileName = fileInfo.FullName;
            return "";
        }
        //接收消息
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            try
            {
                switch (m.Msg)
                {
                    case WM_COPYDATA:
                        COPYDATASTRUCT mystr1 = new COPYDATASTRUCT();
                        Type mytype = mystr1.GetType();
                        mystr1 = (COPYDATASTRUCT)m.GetLParam(mytype);
                        //this.textBoxPass.Text = mystr1.lpData;
                        m.Result = Marshal.StringToHGlobalAnsi("222");
                        CallDll.CallDll(mystr1.lpData.Replace("CALL_SOURCE=1", ""));
                        //strError = "测试";
                        //char[] cError = strError.ToCharArray();
                        //int len = cError.Length;
                        //m.Result = (IntPtr)len;
                        //strError = CallDll.CallDll(mystr.lpData);
                        break;
                    default:
                        base.DefWndProc(ref m);
                        break;
                }
            }
            catch
            { 
            
            }
        }

        ///　<summary>
        ///　DES加密字符串
        ///　</summary>
        ///　<param　name="encryptString">待加密的字符串</param>
        ///　<param　name="encryptKey">加密密钥,要求为8位</param>
        ///　<returns>加密成功返回加密后的字符串，失败返回源串</returns>
        public string EncryptDES(string encryptString, string encryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            catch
            {
                return encryptString;
            }
        }

        private void bExit_Click(object sender, EventArgs e)
        {
            CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=9");
            Application.Exit();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            decimal decvalue;
            decvalue = Convert.ToDecimal(0.02);
            MessageBox.Show(decvalue.ToString());
        }
        /// <summary>
        /// 最小化到任务栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (bolLongin)
            {
                this.Visible = true;  //设置当前窗口显示
                this.WindowState = FormWindowState.Normal;
                this.notifyIcon1.Visible = false; //设置图标隐藏 
            }
        }
        /// <summary>
        /// 最小化后显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 显示ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = true;  //设置当前窗口显示
            this.WindowState = FormWindowState.Normal;
            this.notifyIcon1.Visible = false; //设置图标隐藏 
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确实要退出通讯程序吗？", "LIS通讯程序", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Environment.Exit(0);
                Application.Exit();
            }
        }

        private void LisComm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (bolCon) CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=9");
            Environment.Exit(0);
            Application.Exit();
        }

        private void 增加仪器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strSele = "";
            string strNewDevice = "";
            foreach (ListViewItem i in lv_Device.Items)
                strSele = strSele + "," + i.Tag;
            if (strSele != "")
                strSele = strSele.Substring(1);

            SelDevice newdevice = new SelDevice();
            newdevice.strSelDevice = strSele;
            newdevice.ShowDialog();
            strNewDevice = newdevice.newDevice;
            if (instrument_id != "") strNewDevice = instrument_id + "," + strNewDevice;
            long OpStation = WritePrivateProfileString("EQUIPMENT", "INSTRUMENTID", strNewDevice, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            initLvDevice();
        }






        /// <summary>
        /// 打开日志文件
        /// </summary>
        /// <param name="type">日志类型</param>       
        private void OpenLog(string type)
        {
            string strpath;
            string strFileName;
            if (lv_Device.SelectedItems.Count < 1)
            {
                MessageBox.Show("请选择仪器!", "查看", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            strpath = Directory.GetCurrentDirectory();
            strpath = strpath + "\\" + lv_Device.SelectedItems[0].Text.ToString() + "\\" + type;
            strFileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            try
            { System.Diagnostics.Process.Start(strpath + "\\" + strFileName); }
            catch
            {
                MessageBox.Show("打开失败,可能文件还未生成！", "查看", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// 打开日志文件夹
        /// </summary>
        /// <param name="type">日志类型</param>       
        private void OpenFolder(string type)
        {
            string strpath;
            if (lv_Device.SelectedItems.Count < 1)
            {
                MessageBox.Show("请选择仪器!", "查看", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            strpath = Directory.GetCurrentDirectory();
            strpath = strpath + "\\" + lv_Device.SelectedItems[0].Text.ToString() + "\\" + type;
            if (!Directory.Exists(strpath))
            {
                MessageBox.Show("打开失败,可能文件夹还未生成！", "查看", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }          
            System.Diagnostics.Process.Start("explorer.exe", strpath);          

        }

        private void but_begin_Click(object sender, EventArgs e)
        {
            BeginConn();
        }

        private void but_end_Click(object sender, EventArgs e)
        {
            but_end.Enabled = false;
            but_begin.Enabled = true;            
            if (bolCon) CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=9");
            bolCon = false;
        }

        private void but_set_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("您确实要退出通讯程序吗？", "LIS通讯程序", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Environment.Exit(0);
                Application.Exit();
            }
        }
        //刷新仪器列表
        private void initLvDevice()
        {
            string Return_Value;

            Return_Value = INIExists(inifile);
            if (!String.IsNullOrEmpty(Return_Value)) MessageBox.Show(Return_Value);
            StringBuilder objStrBd = new StringBuilder(256);
            GetPrivateProfileString("EQUIPMENT", "INSTRUMENTID", "", objStrBd, 256, FileName);
            instrument_id = objStrBd.ToString();

            using (OracleConnection _connection = new OracleConnection(OracleHelper.GetConnectionstring()))
            {
                DataTable dt = new DataTable();
                ListViewItem item = new ListViewItem();
                try
                {
                    _connection.Open();
                    OracleDataAdapter da = new OracleDataAdapter();
                    da.SelectCommand = new OracleCommand("select id,名称 from 检验仪器 where instr('" + instrument_id + "',id)>0", _connection);
                    da.Fill(dt);
                    bolLongin = true;
                }
                catch (Exception exs)
                {
                    LoginForm log = new LoginForm();
                    if (log.ShowDialog() == DialogResult.OK) bolLongin = true;
                    else Application.Exit();
                }
                finally { _connection.Close(); }

                lv_Device.Items.Clear();
                foreach (DataRow i in dt.Rows)
                {
                    item = lv_Device.Items.Add(i["名称"].ToString(), 1);
                    item.Tag = i["id"].ToString();
                    lv_Device.Columns[2].Text = "0";
                }
            }
        }

        private void delToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strDevice="";
            if (lv_Device.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("是否要移除当前选定仪器？", "移除", MessageBoxButtons.OKCancel,MessageBoxIcon.Question ) == DialogResult.OK)
                {
                    lv_Device.Items.Remove(lv_Device.SelectedItems[0]);
                    foreach(ListViewItem item in lv_Device.Items)
                        strDevice = strDevice+"," + item.Tag;
                    if (strDevice != "")                    
                        strDevice = strDevice.Substring(1);
                    long OpStation = WritePrivateProfileString("EQUIPMENT", "INSTRUMENTID", strDevice, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
                    //重新开始连接
                    BeginConn();
                }
            }
        }

        private void AddToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string strSele = "";
            string strNewDevice = "";
            foreach (ListViewItem i in lv_Device.Items)
                strSele = strSele + "," + i.Tag;
            if (strSele != "")
                strSele = strSele.Substring(1);

            SelDevice newdevice = new SelDevice();
            newdevice.strSelDevice = strSele;
            newdevice.ShowDialog();
            strNewDevice = newdevice.newDevice;
            if (strNewDevice != "")
            {
                if (instrument_id != "") strNewDevice = instrument_id + "," + strNewDevice;
                long OpStation = WritePrivateProfileString("EQUIPMENT", "INSTRUMENTID", strNewDevice, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
                initLvDevice();
                if (MessageBox.Show("仪器已改变是否要重新连接?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    BeginConn();
            }
        }
        //连接
        private void BeginConn()
        {
            string Return_Value;

            
            //如果已连接则先断开连接
            if (bolCon) CallDll.CallDll("TEST_INSTRUMENT_ID=" + instrument_id + ";type=9");

            Return_Value = INIExists(inifile);
            if (!String.IsNullOrEmpty(Return_Value)) MessageBox.Show(Return_Value);
            StringBuilder objStrBd = new StringBuilder(256);
            GetPrivateProfileString("EQUIPMENT", "INSTRUMENTID", "", objStrBd, 256, FileName);
            instrument_id = objStrBd.ToString();
            try
            {
                if (instrument_id != "")
                {
                    but_begin.Enabled = false;
                    but_end.Enabled = true;
                    backgroundWorker1.RunWorkerAsync();
                    bolCon = true;
                }
            }
            catch
            { 
            
            }
        }



        private void LisCommForm_MaximumSizeChanged(object sender, EventArgs e)
        {
            this.Visible = false; ;  //设置窗口隐藏
            this.notifyIcon1.Visible = true; //设置图标显示 
        }



        private void lab_logView_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
                OpenLog( "log");
        }

        private void lab_resultView_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {            
                OpenLog( "result");            
        }

        private void lab_drwView_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {            
                OpenLog("raw");             
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFolder("log");
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFolder("result");
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenFolder("raw");
            
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error!=null)
            {
                MessageBox.Show(e.Error.Message);
            }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            frm_选择机构 f = new frm_选择机构();
            f.Owner = this;
            f.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frm_设置编码 f = new frm_设置编码();
            f.ShowDialog();
        }


    }
}
