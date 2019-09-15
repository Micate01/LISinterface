        using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;


namespace ZLCHSLisComm
{
   public class INIHelper  
   {



        private static INIHelper ini;
        private string path;
        private INIHelper(string INIPath)
        {
            path = INIPath;
        }
        public static INIHelper getInstance(string INIPath)
        {
            if (ini == null)
            {
                ini = new INIHelper(INIPath);
            }
            return ini;

        }
        public static INIHelper getInstance()
        {
            if (ini == null)
            {
                ini = new INIHelper(@".\SOLVESET.ini");
            }
            return ini;

        }
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);
        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="Section"></param>
        /// <param name="Key"></param>
        /// <returns></returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(2550);
            int i = GetPrivateProfileString(Section, Key, "", temp, 2550, this.path);
            return temp.ToString();
        }
        public byte[] IniReadValues(string section, string key)
        {
            byte[] temp = new byte[2550];
            int i = GetPrivateProfileString(section, key, "", temp, 2550, this.path);
            return temp;
        }
        /// <summary>
        /// 删除ini文件下所有段落
        /// </summary>
        public void ClearAllSection()
        {
            IniWriteValue(null, null, null);
        }
        /// <summary>
        /// 删除ini文件下personal段落下的所有键
        /// </summary>
        /// <param name="Section"></param>
        public void ClearSection(string Section)
        {
            IniWriteValue(Section, null, null);
        }
    }
}

