using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Models
{
    //Errors returned from Salsify
    public class Error
    {
        public class Unprocessable_Entity
        {
            public string[] errors { get; set; }
        }
    }

    //Array of errors to build excel file with
    public class Exceptions
    {
        public string SKU { get; set; }
        public string Run_Result { get; set; }
        public string Task { get; set; }
        public string StatusCode { get; set; }
        public string Error_Details { get; set; }
        public string Date { get; set; }
    }

}
