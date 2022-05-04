using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Apim.WebClient.Controllers
{
    public class ApiRestMgmntController : Controller
    {

        public string id = "integration";
        // key - either the primary or secondary key from that same tab.
        public string key = "";
        public static string baseUrl;
        public string serviceName = "apiedmond";
        public static string sharedAccessSignature = "uid=...&ex=...";
        public string subscriptionId = "";
        public string resourceGroupName = "API-TEST-RG";
        public string apiVersion = "2021-08-01";
        public DateTime expiry = DateTime.UtcNow.AddDays(1);
        private readonly IConfiguration _configuration;


        public ApiRestMgmntController(IConfiguration Configuration)
        {
            _configuration = Configuration;
            key= _configuration["key"];
            serviceName = _configuration["serviceName"];
            subscriptionId = _configuration["subscriptionId"];
            apiVersion = _configuration["api-version"];
            resourceGroupName= _configuration["resourceGroupName"];


            baseUrl = string.Format("https://{0}.management.azure-api.net:3443", serviceName);
            sharedAccessSignature = CreateSharedAccessToken(id, key, expiry);
        }
        public IActionResult Index()
        {
             return View("ListApis");
        }

        public IActionResult ListDetailAPIs()
        {
            return View("ListDetailAPIs");
        }

        public IActionResult ApiUsingSwaggerImport()
        {

var bodyJson =         @"{
            ""properties"":{
            ""format"": ""swagger-link-json"",
            ""value"" : ""http://petstore.swagger.io/v2/swagger.json"",
            ""path""  : ""petstore""
                          }
                        }";

            ViewBag.bodyJson = bodyJson;
            ViewBag.serviceName = serviceName;
            ViewBag.apiversion= apiVersion;
            ViewBag.resourceGroupName = resourceGroupName;
            return View("ApiUsingSwaggerImport");
        }


        public IActionResult ApiUsingOai3Import()
        {

            var bodyJson = @"{
            ""properties"":{
            ""format"": ""openapi-link"",
            ""value"" : ""https://raw.githubusercontent.com/OAI/OpenAPI-Specification/master/examples/v3.0/petstore.yaml"",
            ""path""  : ""petstore""
                          }
                        }";

            ViewBag.bodyJson = bodyJson;
            ViewBag.serviceName = serviceName;
            ViewBag.apiversion = apiVersion;
            ViewBag.resourceGroupName = resourceGroupName;
            return View("ApiUsingOai3Import");
        }

        public IActionResult SoapToRestApiUsingWsdlImport()
        {

            var bodyJson = @"{
              ""properties"": {
                ""format"": ""wsdl-link"",
                ""value"": ""https://www.dataaccess.com/webservicesserver/numberconversion.wso?WSDL"",
                ""path"": ""currency"",
                ""wsdlSelector"": {
                  ""wsdlServiceName"": ""NumberConversion"",
                  ""wsdlEndpointName"": ""NumberConversionSoap""
                }
              }
            }  ";

            ViewBag.bodyJson = bodyJson;
            ViewBag.serviceName = serviceName;
            ViewBag.apiversion = apiVersion;
            ViewBag.resourceGroupName = resourceGroupName;
            return View("SoapToRestApiUsingWsdlImport");
        }

        private string CreateSharedAccessToken(string id, string key, DateTime expiry)
        {
            using (var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                string dataToSign = id + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                string x = string.Format("{0}\n{1}", id, expiry.ToString("O", CultureInfo.InvariantCulture));
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var signature = Convert.ToBase64String(hash);
                string encodedToken = string.Format("uid={0}&ex={1:o}&sn={2}", id, expiry, signature);
                return encodedToken;
            }
        }

        public async Task<IActionResult> GetRestAPIs()
        {
            string apis = GetAPIs().Result;
            return Json(new { result = apis });
        }


        public async Task<IActionResult> GetRestDetailAPIs()
        {
            var response = "";
            // Get a list of all APIs - GET /apis
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            string apis = GetAPIs().Result;
            List<string> ListDetailApis = new List<string>();
            // Parse the APIs JSON result and display information about the returned APIs.
            JObject o = JObject.Parse(apis);

            // How many apis are returned?
            int count = (int)o["value"].Count<object>();
            Console.WriteLine("APIs: {0}", count);

            // Display the results of the call to GetAPIs
            Console.WriteLine(FormatJSON(apis));

            // Retrieve information about each API. There is overlap between these results
            // and the results of the call to GetAPIs; we show both here for demo purposes.
            for (int i = 0; i < count; i++)
            {
                // This returns the format /apis/2bc3baae-1bfc-4c0e-a1ab-7e76c88dfa79
                string id = (string)o["value"][i]["id"];

                // Get just the guid part, used for subsequent calls.
                string apiId = id.Substring(id.Length - 24);
                apiId = (string)o["value"][i]["name"];

                // Gets the details of a specific API - GET /apis/{apiId}
                // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#GetAPI
                // Note that this information is also present in GET /apis
                // but for demonstration purposes we call both.
                string api = GetAPI(apiId).Result;
                Console.WriteLine("API {0} - {1}:", apiId, o["value"][i]["name"]);
                Console.WriteLine(FormatJSON(api));

                // Gets the details of a specific API - GET /apis/{apiId}
                // with export=true to include information about the operations.
                // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#GetAPI
                // Note that this information is also present in GET /apis
                // but for demonstration purposes we call both.
                api = GetAPIExportJson(apiId).Result;
                Console.WriteLine("API with exported JSON:");
                Console.WriteLine(FormatJSON(api));
                response = FormatJSON(api);
                ListDetailApis.Add(response);
            }
            //var data = JsonDocument.Parse(response);
            //return Json(new { response = data });
            //return View();
            return Json(new { result = ListDetailApis });
        }
        public  async Task<string> GetAPIs()
        {
            // Call the GET /apis operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            var url = baseUrl;
            string responseBody;
            string requestUrl  = baseUrl +"/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis?api-version=2021-08-01";
            //requestUrl = string.Concat(baseUrl, "/apis", "?api-version=", apiVersion);
           
            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Default media type for this operation is application/json, no need to
                // set the accept header.

                // Set the SharedAccessSignature header
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("SharedAccessSignature", sharedAccessSignature);

                // Make the request
                HttpResponseMessage response = await httpClient.SendAsync(request);

                // Throw if there is an error
                response.EnsureSuccessStatusCode();

               responseBody = await response.Content.ReadAsStringAsync();

                //return responseBody;
            }
            return responseBody;
        }

       public  async Task<string> GetAPI(string apiId)
        {
            // Call the GET /apis/{apiId} operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#GetAPI
           // string requestUrl = string.Format("{0}/apis/{1}?api-version={2}", baseUrl, apiId, apiVersion);
            // requestUrl = "https://apiedmond.management.azure-api.net:3443/apis/apiedmond/apis/echo-api?api-version=2021-08-01";
            
            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis/"+ apiId + "?api-version=2021-08-01";

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Set the SharedAccessSignature header
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("SharedAccessSignature", sharedAccessSignature);

                // Content Type header not required since default is application/json, no need to
                // set accept header

                // Make the request
                HttpResponseMessage response = await httpClient.SendAsync(request);

                // Throw if there is an error
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }
        // Format the JSON into an indented multiple line
        // format for display.
        private static string FormatJSON(string json)
        {
            dynamic parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }

      public async Task<string> GetAPIExportJson(string apiId)
        {
            // Call the GET /apis/{apiId} operation with export=true
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            //string requestUrl = string.Format("{0}/apis/{1}?api-version={2}&export=true", baseUrl, apiId, apiVersion);
            //GET https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ApiManagement/service/{serviceName}/apis/{apiId}?format={format}&export=true&api-version=2021-08-01

            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis/" + apiId + "?format=json&export=true&api-version=2021-08-01";
            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                // Set the SharedAccessSignature header
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("SharedAccessSignature", sharedAccessSignature);

                // Content Type header is required when export=true
                // See GET /apis/{apiId} documentation for export type values.
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Make the request
                HttpResponseMessage response = await httpClient.SendAsync(request);

                // Throw if there is an error
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;
            }
        }




        //***********************************************************
        public async Task<string> CloneAPIs()
        {
            // Call the GET /apis operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            var url = baseUrl;
            string responseBody;
            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis/echo-clone?api-version=2021-08-01";




var bodyjson = @" {
    ""properties"": {
    ""displayName"": ""Echo API3"",
    ""description"": ""Copy of Existing Echo Api including Operations."",
    ""subscriptionRequired"": true,
    ""serviceUrl"": ""http://echoapi.cloudapp.net/api"",
    ""path"": ""echo2"",
    ""protocols"": [
      ""http"",
      ""https""
    ],
    ""isCurrent"": true,
    ""sourceApiId"": ""/subscriptions/6fba467a-c738-4c66-9590-4021ecff8d3c/resourceGroups/API-TEST-RG/providers/Microsoft.ApiManagement/service/apiedmond/apis/echo-clone""
  }
}      ";




          var response = await SendRequest(HttpMethod.Put, requestUrl, sharedAccessSignature, bodyjson);
            // Throw if there is an error
            response.EnsureSuccessStatusCode();

            responseBody = await response.Content.ReadAsStringAsync();


            return responseBody;
        }


        public async Task<IActionResult> CreateApiUsingSwaggerImport(Models.ApiParametersModel apiParameters)
        {
            // Call the GET /apis operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            var url = baseUrl;
            string responseBody;
           // string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis/petstore?api-version=2021-08-01";
            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + apiParameters.resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + apiParameters.serviceName + "/apis/"+ apiParameters.apiName + "?api-version=2021-08-01";



            //var bodyjson = @" {
            //  ""properties"": {
            //    ""format"": ""swagger-link-json"",
            //    ""value"": ""http://petstore.swagger.io/v2/swagger.json"",
            //    ""path"": ""petstore""
            //  }
            //}  ";
            var bodyjson = apiParameters.bodyJson;

            var response = await SendRequest(HttpMethod.Put, requestUrl, sharedAccessSignature, bodyjson);
            // Throw if there is an error
            //var rep= response.EnsureSuccessStatusCode();
            // var statusCode = response.EnsureSuccessStatusCode().StatusCode;
            // responseBody = await response.Content.ReadAsStringAsync();
            var statusCode = response.StatusCode;
            var ReasonPhrase = response.ReasonPhrase;
            return Json(new { statusCode = statusCode , ReasonPhrase = ReasonPhrase });
          
        }


        public async Task<IActionResult> CreateApiUsingOai3Import(Models.ApiParametersModel apiParameters)
        {
            // Call the GET /apis operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            var url = baseUrl;
            string responseBody;
            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + apiParameters.resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + apiParameters.serviceName + "/apis/" + apiParameters.apiName + "?api-version=2021-08-01";
            var bodyjson = apiParameters.bodyJson;

            var response = await SendRequest(HttpMethod.Put, requestUrl, sharedAccessSignature, bodyjson);
            // Throw if there is an error
            var statusCode = response.StatusCode;
            var ReasonPhrase = response.ReasonPhrase;
            return Json(new { statusCode = statusCode, ReasonPhrase = ReasonPhrase });

        }

        public async Task<IActionResult> CreateSoapToRestApiUsingWsdlImport(Models.ApiParametersModel apiParameters)
        {
            // Call the GET /apis operation
            // https://msdn.microsoft.com/en-us/library/azure/dn781423.aspx#ListAPIs
            var url = baseUrl;
            string responseBody;
            string requestUrl = baseUrl + "/subscriptions/" + subscriptionId + "/resourceGroups/" + apiParameters.resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + apiParameters.serviceName + "/apis/" + apiParameters.apiName + "?api-version=2021-08-01";
            var bodyjson = apiParameters.bodyJson;

            var response = await SendRequest(HttpMethod.Put, requestUrl, sharedAccessSignature, bodyjson);
            // Throw if there is an error
            var statusCode = response.StatusCode;
            var ReasonPhrase = response.ReasonPhrase;
            return Json(new { statusCode = statusCode, ReasonPhrase = ReasonPhrase });
        }

        public static async Task<HttpResponseMessage> SendRequest(HttpMethod method, string endPoint, string accessToken, dynamic content = null)
        {
            HttpResponseMessage response = null;
            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(method, endPoint))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("SharedAccessSignature", accessToken);
                    if (content != null)
                    {
                        string c;
                        if (content is string)
                            c = content;
                        else
                            c = JsonConvert.SerializeObject(content);
                        request.Content = new StringContent(c, Encoding.UTF8, "application/json");
                    }

                    response = await client.SendAsync(request).ConfigureAwait(false);
                }
            }
            return response;

        }

        //***********************************************************
        public async Task<IActionResult> TryApiSend(Models.ApiViewModel api)
        {

        
            return View();

        }
    }
}
