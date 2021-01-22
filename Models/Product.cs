using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Models
{
    public class Product
    {
        public string apiResponse { get; set; }

        public class Coord
        {
            public double lon { get; set; }
            public double lat { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        public class Main
        {
            public double temp { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
            public double temp_min { get; set; }
            public double temp_max { get; set; }
        }

        public class Wind
        {
            public double speed { get; set; }
            public int deg { get; set; }
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public double message { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }

        public class ResponseWeather
        {
            public Coord coord { get; set; }
            public List<Weather> weather { get; set; }
            public string @base { get; set; }
            public Main main { get; set; }
            public int visibility { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }
        }
        public string Sku { get; set; }

        public List<SelectListItem> Skus { get; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "0171-3DTP", Text = "0171-3DTP" },
            new SelectListItem { Value = "CA", Text = "Canada" },
            new SelectListItem { Value = "US", Text = "USA"  },
        };

 
        public class SalsifyPricing
        {
            //[JsonProperty("Part Number")]
            //public string PartNumber { get; set; }
            ////[JsonProperty("")]
            //public int CountryCode { get; set; }
            [JsonProperty("List Price (MSRP)")]
            public Decimal ListPrice { get; set; }
            //[JsonProperty("MAP Price")]
            //public Decimal MAPPrice { get; set; }
            //[JsonProperty("Wholesale Cost")]
            //public Decimal WholesalePrice { get; set; }
        }
        public class ProductMaster
        {
            [JsonProperty("Part Number")]
            public string Part_Number { get; set; }
            [JsonProperty("Abrasion Resistant")]
            public string Abrasion_Resistant { get; set; }
            [JsonProperty("Active Status")]
            public string Active_Status { get; set; }
            [JsonProperty("ADA Compliant")]
            public string ADA_Compliant { get; set; }
            [JsonProperty("ASIN")]
            public string ASIN { get; set; }
            [JsonProperty("Available on Ecommerce")]
            public string Available_on_Ecommerce { get; set; }
            [JsonProperty("Barcode Number")]
            public string Barcode_Number { get; set; }
            [JsonProperty("Base GTIN")]
            public string Base_GTIN { get; set; }
            [JsonProperty("Brief Product Description")]
            public string Brief_Product_Description { get; set; }
            [JsonProperty("Buy Pack")]
            public int? Buy_Pack { get; set; }
            [JsonProperty("Buy Pack Height (in)")]
            public Decimal? Buy_Pack_Height_in { get; set; }
            [JsonProperty("Buy Pack Length (in)")]
            public Decimal? Buy_Pack_Length_in { get; set; }
            [JsonProperty("Buy Pack Weight (lb)")]
            public Decimal? Buy_Pack_Weight_lb { get; set; }
            [JsonProperty("Buy Pack Width (in)")]
            public Decimal? Buy_Pack_Width_in { get; set; }
            [JsonProperty("Cal Green Compliant")]
            public string Cal_Green_Compliant { get; set; }
            [JsonProperty("CEC Listed")]
            public string CEC_Listed { get; set; }
            [JsonProperty("Certifier")]
            public string Certifier { get; set; }
            [JsonProperty("Code")]
            public string Code { get; set; }
            [JsonProperty("Collection")]
            public string Collection { get; set; }
            [JsonProperty("Construction Material")]
            public string Construction_Material { get; set; }
            [JsonProperty("Corrosion Resistant")]
            public string Corrosion_Resistant { get; set; }
            [JsonProperty("Country of Origin")]
            public string Country_of_Origin { get; set; }
            [JsonProperty("Cover Plate Included")]
            public string Cover_Plate_Included { get; set; }
            [JsonProperty("CSA Certified")]
            public string CSA_Certified { get; set; }
            [JsonProperty("cUPC Certified")]
            public string cUPC_Certified { get; set; }
            [JsonProperty("Diverter Included")]
            public string Diverter_Included { get; set; }
            [JsonProperty("Diverter Location")]
            public string Diverter_Location { get; set; }
            [JsonProperty("Drain Included")]
            public string Drain_Included { get; set; }
            [JsonProperty("Escutcheon Width (in)")]
            public Decimal? Escutcheon_Width_in { get; set; }
            [JsonProperty("Feature Bullet 1")]
            public string Feature_Bullet_1 { get; set; }
            [JsonProperty("Feature Bullet 10")]
            public string Feature_Bullet_10 { get; set; }
            [JsonProperty("Feature Bullet 11")]
            public string Feature_Bullet_11 { get; set; }
            [JsonProperty("Feature Bullet 12")]
            public string Feature_Bullet_12 { get; set; }
            [JsonProperty("Feature Bullet 2")]
            public string Feature_Bullet_2 { get; set; }
            [JsonProperty("Feature Bullet 3")]
            public string Feature_Bullet_3 { get; set; }
            [JsonProperty("Feature Bullet 4")]
            public string Feature_Bullet_4 { get; set; }
            [JsonProperty("Feature Bullet 5")]
            public string Feature_Bullet_5 { get; set; }
            [JsonProperty("Feature Bullet 6")]
            public string Feature_Bullet_6 { get; set; }
            [JsonProperty("Feature Bullet 7")]
            public string Feature_Bullet_7 { get; set; }
            [JsonProperty("Feature Bullet 8")]
            public string Feature_Bullet_8 { get; set; }
            [JsonProperty("Feature Bullet 9")]
            public string Feature_Bullet_9 { get; set; }
            [JsonProperty("Finish")]
            public string Finish { get; set; }
            [JsonProperty("Flow Rate GPM")]
            public string Flow_Rate_GPM { get; set; }
            [JsonProperty("GPC Code")]
            public string GPC_Code { get; set; }
            [JsonProperty("GTIN Buy Pack")]
            public string GTIN_Buy_Pack { get; set; }
            [JsonProperty("Hand Shower Face Diameter (in)")]
            public Decimal? Hand_Shower_Face_Diameter_in { get; set; }
            [JsonProperty("Hand Shower Included")]
            public string Hand_Shower_Included { get; set; }
            [JsonProperty("Handle Material")]
            public string Handle_Material { get; set; }
            [JsonProperty("Handle Style")]
            public string Handle_Style { get; set; }
            [JsonProperty("Harmonized Code")]
            public string Harmonized_Code { get; set; }
            [JsonProperty("Holder Type")]
            public string Holder_Type { get; set; }
            [JsonProperty("Houzz Product ID")]
            public string Houzz_Product_ID { get; set; }
            //[JsonProperty("Included Components")]
            //public string Included_Components { get; set; }
            //[JsonProperty("Installation / Mount Type")]
            //public string Installation_Mount_Type { get; set; }
            [JsonProperty("Item Height (in)")]
            public Decimal? Item_Height_in { get; set; }
            [JsonProperty("Item Length (in)")]
            public Decimal? Item_Length_in { get; set; }
            [JsonProperty("Item Weight (lb)")]
            public Decimal? Item_Weight_lb { get; set; }
            [JsonProperty("Item Width (in)")]
            public Decimal? Item_Width_in { get; set; }
            [JsonProperty("Keyword")]
            public String Keyword { get; set; }
            [JsonProperty("Lead Time (Days)")]
            public int? Lead_Time_Days { get; set; }
            [JsonProperty("Low Lead")]
            public string Low_Lead { get; set; }
            [JsonProperty("Lowe's Item #")]
            public string Lowes_Item_Nbr { get; set; }
            [JsonProperty("Marketing Copy")]
            public String Marketing_Copy { get; set; }
            [JsonProperty("Master Height (in)")]
            public Decimal? Master_Height_in { get; set; }
            [JsonProperty("Master Length (in)")]
            public Decimal? Master_Length_in { get; set; }
            [JsonProperty("Master Pack Barcode")]
            public String Master_Pack_Barcode { get; set; }
            [JsonProperty("Master Pack GTIN")]
            public String Master_Pack_GTIN { get; set; }
            [JsonProperty("Master Packs Per Pallet")]
            public int? Master_Packs_Per_Pallet { get; set; }
            [JsonProperty("Master Packs Per Tier")]
            public int? Master_Packs_Per_Tier { get; set; }
            [JsonProperty("Master Weight (lb)")]
            public Decimal? Master_Weight_lb { get; set; }
            [JsonProperty("Master Width (in)")]
            public Decimal? Master_Width_in { get; set; }
            [JsonProperty("Maximum Operating Pressure (psi)")]
            public Decimal? Maximum_Operating_Pressure_psi { get; set; }
            [JsonProperty("Maximum Temperature")]
            public Decimal? Maximum_Temperature { get; set; }
            [JsonProperty("Nested")]
            public string Nested { get; set; }
            [JsonProperty("Number of Functions on Showerhead")]
            public string Number_of_Functions_on_Showerhead { get; set; }
            [JsonProperty("Number of Handles")]
            public string Number_of_Handles { get; set; }
            [JsonProperty("Number of Holes For Installation")]
            public string Number_of_Holes_For_Installation { get; set; }
            [JsonProperty("Number of Inlets")]
            public string Number_of_Inlets { get; set; }
            [JsonProperty("Number of Outlets")]
            public string Number_of_Outlets { get; set; }
            [JsonProperty("OMSID")]
            public string OMSID { get; set; }
            [JsonProperty("Packaging Material")]
            public String Packaging_Material { get; set; }
            [JsonProperty("Pallet Barcode")]
            public String Pallet_Barcode { get; set; }
            [JsonProperty("Pallet GTIN")]
            public String Pallet_GTIN { get; set; }
            [JsonProperty("Pallet Height (in)")]
            public Decimal? Pallet_Height_in { get; set; }
            [JsonProperty("Pallet Length (in)")]
            public Decimal? Pallet_Length_in { get; set; }
            [JsonProperty("Pallet Weight (lb)")]
            public Decimal? Pallet_Weight_lb { get; set; }
            [JsonProperty("Pallet Width (in)")]
            public Decimal? Pallet_Width_in { get; set; }
            [JsonProperty("Pressure Balance/Anti-Scald")]
            public string Pressure_Balance_Anti_Scald { get; set; }
            [JsonProperty("Product Name")]
            public String Product_Name { get; set; }
            [JsonProperty("Pull Out Extension")]
            public string Pull_Out_Extension { get; set; }
            [JsonProperty("Pull Out Spray")]
            public string Pull_Out_Spray { get; set; }
            [JsonProperty("Quantity Per Master Pack")]
            public int? Quantity_Per_Master_Pack { get; set; }
            [JsonProperty("Restricted States")]
            public String Restricted_States { get; set; }
            [JsonProperty("Sell Pack")]
            public int? Sell_Pack { get; set; }
            [JsonProperty("Ship From State")]
            public string Ship_From_State { get; set; }
            [JsonProperty("Shipping Height (in)")]
            public Decimal? Shipping_Height_in { get; set; }
            [JsonProperty("Shipping Length (in)")]
            public Decimal? Shipping_Length_in { get; set; }
            [JsonProperty("Shipping Weight (lb)")]
            public Decimal? Shipping_Weight_lb { get; set; }
            [JsonProperty("Shipping Width (in)")]
            public Decimal? Shipping_Width_in { get; set; }
            [JsonProperty("Showerhead Face Diameter (in)")]
            public Decimal? Showerhead_Face_Diameter_in { get; set; }
            [JsonProperty("Spout Height (in)")]
            public Decimal? Spout_Height_in { get; set; }
            [JsonProperty("Spout Reach (in)")]
            public Decimal? Spout_Reach_in { get; set; }
            [JsonProperty("Spout Type")]
            public String Spout_Type { get; set; }
            [JsonProperty("Stackable")]
            public string Stackable { get; set; }
            [JsonProperty("Theme")]
            public String Style { get; set; }
            [JsonProperty("Tarnish Resistant")]
            public string Tarnish_Resistant { get; set; }
            [JsonProperty("THD Category")]
            public String THD_Category { get; set; }
            [JsonProperty("THD MFG Part #")]
            public String THD_MFG_Part_Nbr { get; set; }
            [JsonProperty("The Buy American Act")]
            public string The_Buy_American_Act { get; set; }
            [JsonProperty("New Tier")]
            public String Tier { get; set; }
            [JsonProperty("Tiers Per Pallet")]
            public int? Tiers_Per_Pallet { get; set; }
            [JsonProperty("Total Units Per Pallet")]
            public int? Total_Units_Per_Pallet { get; set; }
            [JsonProperty("Type of Connection")]
            public string Type_of_Connection { get; set; }
            [JsonProperty("Units Per Pallet Tier")]
            public int? Units_Per_Pallet_Tier { get; set; }
            [JsonProperty("UNSPSC Code")]
            public string UNSPSC_Code { get; set; }
            [JsonProperty("UPC Code")]
            public string UPC_Code { get; set; }
            [JsonProperty("Valve Style")]
            public String Valve_Style { get; set; }
            [JsonProperty("Valve System Type")]
            public String Valve_System_Type { get; set; }
            [JsonProperty("Vandal Resistant")]
            public string Vandal_Resistant { get; set; }
            [JsonProperty("Variation")]
            public String Variation { get; set; }
            [JsonProperty("Walmart Item ID")]
            public string Walmart_Item_ID { get; set; }
            [JsonProperty("Walmart Item Number")]
            public string Walmart_Item_Number { get; set; }
            [JsonProperty("WaterSense Certified")]
            public string WaterSense_Certified { get; set; }
            [JsonProperty("Wayfair SKU")]
            public String Wayfair_SKU { get; set; }
            //[JsonProperty("List Price (MSRP)")]
            //public string ListPrice { get; set; }
            [JsonProperty("CA List Price (MSRP)")]
            public string CAListPrice { get; set; }
            //[JsonProperty("MAP Price")]
            //public string MAPPrice { get; set; }
            //[JsonProperty("Wholesale Cost")]
            //public string WholesalePrice { get; set; }
            //public SalsifyDigitalAsset digital { get; set; }
            //public SalsifyPricing pricing { get; set; }
            //[JsonProperty("Main Image")]
            //public string MainImage { get; set; }
            //public DateTime Date_Added { get; set; }
            //public DateTime Date_Updated { get; set; }
            [JsonProperty("Ready for Website")]
            public string ReadyWebSite { get; set; }
            [JsonProperty("Special Attribute: Assist Products")]
            public string AssistProduct { get; set; }
            [JsonProperty("Special Attribute: Lead Free")]
            public string LeadFree { get; set; }
            [JsonProperty("Product Category")]
            public string Product_Category { get; set; }
        }

        public class SalsifyDigitalAsset
        {

            //[JsonProperty("Part Number")]
            //public string PartNumber { get; set; }
            [JsonProperty("Main Image")]
            public string MainImage_ID { get; set; }
            [JsonProperty("Lifestyle Image 1")]
            public string LifestyleImage1_ID { get; set; }
            [JsonProperty("Lifestyle Image 2")]
            public string LifestyleImage2_ID { get; set; }
            [JsonProperty("Lifestyle Image 3")]
            public string LifestyleImage3_ID { get; set; }
            [JsonProperty("Lifestyle Image 4")]
            public string LifestyleImage4_ID { get; set; }
            [JsonProperty("Lifestyle Image 5")]
            public string LifestyleImage5_ID { get; set; }
            [JsonProperty("Lifestyle Image 6")]
            public string LifestyleImage6_ID { get; set; }
            [JsonProperty("Lifestyle Image 7")]
            public string LifestyleImage7_ID { get; set; }
            [JsonProperty("Silo Image 1")]
            public string SiloImage1_ID { get; set; }
            [JsonProperty("Silo Image 2")]
            public string SiloImage2_ID { get; set; }
            [JsonProperty("Silo Image 3")]
            public string SiloImage3_ID { get; set; }
            [JsonProperty("Silo Image 4")]
            public string SiloImage4_ID { get; set; }
            [JsonProperty("Silo Image 5")]
            public string SiloImage5_ID { get; set; }
            [JsonProperty("Dimensions Graphic")]
            public string DimensionsGraphic_ID { get; set; }
            [JsonProperty("Features Graphic")]
            public string FeaturesGraphic_ID { get; set; }
            [JsonProperty("Inline Product Features Content (Home Depot Exclusive)")]
            public string InlineProductFeaturesContent_ID { get; set; }
            [JsonProperty("Spec Sheet")]
            public string SpecSheet_ID { get; set; }
            //public string SpecSheet2_ID { get; set; }
            //public string SpecSheet3_ID { get; set; }
            [JsonProperty("Installation Guide")]
            public string InstallationGuide_ID { get; set; }
            [JsonProperty("Swatch")]
            public string Swatch_ID { get; set; }
            [JsonProperty("Product Video")]
            public string ProductVideo_ID { get; set; }
            [JsonProperty("Testimonial Video")]
            public string TestimonialVideo_ID { get; set; }
            [JsonProperty("Cleaning & Care")]
            public string CleaningCare_ID { get; set; }
            [JsonProperty("Warranty")]
            public string Warranty_ID { get; set; }
            [JsonProperty("Logo")]
            public string Logo_ID { get; set; }
            [JsonProperty("Prop 65 Warning")]
            public string Prop65Warning_ID { get; set; }
            //public string CNVDiscontinuedUrl_ID { get; set; }
            //public string CNVDocuments1_ID { get; set; }
            //public string CNVDocuments2_ID { get; set; }
            //public string CNVDocuments3_ID { get; set; }
            //public string CNVImages1_ID { get; set; }
            //public string CNVImages2_ID { get; set; }
            //public string CNVImages3_ID { get; set; }
            //public string CNVImages4_ID { get; set; }
            //public Decimal DateUpdated { get; set; }
            //public Decimal TmeOfDay { get; set; }
        }

        public class PMDGroup
        {
            public List<ProductMaster> master { get; set; }

            public List<SalsifyDigitalAsset> assets { get; set; }

            public List<SalsifyPricing> price { get; set; }
        }

}

}

