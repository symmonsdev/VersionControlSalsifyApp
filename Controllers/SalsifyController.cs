using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SalsifyApp.Models;

namespace SalsifyApp.Controllers
{
    public class SalsifyController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Delete()
        {
            return View();
        }

        public IActionResult Update()
        {
            return View();
        }

        public IActionResult CreateMulti()
        {
            return View();
        }

        public IActionResult UpdateMulti()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AutoComplete(string SKU_Nbr)
        {
            
            SKUContext SKUdb = new SKUContext();
            return Json(SKUdb.GetSKUList(SKU_Nbr));

        }

        [HttpPost]
        public ActionResult Create(string SKUName)
        {
            var model = new Product();

            //get pmd data from portal table and put in ProductMaster class
            var models = new ProductContext();

            //get pricing data from portal table and put in SalsifyPricing class
            List<Product.SalsifyPricing> pricing = models.GetPricingInfo(SKUName);

            //get pmd data from portal table and put in ProductMaster class
            List<Product.ProductMaster> pmd = models.GetPMDInfo(SKUName);

            if (pmd.Count == 0)
            {
                ViewData["Message"] = SKUName + " was not found in the Portal Product Master";
                return View();
            }

            //get digital asset data from portal table and put in SalsifyDigitalAsset class
            List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

            //harcode for test
            //pmd[0].Country_of_Origin = "United States";
            //pmd[0].GPC_Code = null;
            //pmd[0].Finish = null;
            //pmd[0].Cal_Green_Compliant = "Y";
            //pmd[0].Escutcheon_Width_in = null;
            //pmd[0].Lead_Time_Days = null;

            //*** TEST ONLY REMOVE THIS when digital assets logic in place
            //digital[0].MainImage_ID = "ffb70efc5c97c5ad8b60e0c954329579624170c0"; //must be ID

            //Check if Salsify Product ID in Salsify is different than what's being passed from the UDC table F0005
            pmd[0].Cal_Green_Compliant = GetSalsifyValue("Cal_Green_Compliant", pmd[0].Cal_Green_Compliant);
            pmd[0].Country_of_Origin = GetSalsifyValue("Country_of_Origin", pmd[0].Country_of_Origin);
            pmd[0].Diverter_Location = GetSalsifyValue("Diverter_Location", pmd[0].Diverter_Location);
            pmd[0].Holder_Type = GetSalsifyValue("Holder_Type", pmd[0].Holder_Type);
            pmd[0].Spout_Type = GetSalsifyValue("Spout_Type", pmd[0].Spout_Type);
            pmd[0].THD_Category = GetSalsifyValue("THD_Category", pmd[0].THD_Category);
            pmd[0].Type_of_Connection = GetSalsifyValue("Type_of_Connection", pmd[0].Type_of_Connection);
            pmd[0].Valve_System_Type = GetSalsifyValue("Valve_System_Type", pmd[0].Valve_System_Type);
            pmd[0].Product_Category = GetSalsifyValue("Product_Category", pmd[0].Product_Category);

            //Add Pricing US

            // spot for US indicator
            //pmd[0].ListPrice = pricing[0].ListPrice.ToString("0.00");
            //pmd[0].MAPPrice = pricing[0].MAPPrice.ToString("0.00");
            //pmd[0].WholesalePrice = pricing[0].WholesalePrice.ToString("0.00");

            //Add Pricing Canada  use [1]
            if (pricing.Count != 0)
            {
                pmd[0].CAListPrice = pricing[1].ListPrice.ToString("0.00");
            }

            //if (pmd[0].Tier != null)
            //{
            //    switch (pmd[0].Tier)
            //    {
            //        case "RDS":
            //            pmd[0].Tier = "ReadyStock";
            //            break;
            //        case "LTS":
            //            pmd[0].Tier = "Limited Stock";
            //            break;
            //        case "SPO":
            //            pmd[0].Tier = "Special Order";
            //            break;
            //    }
            //}

            //remove special characters
            //CleanupPropertyValues(pmd);

            //replace "" with null
            DefaultBlankstoNull(pmd);
            DefaultDigitalBlankstoNull(digital);
            DefaultPricingBlankstoNull(pricing);

            using (var client = new HttpClient())
            {
                var responseBody = String.Empty;

                Product.PMDGroup group = new Product.PMDGroup();
                group.master = new List<Product.ProductMaster>();
                group.master.Add(pmd[0]);
                if (digital.Count != 0)
                { 
                    group.assets = new List<Product.SalsifyDigitalAsset>();
                    group.assets.Add(digital[0]);
                }
                if (pricing.Count != 0)
                {
                    group.price = new List<Product.SalsifyPricing>();
                    group.price.Add(pricing[0]);
                }

                var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                jsonRequest = CleanUpRequest(jsonRequest);

                 var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                //HTTP POST
                var postTask = client.PostAsync(client.BaseAddress.ToString(), content);
                postTask.Wait();

                var result = postTask.Result;

                //Digital Assets
                //var jsonRequestIM = JsonConvert.SerializeObject(digital[0]);

                //var contentIM = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                //var clientIM = new HttpClient();
                //clientIM.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/digital_assets/?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                ////HTTP POST
                //var postTaskIM = clientIM.PostAsync(clientIM.BaseAddress.ToString(), contentIM);
                //postTaskIM.Wait();

                //var resultIM = postTaskIM.Result;


                var msg = result.RequestMessage;
                if (result.IsSuccessStatusCode)
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Create", SKUName);

                    ViewData["Message"] = SKUName + " has been created in Salsify";
                    return View();
                }
                else
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Failed", "Create", SKUName);

                    ViewData["Message"] = "Error Creating Product " + SKUName;
                    return View();
                }
            
        }

        //ViewBag.Message = SKUName + " has been created in Salsify";
        //    return View();
        }

        [HttpPost]
        public ActionResult CreateMulti(string FromDate, string ToDate)
        {
            int NbrCreated = 0;
            int NbrFailed = 0;

            var model = new Product();

            //get pmd data from portal table and put in ProductMaster class
            var models = new ProductContext();

            //get list of SKUs to process based on date range
            List<string> SKUs = models.GetPMD_SKUs(FromDate, ToDate, "GetNewPMDSKUs_ByDateRange");

            foreach (string SKUName in SKUs)
            {

                //get pricing data from portal table and put in SalsifyPricing class
                List<Product.SalsifyPricing> pricing = models.GetPricingInfo(SKUName);

                //get pmd data from portal table and put in ProductMaster class
                List<Product.ProductMaster> pmd = models.GetPMDInfo(SKUName);

                if (pmd.Count != 0) //Go to next SKU if not found in DB
                {
                    //get digital asset data from portal table and put in SalsifyDigitalAsset class
                    List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

                    //harcode for test
                    //pmd[0].Country_of_Origin = "United States";
                    //pmd[0].GPC_Code = null;
                    //pmd[0].Finish = null;
                    //pmd[0].Cal_Green_Compliant = "Y";
                    //pmd[0].Escutcheon_Width_in = null;
                    //pmd[0].Lead_Time_Days = null;

                    //*** TEST ONLY REMOVE THIS when digital assets logic in place
                    //digital[0].MainImage_ID = "ffb70efc5c97c5ad8b60e0c954329579624170c0"; //must be ID

                    //Check if Salsify Product ID in Salsify is different than what's being passed from the UDC table F0005
                    pmd[0].Cal_Green_Compliant = GetSalsifyValue("Cal_Green_Compliant", pmd[0].Cal_Green_Compliant);
                    pmd[0].Country_of_Origin = GetSalsifyValue("Country_of_Origin", pmd[0].Country_of_Origin);
                    pmd[0].Diverter_Location = GetSalsifyValue("Diverter_Location", pmd[0].Diverter_Location);
                    pmd[0].Holder_Type = GetSalsifyValue("Holder_Type", pmd[0].Holder_Type);
                    pmd[0].Spout_Type = GetSalsifyValue("Spout_Type", pmd[0].Spout_Type);
                    pmd[0].THD_Category = GetSalsifyValue("THD_Category", pmd[0].THD_Category);
                    pmd[0].Type_of_Connection = GetSalsifyValue("Type_of_Connection", pmd[0].Type_of_Connection);
                    pmd[0].Valve_System_Type = GetSalsifyValue("Valve_System_Type", pmd[0].Valve_System_Type);
                    pmd[0].Product_Category = GetSalsifyValue("Product_Category", pmd[0].Product_Category);

                    //Add Pricing US
                    // spot for US indicator
                    //pmd[0].ListPrice = pricing[0].ListPrice.ToString("0.00");
                    //pmd[0].MAPPrice = pricing[0].MAPPrice.ToString("0.00");
                    //pmd[0].WholesalePrice = pricing[0].WholesalePrice.ToString("0.00");

                    //Add Pricing Canada  use [1]
                    if (pricing.Count != 0)
                    { 
                        pmd[0].CAListPrice = pricing[1].ListPrice.ToString("0.00");
                    }

                    //if (pmd[0].Tier != null)
                    //{
                    //    switch (pmd[0].Tier)
                    //    {
                    //        case "RDS":
                    //            pmd[0].Tier = "ReadyStock";
                    //            break;
                    //        case "LTS":
                    //            pmd[0].Tier = "Limited Stock";
                    //            break;
                    //        case "SPO":
                    //            pmd[0].Tier = "Special Order";
                    //            break;
                    //    }
                    //}


                    //ViewData["Message"] = SKUName + " was not found in the Portal Product Master";
                    //return View();

                    //remove special characters
                    //CleanupPropertyValues(pmd);

                    //replace "" with null
                    DefaultBlankstoNull(pmd);
                    DefaultDigitalBlankstoNull(digital);
                    DefaultPricingBlankstoNull(pricing);


                    using (var client = new HttpClient())
                    {
                        var responseBody = String.Empty;

                        Product.PMDGroup group = new Product.PMDGroup();
                        group.master = new List<Product.ProductMaster>();
                        group.master.Add(pmd[0]);
                        if (digital.Count != 0)
                        {
                            group.assets = new List<Product.SalsifyDigitalAsset>();
                            group.assets.Add(digital[0]);
                        }
                        if (pricing.Count != 0)
                        {
                            group.price = new List<Product.SalsifyPricing>();
                            group.price.Add(pricing[0]);
                        }

                        var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                        jsonRequest = CleanUpRequest(jsonRequest);

                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                        //HTTP POST
                        var postTask = client.PostAsync(client.BaseAddress.ToString(), content);
                        postTask.Wait();

                        var result = postTask.Result;

                        //Digital Assets
                        //var jsonRequestIM = JsonConvert.SerializeObject(digital[0]);

                        //var contentIM = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        //var clientIM = new HttpClient();
                        //clientIM.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/digital_assets/?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                        ////HTTP POST
                        //var postTaskIM = clientIM.PostAsync(clientIM.BaseAddress.ToString(), contentIM);
                        //postTaskIM.Wait();

                        //var resultIM = postTaskIM.Result;


                        var msg = result.RequestMessage;
                        if (result.IsSuccessStatusCode)
                        {
                            //record result in history
                            int recordAffected = models.InsertHistoryRecord("Succeeded", "Create", SKUName);

                            NbrCreated += 1;
                            //ViewData["Message"] = SKUName + " has been created in Salsify";
                            //return View();
                        }
                        else
                        {
                            //record result in history
                            int recordAffected = models.InsertHistoryRecord("Failed", "Create", SKUName);

                            NbrFailed += 1;
                            //ViewData["Message"] = "Error Creating Product " + SKUName;
                            //return View();
                        }

                    }

                }

            }

            ViewData["Message"] = SKUs.Count + " SKUs have been process by Salsify, " + NbrCreated + " Created, " + NbrFailed + " Failed";
            return View();
        }

        [HttpPost]
        public ActionResult Update(string SKUName)
        {
            var model = new Product();

            //get pmd data from portal table and put in ProductMaster class
            var models = new ProductContext();

            //get pricing data from portal table and put in SalsifyPricing class
            List<Product.SalsifyPricing> pricing = models.GetPricingInfo(SKUName);

            //get pricing data from portal table and put in SalsifyPricing class
            List<Product.ProductMaster> pmd = models.GetPMDInfo(SKUName);

            if (pmd.Count == 0)
            {
                ViewData["Message"] = SKUName + " was not found in the Portal Product Master";
                return View();
            }

            //get digital asset data from portal table and put in SalsifyDigitalAsset class
            List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

            //harcode for test
            //pmd[0].Country_of_Origin = "United States";
            //pmd[0].GPC_Code = null;
            //pmd[0].Finish = null;
            //**** REMOVE TEST ONLY
            //pmd[0].Cal_Green_Compliant = "CALGreen";
            //*** TEST ONLY REMOVE THIS when digital assets logic in place
            //digital[0].MainImage_ID = "0befd3e19af88801880efae987f4fcc0ee08ab5e"; //must be ID

            pmd[0].Cal_Green_Compliant = GetSalsifyValue("Cal_Green_Compliant", pmd[0].Cal_Green_Compliant);
            pmd[0].Country_of_Origin = GetSalsifyValue("Country_of_Origin", pmd[0].Country_of_Origin);
            pmd[0].Diverter_Location = GetSalsifyValue("Diverter_Location", pmd[0].Diverter_Location);
            pmd[0].Holder_Type = GetSalsifyValue("Holder_Type", pmd[0].Holder_Type);
            pmd[0].Spout_Type = GetSalsifyValue("Spout_Type", pmd[0].Spout_Type);
            pmd[0].THD_Category = GetSalsifyValue("THD_Category", pmd[0].THD_Category);
            pmd[0].Type_of_Connection = GetSalsifyValue("Type_of_Connection", pmd[0].Type_of_Connection);
            pmd[0].Valve_System_Type = GetSalsifyValue("Valve_System_Type", pmd[0].Valve_System_Type);
            pmd[0].Product_Category = GetSalsifyValue("Product_Category", pmd[0].Product_Category);

            //Add Pricing US
            //pmd[0].ListPrice = pricing[0].ListPrice.ToString("0.00");

            //pmd[0].MAPPrice = pricing[0].MAPPrice.ToString("0.00");
            //pmd[0].WholesalePrice = pricing[0].WholesalePrice.ToString("0.00");

            //Add Pricing Canada use [1]
            if (pricing.Count != 0)
            {
                pmd[0].CAListPrice = pricing[1].ListPrice.ToString("0.00");
            }

            //remove special characters
            //CleanupPropertyValues(pmd);

            //replace "" with null
            DefaultBlankstoNull(pmd);
            DefaultDigitalBlankstoNull(digital);
            DefaultPricingBlankstoNull(pricing);

            using (var client = new HttpClient())
            {
                var responseBody = String.Empty;

                Product.PMDGroup group = new Product.PMDGroup();
                group.master = new List<Product.ProductMaster>();
                group.master.Add(pmd[0]);
                if (digital.Count != 0)
                {
                    group.assets = new List<Product.SalsifyDigitalAsset>();
                    group.assets.Add(digital[0]);
                }
                if (pricing.Count != 0)
                {
                    group.price = new List<Product.SalsifyPricing>();
                    group.price.Add(pricing[0]);
                }

                var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                jsonRequest = CleanUpRequest(jsonRequest);

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                //HTTP POST
                var postTask = client.PutAsync(client.BaseAddress.ToString(), content);
                postTask.Wait();

                var result = postTask.Result;
                var msg = result.RequestMessage;
                if (result.IsSuccessStatusCode)
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Update", SKUName);

                    ViewData["Message"] = SKUName + " has been updated in Salsify";
                    return View();
                }
                else
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Failed", "Update", SKUName);

                    ViewData["Message"] = "Error Updating Product " + SKUName;
                    return View();
                }

            }

        }

        [HttpPost]
        public ActionResult UpdateMulti(string FromDate, string ToDate)
        {
            int NbrCreated = 0;
            int NbrFailed = 0;

            var model = new Product();

            //get pmd data from portal table and put in ProductMaster class
            var models = new ProductContext();

            //get list of SKUs to process based on date range
            List<string> SKUs = models.GetPMD_SKUs(FromDate, ToDate, "GetUpdatedPMDSKUs_ByDateRange");

            foreach (string SKUName in SKUs)
            {

                //get pricing data from portal table and put in SalsifyPricing class
                List<Product.SalsifyPricing> pricing = models.GetPricingInfo(SKUName);

                //get pmd data from portal table and put in ProductMaster class
                List<Product.ProductMaster> pmd = models.GetPMDInfo(SKUName);

                if (pmd.Count != 0) //Go to next SKU if not found in DB
                {

                    //get digital asset data from portal table and put in SalsifyDigitalAsset class
                    List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

                    //harcode for test
                    //pmd[0].Country_of_Origin = "United States";
                    //pmd[0].GPC_Code = null;
                    //pmd[0].Finish = null;
                    //**** REMOVE TEST ONLY
                    //pmd[0].Cal_Green_Compliant = "CALGreen";
                    //*** TEST ONLY REMOVE THIS when digital assets logic in place
                    //digital[0].MainImage_ID = "0befd3e19af88801880efae987f4fcc0ee08ab5e"; //must be ID

                    pmd[0].Cal_Green_Compliant = GetSalsifyValue("Cal_Green_Compliant", pmd[0].Cal_Green_Compliant);
                    pmd[0].Country_of_Origin = GetSalsifyValue("Country_of_Origin", pmd[0].Country_of_Origin);
                    pmd[0].Diverter_Location = GetSalsifyValue("Diverter_Location", pmd[0].Diverter_Location);
                    pmd[0].Holder_Type = GetSalsifyValue("Holder_Type", pmd[0].Holder_Type);
                    pmd[0].Spout_Type = GetSalsifyValue("Spout_Type", pmd[0].Spout_Type);
                    pmd[0].THD_Category = GetSalsifyValue("THD_Category", pmd[0].THD_Category);
                    pmd[0].Type_of_Connection = GetSalsifyValue("Type_of_Connection", pmd[0].Type_of_Connection);
                    pmd[0].Valve_System_Type = GetSalsifyValue("Valve_System_Type", pmd[0].Valve_System_Type);
                    pmd[0].Product_Category = GetSalsifyValue("Product_Category", pmd[0].Product_Category);

                    //Add Pricing US
                    //pmd[0].ListPrice = pricing[0].ListPrice.ToString("0.00");

                    //pmd[0].MAPPrice = pricing[0].MAPPrice.ToString("0.00");
                    //pmd[0].WholesalePrice = pricing[0].WholesalePrice.ToString("0.00");

                    //Add Pricing Canada use pricing[1]
                    if (pricing.Count != 0)
                    {
                        pmd[0].CAListPrice = pricing[1].ListPrice.ToString("0.00");
                    }

                    //if (pmd.Count == 0)
                    //{
                    //    ViewData["Message"] = SKUName + " was not found in the Portal Product Master";
                    //    return View();
                    //}

                    //remove special characters
                    //CleanupPropertyValues(pmd);

                    //replace "" with null
                    DefaultBlankstoNull(pmd);
                    DefaultDigitalBlankstoNull(digital);
                    DefaultPricingBlankstoNull(pricing);


                    using (var client = new HttpClient())
                    {
                        var responseBody = String.Empty;

                        Product.PMDGroup group = new Product.PMDGroup();
                        group.master = new List<Product.ProductMaster>();
                        group.master.Add(pmd[0]);
                        if (digital.Count != 0)
                        {
                            group.assets = new List<Product.SalsifyDigitalAsset>();
                            group.assets.Add(digital[0]);
                        }
                        if (pricing.Count != 0)
                        {
                            group.price = new List<Product.SalsifyPricing>();
                            group.price.Add(pricing[0]);
                        }

                        var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                        jsonRequest = CleanUpRequest(jsonRequest);

                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                        //HTTP POST
                        var postTask = client.PutAsync(client.BaseAddress.ToString(), content);
                        postTask.Wait();

                        var result = postTask.Result;
                        var msg = result.RequestMessage;
                        if (result.IsSuccessStatusCode)
                        {
                            //record result in history
                            int recordAffected = models.InsertHistoryRecord("Succeeded", "Update", SKUName);

                            NbrCreated += 1;
                            //ViewData["Message"] = SKUName + " has been updated in Salsify";
                            //return View();
                        }
                        else
                        {
                            //record result in history
                            int recordAffected = models.InsertHistoryRecord("Failed", "Update", SKUName);

                            NbrFailed += 1;
                            //ViewData["Message"] = "Error Updating Product " + SKUName;
                            //return View();
                        }

                    }

                }
            }

            ViewData["Message"] = SKUs.Count + " SKUs have been process by Salsify, " + NbrCreated + " Updated, " + NbrFailed + " Failed";
            return View();

        }

        [HttpPost]
        public ActionResult Delete(string SKUName)
        {
            using (var client = new HttpClient())
            {

                var responseBody = String.Empty;

                var models = new ProductContext();

                client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                //HTTP POST
                var postTask = client.DeleteAsync(client.BaseAddress.ToString());
                postTask.Wait();

                var result = postTask.Result;
                var msg = result.RequestMessage;
                if (result.IsSuccessStatusCode)
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Delete", SKUName);

                    ViewData["Message"] = SKUName + " has been deleted in Salsify";
                    return View();
                }
                else
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Delete", SKUName);

                    ViewData["Message"] = SKUName + " not found in Salsify";
                    return View();
                }
            }

        }

        public string CleanUpRequest(string Json) //remove class tags, brackets etc...
        {
            Json = Json.Replace("{\"master\":[", "");
            Json = Json.Replace("}],\"assets\":[{", ",");
            Json = Json.Replace("]}", "");
            Json = Json.Replace("],\"assets\":null}","");// if no digital record
            Json = Json.Replace("}],\"price\":[{", ",");
            Json = Json.Replace("],\"price\":null}", "");// if no pricing record
            Json = Json.Replace("],\"assets\":null,\"price\":null}", "");// if no digital & pricing record
            Json = Json.Replace("}],\"assets\":null,\"price\":[{", ",");// if no digital & pricing record exist

            return Json;
        }
        public void CleanupPropertyValues(List<Product.ProductMaster> pmd) //remove special characters
        {
            //Add loop for index for multi skus later *************************
            PropertyInfo[] properties =
                typeof(Product.ProductMaster).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    string currentValue = (string)property.GetValue(pmd[0], null);
                    if (!string.IsNullOrEmpty(currentValue))
                    {
                        string newValue = _cleanupRegex.Replace(currentValue, " ");

                        if (newValue != currentValue)
                        {
                            property.SetValue(pmd[0], newValue);
                        }
                    }
                }
            }
        }

         public void DefaultBlankstoNull(List<Product.ProductMaster> pmd) //if field is blank make null so not uploaded to salsify
        {
            //Add loop for index for multi skus later *************************
            PropertyInfo[] properties =
                typeof(Product.ProductMaster).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    string currentValue = (string)property.GetValue(pmd[0], null);
                    if (currentValue == "")
                    {
                            property.SetValue(pmd[0], null);
                    }
                    else if (currentValue == " ")
                    {
                        property.SetValue(pmd[0], null);
                    }
                }
                if (property.PropertyType == typeof(Decimal?))
                {
                    Decimal currentValue = (Decimal)property.GetValue(pmd[0], null);
                    if (currentValue.ToString() == "")
                    {
                        property.SetValue(pmd[0], null);
                    }
                    else if (currentValue == 0)
                    {
                        property.SetValue(pmd[0], null);
                    }
                    //else if (currentValue.ToString() == "0.0")
                    //{
                    //    property.SetValue(pmd[0], null);
                    //}
                }
                if (property.PropertyType == typeof(int?))
                {
                    int currentValue = (int)property.GetValue(pmd[0], null);
                    if (currentValue.ToString() == "")
                    {
                        property.SetValue(pmd[0], null);
                    }
                    else if (currentValue == 0)
                    {
                        property.SetValue(pmd[0], null);
                    }
                    //else if (currentValue.ToString() == "0.0")
                    //{
                    //    property.SetValue(pmd[0], null);
                    //}
                }
            }
        }

        public void DefaultPricingBlankstoNull(List<Product.SalsifyPricing> pricing) //if field is blank make null so not uploaded to salsify
        {
            //Add loop for index for multi skus later *************************
            PropertyInfo[] properties =
                typeof(Product.SalsifyPricing).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    for (int i = 0; i < pricing.Count; i++)
                    {
                        string currentValue = (string)property.GetValue(pricing[i], null);
                        if (currentValue == "")
                        {
                            property.SetValue(pricing[i], null);
                        }
                    }
                }
                if (property.PropertyType == typeof(Decimal))
                {
                    for (int i = 0; i < pricing.Count; i++)
                    {
                        Decimal currentValue = (Decimal)property.GetValue(pricing[i], null);
                        if (currentValue.ToString() == "")
                        {
                            property.SetValue(pricing[i], null);
                        }
                    }
                    //else if (currentValue == 0)
                    //{
                    //    property.SetValue(pmd[0], null);
                    //}
                    //else if (currentValue.ToString() == "0.0")
                    //{
                    //    property.SetValue(pmd[0], null);
                    //}
                }
            }
        }

        public void DefaultDigitalBlankstoNull(List<Product.SalsifyDigitalAsset> digital) //if field is blank make null so not uploaded to salsify
        {
            //Add loop for index for multi skus later *************************
            PropertyInfo[] properties =
                typeof(Product.SalsifyDigitalAsset).GetProperties(BindingFlags.Instance | BindingFlags.Public);

            if (digital.Count != 0)
            {
                foreach (PropertyInfo property in properties)
                {
                    if (property.PropertyType == typeof(string))
                    {
                        string currentValue = (string)property.GetValue(digital[0], null);
                        if (currentValue == "")
                        {
                           property.SetValue(digital[0], null);
                        }
                    
                    }
                }
            }
        }

        public string GetSalsifyValue(string ClassProperty, string PortalValue)
        {
            if (PortalValue != null)
            {
                var models = new ProductContext();
                string Result = models.GetSalsifyValue(ClassProperty, PortalValue);

                return Result;
            }
            else
            {
                return null;//if passed null return same value
            }
        }

        private static Regex _cleanupRegex = new Regex("[^a-zA-Z0-9-_/.]");//remove special chars except -_/.
    }
}
//[a-zA-Z0-9_] // only alphanumeric