using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Infrastructure
{

    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;

    public class Database
    {

        private readonly SqlConnection _connection;

        public Database()
        {
            _connection = new SqlConnection("Server=SYM-CITY-TEST\\SYMMONSTEST;;Initial Catalog=SymPortal;Persist Security Info=True;User ID=admsymportal;Password=symportal05");

            _connection.Open();
        }

        public DbDataReader Execute_GetPMDSKUs_Reader(string query, string FromDate, string ToDate)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection);
            // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@FromDate", FromDate);
            sqlCmd.Parameters.AddWithValue("@ToDate", ToDate);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            return sqlCmd.ExecuteReader();
        }

        public DbDataReader Execute_GetPMDInfo_Reader(string query, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection); 
           // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            return sqlCmd.ExecuteReader();
        }
        public int Execute_InsertHistoryRecord(string result, string task, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand("InsertSalsifyHistory", _connection);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@Result", result);
            sqlCmd.Parameters.AddWithValue("@Task", task);
            sqlCmd.Parameters.AddWithValue("@SKU", SKU_Nbr);

            return sqlCmd.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string query)
        {
            var sqlQuery = new SqlCommand(query, _connection);

            return sqlQuery.ExecuteNonQuery();
        }

        public string[] Execute_GetSKUList_Reader(string query, string SKU_Nbr)
        {
            
            List<string> skuResult = new List<string>();

            SqlCommand sqlCmd = new SqlCommand(query, _connection);
            sqlCmd.CommandType = CommandType.Text;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);

            SqlDataReader dr = sqlCmd.ExecuteReader();
            while (dr.Read())
            {
                //skuResult.Add(dr["IMLITM"].ToString());//F4101
                skuResult.Add(dr["BHLITM"].ToString());//F554101A
            }

            dr.Close();

            return skuResult.ToArray();
        }

        public string Execute_GetSalsifyValue_Reader(string ClassProperty, string PortalValue)
        {

            string Result = "";

            SqlCommand sqlCmd = new SqlCommand("GetSalsifyValue", _connection);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@ClassProperty", ClassProperty);
            sqlCmd.Parameters.AddWithValue("@PortalValue", PortalValue);

            SqlDataReader dr = sqlCmd.ExecuteReader();

            if (dr.HasRows) { 
               while (dr.Read())
               {
                       Result = dr["Salsify_Property_ID"].ToString();
               }
            }
            else
            {
                Result = PortalValue; //return passed value if no match in db table
            }

            dr.Close();

            return Result;
        }

        public DbDataReader Execute_GetPricingInfo_Reader(string query, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection);
            // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            return sqlCmd.ExecuteReader();
        }

    }


}
