using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace ZLCHSLisComm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
             Process instance = RunningInstance();

             //Process[] pros = Process.GetProcesses();
             //int proCount = 0;
             //for (int i = 0; i < pros.Length; i++)
             //{
             //    if (pros[i].ProcessName == instance.ProcessName)
             //        proCount++;
             //}

             if (instance == null  )
             {
                 Application.EnableVisualStyles();
                 Application.SetCompatibleTextRenderingDefault(false);
                 LoginForm login = new LoginForm();
                 login.ShowDialog();
                 if (login.DialogResult == DialogResult.OK) Application.Run(new LisCommForm());
             }
             else
             {
                 MessageBox.Show(null, "系统检测到有一个和本程序相同的通讯程序正在运行，请不要同时运行多个通讯程序。\r\n这个程序即将退出。", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 Application.Exit();//退出程序   
             }
        }

        /// <summary>
        /// 进程判断
        /// </summary>
        /// <returns></returns>
        public static Process RunningInstance()
        {
            Process current = Process.GetCurrentProcess();
            Process[] processes = Process.GetProcessesByName(current.ProcessName);
            foreach (Process process in processes)
            {
                if (process.Id != current.Id)
                {
                    // 确保例程从EXE文件运行
                    if (Assembly.GetExecutingAssembly().Location.Replace("/", "\\") == current.MainModule.FileName)
                    {
                        return process;
                    }
                }
            }
            return null;
        }
    }
}