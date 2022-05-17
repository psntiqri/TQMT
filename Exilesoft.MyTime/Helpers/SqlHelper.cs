using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Exilesoft.MyTime.Util
{
    public static class SqlHelper
    {
        public static DataTable ExecuteStatement(string query)
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["LMSConnectionString"].ToString();
            SqlConnection Conn = new SqlConnection(ConnectionString);
            DataTable dt = new DataTable(); 
            try
            {
                Conn.Open();
                SqlDataAdapter DA = new SqlDataAdapter(query, Conn);
                DA.Fill(dt);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (Conn.State == ConnectionState.Open)
                {
                    Conn.Close();
                }
            }
            return dt;
        }
    }
}