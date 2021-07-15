using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
//using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using SalsifyApp.Models;
using static SalsifyApp.Models.Error;

namespace SalsifyApp.Controllers
{
    public class SalsifyController : Controller
    {
        private readonly IOptions<Settings> appSettings;

        public SalsifyController(IOptions<Settings> app)
        {
            appSettings = app;
        }

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
            List<Exceptions> exceptions = new List<Exceptions>();

            Unprocessable_Entity myerrors = new Unprocessable_Entity();

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

            //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
            //get digital asset data from portal table and put in SalsifyDigitalAsset class
            //List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

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
            pmd[0].Style = GetSalsifyValue("Style", pmd[0].Style);

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
            //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
            //DefaultDigitalBlankstoNull(digital);
            DefaultPricingBlankstoNull(pricing);

            using (var client = new HttpClient())
            {
                var responseBody = String.Empty;

                Product.PMDGroup group = new Product.PMDGroup();
                group.master = new List<Product.ProductMaster>();
                group.master.Add(pmd[0]);
                //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                //if (digital.Count != 0)
                //{ 
                //    group.assets = new List<Product.SalsifyDigitalAsset>();
                //    group.assets.Add(digital[0]);
                //}
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
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Create", SKUName, result.StatusCode.ToString(), "");

                    ViewData["Message"] = SKUName + " has been created in Salsify";
                    return View();
                }
                else
                {

                    string resultcontent = result.Content.ReadAsStringAsync().Result;
                    myerrors = JsonConvert.DeserializeObject<Unprocessable_Entity>(resultcontent);

                    //convert array of errors to delimited string
                    string myerrorsJoin = string.Join(", ", myerrors.errors);

                    //add error details to exception class to be used in excel email attachment
                    exceptions.Add(new Exceptions() { SKU = SKUName, Run_Result = "Failed", Task = "Create", StatusCode = result.StatusCode.ToString(), Error_Details = myerrorsJoin, Date = DateTime.Now.ToString("MM/dd/yyyy") });

                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Failed", "Create", SKUName, result.StatusCode.ToString(), myerrorsJoin);

                    string results_Msg = "Error Creating Product " + SKUName;

                    //call email method
                    CreateSendEmail(exceptions, results_Msg, "(Create - Single SKU)");

                    ViewData["Message"] = results_Msg + " (See exceptions email for error details)";
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

            List<Exceptions> exceptions = new List<Exceptions>();

            Unprocessable_Entity myerrors = new Unprocessable_Entity();

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
                    //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                    //get digital asset data from portal table and put in SalsifyDigitalAsset class
                    //List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

                    //harcode for test
                    //pmd[0].Country_of_Origin = "United States";
                    //pmd[0].GPC_Code = null;
                    //pmd[0].Finish = "Test";
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
                    pmd[0].Style = GetSalsifyValue("Style", pmd[0].Style);

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
                    //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                    //DefaultDigitalBlankstoNull(digital);
                    DefaultPricingBlankstoNull(pricing);


                    using (var client = new HttpClient())
                    {
                        var responseBody = String.Empty;

                        Product.PMDGroup group = new Product.PMDGroup();
                        group.master = new List<Product.ProductMaster>();
                        group.master.Add(pmd[0]);
                        //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                        //if (digital.Count != 0)
                        //{
                        //    group.assets = new List<Product.SalsifyDigitalAsset>();
                        //    group.assets.Add(digital[0]);
                        //}
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
                            int recordAffected = models.InsertHistoryRecord("Succeeded", "Create", SKUName, result.StatusCode.ToString(), "");

                            NbrCreated += 1;
                            //ViewData["Message"] = SKUName + " has been created in Salsify";
                            //return View();
                        }
                        else
                        {

                            NbrFailed += 1;
                            //ViewData["Message"] = "Error Creating Product " + SKUName;
                            //return View();
                            
                            string resultcontent = result.Content.ReadAsStringAsync().Result;
                            myerrors = JsonConvert.DeserializeObject<Unprocessable_Entity>(resultcontent);

                            //convert array of errors to delimited string
                            string myerrorsJoin = string.Join(", ", myerrors.errors);

                            //add error details to exception class to be used in excel email attachment
                            exceptions.Add(new Exceptions() { SKU = SKUName, Run_Result = "Failed", Task = "Create", StatusCode = result.StatusCode.ToString(), Error_Details = myerrorsJoin, Date = DateTime.Now.ToString("MM/dd/yyyy") });

                            //record result in salsify history table
                            int recordAffected = models.InsertHistoryRecord("Failed", "Create", SKUName, result.StatusCode.ToString(), myerrorsJoin);

                        }

                    }

                }

            }

            string results_Msg = SKUs.Count + " SKUs have been processed by Salsify, " + NbrCreated + " Created, " + NbrFailed + " Failed";


            if(NbrFailed > 0)
            {
                //call email method
                CreateSendEmail(exceptions, results_Msg, "(Create Multi - Date Range)");

                ViewData["Message"] = results_Msg + " (See exceptions email for error details)";
            }
            else
            {
                ViewData["Message"] = results_Msg;
            }

            return View();
        }

        [HttpPost]
        public ActionResult Update(string SKUName)
        {
            List<Exceptions> exceptions = new List<Exceptions>();

            Unprocessable_Entity myerrors = new Unprocessable_Entity();

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

            //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
            //get digital asset data from portal table and put in SalsifyDigitalAsset class
            //List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

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
            pmd[0].Style = GetSalsifyValue("Style", pmd[0].Style);

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
            //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
            //DefaultDigitalBlankstoNull(digital);
            DefaultPricingBlankstoNull(pricing);

            using (var client = new HttpClient())
            {
                var responseBody = String.Empty;

                Product.PMDGroup group = new Product.PMDGroup();
                group.master = new List<Product.ProductMaster>();
                group.master.Add(pmd[0]);
                //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                //if (digital.Count != 0)
                //{
                //    group.assets = new List<Product.SalsifyDigitalAsset>();
                //    group.assets.Add(digital[0]);
                //}
                if (pricing.Count != 0)
                {
                    group.price = new List<Product.SalsifyPricing>();
                    group.price.Add(pricing[0]);
                }

                var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                jsonRequest = CleanUpRequest(jsonRequest);

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName.Replace("/","%2F") + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                //HTTP POST
                var postTask = client.PutAsync(client.BaseAddress.ToString(), content);
                postTask.Wait();

                var result = postTask.Result;
                var msg = result.RequestMessage;
                if (result.IsSuccessStatusCode)
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Update", SKUName, result.StatusCode.ToString(), "");

                    ViewData["Message"] = SKUName + " has been updated in Salsify";
                    return View();
                }
                else
                {
                    string resultcontent = result.Content.ReadAsStringAsync().Result;
                    myerrors = JsonConvert.DeserializeObject<Unprocessable_Entity>(resultcontent);

                    //convert array of errors to delimited string
                    string myerrorsJoin = string.Join(", ", myerrors.errors);

                    //add error details to exception class to be used in excel email attachment
                    exceptions.Add(new Exceptions() { SKU = SKUName, Run_Result = "Failed", Task = "Update", StatusCode = result.StatusCode.ToString(), Error_Details = myerrorsJoin, Date = DateTime.Now.ToString("MM/dd/yyyy") });

                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Failed", "Update", SKUName, result.StatusCode.ToString(), myerrorsJoin);

                    string results_Msg = "Error Updating Product " + SKUName;

                    //call email method
                    CreateSendEmail(exceptions, results_Msg, "(Update - Single SKU)");

                    ViewData["Message"] = results_Msg + " (See exceptions email for error details)";
                    return View();
                }

            }

        }

        [HttpPost]
        public ActionResult UpdateMulti(string FromDate, string ToDate)
        {
            int NbrCreated = 0;
            int NbrFailed = 0;

            List<Exceptions> exceptions = new List<Exceptions>();

            Unprocessable_Entity myerrors = new Unprocessable_Entity();

            var model = new Product();

            //get pmd data from portal table and put in ProductMaster class
            var models = new ProductContext();

            //get list of SKUs to process based on date range
            List<string> SKUs = models.GetPMD_SKUs(FromDate, ToDate, "GetUpdatedPMDSKUs_ByDateRange");

            //var SKUs = new List<string>()
            //        {
            //            "363TP-BBZ",
            //            "363TP-MB",
            //            "363TP-STN",
            //            "363TR",
            //            "363TR-BBZ",
            //            "363TR-MB",
            //            "363TR-STN",
            //            "363TS-22",
            //            "363TS-22-STN",
            //            "3630STNTRMTC",
            //            "3630TRMTC",
            //            "4-10A",
            //            "4-10B",
            //            "4-10C",
            //            "4-135F",
            //            "4-135F-1.5",
            //            "4-135F-2.0",
            //            "4-137",
            //            "4-141",
            //            "4-141-STN",
            //            "4-141-STN-1.5",
            //            "4-141-STN-2.0",
            //            "4-141-1.5",
            //            "4-141-2.0",
            //            "4-143",
            //            "4-143-STN",
            //            "4-143-STN-1.5",
            //            "4-143-STN-1.75",
            //            "4-143-STN-2.0",
            //            "4-143-1.5",
            //            "4-143-1.75",
            //            "4-143-2.0",
            //            "4-145",
            //            "4-145-1.5",
            //            "4-145-2.0",
            //            "4-150",
            //            "4-150-A",
            //            "4-150-A-1.5",
            //            "4-150-1.5",
            //            "4-150-15",
            //            "4-150-15-1.5",
            //            "4-150-15-2.0",
            //            "4-150-2.0",
            //            "4-151",
            //            "4-151-A",
            //            "4-151-A-2.0",
            //            "4-151-B-2.0",
            //            "4-151-IPS",
            //            "4-151-2.0",
            //            "4-161",
            //            "4-161-STN",
            //            "4-163",
            //            "4-163-STN",
            //            "4-163-STN-1.5",
            //            "4-163-STN-2.0",
            //            "4-163-1.5",
            //            "4-163-2.0",
            //            "4-166",
            //            "4-166-STN",
            //            "4-166-304",
            //            "4-174",
            //            "4-174-STN",
            //            "4-174-2.0",
            //            "4-221",
            //            "4-221-STN",
            //            "4-221-STN-1.5",
            //            "4-221-1.5",
            //            "4-221-2.0",
            //            "4-221M",
            //            "4-221M-1.5",
            //            "4-226F",
            //            "4-226F-STN",
            //            "4-226F-STN-2.0",
            //            "4-226F-1.5",
            //            "4-226F-1.75",
            //            "4-226F-2.0",
            //            "4-226FVP",
            //            "4-226FVP-1.5",
            //            "4-226M",
            //            "4-226M-2.0",
            //            "4-231",
            //            "4-231-STN",
            //            "4-231-STN-1.5",
            //            "4-231-STN-2.0",
            //            "4-231-1.5",
            //            "4-231-2.0",
            //            "4-231M",
            //            "4-231M-1.5",
            //            "4-231M-2.0",
            //            "4-236",
            //            "4-236-STN",
            //            "4-236-STN-1.5",
            //            "4-236-STN-2.0",
            //            "4-236-1.5",
            //            "4-236-2.0",
            //            "4-241",
            //            "4-241-STN",
            //            "4-241-STN-2.0",
            //            "4-241-1.5",
            //            "4-241-2.0",
            //            "4-243",
            //            "4-270F",
            //            "4-270F-1.5",
            //            "4-270F-2.0",
            //            "4-282F",
            //            "4-282F-1.5",
            //            "4-282F-2.0",
            //            "4-282FVP-1.5",
            //            "4-282M",
            //            "4-282M-1.5",
            //            "4-282M-2.0",
            //            "4-285F",
            //            "4-285F-1.5",
            //            "4-285M",
            //            "4-285M-1.5",
            //            "4-295",
            //            "4-295-A",
            //            "4-295-A-IPS",
            //            "4-295-A-1.5",
            //            "4-295-A-2.0",
            //            "4-295-B",
            //            "4-295-B-1.5",
            //            "4-295-B-2.0",
            //            "4-295-IPS",
            //            "4-295-IPS-1.5",
            //            "4-295-1.5",
            //            "4-295-15",
            //            "4-295-15-1.5",
            //            "4-295-15-2.0",
            //            "4-295-2.0",
            //            "4-295-282",
            //            "4-385",
            //            "4-385-A-2.0",
            //            "4-385-1.5",
            //            "4-385-2.0",
            //            "4-412",
            //            "4-412-STN",
            //            "4-420",
            //            "4-420-BODY",
            //            "4-420-TRM",
            //            "4-420H",
            //            "4-425",
            //            "4-427",
            //            "4-427-TRM",
            //            "4-427R",
            //            "4-427R-TRM",
            //            "4-428",
            //            "4-428-R",
            //            "4-428-R-TRM",
            //            "4-428-TRM",
            //            "4-440",
            //            "4-445",
            //            "4-451",
            //            "4-458S",
            //            "4-458S-BODY",
            //            "4-473-BODY",
            //            "4-473-LAM",
            //            "4-473-STN-LAM",
            //            "4-500",
            //            "4-500-BODY-X-CHKS",
            //            "4-500-BX",
            //            "4-500-TRM",
            //            "4-500-VP-X",
            //            "4-500-X",
            //            "4-500-X-BODY",
            //            "4-500-X-CHKS",
            //            "4-500VT",
            //            "4-500VT-BODY",
            //            "4-500VT-TRM",
            //            "4-500VT-X",
            //            "4-500VT-X-BODY",
            //            "4-500VT-X-CHKS",
            //            "4-500VT-X-CHKS-BODY",
            //            "4-500VT-X-3/4",
            //            "4-5000VT",
            //            "4-5000VT-STN",
            //            "4-5000VT-STN-TRM",
            //            "4-5000VT-TRM",
            //            "4-5000VT-X",
            //            "4-5000VT-X-CHKS",
            //            "9600-X-PLR-OP",
            //            "9600-X-PLR-STN",
            //            "9600-X-PLR-VP",
            //            "9600CHKSPLRSTN",
            //            "9600L7",
            //            "9600L7TRM",
            //            "9600L7TRMSTN",
            //            "9600PLROPTRMTC",
            //            "9600PLRTRMTC",
            //            "9600PTRMTC",
            //            "9600REVXL7B",
            //            "9600XL7",
            //            "9600XL7B",
            //            "9601-CHKS-P",
            //            "9601-CHKS-P-2.0",
            //            "9601-CHKS-PLR",
            //            "9601-CHKS-PLR-B",
            //            "9601-CHKS-PLR-B-1.5",
            //            "9601-CHKS-PLR-VP",
            //            "9601-CHKS-PLR-VP-1.5",
            //            "9601-CHKS-PLR-VP-2.0",
            //            "9601-CHKS-PLR-1.5",
            //            "9601-CHKS-PLR-2.0",
            //            "9601-P",
            //            "9601-P-B-TRM",
            //            "9601-P-B-1.5-TRM",
            //            "9601-P-TRM",
            //            "9601-P-VP-1.5-TRM",
            //            "9601-P-1.5",
            //            "9601-P-1.5-TRM",
            //            "9601-P-2.0",
            //            "9601-P-2.0-TRM",
            //            "9601-PLR",
            //            "9601-PLR-B-TRM",
            //            "9601-PLR-B-VP",
            //            "9601-PLR-B-VP-1.5",
            //            "9601-PLR-B-1.5-TRM",
            //            "9601-PLR-B-2.0-TRM",
            //            "9601-PLR-STN",
            //            "9601-PLR-TRM",
            //            "9601-PLR-TRM-STN",
            //            "9601-PLR-VP",
            //            "9601-PLR-VP-1.5",
            //            "9601-PLR-1.5",
            //            "9601-PLR-1.5-STN",
            //            "9601-PLR-1.5-TRM",
            //            "9601-PLR-1.5-TRM-STK",
            //            "9601-PLR-1.5-TRM-STN",
            //            "9601-PLR-2.0",
            //            "9601-PLR-2.0-STN",
            //            "9601-PLR-2.0-TRM",
            //            "9601-PLR-2.0-TRM-STN",
            //            "9601-REV-X-P",
            //            "9601-REV-X-P-1.5",
            //            "9601-REV-X-P-2.0",
            //            "9601-REV-X-PLR",
            //            "9601-REV-X-PLR-B-1.5",
            //            "9601-REV-X-PLR-1.5",
            //            "9601-X-LP",
            //            "9601-X-P",
            //            "9601-X-P-B",
            //            "9601-X-P-B-1.5",
            //            "9601-X-P-VP-1.5",
            //            "9601-X-P-1.5",
            //            "9601-X-P-2.0",
            //            "9601-X-PLR",
            //            "9601-X-PLR-B",
            //            "9601-X-PLR-B-1.5",
            //            "9601-X-PLR-B-2.0",
            //            "9601-X-PLR-OP",
            //            "9601-X-PLR-STN",
            //            "9601-X-PLR-VP",
            //            "9601-X-PLR-VP-1.5",
            //            "9601-X-PLR-1.5",
            //            "9601-X-PLR-1.5-STN",
            //            "9601-X-PLR-2.0",
            //            "9601-X-PLR-2.0-STN",
            //            "9601CHKSPBL1",
            //            "9601CHKSPLRBL1VP",
            //            "9601CHKSPLRL1",
            //            "9601CHKSPL1",
            //            "9601L1L7TRM",
            //            "9601L7",
            //            "9601L7TRM",
            //            "9601L715TRM",
            //            "9601L720",
            //            "9601L720TRM",
            //            "9601PLRBL1",
            //            "9601PLRBL1TRM",
            //            "9601PLRL1",
            //            "9601PLRL1TRM",
            //            "9601PLRL1TRMSTN",
            //            "9601PLRL1VPTRM",
            //            "9601PLROPL1TRM",
            //            "9601PLRTRMTC",
            //            "9601PLR15TRMTC",
            //            "9601PL1",
            //            "9601PL1TRM",
            //            "9601PTRMTC",
            //            "9601P15TRMTC",
            //            "9601REVXPBL1",
            //            "9601REVXPLRL1",
            //            "9601XL1L7",
            //            "9601XL7",
            //            "9601XL715",
            //            "9601XL720",
            //            "9601XPBL1",
            //            "9601XPLRBL1",
            //            "9601XPLRL1",
            //            "9601XPLROPL1",
            //            "9601XPL1",
            //            "9602-CHKS-PLR",
            //            "9602-CHKS-PLR-VP",
            //            "9602-CHKS-PLR-VP-1.5",
            //            "9602-CHKS-PLR-1.5",
            //            "9602-CHKS-PLR-2.0",
            //            "9602-LP-SS-TRM",
            //            "9602-P",
            //            "9602-P-SS",
            //            "9602-P-SS-TRM",
            //            "9602-P-SS-1.5-TRM",
            //            "9602-P-SS-2.0",
            //            "9602-P-SS-2.0-TRM",
            //            "9602-P-TRM",
            //            "9602-P-1.5",
            //            "9602-P-1.5-TRM",
            //            "9602-P-2.0",
            //            "9602-P-2.0-TRM",
            //            "9602-PLR",
            //            "9602-PLR-B-SS-1.5-TRM",
            //            "9602-PLR-B-TRM",
            //            "9602-PLR-B-1.5-TRM",
            //            "9602-PLR-B-2.0-TRM",
            //            "9602-PLR-OP",
            //            "9602-PLR-SS",
            //            "9602-PLR-SS-STN",
            //            "9602-PLR-SS-TRM",
            //            "9602-PLR-SS-TRM-STN",
            //            "9602-PLR-SS-1.5",
            //            "9602-PLR-SS-1.5-TRM",
            //            "9602-PLR-SS-2.0",
            //            "9602-PLR-SS-2.0-STN",
            //            "9602-PLR-SS-2.0-TRM",
            //            "9602-PLR-SS-2.0-TRM-STN",
            //            "9602-PLR-STN",
            //            "9602-PLR-TRM",
            //            "9602-PLR-TRM-STN",
            //            "9602-PLR-1.5",
            //            "9602-PLR-1.5-STN",
            //            "9602-PLR-1.5-TRM",
            //            "9602-PLR-1.5-TRM-STN",
            //            "9602-PLR-2.0",
            //            "9602-PLR-2.0-STN",
            //            "9602-PLR-2.0-TRM",
            //            "9602-PLR-2.0-TRM-STN",
            //            "9602-REV-X-P",
            //            "9602-REV-X-PLR",
            //            "9602-REV-X-PLR-2.0",
            //            "9602-X-LP",
            //            "9602-X-P",
            //            "9602-X-P-B-1.5",
            //            "9602-X-P-SS",
            //            "9602-X-P-SS-2.0",
            //            "9602-X-P-1.5",
            //            "9602-X-P-2.0",
            //            "9602-X-PLR",
            //            "4-5000VT-X-3/4",
            //            "4-521",
            //            "4-521-CX",
            //            "4-521-W",
            //            "4-521-X",
            //            "40",
            //            "402SH1",
            //            "402SH1-STN",
            //            "402SH1-STN-2.0",
            //            "402SH1-1.5",
            //            "402SH1-2.0",
            //            "402SH2-STN",
            //            "402SH2-STN-1.5",
            //            "402W",
            //            "402W-BLK",
            //            "402W-BLK-1.5",
            //            "402W-BLK-2.0",
            //            "402W-STN",
            //            "402W-STN-1.5",
            //            "402W-STN-2.0",
            //            "402W-1.5",
            //            "402W-2.0",
            //            "41-2DIV-PNL-TRM",
            //            "41-2DIV-STN-TRM",
            //            "41-2DIV-TRM",
            //            "41-3DIV-PNL-TRM",
            //            "41-3DIV-STN-TRM",
            //            "41-3DIV-TRM",
            //            "4100-PNL-TRM",
            //            "4100-REB-TRM",
            //            "4100-STN-REB-TRM",
            //            "4100-STN-TRM",
            //            "4100-TRM",
            //            "4100PNLTRMTC",
            //            "4100STNTRMTC",
            //            "4100TRMTC",
            //            "4101-PNL-TRM",
            //            "4101-PNL-2.0-TRM",
            //            "4101-REB-TRM",
            //            "4101-STN-REB-TRM",
            //            "4101-STN-TRM",
            //            "4101-STN-2.0-TRM",
            //            "4101-TRM",
            //            "4101-1.5-TRM",
            //            "4101-2.0-TRM",
            //            "4101PNLTRMTC",
            //            "4101STNTRMTC",
            //            "4101TRMTC",
            //            "4103-PNL-TRM",
            //            "4103-PNL-2.0-TRM",
            //            "4103-STN-TRM",
            //            "4103-STN-2.0-TRM",
            //            "4103-TRM",
            //            "4103-2.0-TRM",
            //            "4103PNLTRMTC",
            //            "4103STNTRMTC",
            //            "4103TRMTC",
            //            "4105-PNL-TRM",
            //            "4105-PNL-2.0-TRM",
            //            "4105-STN-TRM",
            //            "4105-STN-2.0-TRM",
            //            "4105-TRM",
            //            "4105-2.0-TRM",
            //            "4105PNLTRMTC",
            //            "4105STNTRMTC",
            //            "4105TRMTC",
            //            "4106-PNL-TRM",
            //            "4106-PNL-2.0-TRM",
            //            "4106-STN-TRM",
            //            "4106-STN-2.0-TRM",
            //            "4106-TRM",
            //            "4106-2.0-TRM",
            //            "4106PNLTRMTC",
            //            "4106STNTRMTC",
            //            "4106TRMTC",
            //            "412HS",
            //            "412HS-LHS",
            //            "412HS-LHS-STN",
            //            "412HS-PNL",
            //            "412HS-PNL-2.0",
            //            "412HS-STN",
            //            "412HS-STN-1.5",
            //            "412HS-STN-2.0",
            //            "412HS-1.5",
            //            "412SH",
            //            "412SH-BBZ",
            //            "412SH-BBZ-2.0",
            //            "412SH-PNL",
            //            "412SH-PNL-2.0",
            //            "412SH-STN",
            //            "412SH-STN-1.5",
            //            "412SH-STN-1.75",
            //            "412SH-STN-2.0",
            //            "412SH-1.5",
            //            "412SH-1.75",
            //            "412SH-2.0",
            //            "412TS",
            //            "412TS-PNL",
            //            "412TS-STN",
            //            "412W",
            //            "412W-BBZ",
            //            "412W-BBZ-2.0",
            //            "412W-PNL",
            //            "412W-PNL-2.0",
            //            "412W-STN",
            //            "412W-STN-1.5",
            //            "412W-STN-2.0",
            //            "412W-1.5",
            //            "412W-2.0",
            //            "413RH",
            //            "413RH-PNL",
            //            "413RH-STN",
            //            "413TB-18",
            //            "413TB-18-PNL",
            //            "413TB-18-STN",
            //            "413TB-24",
            //            "413TB-24-PNL",
            //            "413TB-24-STN",
            //            "413TP",
            //            "413TP-PNL",
            //            "413TP-STN",
            //            "413TRL",
            //            "413TRL-PNL",
            //            "413TRL-STN",
            //            "413TRR",
            //            "413TRR-PNL",
            //            "413TRR-STN",
            //            "4141STN12",
            //            "414112",
            //            "42-2DIV-STN-TRM",
            //            "42-2DIV-TRM",
            //            "42-3DIV-STN-TRM",
            //            "42-3DIV-TRM",
            //            "4200-STN-TRM",
            //            "4200-TRM",
            //            "4200STNTRMTC",
            //            "4200TRMTC",
            //            "4201-STN-TRM",
            //            "4201-STN-1.5-TRM",
            //            "4201-STN-2.0-TRM",
            //            "4201-TRM",
            //            "4201-1.5-TRM",
            //            "4201-2.0-TRM",
            //            "4201STNTRMTC",
            //            "4201TRMTC",
            //            "4202-STN-TRM",
            //            "4202-STN-1.5-TRM",
            //            "4202-STN-2.0-TRM",
            //            "4202-TRM",
            //            "4202-1.5-TRM",
            //            "4202-2.0-TRM",
            //            "4202STNTRMTC",
            //            "4202TRMTC",
            //            "4203-STN-TRM",
            //            "4203-STN-1.5-TRM",
            //            "4203-STN-2.0-TRM",
            //            "4203-TRM",
            //            "4203-1.5-TRM",
            //            "4203-2.0-TRM",
            //            "4203STNTRMTC",
            //            "4203TRMTC",
            //            "4204-STN-TRM",
            //            "4204-TRM",
            //            "4205-STN-TRM",
            //            "4205-STN-1.5-TRM",
            //            "4205-STN-2.0-TRM",
            //            "4205-TRM",
            //            "4205-1.5-TRM",
            //            "4205STNTRMTC",
            //            "4205TRMTC",
            //            "4206-STN-TRM",
            //            "4206-STN-1.5-TRM",
            //            "4206-STN-2.0-TRM",
            //            "4206-TRM",
            //            "4206-1.5-TRM",
            //            "4206-2.0-TRM",
            //            "4206STNTRMTC",
            //            "4206TRMTC",
            //            "422HS",
            //            "422HS-STN",
            //            "422HS-STN-1.5",
            //            "422HS-STN-2.0",
            //            "422HS-2.0",
            //            "422SA",
            //            "422SA-STN",
            //            "422SH",
            //            "422SH-STN",
            //            "422SH-STN-1.5",
            //            "422SH-STN-2.0",
            //            "422SH-1.5",
            //            "422SH-2.0",
            //            "422TS",
            //            "422TS-STN",
            //            "422TSD",
            //            "422TSD-STN",
            //            "422W",
            //            "422W-STN",
            //            "422W-STN-1.5",
            //            "422W-STN-2.0",
            //            "422W-1.5",
            //            "422W-2.0",
            //            "423RH",
            //            "423RH-STN",
            //            "423TB-18",
            //            "423TB-18-STN",
            //            "423TB-24",
            //            "423TB-24-STN",
            //            "423TP",
            //            "423TP-STN",
            //            "423TR",
            //            "423TR-STN",
            //            "4270FVP15",
            //            "43-2DIV-STN-TRM",
            //            "43-2DIV-TRM",
            //            "430SS",
            //            "4300-REB-TRM",
            //            "4300-STN-REB-TRM",
            //            "4300-STN-TRM",
            //            "4300-TRM",
            //            "4300STNTRMTC",
            //            "4300TRMTC",
            //            "4301-STN-TRM",
            //            "4301-STN-1.5-TRM",
            //            "4301-STN-2.0-TRM",
            //            "4301-TRM",
            //            "4301-1.5-TRM",
            //            "4301-2.0-TRM",
            //            "4301STNTRMTC",
            //            "4301STN15TRMTC",
            //            "4301TRMTC",
            //            "430115TRMTC",
            //            "4303-STN-TRM",
            //            "4303-STN-1.5-TRM",
            //            "4303-STN-2.0-TRM",
            //            "4303-TRM",
            //            "4303-1.5-TRM",
            //            "4303-2.0-TRM",
            //            "4303STNTRMTC",
            //            "4303STN15TRMTC",
            //            "4303TRMTC",
            //            "430315TRMTC",
            //            "4305-STN-TRM",
            //            "4305-STN-1.5-TRM",
            //            "4305-STN-2.0-TRM",
            //            "4305-TRM",
            //            "4305-1.5-TRM",
            //            "4305-2.0-TRM",
            //            "4305STNTRMTC",
            //            "4305STN15TRMTC",
            //            "4305TRMTC",
            //            "430515TRMTC",
            //            "4306-STN-TRM",
            //            "4306-STN-1.5-TRM",
            //            "4306-STN-2.0-TRM",
            //            "4306-TRM",
            //            "4306-1.5-TRM",
            //            "4306-2.0-TRM",
            //            "4306STNTRMTC",
            //            "4306STN15TRMTC",
            //            "4306TRMTC",
            //            "430615TRMTC",
            //            "432HS",
            //            "432HS-STN",
            //            "432HS-STN-2.0",
            //            "432HS-1.5",
            //            "432HS-2.0",
            //            "432HSB-1.5",
            //            "432SH",
            //            "432SH-STN",
            //            "432SH-STN-1.5",
            //            "432SH-STN-2.0",
            //            "432SH-1.5",
            //            "432SH-2.0",
            //            "432TS",
            //            "432TS-STN",
            //            "432W",
            //            "432W-STN",
            //            "432W-STN-2.0",
            //            "432W-1.5",
            //            "432W-1.75",
            //            "432W-2.0",
            //            "433RH",
            //            "433RH-STN",
            //            "433TB-18",
            //            "433TB-18-STN",
            //            "433TB-24",
            //            "433TB-24-STN",
            //            "433TP",
            //            "433TP-STN",
            //            "433TR",
            //            "433TR-STN",
            //            "44-2DIV-LAM-TRM",
            //            "44-2DIV-STN-TRM",
            //            "44-2DIV-TRM",
            //            "4400-LAM-TRM",
            //            "4400-REB-TRM",
            //            "4400-STN-REB-TRM",
            //            "4400-STN-TRM",
            //            "4400-TRM",
            //            "4400STNTRMTC",
            //            "4400TRMTC",
            //            "4401-LAM-TRM",
            //            "4401-STN-TRM",
            //            "4401-STN-1.5-TRM",
            //            "4401-STN-2.0-TRM",
            //            "4401-TRM",
            //            "4401-1.5-TRM",
            //            "4401-2.0-TRM",
            //            "4401L1L7TRM",
            //            "4401L7TRM",
            //            "4401STNL1L7TRM",
            //            "4401STNL7TRM",
            //            "4401STNTRMTC",
            //            "4401STN15TRMTC",
            //            "4401TRMTC",
            //            "440115TRMTC",
            //            "4403-STN-TRM",
            //            "4403-STN-1.5-TRM",
            //            "4403-STN-2.0-TRM",
            //            "4403-TRM",
            //            "4403-1.5-TRM",
            //            "4403-2.0-TRM",
            //            "4403STNTRMTC",
            //            "4403STN15TRMTC",
            //            "4403TRMTC",
            //            "440315TRMTC",
            //            "4405-LAM-TRM",
            //            "4405-STN-TRM",
            //            "4405-STN-1.5-TRM",
            //            "4405-STN-2.0-TRM",
            //            "4405-TRM",
            //            "4405-1.5-TRM",
            //            "4405-2.0-TRM",
            //            "4405L7TRM",
            //            "4405STNL7TRM",
            //            "4405STNTRMTC",
            //            "4405STN15TRMTC",
            //            "4405TRMTC",
            //            "440515TRMTC",
            //            "4406-LAM-TRM",
            //            "4406-STN-TRM",
            //            "4406-STN-1.5-TRM",
            //            "4406-STN-2.0-TRM",
            //            "4406-TRM",
            //            "4406-1.5-TRM",
            //            "4406-2.0-TRM",
            //            "4406STNL7TRM",
            //            "4406STNTRMTC",
            //            "4406STN15TRMTC",
            //            "4406TRMTC",
            //            "440615TRMTC",
            //            "442DIVL7TRM",
            //            "442DIVSTNL7TRM",
            //            "442HS",
            //            "442HS-LHS-STN",
            //            "442HS-STN",
            //            "442HS-STN-1.5",
            //            "442HS-STN-2.0",
            //            "442HS-1.5",
            //            "442HS-2.0",
            //            "442HSB",
            //            "442HSB-STN",
            //            "442HSB-1.5",
            //            "442SH",
            //            "442SH-STN",
            //            "442SH-STN-2.0",
            //            "442SH-1.5",
            //            "442SH-2.0",
            //            "442W",
            //            "442W-STN",
            //            "442W-STN-1.5",
            //            "442W-STN-2.0",
            //            "442W-1.5",
            //            "442W-2.0",
            //            "443RH",
            //            "443RH-STN",
            //            "443TB-18",
            //            "443TB-18-STN",
            //            "443TB-24",
            //            "443TB-24-STN",
            //            "443TP",
            //            "443TP-STN",
            //            "443TR",
            //            "443TR-STN",
            //            "45-2DIV-STN-TRM",
            //            "45-2DIV-TRM",
            //            "45-3DIV-TRM",
            //            "4500-REB-TRM",
            //            "4500-STN-REB-TRM",
            //            "4500-STN-TRM",
            //            "4500-TRM",
            //            "4500STNTRMTC",
            //            "4500TRMTC",
            //            "4501-STN-TRM",
            //            "4501-STN-1.5-TRM",
            //            "4501-STN-2.0-TRM",
            //            "4501-TRM",
            //            "4501-1.5-TRM",
            //            "4501-2.0-TRM",
            //            "4501STNTRMTC",
            //            "4501STN15TRMTC",
            //            "4501TRMTC",
            //            "450115TRMTC",
            //            "4503-STN-TRM",
            //            "4503-STN-1.5-TRM",
            //            "4503-STN-2.0-TRM",
            //            "4503-TRM",
            //            "4503-2.0-TRM",
            //            "4503STNTRMTC",
            //            "4503STN15TRMTC",
            //            "4503TRMTC",
            //            "450315TRMTC",
            //            "4505-STN-TRM",
            //            "4505-STN-1.5-TRM",
            //            "4505-STN-2.0-TRM",
            //            "4505-TRM",
            //            "4505-1.5-TRM",
            //            "4505-2.0-TRM",
            //            "4505STNTRMTC",
            //            "4505STN15TRMTC",
            //            "4505TRMTC",
            //            "450515TRMTC",
            //            "4506-STN-TRM",
            //            "4506-STN-1.5-TRM",
            //            "4506-STN-2.0-TRM",
            //            "4506-TRM",
            //            "4506-1.5-TRM",
            //            "4506-2.0-TRM",
            //            "4506CRTRMTC",
            //            "4506CR15TRMTC",
            //            "4506STNTRMTC",
            //            "4506STN15TRMTC",
            //            "4506TRMTC",
            //            "450615TRMTC",
            //            "453RH",
            //            "453RH-STN",
            //            "453TB-18",
            //            "453TB-18-STN",
            //            "453TB-24",
            //            "453TB-24-STN",
            //            "453TP",
            //            "453TP-STN",
            //            "453TR",
            //            "453TR-STN",
            //            "46-1X-BODY",
            //            "46-2-BODY",
            //            "462W",
            //            "462W-PNL",
            //            "462W-STN",
            //            "462W-STN-1.5",
            //            "462W-STN-2.0",
            //            "462W-1.5",
            //            "462W-2.0",
            //            "463RH",
            //            "463RH-STN",
            //            "463TB-18",
            //            "463TB-18-STN",
            //            "463TB-24",
            //            "463TB-24-STN",
            //            "463TP",
            //            "463TP-STN",
            //            "463TR",
            //            "463TR-STN",
            //            "47-2DIV-STN",
            //            "47-2DIV-STN-TRM",
            //            "47-2DIV-TRM",
            //            "47-3DIV-STN-TRM",
            //            "4700-REB-TRM",
            //            "4700-STN-REB-TRM",
            //            "4700-STN-TRM",
            //            "4700-TRM",
            //            "4700L7TRM",
            //            "4700STNL7TRM",
            //            "4700STNTRMTC",
            //            "4700TRMTC",
            //            "4701-STN-TRM",
            //            "4701-STN-1.5-TRM",
            //            "4701-STN-2.0-TRM",
            //            "4701-TRM",
            //            "4701-1.5-TRM",
            //            "4701-2.0-TRM",
            //            "4701L715TRM",
            //            "4701STNL1TRM",
            //            "4701STNTRMTC",
            //            "4701STN15TRMTC",
            //            "4701TRM",
            //            "4701TRMTC",
            //            "470115TRMTC",
            //            "4702-STN-SS-TRM",
            //            "4702-STN-SS-1.5-TRM",
            //            "4702-STN-SS-2.0-TRM",
            //            "4702-STN-TRM",
            //            "4702-STN-1.5-TRM",
            //            "4702-STN-2.0-TRM",
            //            "4702-TRM",
            //            "4702-1.5-TRM",
            //            "4702-2.0-TRM",
            //            "4702L1TRM",
            //            "4702STNL1L6TRM",
            //            "4702STNL1TRM",
            //            "4702STNL6TRM",
            //            "4702STNL615TRM",
            //            "4702STNL620TRM",
            //            "4702STNTRMTC",
            //            "4702STN15TRMTC",
            //            "4702TRMTC",
            //            "470215TRMTC",
            //            "4703-STN-TRM",
            //            "4703-STN-1.5-TRM",
            //            "4703-STN-2.0-TRM",
            //            "4703-TRM",
            //            "4703-1.5-TRM",
            //            "4703-2.0-TRM",
            //            "4703STNTRMTC",
            //            "4703STN15TRMTC",
            //            "4703TRMTC",
            //            "470315TRMTC",
            //            "4704L5TRM",
            //            "4705-STN-TRM",
            //            "4705-STN-1.5-TRM",
            //            "4705-STN-2.0-TRM",
            //            "4705-TRM",
            //            "4705-1.5-TRM",
            //            "4705-2.0-TRM",
            //            "4705L5L7TRM",
            //            "4705L5TRM",
            //            "4705L515TRM",
            //            "4705L520TRM",
            //            "4705STNL5TRM",
            //            "4705STNTRMTC",
            //            "4705STN15TRMTC",
            //            "4705TRMTC",
            //            "470515TRMTC",
            //            "4706-STN-TRM",
            //            "4706-STN-1.5-TRM",
            //            "4706-STN-2.0-TRM",
            //            "4706-TRM",
            //            "4706-1.5-TRM",
            //            "4706-2.0-TRM",
            //            "4706STNTRMTC",
            //            "4706STN15TRMTC",
            //            "4706TRMTC",
            //            "470615TRMTC",
            //            "473RH",
            //            "473RH-STN",
            //            "473TB-18",
            //            "473TB-18-STN",
            //            "473TB-24",
            //            "473TB-24-STN",
            //            "473TP",
            //            "473TP-STN",
            //            "473TR",
            //            "473TR-STN",
            //            "51-2DIV-STN-TRM",
            //            "51-2DIV-TRM",
            //            "5100-REB-TRM",
            //            "5100-STN-REB-TRM",
            //            "5100-STN-TRM",
            //            "5100-TRM",
            //            "5100STNTRMTC",
            //            "5100TRMTC",
            //            "5101-STN-TRM",
            //            "5101-STN-1.5-TRM",
            //            "5101-STN-2.0-TRM",
            //            "5101-TRM",
            //            "5101-1.5-TRM",
            //            "5101-2.0-TRM",
            //            "5101STNTRMTC",
            //            "5101TRMTC",
            //            "5103-STN-TRM",
            //            "5103-STN-1.5-TRM",
            //            "5103-STN-2.0-TRM",
            //            "5103-TRM",
            //            "5103-1.5-TRM",
            //            "5103-2.0-TRM",
            //            "5103STNTRMTC",
            //            "5103TRMTC",
            //            "5105-STN-TRM",
            //            "5105-STN-1.5-TRM",
            //            "5105-STN-2.0-TRM",
            //            "5105-TRM",
            //            "5105-1.5-TRM",
            //            "5105-2.0-TRM",
            //            "5105STNTRMTC",
            //            "5105TRMTC",
            //            "5106-STN-TRM",
            //            "5106-STN-1.5-TRM",
            //            "5106-STN-2.0-TRM",
            //            "5106-TRM",
            //            "5106-1.5-TRM",
            //            "5106-2.0-TRM",
            //            "5106STNTRMTC",
            //            "5106TRMTC",
            //            "511SH-STN",
            //            "511SH-STN-1.5",
            //            "511SH-STN-2.0",
            //            "512HS",
            //            "512HS-STN",
            //            "512HS-STN-1.5",
            //            "512HS-STN-2.0",
            //            "512HS-1.5",
            //            "512HS-2.0",
            //            "512HSA",
            //            "512HSA-STN",
            //            "512HSA-STN-1.5",
            //            "512HSA-STN-2.0",
            //            "512HSA-1.5",
            //            "512HSA-2.0",
            //            "512SA",
            //            "512SA-SQ",
            //            "512SA-STN",
            //            "512TS",
            //            "512TS-STN",
            //            "513RH",
            //            "513RH-STN",
            //            "513TB-18",
            //            "513TB-18-STN",
            //            "513TB-24",
            //            "513TB-24-STN",
            //            "513TP",
            //            "513TP-STN",
            //            "513TR",
            //            "513TR-STN",
            //            "52-2DIV-BBZ-TRM",
            //            "52-2DIV-STN-TRM",
            //            "52-2DIV-TRM",
            //            "52-3DIV-BBZ-TRM",
            //            "52-3DIV-STN-TRM",
            //            "52-3DIV-TRM",
            //            "5200-BBZ-TRM",
            //            "5200-REB-TRM",
            //            "5200-STN-REB-TRM",
            //            "5200-STN-TRM",
            //            "5200-TRM",
            //            "5201-BBZ-TRM",
            //            "5201-BBZ-2.0-TRM",
            //            "5201-REB-TRM",
            //            "5201-REB-1.5-TRM",
            //            "5201-STN-REB-TRM",
            //            "5201-STN-REB-1.5-TRM",
            //            "5201-STN-TRM",
            //            "5201-STN-1.5-TRM",
            //            "5201-STN-2.0-TRM",
            //            "5201-TRM",
            //            "5201-1.5-TRM",
            //            "5201-2.0-TRM",
            //            "5201BBZ15TRM",
            //            "5203-BBZ-TRM",
            //            "5203-BBZ-2.0-TRM",
            //            "5203-STN-TRM",
            //            "5203-STN-2.0-TRM",
            //            "5203-TRM",
            //            "5203-2.0-TRM",
            //            "5205-BBZ-TRM",
            //            "5205-BBZ-2.0-TRM",
            //            "5205-STN-TRM",
            //            "5205-STN-2.0-TRM",
            //            "5205-TRM",
            //            "5205-1.5-TRM",
            //            "5205-2.0-TRM",
            //            "5206-BBZ-TRM",
            //            "5206-BBZ-2.0-TRM",
            //            "5206-STN-TRM",
            //            "5206-STN-2.0-TRM",
            //            "5206-TRM",
            //            "5206-2.0-TRM",
            //            "522HS",
            //            "522HS-BBZ",
            //            "522HS-BBZ-1.5",
            //            "522HS-BBZ-2.0",
            //            "522HS-STN",
            //            "522HS-STN-1.5",
            //            "522HS-STN-2.0",
            //            "522HS-1.5",
            //            "522HS-2.0",
            //            "522SA",
            //            "522SA-BBZ",
            //            "522SA-STN",
            //            "522TS",
            //            "522TS-BBZ",
            //            "522TS-STN",
            //            "523RH",
            //            "523RH-BBZ",
            //            "523RH-STN",
            //            "523TB-18",
            //            "523TB-18-BBZ",
            //            "523TB-18-STN",
            //            "523TB-24",
            //            "523TB-24-BBZ",
            //            "523TB-24-STN",
            //            "523TP",
            //            "523TP-BBZ",
            //            "523TP-STN",
            //            "523TR",
            //            "523TR-BBZ",
            //            "523TR-STN",
            //            "53-2DIV-BLK-TRM",
            //            "53-2DIV-STN-TRM",
            //            "53-2DIV-TRM",
            //            "5300-BLK-TRM",
            //            "5300-REB-TRM",
            //            "5300-STN-REB-TRM",
            //            "5300-STN-TRM",
            //            "5300-TRM",
            //            "5300BLKTRMTC",
            //            "5300STNTRMTC",
            //            "5300TRMTC",
            //            "5301-BLK-TRM",
            //            "5301-BLK-1.5-TRM",
            //            "5301-BLK-2.0-TRM",
            //            "5301-STN-TRM",
            //            "5301-STN-1.5-TRM",
            //            "5301-STN-2.0-TRM",
            //            "5301-TRM",
            //            "5301-1.5-TRM",
            //            "5301-2.0-TRM",
            //            "5301BLKTRMTC",
            //            "5301STNTRMTC",
            //            "5301TRMTC",
            //            "5302-BLK-TRM",
            //            "5302-BLK-1.5-TRM",
            //            "5302-BLK-2.0-TRM",
            //            "5302-STN-TRM",
            //            "5302-STN-1.5-TRM",
            //            "5302-STN-2.0-TRM",
            //            "5302-TRM",
            //            "5302-1.5-TRM",
            //            "5302-2.0-TRM",
            //            "5302BLKTRMTC",
            //            "5302STNTRMTC",
            //            "5302TRMTC",
            //            "5303-BLK-TRM",
            //            "5303-BLK-1.5-TRM",
            //            "5303-BLK-2.0-TRM",
            //            "5303-STN-TRM",
            //            "5303-STN-1.5-TRM",
            //            "5303-STN-2.0-TRM",
            //            "5303-TRM",
            //            "5303-1.5-TRM",
            //            "5303-2.0-TRM",
            //            "5303BLKTRMTC",
            //            "5303STNTRMTC",
            //            "5303TRMTC",
            //            "5305-BLK-TRM",
            //            "5305-BLK-1.5-TRM",
            //            "5305-BLK-2.0-TRM",
            //            "5305-STN-TRM",
            //            "5305-STN-1.5-TRM",
            //            "5305-STN-2.0-TRM",
            //            "5305-TRM",
            //            "5305-1.5-TRM",
            //            "5305-2.0-TRM",
            //            "5305BLKTRMTC",
            //            "5305STNTRMTC",
            //            "5305TRMTC",
            //            "5306-BLK-TRM",
            //            "5306-BLK-1.5-TRM",
            //            "5306-BLK-2.0-TRM",
            //            "5306-STN-TRM",
            //            "5306-STN-1.5-TRM",
            //            "5306-STN-2.0-TRM",
            //            "5306-TRM",
            //            "5306-1.5-TRM",
            //            "5306-2.0-TRM",
            //            "5306BLKTRMTC",
            //            "5306STNTRMTC",
            //            "5306TRMTC",
            //            "532HS",
            //            "532HS-BLK",
            //            "532HS-BLK-1.5",
            //            "532HS-BLK-2.0",
            //            "532HS-STN",
            //            "532HS-STN-1.5",
            //            "532HS-STN-2.0",
            //            "532HS-1.5",
            //            "532HS-2.0",
            //            "532HSB-STN",
            //            "532SA",
            //            "532SA-BLK",
            //            "532SA-STN",
            //            "532SH",
            //            "532SH-BLK",
            //            "532SH-BLK-1.5",
            //            "532SH-BLK-2.0",
            //            "532SH-STN",
            //            "532SH-STN-1.5",
            //            "532SH-STN-2.0",
            //            "532SH-1.5",
            //            "532SH-2.0",
            //            "532TS",
            //            "532TS-BLK",
            //            "532TS-STN",
            //            "532TSD",
            //            "532TSD-BLK",
            //            "532TSD-STN",
            //            "533RH",
            //            "533RH-BLK",
            //            "533RH-STN",
            //            "533TB-18",
            //            "533TB-18-BLK",
            //            "533TB-18-STN",
            //            "533TB-24",
            //            "533TB-24-BLK",
            //            "533TB-24-STN",
            //            "533TPL",
            //            "533TPL-BLK",
            //            "533TPL-STN",
            //            "533TPR",
            //            "533TPR-BLK",
            //            "533TPR-STN",
            //            "533TR",
            //            "533TR-BLK",
            //            "533TR-STN",
            //            "54-2DIV-STN-TRM",
            //            "54-2DIV-TRM",
            //            "5400-REB-TRM",
            //            "5400-STN-REB-TRM",
            //            "5400-STN-TRM",
            //            "5400-TRM",
            //            "5400STNTRMTC",
            //            "5400TRMTC",
            //            "5401-STN-TRM",
            //            "5401-STN-1.5-TRM",
            //            "5401-STN-2.0-TRM",
            //            "5401-TRM",
            //            "5401-1.5-TRM",
            //            "5401-2.0-TRM",
            //            "5401STNTRMTC",
            //            "5401STN15TRMTC",
            //            "5401TRMTC",
            //            "540115TRMTC",
            //            "5402-STN-TRM",
            //            "5402-STN-1.5-TRM",
            //            "5402-STN-2.0-TRM",
            //            "5402-TRM",
            //            "5402-1.5-TRM",
            //            "5402-2.0-TRM",
            //            "5402STNTRMTC",
            //            "5402STN15TRMTC",
            //            "5402TRMTC",
            //            "540215TRMTC",
            //            "5403-STN-TRM",
            //            "5403-STN-1.5-TRM",
            //            "5403-STN-2.0-TRM",
            //            "5403-TRM",
            //            "5403-1.5-TRM",
            //            "5403-2.0-TRM",
            //            "5403STNTRMTC",
            //            "5403STN15TRMTC",
            //            "5403TRMTC",
            //            "540315TRMTC",
            //            "5405-STN-TRM",
            //            "5405-STN-1.5-TRM",
            //            "5405-STN-2.0-TRM",
            //            "5405-TRM",
            //            "5405-1.5-TRM",
            //            "5405-2.0-TRM",
            //            "5405L1TRM",
            //            "5405STNTRMTC",
            //            "5405STN15TRMTC",
            //            "5405TRMTC",
            //            "540515TRMTC",
            //            "5406-STN-TRM",
            //            "5406-STN-1.5-TRM",
            //            "5406-STN-2.0-TRM",
            //            "5406-TRM",
            //            "5406-1.5-TRM",
            //            "5406-2.0-TRM",
            //            "5406STNTRMTC",
            //            "5406STN15TRMTC",
            //            "5406TRMTC",
            //            "540615TRMTC",
            //            "542TS",
            //            "542TS-STN",
            //            "542TSD",
            //            "542TSD-STN",
            //            "543RH",
            //            "543RH-STN",
            //            "543TB-18",
            //            "543TB-18-STN",
            //            "543TB-24",
            //            "543TB-24-STN",
            //            "543TPL",
            //            "543TPL-STN",
            //            "543TPR",
            //            "543TPR-STN",
            //            "543TR",
            //            "543TR-STN",
            //            "55-2DIV-SBZ-TRM",
            //            "55-2DIV-STN-TRM",
            //            "55-2DIV-TRM",
            //            "55AC3BUNDLE",
            //            "55AC3BUNDLESBZ",
            //            "55AC3BUNDLESTN",
            //            "55AC4BUNDLE",
            //            "55AC4BUNDLESBZ",
            //            "55AC4BUNDLESTN",
            //            "5500-REB-TRM",
            //            "5500-SBZ-REB-TRM",
            //            "5500-SBZ-TRM",
            //            "5500-STN-REB-TRM",
            //            "5500-STN-TRM",
            //            "5500-TRM",
            //            "5500SBZTRMTC",
            //            "5500STNTRMTC",
            //            "5500TRMTC",
            //            "5501-SBZ-TRM",
            //            "5501-SBZ-1.5-TRM",
            //            "5501-SBZ-2.0-TRM",
            //            "5501-STN-TRM",
            //            "5501-STN-1.5-TRM",
            //            "5501-STN-2.0-TRM",
            //            "5501-TRM",
            //            "5501-1.5-TRM",
            //            "5501-2.0-TRM",
            //            "5501L1TRM",
            //            "5501SBZTRMTC",
            //            "5501SBZ15TRMTC",
            //            "5501STNTRMTC",
            //            "5501STN15TRMTC",
            //            "5501TRMTC",
            //            "550115TRMTC",
            //            "5502-SBZ-TRM",
            //            "5502-SBZ-1.5-TRM",
            //            "5502-SBZ-2.0-TRM",
            //            "5502-STN-TRM",
            //            "5502-STN-1.5-TRM",
            //            "5502-STN-2.0-TRM",
            //            "5502-TRM",
            //            "5502-1.5-TRM",
            //            "5502-2.0-TRM",
            //            "5502L1TRM",
            //            "5502SBZTRMTC",
            //            "5502SBZ15TRMTC",
            //            "5502STNL1TRM",
            //            "5502STNTRMTC",
            //            "5502STN15TRMTC",
            //            "5502TRMTC",
            //            "550215TRMTC",
            //            "5503-SBZ-TRM",
            //            "5503-SBZ-1.5-TRM",
            //            "5503-SBZ-2.0-TRM",
            //            "5503-STN-TRM",
            //            "5503-STN-1.5-TRM",
            //            "5503-STN-2.0-TRM",
            //            "5503-TRM",
            //            "5503-1.5-TRM",
            //            "5503-2.0-TRM",
            //            "5503SBZTRMTC",
            //            "5503SBZ15TRMTC",
            //            "5503STNTRMTC",
            //            "5503STN15TRMTC",
            //            "5503TRMTC",
            //            "550315TRMTC",
            //            "5505-SBZ-TRM",
            //            "5505-SBZ-1.5-TRM",
            //            "5505-SBZ-2.0-TRM",
            //            "5505-STN-TRM",
            //            "5505-STN-1.5-TRM",
            //            "5505-STN-2.0-TRM",
            //            "5505-TRM",
            //            "5505-1.5-TRM",
            //            "5505-2.0-TRM",
            //            "5505L1L5TRM",
            //            "5505L515TRM",
            //            "5505L520TRM",
            //            "5505SBZTRMTC",
            //            "5505SBZ15TRMTC",
            //            "5505STNTRMTC",
            //            "5505STN15TRMTC",
            //            "5505TRMTC",
            //            "550515TRMTC",
            //            "5506-SBZ-TRM",
            //            "5506-SBZ-1.5-TRM",
            //            "5506-SBZ-2.0-TRM",
            //            "5506-STN-TRM",
            //            "5506-STN-1.5-TRM",
            //            "5506-STN-2.0-TRM",
            //            "5506-TRM",
            //            "5506-1.5-TRM",
            //            "5506-2.0-TRM",
            //            "5506SBZTRMTC",
            //            "5506SBZ15TRMTC",
            //            "5506STNTRMTC",
            //            "5506STN15TRMTC",
            //            "5506TRMTC",
            //            "550615TRMTC",
            //            "552HS",
            //            "552HS-SBZ",
            //            "552HS-SBZ-1.5",
            //            "552HS-STN",
            //            "552HS-STN-1.5",
            //            "552HS-1.5",
            //            "552HSB",
            //            "552HSB-SBZ",
            //            "552HSB-SBZ-1.5",
            //            "552HSB-STN",
            //            "552HSB-STN-1.5",
            //            "552HSB-1.5",
            //            "552HSB-2.0",
            //            "552SH",
            //            "552SH-SBZ",
            //            "552SH-SBZ-1.5",
            //            "552SH-SBZ-1.75",
            //            "552SH-SBZ-2.0",
            //            "552SH-STN",
            //            "552SH-STN-1.5",
            //            "552SH-STN-1.75",
            //            "552SH-STN-2.0",
            //            "552SH-1.5",
            //            "552SH-1.75",
            //            "552SH-2.0",
            //            "552TS",
            //            "552TS-SBZ",
            //            "552TS-STN",
            //            "552TSD",
            //            "552TSD-SBZ",
            //            "552TSD-STN",
            //            "552W",
            //            "552W-SBZ",
            //            "552W-SBZ-1.5",
            //            "552W-SBZ-2.0",
            //            "552W-STN",
            //            "552W-STN-1.5",
            //            "552W-STN-2.0",
            //            "552W-1.5",
            //            "552W-2.0",
            //            "553RH",
            //            "553RH-SBZ",
            //            "553RH-STN",
            //            "553TB-18",
            //            "553TB-18-SBZ",
            //            "553TB-18-STN",
            //            "553TB-24",
            //            "553TB-24-SBZ",
            //            "553TB-24-STN",
            //            "553TP",
            //            "553TP-SBZ",
            //            "553TP-STN",
            //            "553TR",
            //            "553TR-SBZ",
            //            "553TR-STN",
            //            "57-2DIV-TRM",
            //            "5700-REB-TRM",
            //            "5700-TRM",
            //            "626RH",
            //            "66-DIV-TRM",
            //            "66-DIV-TRM-STN",
            //            "66AC-BUNDLE",
            //            "66AC-BUNDLE-STN",
            //            "6600-ESC",
            //            "6600-ESC-STN",
            //            "6600-OP-TRM",
            //            "6600-OP-TRM-STN",
            //            "6600-REB-TRM",
            //            "6600-REB-TRM-STN",
            //            "6600-TRM",
            //            "6600-TRM-STN",
            //            "6601-TRM",
            //            "6601-TRM-STN",
            //            "6601-1.5-TRM",
            //            "6601-1.5-TRM-STN",
            //            "6601-2.0-TRM",
            //            "6601-2.0-TRM-STN",
            //            "6601L1TRM",
            //            "6601L7TRM",
            //            "6601L715TRM",
            //            "6601TRMTC",
            //            "660115TRMTC",
            //            "6602-SS-TRM",
            //            "6602-SS-TRM-STN",
            //            "6602-TRM",
            //            "6602-TRM-STN",
            //            "6602-1.5-TRM",
            //            "6602-1.5-TRM-STN",
            //            "6602-2.0-TRM",
            //            "6602-2.0-TRM-STN",
            //            "6602L1TRM",
            //            "6602L1TRMSTN",
            //            "6602L6TRM",
            //            "6602L6TRMSTN",
            //            "6602L7TRM",
            //            "6602TRMTC",
            //            "660215TRMTC",
            //            "6603-TRM",
            //            "6603-TRM-STN",
            //            "6603-1.5-TRM",
            //            "6603-1.5-TRM-STN",
            //            "6603-2.0-TRM",
            //            "6603-2.0-TRM-STN",
            //            "6603TRMTC",
            //            "660315TRMTC",
            //            "67-DIV-ESC-STN",
            //            "6604-TRM",
            //            "6604-TRM-STN",
            //            "6604-1.5-TRM",
            //            "6604-1.5-TRM-STN",
            //            "6604-2.0-TRM",
            //            "6604TRMTC",
            //            "660415TRMTC",
            //            "6605-TRM",
            //            "6605-TRM-STN",
            //            "6605-1.5-TRM",
            //            "6605-1.5-TRM-STN",
            //            "6605-2.0-TRM",
            //            "6605-2.0-TRM-STN",
            //            "6605L115TRM",
            //            "6605L5TRM",
            //            "6605TRMTC",
            //            "660515TRMTC",
            //            "6606-TRM",
            //            "6606-TRM-STN",
            //            "6606-1.5-TRM",
            //            "6606-1.5-TRM-STN",
            //            "6606-2.0-TRM",
            //            "6606-2.0-TRM-STN",
            //            "6606L215TRM",
            //            "6606L5TRM",
            //            "6606L515TRM",
            //            "6606TRMTC",
            //            "660615TRMTC",
            //            "663RH",
            //            "663RH-STN",
            //            "663TB-18",
            //            "663TB-18-STN",
            //            "663TB-24",
            //            "663TB-24-STN",
            //            "663TP",
            //            "663TP-STN",
            //            "663TR",
            //            "663TR-STN",
            //            "67-DIV-ESC",
            //            "67-DIV-TRM",
            //            "67-DIV-TRM-STN",
            //            "67AC3BUNDLE",
            //            "67AC3BUNDLESTN",
            //            "67AC4BUNDLE",
            //            "67AC4BUNDLESTN",
            //            "6700-ESC",
            //            "6700-ESC-STN",
            //            "6700-REB-TRM",
            //            "6700-TRM",
            //            "6700-TRM-STN",
            //            "6700L7TRM",
            //            "6700L7TRMSTN",
            //            "6700TRMTC",
            //            "6701-TRM",
            //            "6701-TRM-STN",
            //            "6701-1.5-TRM",
            //            "6701-1.5-TRM-STN",
            //            "6701-2.0-TRM",
            //            "6701-2.0-TRM-STN",
            //            "6701L1TRM",
            //            "6701L1TRMSTN",
            //            "6701L7TRM",
            //            "6701L7TRMSTN",
            //            "6701TRMTC",
            //            "670115TRMTC",
            //            "6702-SS-TRM",
            //            "6702-SS-1.5-TRM",
            //            "6702-TRM",
            //            "6702-TRM-STN",
            //            "6702-1.5-TRM",
            //            "6702-1.5-TRM-STN",
            //            "6702-2.0-TRM",
            //            "6702-2.0-TRM-STN",
            //            "6702L1L6TRM",
            //            "6702L1L6TRMSTN",
            //            "6702L1TRM",
            //            "6702L1TRMSTN",
            //            "6702L6TRM",
            //            "6702L6TRMSTN",
            //            "6702L615TRM",
            //            "6702L7TRM",
            //            "6702L7TRMSTN",
            //            "6702TRMTC",
            //            "670215TRMTC",
            //            "6703-TRM",
            //            "6703-TRM-STN",
            //            "6703-1.5-TRM",
            //            "6703-1.5-TRM-STN",
            //            "6703-2.0-TRM",
            //            "6703-2.0-TRM-STN",
            //            "6703L2TRM",
            //            "6703L2TRMSTN",
            //            "6703L5TRM",
            //            "6703L5TRMSTN",
            //            "6703TRMTC",
            //            "670315TRMTC",
            //            "6704-TRM",
            //            "6704-TRM-STN",
            //            "6704-1.5-TRM",
            //            "6704-1.5-TRM-STN",
            //            "6704-2.0-TRM",
            //            "6704-2.0-TRM-STN",
            //            "6704L5TRM",
            //            "6704L5TRMSTN",
            //            "6704TRMTC",
            //            "670415TRMTC",
            //            "6705-TRM",
            //            "6705-TRM-STN",
            //            "6705-1.5-TRM",
            //            "6705-1.5-TRM-STN",
            //            "6705-2.0-TRM",
            //            "6705-2.0-TRM-STN",
            //            "6705L1L5TRM",
            //            "6705L1TRM",
            //            "6705L1TRMSTN",
            //            "6705L2TRM",
            //            "6705L2TRMSTN",
            //            "6705L3TRM",
            //            "6705L3TRMSTN",
            //            "6705TRMTC",
            //            "670515TRMTC",
            //            "6706-TRM",
            //            "6706-TRM-STN",
            //            "6706-1.5-TRM",
            //            "6706-1.5-TRM-STN",
            //            "6706-2.0-TRM",
            //            "6706-2.0-TRM-STN",
            //            "6706L1TRM",
            //            "6706L1TRMSTN",
            //            "6706L2TRM",
            //            "6706L2TRMSTN",
            //            "6706L3TRM",
            //            "6706L3TRMSTN",
            //            "6706TRMTC",
            //            "670615TRMTC",
            //            "672SH",
            //            "672SH-STN",
            //            "672SH-STN-1.5",
            //            "672SH-STN-2.0",
            //            "672SH-1.5",
            //            "672SH-2.0",
            //            "673RH",
            //            "673RH-STN",
            //            "673TB-STN-18",
            //            "673TB-STN-24",
            //            "673TB-18",
            //            "673TB-24",
            //            "673TP",
            //            "673TP-STN",
            //            "673TR",
            //            "673TR-STN",
            //            "7-100",
            //            "7-100NW",
            //            "7-1000",
            //            "7-1000-W",
            //            "7-1000-102-PRV",
            //            "7-1000-200-PRV",
            //            "7-1000A",
            //            "7-1000A-P-W",
            //            "7-1000A-W",
            //            "7-1000B",
            //            "7-1000B-102-PRV",
            //            "7-1000B-200-PRV",
            //            "7-1000BW",
            //            "7-1000NW",
            //            "7-102",
            //            "7-102A",
            //            "7-102A-V",
            //            "7-102B",
            //            "7-102B-S-T",
            //            "7-102NW",
            //            "7-102P",
            //            "7-102P-NW",
            //            "7-200",
            //            "7-200-NI",
            //            "7-200-NI-W",
            //            "7-200-W",
            //            "7-200-102-PRV",
            //            "7-200A",
            //            "7-200A-ASB",
            //            "7-200A-ASB-V-W",
            //            "7-200A-P",
            //            "7-200A-P-W",
            //            "7-200A-V",
            //            "7-200A-V-W",
            //            "7-200A-W",
            //            "7-200B",
            //            "7-200B-M-T",
            //            "7-200B-S",
            //            "7-200B-S-T",
            //            "7-200B-T",
            //            "7-200B-102-PRV",
            //            "7-200NW",
            //            "7-225-CK-F",
            //            "7-225-CK-F-NI",
            //            "7-225-CK-F-NI-W",
            //            "7-225-CK-F-W",
            //            "7-225-CK-MS",
            //            "7-225-CK-MS-NI",
            //            "7-225-CK-MS-NI-W",
            //            "7-225-CK-MS-NI-X",
            //            "7-225-CK-MS-W",
            //            "7-225-CK-MS-W-X",
            //            "7-225-CK-MS-X",
            //            "7-225-CK-PEX",
            //            "7-225B-CK-F",
            //            "7-225B-CK-MS",
            //            "7-225B-CK-MS-T",
            //            "7-225B-CK-MS-T-X",
            //            "7-225B-CK-MS-X",
            //            "7-225B-CK-PEX",
            //            "7-230-CK-FS",
            //            "7-230-CK-M",
            //            "7-230-CK-M-W",
            //            "7-230B-CK-FS",
            //            "7-230B-CK-M",
            //            "7-230B-CK-M-T",
            //            "7-400",
            //            "7-400-NI",
            //            "7-400-W",
            //            "7-400-102-PRV",
            //            "7-400A",
            //            "7-400A-ASB",
            //            "7-400A-ASB-W",
            //            "7-400A-NI-W",
            //            "7-400A-P-W",
            //            "7-400A-V",
            //            "7-400A-V-W",
            //            "7-400A-W",
            //            "7-400B",
            //            "7-400B-S",
            //            "7-400B-T",
            //            "7-400B-102-PRV",
            //            "7-400NW",
            //            "7-500",
            //            "7-500-NI",
            //            "7-500-W",
            //            "7-500-102-PRV",
            //            "7-500A",
            //            "7-500A-ASB-W",
            //            "7-500A-V",
            //            "7-500A-W",
            //            "7-500B",
            //            "7-500B-M",
            //            "7-500B-102-PRV-T",
            //            "7-500BW",
            //            "7-500NW",
            //            "7-700",
            //            "7-700-P-W",
            //            "7-700-W",
            //            "7-700-102-PRV",
            //            "7-700A",
            //            "7-700A-P-W",
            //            "7-700A-W",
            //            "7-700B",
            //            "7-700B-M-TOP",
            //            "7-700NW",
            //            "7-900",
            //            "7-900-W",
            //            "7-900-102-PRV",
            //            "7-900-200-PRV",
            //            "7-900A",
            //            "7-900A-W",
            //            "7-900NW",
            //            "72SH-RP",
            //            "72SH-STN-RP",
            //            "76HS-RP",
            //            "76SH-RP",
            //            "8210BCK",
            //            "8210BCKD",
            //            "8210BCKDT",
            //            "8210BCKNI",
            //            "8210BCKNIT",
            //            "8210BCKT",
            //            "8210CK",
            //            "8210CKD",
            //            "8210NICK",
            //            "8210NICKD",
            //            "96-DIV-PLR-TRM",
            //            "96-DIV-PLR-TRM-STN",
            //            "9600-PLR-ESC-STN",
            //            "96-2DIV-PLR",
            //            "96-2DIV-PLR-NS",
            //            "96-2DIV-PLR-STN",
            //            "96-2DIV-PLR-STN-NS",
            //            "96-66-DIV-ESC",
            //            "96SH-RP",
            //            "9600-CHKS-P",
            //            "9600-CHKS-PLR",
            //            "9600-CHKS-PLR-B",
            //            "9600-OP-ESC",
            //            "9600-P",
            //            "9600-P-B-ESC",
            //            "9600-P-B-TRM",
            //            "9600-P-ESC",
            //            "9600-P-TRM",
            //            "9600-PLR",
            //            "9600-PLR-B-ESC",
            //            "9600-PLR-B-TRM",
            //            "9600-PLR-ESC",
            //            "9600-PLR-OP-TRM",
            //            "9600-PLR-OP-TRM-STN",
            //            "9600-PLR-STN",
            //            "9600-PLR-TRM",
            //            "9600-PLR-TRM-STN",
            //            "9600-REV-X-P",
            //            "9600-REV-X-PLR",
            //            "9600-X-P",
            //            "9600-X-PLR",
            //            "9600-X-PLR-B",
            //            "3605L1TRM",
            //            "3605L1TRMTC",
            //            "3605L115TRM",
            //            "3605L115TRMTC",
            //            "3605L120TRM",
            //            "3605L3TRM",
            //            "3605MBL1TRM",
            //            "3605MBL1TRMTC",
            //            "3605MBL115TRM",
            //            "3605MBL115TRMTC",
            //            "3605STNL1TRMTC",
            //            "3605STNL2TRM",
            //            "3606-H321-V-BBZ-TRM",
            //            "3606-H321-V-MB-TRM",
            //            "3606-H321-V-MB-1.5-TRM",
            //            "3606-H321-V-STN-TRM",
            //            "3606-H321-V-STN-1.5-TRM",
            //            "3606-H321-V-STN-2.0-TRM",
            //            "3606-H321-V-TRM",
            //            "3606-H321-V-1.5-TRM",
            //            "3606-H321-V-2.0-TRM",
            //            "3606BBZL1L6TRMTC",
            //            "3606BBZL1L615TRMTC",
            //            "3606BBZL1TRM",
            //            "3606BBZL2TRM",
            //            "3606BBZL3TRM",
            //            "3606BBZL6TRM",
            //            "3606H321BBZTRMTC",
            //            "3606H321BBZ15TRM",
            //            "3606H321BBZ15TRMTC",
            //            "3606H321MBTRMTC",
            //            "3606H321MB15TRMTC",
            //            "3606H321STNTRMTC",
            //            "3606H321STN15TRMTC",
            //            "3606H321TRMTC",
            //            "3606H32115TRMTC",
            //            "3606L1L6TRM",
            //            "3606L1L6TRMTC",
            //            "3606L1L615TRMTC",
            //            "3606L1L7TRM",
            //            "3606L1L715TRM",
            //            "3606L115TRM",
            //            "3606L2TRM",
            //            "3606L3TRM",
            //            "3606L6L7TRM",
            //            "3606L6L715TRM",
            //            "3606L6TRM",
            //            "3606L7TRM",
            //            "3606MBL1L6TRM",
            //            "3606MBL1L6TRMTC",
            //            "3606MBL1L615TRM",
            //            "3606MBL1L615TRMTC",
            //            "3606MBL1TRM",
            //            "3606MBL115TRM",
            //            "3606MBL2TRM",
            //            "3606MBL3TRM",
            //            "3606MBL6TRM",
            //            "3606MBL615TRM",
            //            "3606STNL1L6TRMTC",
            //            "3606STNL1L615TRMTC",
            //            "3606STNL1TRM",
            //            "3606STNL2TRM",
            //            "3606STNL3TRM",
            //            "3606STNL6TRM",
            //            "361DTS",
            //            "361DTS-BBZ",
            //            "361DTS-MB",
            //            "361DTS-SS",
            //            "361DTS-SS-STN",
            //            "361DTS-STN",
            //            "361SH",
            //            "361SH-BBZ",
            //            "361SH-BBZ-1.5",
            //            "361SH-MB",
            //            "361SH-MB-1.5",
            //            "361SH-STN",
            //            "361SH-STN-1.5",
            //            "361SH-STN-2.0",
            //            "361SH-1.5",
            //            "361SH-2.0",
            //            "361SH-300SQ",
            //            "361SH-300SQ-STN-1.5",
            //            "361SH-6",
            //            "361SH-6-STN",
            //            "361SH-6-STN-1.5",
            //            "361SH-6-STN-2.0",
            //            "361SH-6-1.5",
            //            "361SH-6-2.0",
            //            "361SH-8",
            //            "361SH-8-2.0",
            //            "361TS",
            //            "361TS-BBZ",
            //            "361TS-MB",
            //            "361TS-SS",
            //            "361TS-SS-STN",
            //            "361TS-STN",
            //            "362SH",
            //            "362SH-BBZ",
            //            "362SH-BBZ-1.5",
            //            "362SH-BBZ-2.0",
            //            "362SH-MB",
            //            "362SH-MB-2.0",
            //            "362SH-STN",
            //            "362SH-STN-1.5",
            //            "362SH-STN-2.0",
            //            "362SH-1.5",
            //            "362SH-2.0",
            //            "3620-TRM",
            //            "3620STNTRMTC",
            //            "3620TRMTC",
            //            "363-SD2",
            //            "3621-1.5-TRM",
            //            "3626-SH1-T4-D2-TRM",
            //            "363DTB-18",
            //            "363DTB-18-STN",
            //            "363DTB-24",
            //            "363DTB-24-STN",
            //            "363GB-12",
            //            "363GB-12-STN",
            //            "363GB-18",
            //            "363GB-18-STN",
            //            "363GB-24",
            //            "363GB-24-STN",
            //            "363GB-36",
            //            "363GB-36-STN",
            //            "363GB-42",
            //            "363GB-42-STN",
            //            "363GBTB-18",
            //            "363GBTB-18-STN",
            //            "363GBTB-24",
            //            "363GBTB-24-STN",
            //            "363GBTP",
            //            "363GBTP-STN",
            //            "363RH",
            //            "363RH-BBZ",
            //            "363RH-MB",
            //            "363RH-STN",
            //            "363TB-18",
            //            "363TB-18-BBZ",
            //            "363TB-18-MB",
            //            "363TB-18-STN",
            //            "363TB-24",
            //            "363TB-24-BBZ",
            //            "363TB-24-MB",
            //            "9602-X-PLR-B",
            //            "9602-X-PLR-B-SS",
            //            "9602-X-PLR-B-1.5",
            //            "9602-X-PLR-OP",
            //            "9602-X-PLR-OP-SS-1.5",
            //            "9602-X-PLR-OP-SS-2.0",
            //            "9602-X-PLR-SS",
            //            "9602-X-PLR-SS-STN",
            //            "9602-X-PLR-SS-1.5",
            //            "9602-X-PLR-SS-1.5-STN",
            //            "9602-X-PLR-SS-2.0",
            //            "9602-X-PLR-SS-2.0-STN",
            //            "9602-X-PLR-STN",
            //            "9602-X-PLR-1.5",
            //            "9602-X-PLR-1.5-STN",
            //            "9602-X-PLR-2.0",
            //            "9602-X-PLR-2.0-STN",
            //            "9602CHKSPLRL1",
            //            "9602CHKSPLRSTN",
            //            "9602L4L7TRM",
            //            "9602L6L7TRM",
            //            "9602L7",
            //            "9602L7TRM",
            //            "9602L720",
            //            "9602L720TRM",
            //            "9602PLRBL615TRM",
            //            "9602PLRL1",
            //            "9602PLRL1L6",
            //            "9602PLRL1L6TRM",
            //            "9602PLRL1L6TRMSTN",
            //            "9602PLRL1TRM",
            //            "9602PLRL1TRMSTN",
            //            "9602PLRL6",
            //            "9602PLRL6STN",
            //            "9602PLRL6TRM",
            //            "9602PLRL615",
            //            "9602PLRL615TRM",
            //            "9602PLRL620",
            //            "9602PLRL620STN",
            //            "9602PLRL620TRMSTN",
            //            "9602PLROPL1TRM",
            //            "9602PLRTRMTC",
            //            "9602PLR15TRMTC",
            //            "9602PLR20TRMTC",
            //            "9602PL1",
            //            "9602PL6",
            //            "9602PL620",
            //            "9602PL620TRM",
            //            "9602PTRMTC",
            //            "9602P15TRMTC",
            //            "9602REVXPLRL1",
            //            "9602XL7",
            //            "9602XL720",
            //            "9602XPLRBL1",
            //            "9602XPLRBL6",
            //            "9602XPLRL1L6",
            //            "9602XPLRL1STN",
            //            "9602XPLRL6",
            //            "9602XPLRL6STN",
            //            "9602XPLRL615",
            //            "9602XPLRL615STN",
            //            "9602XPLRL620",
            //            "9602XPLRL620STN",
            //            "9602XPL1",
            //            "9602XPL6",
            //            "9602XPL620",
            //            "9603-CHKS-P-1.5",
            //            "9603-CHKS-PLR",
            //            "9603-CHKS-PLR-B",
            //            "9603-CHKS-PLR-VP-1.5",
            //            "9603-CHKS-PLR-VP-2.0",
            //            "9603-CHKS-PLR-1.5",
            //            "9603-CHKS-PLR-1.5-STN",
            //            "9603-CHKS-PLR-2.0",
            //            "9603-P",
            //            "9603-P-B-1.5",
            //            "9603-P-TRM",
            //            "9603-P-1.5",
            //            "9603-P-1.5-TRM",
            //            "9603-P-2.0",
            //            "9603-P-2.0-TRM",
            //            "9603-P-72-TRM",
            //            "9603-PLR",
            //            "9603-PLR-B-TRM",
            //            "9603-PLR-B-1.5-TRM",
            //            "9603-PLR-B-2.0-TRM",
            //            "9603-PLR-R-TRM",
            //            "9603-PLR-STN",
            //            "9603-PLR-TRM",
            //            "9603-PLR-TRM-STN",
            //            "9603-PLR-VP",
            //            "9603-PLR-VP-1.5",
            //            "9603-PLR-VP-1.5-TRM",
            //            "9603-PLR-1.5",
            //            "9603-PLR-1.5-STN",
            //            "9603-PLR-1.5-TRM",
            //            "9603-PLR-1.5-TRM-STN",
            //            "9603-PLR-2.0",
            //            "9603-PLR-2.0-STN",
            //            "9603-PLR-2.0-TRM",
            //            "9603-PLR-2.0-TRM-STN",
            //            "9603-PLR-72-TRM",
            //            "9603-PLR-72-TRM-STN",
            //            "9603-PLR-72-2.0",
            //            "9603-PLR-72-2.0-TRM",
            //            "9603-X-P",
            //            "9603-X-P-B",
            //            "9603-X-P-B-1.5",
            //            "9603-X-P-1.5",
            //            "9603-X-P-2.0",
            //            "9603-X-P-72-1.5",
            //            "9603-X-PLR",
            //            "9603-X-PLR-B",
            //            "9603-X-PLR-B-1.5",
            //            "9603-X-PLR-B-2.0",
            //            "9603-X-PLR-B-72",
            //            "9603-X-PLR-B48-L2",
            //            "9603-X-PLR-R",
            //            "9603-X-PLR-R-1.5",
            //            "9603-X-PLR-STN",
            //            "9603-X-PLR-STN-72",
            //            "9603-X-PLR-VP",
            //            "9603-X-PLR-VP-1.5",
            //            "9603-X-PLR-VP-72",
            //            "9603-X-PLR-1.5",
            //            "9603-X-PLR-1.5-STN",
            //            "9603-X-PLR-2.0",
            //            "9603-X-PLR-2.0-STN",
            //            "9603-X-PLR-72",
            //            "9603-X-PLR-72-1.5",
            //            "9603-X-PLR-72-2.0",
            //            "9603CHKSPLRL2",
            //            "9603L7",
            //            "9603L720",
            //            "9603PLRBL2TRM",
            //            "9603PLRL2STN",
            //            "9603PLRL2TRM",
            //            "9603PLRL5TRM",
            //            "9603PLRTRMTC",
            //            "9603PLR15TRMTC",
            //            "S3502BL4L6TRM",
            //            "9603XL715",
            //            "9603XPLRL2",
            //            "9603XPLRL2STN",
            //            "9603XPL2",
            //            "9604-CHKS-PLR",
            //            "9604-P",
            //            "9604-P-SS-1.5-TRM",
            //            "9604-P-TRM",
            //            "9604-P-1.5-TRM",
            //            "9604-P-2.0",
            //            "9604-P-2.0-TRM",
            //            "9604-PLR",
            //            "9604-PLR-B-TRM",
            //            "9604-PLR-SS-TRM",
            //            "9604-PLR-SS-1.5",
            //            "9604-PLR-SS-1.5-TRM",
            //            "9604-PLR-SS-1.5-TRM-STN",
            //            "9604-PLR-SS-2.0-TRM",
            //            "9604-PLR-STN",
            //            "9604-PLR-TRM",
            //            "9604-PLR-TRM-STN",
            //            "9604-PLR-1.5",
            //            "9604-PLR-1.5-STN",
            //            "9604-PLR-1.5-TRM",
            //            "9604-PLR-1.5-TRM-STN",
            //            "9604-PLR-2.0",
            //            "9604-PLR-2.0-STN",
            //            "9604-PLR-2.0-TRM",
            //            "9604-PLR-2.0-TRM-STN",
            //            "9604-PLR-72-TRM",
            //            "9604-PLR-72-2.0-TRM",
            //            "9604-X-P",
            //            "9604-X-P-1.5",
            //            "9604-X-P-2.0",
            //            "9604-X-PLR",
            //            "9604-X-PLR-B",
            //            "9604-X-PLR-B-2.0",
            //            "9604-X-PLR-B-72",
            //            "9604-X-PLR-B-72-1.5",
            //            "9604-X-PLR-B-72-2.0",
            //            "9604-X-PLR-SS",
            //            "9604-X-PLR-SS-1.5",
            //            "9604-X-PLR-SS-2.0-STN",
            //            "9604-X-PLR-STN",
            //            "9604-X-PLR-T724-STN",
            //            "9604-X-PLR-1.5",
            //            "9604-X-PLR-1.5-STN",
            //            "9604-X-PLR-2.0",
            //            "9604-X-PLR-2.0-STN",
            //            "9604-X-PLR-72",
            //            "9604-X-PLR-72-1.5",
            //            "9604CHKSPLRL5",
            //            "9604L7",
            //            "9604PLRL2TRM",
            //            "9604PLRL5",
            //            "9604PLRL5TRM",
            //            "9604PLRL615",
            //            "9604PLRL615TRM",
            //            "9604PLRL615TRMSTN",
            //            "9604PLRL620TRM",
            //            "9604PLRL620TRMSTN",
            //            "9604PLRSS20TRMSTN",
            //            "9604PLRTRMTC",
            //            "9604PLR15TRMTC",
            //            "9604PL5L6TRM",
            //            "9604XPLRL2",
            //            "9604XPLRL5",
            //            "9604XPLRL5L6",
            //            "9604XPLRL5STN",
            //            "9604XPLRL5VP",
            //            "9604XPLRL6",
            //            "9604XPLRL615",
            //            "9604XPLRL620STN",
            //            "9605-CHKS-P-B",
            //            "9605-CHKS-PLR",
            //            "9605-CHKS-PLR-B",
            //            "9605-CHKS-PLR-B-NS",
            //            "9605-CHKS-PLR-B-VP-1.5",
            //            "9605-CHKS-PLR-B-1.5",
            //            "9605-CHKS-PLR-B-1.5-NS",
            //            "9605-CHKS-PLR-NS",
            //            "9605-CHKS-PLR-VP",
            //            "9605-CHKS-PLR-VP-1.5",
            //            "9605-CHKS-PLR-VP-2.0",
            //            "9605-CHKS-PLR-1.5",
            //            "9605-CHKS-PLR-1.5-NS",
            //            "9605-CHKS-PLR-2.0",
            //            "9605-CHKS-PLR-72",
            //            "9605-P",
            //            "9605-P-B-72-1.5-TRM",
            //            "9605-P-NS",
            //            "9605-P-TRM",
            //            "9605-P-1.5",
            //            "9605-P-1.5-NS",
            //            "9605-P-1.5-TRM",
            //            "9605-P-2.0-TRM",
            //            "9605-P-72-TRM",
            //            "9605-P-72-2.0-TRM",
            //            "9605-PLR",
            //            "9605-PLR-B",
            //            "9605-PLR-B-NS",
            //            "9605-PLR-B-1.5-NS",
            //            "9605-PLR-B-1.5-TRM",
            //            "9605-PLR-B-2.0-TRM",
            //            "9605-PLR-B-72-1.5",
            //            "9605-PLR-NS",
            //            "9605-PLR-R-TRM",
            //            "9605-PLR-R-2.0",
            //            "9605-PLR-STN",
            //            "9605-PLR-STN-NS",
            //            "9605-PLR-TRM",
            //            "9605-PLR-TRM-STN",
            //            "9605-PLR-VP-1.5",
            //            "9605-PLR-VP-1.5-TRM",
            //            "9605-PLR-1.5",
            //            "9605-PLR-1.5-NS",
            //            "9605-PLR-1.5-STN",
            //            "9605-PLR-1.5-STN-NS",
            //            "9605-PLR-1.5-TRM",
            //            "9605-PLR-1.5-TRM-STN",
            //            "9605-PLR-2.0",
            //            "9605-PLR-2.0-NS",
            //            "9605-PLR-2.0-STN",
            //            "9605-PLR-2.0-STN-NS"
            //        };

            foreach (string SKUName in SKUs)
            {

                //get pricing data from portal table and put in SalsifyPricing class
                List<Product.SalsifyPricing> pricing = models.GetPricingInfo(SKUName);

                //get pmd data from portal table and put in ProductMaster class
                List<Product.ProductMaster> pmd = models.GetPMDInfo(SKUName);

                if (pmd.Count != 0) //Go to next SKU if not found in DB
                {
                    //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                    //get digital asset data from portal table and put in SalsifyDigitalAsset class
                    //List<Product.SalsifyDigitalAsset> digital = models.GetImageInfo(SKUName); //"1111"

                    //harcode for test
                    //pmd[0].Country_of_Origin = "United test";
                    //pmd[0].GPC_Code = null;
                    pmd[0].Finish = "test";
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
                    pmd[0].Style = GetSalsifyValue("Style", pmd[0].Style);

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
                    //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                    //DefaultDigitalBlankstoNull(digital);
                    DefaultPricingBlankstoNull(pricing);


                    using (var client = new HttpClient())
                    {
                        var responseBody = String.Empty;

                        Product.PMDGroup group = new Product.PMDGroup();
                        group.master = new List<Product.ProductMaster>();
                        group.master.Add(pmd[0]);
                        //******* REMOVED 3/30/2021 from spec, not needed at this time possible later phases *******
                        //if (digital.Count != 0)
                        //{
                        //    group.assets = new List<Product.SalsifyDigitalAsset>();
                        //    group.assets.Add(digital[0]);
                        //}
                        if (pricing.Count != 0)
                        {
                            group.price = new List<Product.SalsifyPricing>();
                            group.price.Add(pricing[0]);
                        }

                        var jsonRequest = JsonConvert.SerializeObject(group); // was pmd[0]

                        jsonRequest = CleanUpRequest(jsonRequest);

                        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                        client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName.Replace("/", "%2F") + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                        //HTTP POST
                        var postTask = client.PutAsync(client.BaseAddress.ToString(), content);
                        postTask.Wait();

                        var result = postTask.Result;
                        var msg = result.RequestMessage;
                        if (result.IsSuccessStatusCode)
                        {
                            //record result in history
                            int recordAffected = models.InsertHistoryRecord("Succeeded", "Update", SKUName, result.StatusCode.ToString(), "");

                            NbrCreated += 1;
                            //ViewData["Message"] = SKUName + " has been updated in Salsify";
                            //return View();
                        }
                        else
                        {
                            NbrFailed += 1;
                            //ViewData["Message"] = "Error Creating Product " + SKUName;
                            //return View();

                            string resultcontent = result.Content.ReadAsStringAsync().Result;
                            myerrors = JsonConvert.DeserializeObject<Unprocessable_Entity>(resultcontent);

                            //convert array of errors to delimited string
                            string myerrorsJoin = string.Join(", ", myerrors.errors);

                            //add error details to exception class to be used in excel email attachment
                            exceptions.Add(new Exceptions() { SKU = SKUName, Run_Result = "Failed", Task = "Update", StatusCode = result.StatusCode.ToString(), Error_Details = myerrorsJoin, Date = DateTime.Now.ToString("MM/dd/yyyy") });

                            //record result in salsify history table
                            int recordAffected = models.InsertHistoryRecord("Failed", "Update", SKUName, result.StatusCode.ToString(), myerrorsJoin);

                        }

                    }

                }
            }

            //ViewData["Message"] = SKUs.Count + " SKUs have been processed by Salsify, " + NbrCreated + " Updated, " + NbrFailed + " Failed";
            string results_Msg = SKUs.Count + " SKUs have been processed by Salsify, " + NbrCreated + " Updated, " + NbrFailed + " Failed";

            if (NbrFailed > 0)
            {
                //call email method
                CreateSendEmail(exceptions, results_Msg, "(Update Multi - Date Range)");

                ViewData["Message"] = results_Msg + " (See exceptions email for error details)";
            }
            else
            {
                ViewData["Message"] = results_Msg;
            }

            return View();

        }

        [HttpPost]
        public ActionResult Delete(string SKUName)
        {
            using (var client = new HttpClient())
            {

                var responseBody = String.Empty;

                var models = new ProductContext();

                client.BaseAddress = new Uri("https://app.salsify.com/api/v1/orgs/s-39696538-9fc5-489d-9d4f-c1d8c28229a7/products/" + SKUName.Replace("/", "%2F") + "?access_token=qbUQS7aKlr0pVruPmYB327gwkCI2xQonShsecISYRN8");

                //HTTP POST
                var postTask = client.DeleteAsync(client.BaseAddress.ToString());
                postTask.Wait();

                var result = postTask.Result;
                var msg = result.RequestMessage;
                if (result.IsSuccessStatusCode)
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Delete", SKUName, result.StatusCode.ToString(), "");

                    ViewData["Message"] = SKUName + " has been deleted in Salsify";
                    return View();
                }
                else
                {
                    //record result in history
                    int recordAffected = models.InsertHistoryRecord("Succeeded", "Delete", SKUName, result.StatusCode.ToString(), "");

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

        public MemoryStream CreateExcelFile(List<Exceptions> exceptions)
        {
            
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    IXLWorksheet worksheet =
                    workbook.Worksheets.Add("Exceptions");
                    worksheet.Cell(1, 1).Value = "SKU";
                    worksheet.Cell(1, 2).Value = "Run Result";
                    worksheet.Cell(1, 3).Value = "Task";
                    worksheet.Cell(1, 4).Value = "Status Code";
                    worksheet.Cell(1, 5).Value = "Error Details";
                    worksheet.Cell(1, 6).Value = "Date";
                    for (int index = 1; index <= exceptions.Count; index++)
                    {
                        worksheet.Cell(index + 1, 1).Value =  exceptions[index - 1].SKU;
                        worksheet.Cell(index + 1, 2).Value =  exceptions[index - 1].Run_Result;
                        worksheet.Cell(index + 1, 3).Value =  exceptions[index - 1].Task;
                        worksheet.Cell(index + 1, 4).Value = exceptions[index - 1].StatusCode;
                        worksheet.Cell(index + 1, 5).Value = exceptions[index - 1].Error_Details;//may need index
                        worksheet.Cell(index + 1, 6).Value = exceptions[index - 1].Date;
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream;
                        //var content = stream.ToArray();

                        //return File(content, contentType, fileName);
                    }
                }
            }
                catch (Exception)
            {
                return null;
            }
        }

        public void CreateSendEmail(List<Exceptions> exceptions, string results_Msg, string title)
        {
            //email
            MimeMessage mmObj = new MimeMessage();

            // set from email address
            MailboxAddress fromEmail = new MailboxAddress(appSettings.Value.FromName, appSettings.Value.FromAddress);
            mmObj.From.Add(fromEmail);


            // set to email address
            MailboxAddress toEmail = new MailboxAddress(appSettings.Value.ToName1, appSettings.Value.ToAddress1);
            mmObj.To.Add(toEmail);

            //may not have more than one To email address
            if (appSettings.Value.ToAddress2 != "")
            {
                MailboxAddress toEmail2 = new MailboxAddress(appSettings.Value.ToName2, appSettings.Value.ToAddress2);
                mmObj.To.Add(toEmail2);
            }

            if (appSettings.Value.ToAddress3 != "")
            {
                MailboxAddress toEmail3 = new MailboxAddress(appSettings.Value.ToName3, appSettings.Value.ToAddress3);
                mmObj.To.Add(toEmail3);
            }

            if (appSettings.Value.ToAddress4 != "")
            {
                MailboxAddress toEmail4 = new MailboxAddress(appSettings.Value.ToName4, appSettings.Value.ToAddress4);
                mmObj.To.Add(toEmail4);
            }

            if (appSettings.Value.ToAddress5 != "")
            {
                MailboxAddress toEmail5 = new MailboxAddress(appSettings.Value.ToName5, appSettings.Value.ToAddress5);
                mmObj.To.Add(toEmail5);
            }

            // Add subject line
            mmObj.Subject = "Salsify API Errors " + title + " " + DateTime.Now.ToString("MM/dd/yyyy");

            // create email body message.
            BodyBuilder emailBody = new BodyBuilder();
            ContentType contentType = new ContentType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "octet-stream");
            string fileName = "SalsifyExceptions.xlsx";
            emailBody.Attachments.Add(fileName, CreateExcelFile(exceptions).ToArray(), contentType);
            //MimeEntity excelAttachment = new MimeEntity(memoryStream, "tempAttachment.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            emailBody.HtmlBody = "<p>Hello,<br><br><p>The following is the Salsify API " + title + " run result summary for " + DateTime.Now.ToString("MM/dd/yyyy") + ", see attachment for error details:<br><br>" + results_Msg + "<br><br><p>Any questions please reply to this email.<br><br><p>Thank you<br><br>";
            //emailBody.HtmlBody = results_Msg; //myerrors.errors[0]; //**** change to success/fail counts
            mmObj.Body = emailBody.ToMessageBody();

            //memoryStream.Flush();

            // create SmtpClient object
            SmtpClient smtpClient = new SmtpClient();
            //smtpClient.CheckCertificateRevocation = false;
            smtpClient.Connect(appSettings.Value.SMTPServer, 0, MailKit.Security.SecureSocketOptions.Auto);
            smtpClient.Authenticate(appSettings.Value.SMTPUser, appSettings.Value.SMTPPW);
            smtpClient.Send(mmObj);
            smtpClient.Disconnect(true);
            smtpClient.Dispose();
        }

        private static Regex _cleanupRegex = new Regex("[^a-zA-Z0-9-_/.]");//remove special chars except -_/.
    }
}
//[a-zA-Z0-9_] // only alphanumeric