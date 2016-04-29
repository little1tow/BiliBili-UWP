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

        #region 播放进度
        private static string DB_NAME = "Info.db";
        private static string SQL_CREATE_TABLE = "CREATE TABLE IF NOT EXISTS PositionTable (Cid TEXT primary key,Position INTEGER);";
        private static string SQL_QUERY_VALUE = "SELECT * FROM PositionTable WHERE Cid = ?;";
        private static string SQL_INSERT = "INSERT INTO PositionTable VALUES(?,?);";
        private static string SQL_UPDATE = "UPDATE PositionTable SET Position = ? WHERE Cid = ?";
        private static string SQL_DELETE = "DELETE FROM PositionTable WHERE Key = ?";
        #endregion

        SQLiteConnection con;
        public void CreateTable()
        {
            con = new SQLiteConnection(DB_NAME);
            using (var state=con.Prepare(SQL_CREATE_TABLE))
            {
                state.Step();
            }
        }
        public void InsertValue(string Cid)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(SQL_INSERT))
            {
                statement.Bind(1, Cid);
                statement.Bind(2, 0);
                statement.Step();
            }
        }

        public void UpdateValue(string Cid, int position)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(SQL_UPDATE))
            {
                statement.Bind(1, position);
                statement.Bind(2, Cid);
                statement.Step();
            }
        }
        public long QueryValue(string cid)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(SQL_QUERY_VALUE))
            {
                statement.Bind(1, cid);
                SQLiteResult result = statement.Step();
                if (SQLiteResult.ROW == result)
                {
                    long a = (long)statement["Position"];
                    return a;
                }
                else
                {
                    return 0;
                }
            }
        }

       public bool ValuesExists(string cid)
        {
            con = new SQLiteConnection(DB_NAME);
            using (var statement = con.Prepare(SQL_QUERY_VALUE))
            {
                statement.Bind(1, cid);
                SQLiteResult result = statement.Step();
                if (SQLiteResult.ROW == result)
                {
                    if (statement["Position"] == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

        }


    }

   

}
