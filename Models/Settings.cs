using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SalsifyApp.Models
{
    public class Settings
    {
        public string SMTPServer { get; set; }
        public string SMTPUser { get; set; }
        public string SMTPPW { get; set; }
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string ToName1 { get; set; }
        public string ToAddress1 { get; set; }
        public string ToName2 { get; set; }
        public string ToAddress2 { get; set; }
        public string ToName3 { get; set; }
        public string ToAddress3 { get; set; }
        public string ToName4 { get; set; }
        public string ToAddress4 { get; set; }
        public string ToName5 { get; set; }
        public string ToAddress5 { get; set; }
    }
}
