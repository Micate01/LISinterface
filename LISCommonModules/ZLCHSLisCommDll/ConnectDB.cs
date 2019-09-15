using System;
using System.Collections.Generic;
using System.Text;
//using System.Data.OleDb;
using System.Data.OracleClient;
using System.IO;
using System.Security.Cryptography;

namespace ZLCHSLisComm
{
    public class ConnectDB
    {
        public static OracleConnection con = new OracleConnection();
        Write_Log writeLog = new Write_Log();
        //public OleDbConnection con = new OleDbConnection();
        string FileName;
  
        /// <summary>
        /// �������ݿ�
        /// </summary>
        public string DBConnect()
        {
            string Return_Value;
            //string DBMS;
            string strUser;
            string strPassword;
            string strServer;
            IniFile ConfigIni = new IniFile("SOLVESET.INI");

            if (con.State == System.Data.ConnectionState.Closed)
            {
                Return_Value = INIExists("SOLVESET.INI");
                if (!String.IsNullOrEmpty(Return_Value)) return Return_Value;

                strServer = ConfigIni.IniReadValue("EQUIPMENT", "DBSERVER");
                if (String.IsNullOrEmpty(strServer)) return "δ�ҵ����䡾EQUIPMENT����ؼ��֡�DBSERVER��!";
                strServer = ConfigIni.DecryptDES(strServer, "zlsofthis");

                strUser = ConfigIni.IniReadValue("EQUIPMENT", "LOGID");
                if (String.IsNullOrEmpty(strServer)) return "δ�ҵ����䡾EQUIPMENT����ؼ��֡�LOGID��!";
                strUser = ConfigIni.DecryptDES(strUser, "zlsofthis");

                strPassword = ConfigIni.IniReadValue("EQUIPMENT", "LOGPASS");
                if (String.IsNullOrEmpty(strServer)) return "δ�ҵ����䡾EQUIPMENT����ؼ��֡�LOGPASS��!";
                strPassword = ConfigIni.DecryptDES(strPassword, "zlsofthis");

                string connStr = "Data Source=" + strServer + ";User Id=" + strUser + ";Password=" + strPassword + ";Integrated Security=no;";
                con.ConnectionString = connStr;
                try
                {
                    con.Open();
                }
                catch (Exception e)
                {
                    con.Close();
                    connStr = "Data Source=orcl;User Id=zlchs;Password=zlchs;Integrated Security=no;";
                    con.ConnectionString = connStr;
                    try
                    {
                        con.Open();
                    }
                    catch (Exception ex)
                    {
                        con.Close();
                        return "���ݿ�����ʧ�ܣ�" + ex.Message;
                    }
                }
            }
            return "";
        }

        /// <summary>
        /// �Ͽ����ݿ�����
        /// </summary>
        /// <param name="con"></param>
        public void DisConnectDB(OracleConnection con) //(OleDbConnection con)
        {
            con.Dispose();
            con.Close();
        }

        /// <summary>
        /// �ж������ļ��Ƿ����
        /// </summary>
        /// <param name="AFileName"></param>
        public string INIExists(string AFileName)
        {
            AFileName = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + AFileName;
            FileInfo fileInfo = new FileInfo(AFileName);
            if ((!fileInfo.Exists))
            {   //�ļ�������
                return "δ�ҵ������ļ�[" + AFileName + "]";
            }
            //��������ȫ·�������������·��
            FileName = fileInfo.FullName;
            return "";
        }

        /////��<summary>
        /////��DES�����ַ���
        /////��</summary>
        /////��<param��name="encryptString">�����ܵ��ַ���</param>
        /////��<param��name="encryptKey">������Կ,Ҫ��Ϊ8λ</param>
        /////��<returns>���ܳɹ����ؼ��ܺ���ַ�����ʧ�ܷ���Դ��</returns>
        //public string EncryptDES(string encryptString, string encryptKey)
        //{
        //    try
        //    {
        //        byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
        //        byte[] rgbIV = Keys;
        //        byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
        //        DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
        //        MemoryStream mStream = new MemoryStream();
        //        CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
        //        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        //        cStream.FlushFinalBlock();
        //        return Convert.ToBase64String(mStream.ToArray());
        //    }
        //    catch
        //    {
        //        return encryptString;
        //    }
        //}



    }
}
