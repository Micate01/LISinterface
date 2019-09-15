/*************************************************************
* MySQL数据库操作类	                                    
* 作者：张祥明 
* 时间：2011-08-15
* 功能描述：执行数据库的增删改查
* Copyright (C) 2011 中联信息产业有限责任公司 版权所有。 
**************************************************************/
using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Web;
using MySQLDriverCS;
namespace ZLCHSLisComm
{
    /// <summary>
    /// MySQL数据库操作助手
    /// </summary>
   public class MySqlHelper
   {
      
           public static Write_Log wg = new Write_Log();
           public static  string cnnStr = "";
    
        /// <summary>
        /// 执行SQL语句
        /// </summary>
        /// <param name="cnn">数据库连接</param>
        /// <param name="sql">SQL语句</param>
        public static void DoSql(MySQLConnection cnn, string sql)
        {
            new MySQLCommand(sql, cnn).ExecuteNonQuery();
        }

        public static long DoSqlcount(MySQLConnection cnn, string sql)
        {
            MySQLCommand command = new MySQLCommand(sql, cnn);
            return (long)Convert.ToInt32(command.ExecuteScalar());
        }


        /// <summary>
        /// 获取MySQL连接
        /// </summary>
        /// <returns></returns>
        public static MySQLConnection GetCnn()
        {
            string cs = "Data Source=BoHui;Password=12345678;User ID=root;Location=localhost";
            MySQLConnection connection = new MySQLConnection(cs);
            connection.Open();
            return connection;
        }

        public static MySQLCommand GetCommand(string sql)
        {
            return new MySQLCommand(sql, GetCnn());
        }

        /// <summary>
        /// 获取DataSet数据对象
        /// </summary>
        /// <param name="sqlstr">SQL语句</param>
        /// <returns></returns>
        public static DataSet GetData(string sqlstr)
        {
            string cs = cnnStr;//source();
            DataSet dataSet = new DataSet();
            using (MySQLConnection connection = new MySQLConnection(cs))
            {
                connection.Open();
                new MySQLDataAdapter(sqlstr, connection).Fill(dataSet);
                connection.Close();
            }
            return dataSet;
        }

        public static MySQLDataAdapter GetDataAdapter(string sqlstr)
        {
            MySQLConnection conn = new MySQLConnection(source());
            conn.Open();
            return new MySQLDataAdapter(sqlstr, conn);
        }

        /// <summary>
        /// 获取MySql数据表
        /// </summary>
        /// <param name="sqlstr">sql语句</param>
        /// <returns></returns>
        public static System.Data.DataTable GetDataTable(string sqlstr)
        {
            try
            {
                string cs = cnnStr;//source();
                System.Data.DataTable dataTable = new System.Data.DataTable();
                using (MySQLConnection connection = new MySQLConnection(cs))
                {
                    connection.Open();
                    new MySQLDataAdapter(sqlstr, connection).Fill(dataTable);
                    connection.Close();
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                wg.Write("log", "MySQL错误：" + ex.ToString());
                return null;
            }
        }

        public static System.Data.DataTable GetDataTableFromExcel(string Path)
        {
            string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source = " + Path + ";Extended Properties ='Excel 8.0;HDR=Yes;IMEX=1'";
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception exception)
            {
                wg.Write("log","MySQL错误：" + exception.ToString());
                throw;
            }
            string selectCommandText = "";
            OleDbDataAdapter adapter = null;
            System.Data.DataTable dataTable = null;
            selectCommandText = "select * from [sheet1$]";
            adapter = new OleDbDataAdapter(selectCommandText, connectionString);
            dataTable = new System.Data.DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            return dataTable;
        }

        public static System.Data.DataTable GetDataTableFromExcel2(string Path)
        {
            string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source = " + Path + ";Extended Properties ='Excel 8.0;HDR=Yes;IMEX=1'";
            OleDbConnection connection = new OleDbConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch
            {
                throw;
            }
            string selectCommandText = "";
            OleDbDataAdapter adapter = null;
            System.Data.DataTable dataTable = null;
            selectCommandText = "select * from [sheet2$]";
            adapter = new OleDbDataAdapter(selectCommandText, connectionString);
            dataTable = new System.Data.DataTable();
            adapter.Fill(dataTable);
            connection.Close();
            return dataTable;
        }

        public static int OperateData(string sql)
        {
            using (MySQLConnection connection = new MySQLConnection(source()))
            {
                connection.Open();
                DoSql(connection, sql);
                connection.Close();
            }
            return 1;
        }

        public static long OperateDatacount(string sql)
        {
            long num = 0;
            using (MySQLConnection connection = new MySQLConnection(source()))
            {
                connection.Open();
                num = DoSqlcount(connection, sql);
                connection.Close();
            }
            return num;
        }

        public static string source()
        {
            string path = @".\source.bh";
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(stream);
            return reader.ReadLine();
        }
    }
}
