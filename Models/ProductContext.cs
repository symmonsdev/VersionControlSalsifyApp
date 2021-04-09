using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Models
{

    using System.Data;
    using System.Data.SqlClient;
    using System.Reflection;
    using Infrastructure;
    using static SalsifyApp.Models.Product;

    public class ProductContext
    {

        //public async Task<List<ProductRootTest>> Open_Orders__ByShipToAsync(int? addrNum, DateTime? fromDate, string userName)
        //{
        //    //Initialize Result 
        //    List<ProductRootTest> lst = new List<ProductRootTest>();
        //    try
        //    {
        //        // Parameters   @addrNum int, @fromDate datetime, @userName nvarchar(100)
        //        SqlParameter p_addrNum = new SqlParameter("@addrNum", addrNum ?? (object)DBNull.Value);
        //        p_addrNum.Direction = ParameterDirection.Input;
        //        p_addrNum.DbType = DbType.Int32;
        //        p_addrNum.Size = 4;

        //        SqlParameter p_fromDate = new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value);
        //        p_fromDate.Direction = ParameterDirection.Input;
        //        p_fromDate.DbType = DbType.DateTime;
        //        p_fromDate.Size = 25;

        //        SqlParameter p_userName = new SqlParameter("@userName", userName ?? (object)DBNull.Value);
        //        p_userName.Direction = ParameterDirection.Input;
        //        p_userName.DbType = DbType.String;
        //        p_userName.Size = 100;


        //        // Processing 
        //        string sqlQuery = $@"EXEC [dbo].[Open_Orders__ByShipTo_CP] @addrNum, @fromDate, @userName";

        //        //var productCategory = "Electronics";

        //        // var product = await this.Open_Orders__ByShipTo.FromSqlRaw("EXECUTE [dbo].[Open_Orders__ByShipTo] {0},{1},{2}", 69249, Convert.ToDateTime("12/26/2019"), "mmartino").ToListAsync();

        //        //Output Data
        //       // lst = await this.ProductRootTest.FromSqlRaw(sqlQuery, p_addrNum, p_fromDate, p_userName).ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    //Return
        //    return lst;
        //}

        public static List<T> DataReaderMapToList<T>(IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    if (!object.Equals(dr[prop.Name], DBNull.Value))
                    {
                        prop.SetValue(obj, dr[prop.Name], null);
                    }
                }
                list.Add(obj);
            }
            return list;
        }

        public List<string> GetPMD_SKUs(string FromDate, string ToDate, string SP_Name)
        {
            var database = new Database();

            var reader = database.Execute_GetPMDSKUs_Reader(SP_Name, FromDate, ToDate); 

            List<string> skuList = new List<string>();

            while (reader.Read())
            {
                skuList.Add(reader.GetString(0));
                //dataList.Add(Convert.ToString(reader.GetValue(i)));  **** possibility
            }

            reader.Close();

            return skuList;
        }

        public int InsertHistoryRecord(string result, string task, string SKU_Nbr)
        {
            var database = new Database();

            int recordAffected = database.Execute_InsertHistoryRecord(result, task, SKU_Nbr); 

            return recordAffected;
        }

        public List<ProductMaster> GetPMDInfo(string SKU_Nbr)
        {
            var database = new Database();

            var reader = database.Execute_GetPMDInfo_Reader("GetPMDInfo_BySKU", SKU_Nbr); //replaced with server side stored procedure

            List<ProductMaster> pmdList = new List<ProductMaster>();
            pmdList = DataReaderMapToList<ProductMaster>(reader);
 
            reader.Close();
            

            //Get the order products
            //var sql2 =
            // "SELECT op.price, op.order_id, op.product_id, op.quantity, p.name, p.price FROM orderproduct op INNER JOIN product p on op.product_id=p.product_id";

            //var reader2 = database.ExecuteReader("sp_GetProducts"); //replaced with server side stored procedure

            //var values2 = new List<OrderProduct>();

            //while (reader2.Read())
            //{
            //    var record2 = (IDataRecord)reader2;

            //    values2.Add(new OrderProduct()
            //    {
            //        OrderId = record2.GetInt32(1),
            //        ProductId = record2.GetInt32(2),
            //        Price = record2.GetDecimal(0),
            //        Quantity = record2.GetInt32(3),
            //        Product = new Product()
            //        {
            //            Name = record2.GetString(4),
            //            Price = record2.GetDecimal(5)
            //        }
            //    });
            //}

            //reader2.Close();

            //foreach (var order in values)
            //{
            //    foreach (var orderproduct in values2)
            //    {
            //        if (orderproduct.OrderId != order.OrderId)
            //            continue;

            //        order.OrderProducts.Add(orderproduct);
            //        order.OrderTotal = order.OrderTotal + (orderproduct.Price * orderproduct.Quantity);
            //    }
            //}

            return pmdList;
        }

        public List<SalsifyDigitalAsset> GetImageInfo(string SKU_Nbr)
        {

            var database = new Database();

            var reader = database.Execute_GetPMDInfo_Reader("GetImageInfo_BySKU", SKU_Nbr); //replaced with server side stored procedure

            List<SalsifyDigitalAsset> imageList = new List<SalsifyDigitalAsset>();
            imageList = DataReaderMapToList<SalsifyDigitalAsset>(reader);

            reader.Close();


            //Get the order products
            //var sql2 =
            // "SELECT op.price, op.order_id, op.product_id, op.quantity, p.name, p.price FROM orderproduct op INNER JOIN product p on op.product_id=p.product_id";

            //var reader2 = database.ExecuteReader("sp_GetProducts"); //replaced with server side stored procedure

            //var values2 = new List<OrderProduct>();

            //while (reader2.Read())
            //{
            //    var record2 = (IDataRecord)reader2;

            //    values2.Add(new OrderProduct()
            //    {
            //        OrderId = record2.GetInt32(1),
            //        ProductId = record2.GetInt32(2),
            //        Price = record2.GetDecimal(0),
            //        Quantity = record2.GetInt32(3),
            //        Product = new Product()
            //        {
            //            Name = record2.GetString(4),
            //            Price = record2.GetDecimal(5)
            //        }
            //    });
            //}

            //reader2.Close();

            //foreach (var order in values)
            //{
            //    foreach (var orderproduct in values2)
            //    {
            //        if (orderproduct.OrderId != order.OrderId)
            //            continue;

            //        order.OrderProducts.Add(orderproduct);
            //        order.OrderTotal = order.OrderTotal + (orderproduct.Price * orderproduct.Quantity);
            //    }
            //}

            return imageList;
        }

        public List<SalsifyPricing> GetPricingInfo(string SKU_Nbr)
        {

            var database = new Database();

            var reader = database.Execute_GetPricingInfo_Reader("GetSalsifyPricing_BySKU", SKU_Nbr); 

            List<SalsifyPricing> pricingList = new List<SalsifyPricing>();
            pricingList = DataReaderMapToList<SalsifyPricing>(reader);

            reader.Close();


            //Get the order products
            //var sql2 =
            // "SELECT op.price, op.order_id, op.product_id, op.quantity, p.name, p.price FROM orderproduct op INNER JOIN product p on op.product_id=p.product_id";

            //var reader2 = database.ExecuteReader("sp_GetProducts"); //replaced with server side stored procedure

            //var values2 = new List<OrderProduct>();

            //while (reader2.Read())
            //{
            //    var record2 = (IDataRecord)reader2;

            //    values2.Add(new OrderProduct()
            //    {
            //        OrderId = record2.GetInt32(1),
            //        ProductId = record2.GetInt32(2),
            //        Price = record2.GetDecimal(0),
            //        Quantity = record2.GetInt32(3),
            //        Product = new Product()
            //        {
            //            Name = record2.GetString(4),
            //            Price = record2.GetDecimal(5)
            //        }
            //    });
            //}

            //reader2.Close();

            //foreach (var order in values)
            //{
            //    foreach (var orderproduct in values2)
            //    {
            //        if (orderproduct.OrderId != order.OrderId)
            //            continue;

            //        order.OrderProducts.Add(orderproduct);
            //        order.OrderTotal = order.OrderTotal + (orderproduct.Price * orderproduct.Quantity);
            //    }
            //}

            return pricingList;
        }

        public string GetSalsifyValue(string ClassProperty, string PortalValue)
        {
            var database = new Database();

            string return_value = database.Execute_GetSalsifyValue_Reader(ClassProperty, PortalValue); //replaced with server side stored procedure

            return return_value;
        }

    }
}
