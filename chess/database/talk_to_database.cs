using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace chess.database
{
    public class talk_to_database
    {
        public const string DBName = "chess.accdb";
        public static string connection_string = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={AppDomain.CurrentDomain.BaseDirectory}..\\..\\..\\database\\{DBName};Persist Security Info=False;";
        public DataSet RetrieveTable(string SQLStr, string table_name)
        {
            OleDbConnection con = new OleDbConnection(connection_string);
            OleDbCommand cmd = new OleDbCommand(SQLStr, con);
            OleDbDataAdapter ad = new OleDbDataAdapter(cmd);
            DataSet ds = new DataSet();
            ad.Fill(ds, table_name);
            return ds;
        }
        public static DataTable FilterTable(DataTable dt, string column, string criteria)
        {
            dt.DefaultView.RowFilter = column + "=" + criteria;
            return dt.DefaultView.ToTable();
        }
        public static object GetScalar(string SQL)
        {
            OleDbConnection con = new OleDbConnection(connection_string);
            OleDbCommand cmd = new OleDbCommand(SQL, con);
            con.Open();
            object scalar = cmd.ExecuteScalar();
            con.Close();
            return scalar;
        }
        public static int ExecuteNonQuery(string SQL)
        {
            OleDbConnection con = new OleDbConnection(connection_string);
            OleDbCommand cmd = new OleDbCommand(SQL, con);
            con.Open();
            int n = cmd.ExecuteNonQuery();
            con.Close();
            return n;
        }
        public int add_game(string player1, string player2)
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");//corrent time
            string SQL = $"INSERT INTO games (white_player_name, black_player_name, [time]) VALUES ('{player1}', '{player2}', #{time}#)"; int res = ExecuteNonQuery(SQL);
            if (res > 0)
            {
                return RetrieveTable($"SELECT * FROM games where time=#{time}#", "games").Tables[0].Rows[0].Field<int>("game_id");
            }
            return -1;
        }
        public int add_move(int game_id, Move move)
        {
            string SQL = $"INSERT INTO moves (game_id,from_x,from_y,to_x,to_y) VALUES ({game_id}, '{move.FromX}','{move.FromY}','{move.ToX}','{move.ToY}')";
            return ExecuteNonQuery(SQL);
        }
        public int amount_of_games()
        {
            int num = RetrieveTable("SELECT * FROM games", "games").Tables[0].Rows.Count;
            return num;
        }
    }
}
