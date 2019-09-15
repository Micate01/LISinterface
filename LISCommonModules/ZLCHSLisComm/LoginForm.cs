using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.IO;
namespace ZLCHSLisComm
{
    public partial class LoginForm : Form
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        private byte[] Keys = { 0xEF, 0xAB, 0x56, 0x78, 0x90, 0x34, 0xCD, 0x12 };
        string inifile = "SOLVESET.INI";
        string FileName;

        public LoginForm()
        {
            InitializeComponent();
        }

        public string INIExists(string AFileName)
        {
            AFileName = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + AFileName;
            FileInfo fileInfo = new FileInfo(AFileName);
            //未找到文件
            if (!fileInfo.Exists) return "未找到配置文件[" + AFileName + "]";
            //必须是完全路径，不能是相对路径
            FileName = fileInfo.FullName;
            return "";
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

        private void bOK_Click(object sender, EventArgs e)
        {
            string strServer, strUser, strPassword;
            string Return_Value;
            System.Data.OracleClient.OracleConnection con = new System.Data.OracleClient.OracleConnection();

            strServer = textBoxServer.Text;
            strUser = textBoxLog.Text;
            strPassword = textBoxPass.Text;

            Return_Value = INIExists(inifile);
            if (!String.IsNullOrEmpty(Return_Value))
            {
                MessageBox.Show(Return_Value);
                return;
            }
            string connStr = "Data Source=" + strServer + ";User Id=" + strUser + ";Password=" + strPassword + ";Integrated Security=no;";
            con.ConnectionString = connStr;
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                con.Close();                
                MessageBox.Show("数据库连接失败！" + ex.Message,"仪器数据接收", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            strServer = EncryptDES(strServer, "zlsofthis");
            strUser = EncryptDES(strUser, "zlsofthis");
            strPassword = EncryptDES(strPassword, "zlsofthis");

            long OpStation = WritePrivateProfileString("EQUIPMENT", "DBSERVER", strServer, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            if (OpStation == 0)
            {
                MessageBox.Show("未找到段落【EQUIPMENT】或关键字【DBSERVER】!");
            }
            OpStation = WritePrivateProfileString("EQUIPMENT", "LOGID", strUser, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            if (OpStation == 0)
            {
                MessageBox.Show("未找到段落【EQUIPMENT】或关键字【LOGID】!");
            }
            OpStation = WritePrivateProfileString("EQUIPMENT", "LOGPASS", strPassword, System.AppDomain.CurrentDomain.BaseDirectory.ToString() + inifile);
            if (OpStation == 0)
            {
                MessageBox.Show("未找到段落【EQUIPMENT】或关键字【LOGPASS】!");
            }
            this.DialogResult = DialogResult.OK;
        }

        private void bExit_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        public void Login_Load(object sender, EventArgs e)
        {
            System.Data.OracleClient.OracleConnection con = new System.Data.OracleClient.OracleConnection();
            con.ConnectionString =OracleHelper.GetConnectionstring();
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                con.Close();                
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
