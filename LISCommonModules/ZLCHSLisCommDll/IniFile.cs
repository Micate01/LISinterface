using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Security.Cryptography;

namespace ZLCHSLisComm
{
    public class IniFile
    {
        public string Path;
        //默认密钥向量
        private byte[] Keys = { 0xEF, 0xAB, 0x56, 0x78, 0x90, 0x34, 0xCD, 0x12 };
        
        public IniFile(string path)
        {
            this.Path = INIExists(path);
        }

        public IniFile()
        { }
        #region 声明读写INI文件的API函数
        [DllImport("kernel32")] 
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath); 
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath); 
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);
        #endregion

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        public void IniWriteValue(string section, string key, string iValue) 
        {
            WritePrivateProfileString(section, key, iValue, this.Path);
        }

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        /// <param name="filename">文件名</param>
        public Boolean IniWriteValue(string section, string key, string iValue,string filename)
        {
            string fullpath;
            fullpath = INIExists(filename);
            WritePrivateProfileString(section, key, iValue, fullpath);
            return true;
        }
        /// <summary>
        /// 读取INI文件
        /// </summary>
       /// <param name="section">段落</param>
       /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key) 
        { 
            StringBuilder temp = new StringBuilder(255); 
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.Path); 
            return temp.ToString();
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key,string filename)
        {
            string fullpath;
            fullpath = INIExists(filename);
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, fullpath);
            return temp.ToString();
        }
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section">段，格式[]</param>
        /// <param name="Key">键</param>
        /// <returns>返回byte类型的section组或键值组</returns>
        public byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[255];
            int i = GetPrivateProfileString(section, key, "", temp, 255, this.Path);
            return temp;
        }
        /// <summary>
        /// 判断配置文件是否存在,存在则返回完整路径，不存在则返回空字符串
        /// </summary>
        /// <param name="AFileName"></param>
        public string INIExists(string AFileName)
        {            
            AFileName = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + AFileName;
            FileInfo fileInfo = new FileInfo(AFileName);
            if ((!fileInfo.Exists)) return "";
            //必须是完全路径，不能是相对路径
            return  fileInfo.FullName;
            
        }

        ///　<summary>
        ///　DES解密字符串
        ///　</summary>
        ///　<param　name="decryptString">待解密的字符串</param>
        ///　<param　name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
        ///　<returns>解密成功返回解密后的字符串，失败返源串</returns>
        public string DecryptDES(string decryptString, string decryptKey)
        {
            try
            {
                byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                byte[] rgbIV = Keys;
                byte[] inputByteArray = Convert.FromBase64String(decryptString);
                DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return decryptString;
            }
        }
    }
 }

