using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;

namespace ZLCHSLisComm
{
    public partial class frm_设置编码 : Form
    {
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        private byte[] Keys = { 0xEF, 0xAB, 0x56, 0x78, 0x90, 0x34, 0xCD, 0x12 };
        string inifile = "SOLVESET.INI";
        string FileName;
        public frm_设置编码()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string encode = textBox1.Text;
            WritePrivateProfileString("EQUIPMENT", "Encode", encode, INIExists(inifile));
            this.Close();
        }
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
            return FileName;
        }

        private void frm_设置编码_Paint(object sender, PaintEventArgs e)
        {
            //Encode=GB2312

            //textBox1.Text = INIHelper.getInstance().IniReadValue("EQUIPMENT", "Encode");

        }
    }
}
