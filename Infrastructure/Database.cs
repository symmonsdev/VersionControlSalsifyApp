using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Infrastructure
{
    using SalsifyApp.Models;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using static SalsifyApp.Models.Product;

    public class Database
    {

        public readonly SqlConnection _connection;

        public Database()
        {

            _connection = new SqlConnection("Server=SYM-CITY-DB;;Initial Catalog=SymPortal;Persist Security Info=True;User ID=admsymportal;Password=symportal05");
            //_connection = new SqlConnection("Server=SYM-CITY-TEST\\SYMMONSTEST;;Initial Catalog=SymPortal;Persist Security Info=True;User ID=admsymportal;Password=symportal05");

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

        public List<SalsifyDigitalAsset> Execute_GetPMD_DigitalInfo_Reader(string query, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection);
            // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataReader reader = sqlCmd.ExecuteReader();
            List<SalsifyDigitalAsset> imageList = new List<SalsifyDigitalAsset>();
            imageList = ProductContext.DataReaderMapToList<SalsifyDigitalAsset>(reader);
            reader.Close();
            _connection.Close();
            return imageList;

            //return sqlCmd.ExecuteReader();
        }

        public List<ProductMaster> Execute_GetPMDInfo_Reader(string query, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection); 
           // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataReader reader = sqlCmd.ExecuteReader();
            List <ProductMaster> pmdList = new List<ProductMaster>();
            pmdList = ProductContext.DataReaderMapToList<ProductMaster>(reader);
            reader.Close();
            _connection.Close();
            return pmdList;

            //return sqlCmd.ExecuteReader();
        }
        public int Execute_InsertHistoryRecord(string result, string task, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand("InsertSalsifyHistory", _connection);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@Result", result);
            sqlCmd.Parameters.AddWithValue("@Task", task);
            sqlCmd.Parameters.AddWithValue("@SKU", SKU_Nbr);

            int recordsAffected = sqlCmd.ExecuteNonQuery();
            sqlCmd.Dispose();
            _connection.Close();
            return recordsAffected;

            //return sqlCmd.ExecuteNonQuery();
        }

        public int ExecuteNonQuery(string query)
        {
            var sqlQuery = new SqlCommand(query, _connection);

            int recordsAffected = sqlQuery.ExecuteNonQuery();
            sqlQuery.Dispose();
            _connection.Close();
            return recordsAffected;

            //return sqlQuery.ExecuteNonQuery();
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
            _connection.Close();

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
            _connection.Close();

            return Result;
        }

        public List<SalsifyPricing> Execute_GetPricingInfo_Reader(string query, string SKU_Nbr)
        {
            SqlCommand sqlCmd = new SqlCommand(query, _connection);
            // sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("@SKUNum", SKU_Nbr);
            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataReader reader = sqlCmd.ExecuteReader();
            List<SalsifyPricing> pricingList = new List<SalsifyPricing>();
            pricingList = ProductContext.DataReaderMapToList<SalsifyPricing>(reader);
            reader.Close();
            _connection.Close();

            return pricingList;

            //return sqlCmd.ExecuteReader();

        }

    }


}
