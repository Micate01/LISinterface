using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ZLCHSLisComm
{
    public class SQLServerHelper
    {

        //sql的两种连接方式


        string constr = "server=.;database=myschool;integrated security=SSPI";
        string constr1 = "server=.;database=myschool;uid=sa;pwd=sa";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sqlstr"></param>
        /// <param name="connStr"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static DataTable GetDataTable(string sql, string connStr, ref string msg)
        {
            msg = "";
            try
            {
                string cs = connStr;//source();
                DataTable dataTable = new DataTable();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dataTable);

                }
                return dataTable;
            }
            catch (Exception ex)
            {
                msg = "执行查询错误:" + ex.ToString();
                return null;
            }
        }
        public static DataTable GetDataTable(string sql, string connStr)
        {

            try
            {
                string cs = connStr;//source();
                DataTable dataTable = new DataTable();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dataTable);

                }
                return dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("尝试连接sqlserver失败！" + connStr + ":失败原因：" + ex.ToString());
                return null;
            }
        }
    }
}