using SQLitePCL;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bilibili2.Class
{
    class SqlHelper
    {
        #region 历史纪录

        #endregion
        #region 播放进度

        #endregion
        #region 下载进程

        #endregion

        private static String DB_NAME = "Info.db";
        private static String SQL_CREATE_TABLE = "CREATE TABLE IF NOT EXISTS {0} (aid TEXT,title TEXT,pic Text,type TEXT);";
        private static String SQL_QUERY_VALUE = "SELECT Value FROM {0} WHERE aid = ?;";
        private static String SQL_INSERT = "INSERT INTO {0} VALUES(?,?,?,?);";
        //private static String SQL_UPDATE = "UPDATE {0} SET Value = ? WHERE Key = ?";
        //private static String SQL_DELETE = "DELETE FROM {0} WHERE Key = ?";

        SQLiteConnection con;
        public void CreateTable(string tableName)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var state=con.Prepare(string.Format(SQL_CREATE_TABLE,tableName)))
            {
                state.Step();
            }
        }
        public void InsertValue()
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(string.Format(SQL_INSERT, "ViewHistory")))
            {
                statement.Bind(1, "1");
                statement.Bind(2, "2");
                statement.Bind(3, "3");
                statement.Bind(4, "4");
                statement.Step();
            }
        }

        public string QueryValue()
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(string.Format(SQL_QUERY_VALUE, "ViewHistory")))
            {
                statement.Bind(1, "1");
                SQLiteResult result = statement.Step();
                if (SQLiteResult.ROW == result)
                {
                    string a = string.Empty;
                    for (int i = 0; i < statement.ColumnCount-1; i++)
                    {
                        a += statement[i] as String+"\t";
                    }
                    return  a;
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        public void CreateShieldingTable()
        {
            con = new SQLiteConnection(DB_NAME);
            using (var state = con.Prepare("CREATE TABLE IF NOT EXISTS Shielding (text TEXT,mode INT);"))
            {
                state.Step();
            }
        }

        public void InsertShieldingValue(string text,int mode)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare("INSERT INTO Shielding VALUES(?,?);"))
            {
                statement.Bind(1, text);
                statement.Bind(2, mode);
                statement.Step();
            }
        }

        public List<string> GetShieldingText()
        {
            con = new SQLiteConnection(DB_NAME);
            using (var state=con.Prepare("SELECT text FROM Shielding WHERE mode = 0;"))
            {
                List<string> list = new List<string>();
                SQLiteResult result = state.Step();
                if (result == SQLiteResult.ROW)
                {

                    list.Add(state[0] as String);
                }
                return list;
            }
        }

       
    }
}
