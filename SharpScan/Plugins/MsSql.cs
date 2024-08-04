using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace SharpScan
{
    internal class MsSqlBroute
    {
        public static void MsSql(String host, String username, String password)
        {
            ArrayList Datebase = MsSQL_DateBase(host, username, password);
            foreach (string date in Datebase)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n[*] DataBases: " + date + " ");
                ArrayList Tables = MsSQL_Table(host, username, password, date);
                foreach (string table in Tables)
                {
                    ArrayList Columns = MsSQL_Column(host, username, password, date, table);
                    int count = MsSQL_Count(host, username, password, date, table);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\n\t[+] Tables: " + String.Format("{0,-12}", table));
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("\n\t\tCount: " + count + "\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("\t\t[-] Columns: [");
                    foreach (string column in Columns)
                    {
                        Console.Write(column + " ");
                    }
                    Console.WriteLine("]");
                }
            }
        }

        public static ArrayList MsSQL_DateBase(string Server, string User, string Password)
        {
            //Ip+端口+数据库名+用户名+密码
            string connectionString = "Server = " + Server + ";" + "Database = master;" + "User ID = " + User + ";" + "Password = " + Password + ";";
            ArrayList datebase = new ArrayList();
            SqlConnection conn = new SqlConnection(connectionString); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "SELECT NAME FROM MASTER.DBO.SYSDATABASES ORDER BY NAME";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    //  Console.WriteLine(" Datebase: " + msqlReader[0]);
                    if ((msqlReader[0].ToString() != "master") && (msqlReader[0].ToString() != "model") && (msqlReader[0].ToString() != "msdb") && (msqlReader[0].ToString() != "tempdb"))
                    {
                        datebase.Add(msqlReader[0]);
                    }
                }
                msqlReader.Close();     //要记得每次调用SqlDataReader读取数据后，都要Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Close();
            }
            return datebase;
        }


        public static ArrayList MsSQL_Table(string Server, string User, string Password, string DataBase)
        {
            //Ip+端口+数据库名+用户名+密码
            string connectionString = "Server = " + Server + ";" + "Database =" + DataBase + ";" + "User ID = " + User + ";" + "Password = " + Password + ";";
            ArrayList tables = new ArrayList();
            SqlConnection conn = new SqlConnection(connectionString); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "SELECT NAME FROM SYSOBJECTS WHERE XTYPE='U' ORDER BY NAME";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    tables.Add(msqlReader[0]);
                }
                msqlReader.Close();     //要记得每次调用SqlDataReader读取数据后，都要Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Close();
            }
            return tables;
        }

        public static ArrayList MsSQL_Column(string Server, string User, string Password, string DataBase, string table)
        {
            //Ip+端口+数据库名+用户名+密码
            string connectionString = "Server = " + Server + ";" + "Database =" + DataBase + ";" + "User ID = " + User + ";" + "Password = " + Password + ";";
            ArrayList columns = new ArrayList();
            SqlConnection conn = new SqlConnection(connectionString); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "SELECT NAME FROM SYSCOLUMNS WHERE ID=OBJECT_ID('" + table + "');";
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    columns.Add(msqlReader[0]);
                }
                msqlReader.Close();     //要记得每次调用SqlDataReader读取数据后，都要Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Close();
            }
            return columns;
        }


        public static int MsSQL_Count(string Server, string User, string Password, string DataBase, string table)
        {
            //Ip+端口+数据库名+用户名+密码
            string connectionString = "Server = " + Server + ";" + "Database =" + DataBase + ";" + "User ID = " + User + ";" + "Password = " + Password + ";";
            ArrayList columns = new ArrayList();
            SqlConnection conn = new SqlConnection(connectionString); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "select count(*) from " + table;
                SqlCommand cmd = new SqlCommand(sql, conn);
                SqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    int count = int.Parse(msqlReader[0].ToString());
                    return count;
                }
                msqlReader.Close();     //要记得每次调用SqlDataReader读取数据后，都要Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Close();
            }
            return 0;
        }

    }
}
