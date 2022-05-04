using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apim.WebClient.Models
{
    public class ApiParametersModel
    {
        public string serviceName { get; set; }
        public string resourceGroupName { get; set; }
        public string apiVersion { get; set; }
        public string apiId { get; set; }
        public string apiName { get; set; }
        public string bodyJson { get; set; }
    
        
    }
}
