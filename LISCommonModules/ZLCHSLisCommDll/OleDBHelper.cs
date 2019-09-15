using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace ZLCHSLisComm
{
    public class OleDBHelper
    {

        private static OleDbConnection oleDBConn=null;
        /// <summary>
        /// 获取access连接
        /// </summary>
        /// <returns></returns>
        private static OleDbConnection GetOledbConnection()
        {
            try
            {
                //获取连接字符串
                string connStr = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + System.Windows.Forms.Application.StartupPath + "\\Data\\Config.mdb";   //System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ToString();
                //string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + System.Windows.Forms.Application.StartupPath + "\\Database.accdb" + ";Persist Security Info=False;";

                //得到连接串
                OleDbConnection conn = new OleDbConnection(connStr);
                //测试使用，有问题直接会抛出异常
                conn.Open();
                conn.Close();
                return conn;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static OleDbConnection GetOledbConnection(string connstr)
        {
            try
            {
                OleDbConnection conn = new OleDbConnection(connstr);
                conn.Open();
                conn.Close();
                return conn;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// 获取数据集DataTable
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static DataTable GetOledbDataTable(string sql,string connStr)
        {

            try
            {
                DataTable mydt = new DataTable();
                using (OleDbConnection conn = GetOledbConnection(connStr))
                {

                    using (OleDbDataAdapter adp = new OleDbDataAdapter(sql, conn))
                    {
                        adp.Fill(mydt);
                        return mydt;
                      
                    }
                 
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        /// <summary>
        /// 获取sql语句查询的值
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static string GetOledbValue(string sql, string connsql)
        {
            try
            {
                string str = "";
                using (OleDbConnection conn = GetOledbConnection(connsql))
                {
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        conn.Open();
                        using (OleDbDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                str = dr[0].ToString();
                            }
                            conn.Close();
                            conn.Dispose();
                            
                        }
                    }
                }
                return str;
            }
            catch (Exception)
            {
                throw;
            }

        }





        /// <summary>
        /// 执行Access非查询操作
        /// </summary>
        /// <param name="sql"></param>
        public static void ExcuteOledbNoQuery(string sql)
        {
            try
            {
                using (OleDbConnection conn = GetOledbConnection())
                {
                    using (OleDbCommand cmd = new OleDbCommand(sql, conn))
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }




        /// <summary>
        /// 执行sql非查询操作
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="para"></param>
        public static void ExcuteOledbNoQuery(string sql, OleDbParameter[] para)
        {
            try
            {
                using (OleDbConnection conn = GetOledbConnection())
                {
                    using (OleDbCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = sql;
                        cmd.Parameters.AddRange(para);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
