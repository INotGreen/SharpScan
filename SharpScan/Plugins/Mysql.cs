using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Collections;


//Mysql.Data.Dll from:https://github.com/mysql/mysql-connector-net/releases/tag/6.9.12


namespace SharpScan
{
    public class MySqlBroute
    {
        //static void Main(string[] args)
        //{
        //    System.Console.WriteLine("");
        //    System.Console.WriteLine("Author: Uknow");
        //    System.Console.WriteLine("Github: https://github.com/uknowsec/SharpSQLDump");
        //    System.Console.WriteLine("");
        //    if (args.Length != 7)
        //    {
        //        System.Console.WriteLine("Usage: SharpSQLDump.exe -h ip -u username -p password -mysql");
        //        System.Console.WriteLine("       SharpSQLDump.exe -h ip -u username -p password -mssql");
        //    }
        //    if (args.Length >= 7 && (args[6] == "-mysql"))
        //    {
        //        Console.WriteLine("\r\n==================== SharpSQLDump --> MySQL ====================\r\n");
        //        MySql(args[1],args[3],args[5]);
        //        Console.ForegroundColor = ConsoleColor.White;
        //    }
        //    if (args.Length >= 7 && (args[6] == "-mssql"))
        //    {
        //        Console.WriteLine("\r\n==================== SharpSQLDump --> MsSQL========== ==========\r\n");
        //        MsSql(args[1], args[3], args[5]);
        //        Console.ForegroundColor = ConsoleColor.White;
        //    }
        //}



        public static void MySql(String host, String username, String password)
        {
            ArrayList Datebase = MySQL_DateBase(host, username, password);
            foreach (string date in Datebase)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\n[*] DataBases: " + date + " ");
                ArrayList Tables = MySQL_Table(host, username, password, date);
                foreach (string table in Tables)
                {
                    ArrayList Columns = MySQL_Column(host, username, password, date, table);
                    int count = MySQL_Count(host, username, password, date, table);
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

        public static ArrayList MySQL_DateBase(string server, string username, string password, string port = "3306")
        {
            //Ip+端口+数据库名+用户名+密码
            string connectStr = "server=" + server + ";port=" + port + ";database=information_schema" + ";user=" + username + ";password=" + password + ";";
            ArrayList datebase = new ArrayList();
            MySqlConnection conn = new MySqlConnection(connectStr); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "select schema_name from  information_schema.schemata";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySql.Data.MySqlClient.MySqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    //  Console.WriteLine(" Datebase: " + msqlReader[0]);
                    if ((msqlReader[0].ToString() != "information_schema") && (msqlReader[0].ToString() != "mysql") && (msqlReader[0].ToString() != "performance_schema") && (msqlReader[0].ToString() != "sys"))
                    {
                        datebase.Add(msqlReader[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Clone();
            }
            return datebase;
        }

        public static ArrayList MySQL_Table(string server, string username, string password, string database, string port = "3306")
        {
            //Ip+端口+数据库名+用户名+密码
            string connectStr = "server=" + server + ";port=" + port + ";database=information_schema" + ";user=" + username + ";password=" + password + ";";
            ArrayList tables = new ArrayList();
            MySqlConnection conn = new MySqlConnection(connectStr); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "select table_name from information_schema.tables where table_schema='" + database + "';";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySql.Data.MySqlClient.MySqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    tables.Add(msqlReader[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Clone();
            }
            return tables;
        }

        public static ArrayList MySQL_Column(string server, string username, string password, string database, string table, string port = "3306")
        {
            //Ip+端口+数据库名+用户名+密码
            string connectStr = "server=" + server + ";port=" + port + ";database=information_schema" + ";user=" + username + ";password=" + password + ";";
            ArrayList columns = new ArrayList();
            MySqlConnection conn = new MySqlConnection(connectStr); ;
            try
            {
                conn.Open();//跟数据库建立连接，并打开连接
                string sql = "select column_name from information_schema.columns where table_schema='" + database + "' and table_name='" + table + "'";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySql.Data.MySqlClient.MySqlDataReader msqlReader = cmd.ExecuteReader();
                while (msqlReader.Read())
                {   //do something with each record
                    columns.Add(msqlReader[0]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                conn.Clone();
            }
            return columns;
        }

        public static int MySQL_Count(string server, string username, string password, string database, string table, string port = "3306")
        {
            string connectStr = "server=" + server + ";port=" + port + ";database=" + database + ";user=" + username + ";password=" + password + ";";
            // server=127.0.0.1/localhost 代表本机，端口号port默认是3306可以不写
            MySqlConnection conn = new MySqlConnection(connectStr);
            try
            {
                conn.Open();//打开通道，建立连接，可能出现异常,使用try catch语句
                string sql = "select count(*) from " + table;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                Object result = cmd.ExecuteScalar();//执行查询，并返回查询结果集中第一行的第一列。所有其他的列和行将被忽略。select语句无记录返回时，ExecuteScalar()返回NULL值
                if (result != null)
                {
                    int count = int.Parse(result.ToString());
                    return count;
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return 0;
        }



    }
}
