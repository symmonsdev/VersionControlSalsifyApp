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

    public class SKUContext
    {

        public string[] GetSKUList(string SKU_Nbr)
        {
            var database = new Database();
            //F554101A BHLITM
            string query = "select Top 10 BHLITM from F554101A where BHLITM LIKE ''+@SKUNum+'%'";

            //string query = "select Top 10 IMLITM from F4101 where IMLITM LIKE ''+@SKUNum+'%'";
            //string query = "select Top 10 IMLITM from F4101 where IMLITM LIKE 'ta-10%'";

            string[] skuList = database.Execute_GetSKUList_Reader(query, SKU_Nbr); //replaced with server side stored procedure
           
            return skuList;
        }

    }
}
