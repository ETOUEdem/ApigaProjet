
using Apim.WebClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Apim.WebClient.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private IHttpContextAccessor _httpContextAccessor;
        private X509Certificate2 cert;
       
        public HomeController(ILogger<HomeController> logger, IConfiguration Configuration, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _configuration = Configuration;
            _httpContextAccessor = httpContextAccessor;
           
        }

        public async Task<IActionResult> Index()
        {
       
            return View();
        }


        public async Task<IActionResult> Logout()
        {
            //*******************
            var authority = _configuration["Authority"];
            var url = authority + "/protocol/openid-connect/logout";
            //****************

            var currentContext = _httpContextAccessor.HttpContext;
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id",_configuration["ClientId"] },
                { "client_secret", _configuration["ClientSecret"]},
                {"refresh_token", refreshToken }
            };
            DeleteCookies();
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
            var statusCode = response.EnsureSuccessStatusCode().StatusCode;
            return View();
        }
        private void DeleteCookies()
        {
            foreach (var cookie in HttpContext.Request.Cookies)
            {
               if(cookie.Key== ".AspNetCore.Cookies")
                {
                    Response.Cookies.Delete(cookie.Key);
                }
                
            }
        }

    

        public async Task<bool>  isExpiredToken(int maxTime)
        {
            bool isExpired = false;
            var currentContext = _httpContextAccessor.HttpContext;
            var expires_at = await currentContext.GetTokenAsync("expires_at");
            if (string.IsNullOrWhiteSpace(expires_at)
           || ((DateTime.Parse(expires_at).AddSeconds(-maxTime)).ToUniversalTime()
           < DateTime.UtcNow))
            {
                isExpired = true;
            }
                return isExpired;
        }

        //[Authorize]
        public async Task<IActionResult> TryApi()
        {

            /*   
              if (await isExpiredToken(30))
               {
                   await RenewTokens();
               }


              string accToken = HttpContext.GetTokenAsync("access_token").Result;
               string refresh_token = HttpContext.GetTokenAsync("refresh_token").Result;
               string idToken = HttpContext.GetTokenAsync("id_token").Result;
               var expires_at = HttpContext.GetTokenAsync("expires_at").Result;
               var date = (DateTime.Parse(expires_at));
               ViewBag.Expires = date.ToLongTimeString();
               ViewBag.token = accToken;
               ViewBag.idToken = idToken;
               ViewBag.refresh_token = refresh_token;*/
            ViewBag.cert = GetCertificate();

            return View();
        }

        public async Task<IActionResult> revoke(Models.ApiViewModel api)
        {
            var authority = _configuration["Authority"];
            var url = authority + "/protocol/openid-connect/revoke";
            //****************

            var currentContext = _httpContextAccessor.HttpContext;
            var Token = await HttpContext.GetTokenAsync("access_token");
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id",_configuration["ClientId"] },
                { "client_secret", _configuration["ClientSecret"] },
                { "grant_type", "refresh_token" },
                //{ "scope", "xxxx" },
                {"token", Token }
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
           var statusCode=    response.EnsureSuccessStatusCode().StatusCode;
            var infoMessage = "";
            if (statusCode.ToString() == "OK")
            {
                infoMessage = "Le token a été revoqué avec success";
            }
         
            return Json(new { infoMessage = infoMessage });
        }

        public async Task<AccessToken> RenewTokens()
        {
            //*******************
            var authority = _configuration["Authority"];
            var url = authority + "/protocol/openid-connect/token";
            //****************

            var currentContext = _httpContextAccessor.HttpContext;
            var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id",_configuration["ClientId"] },
                { "client_secret", _configuration["ClientSecret"]},
                { "grant_type", "refresh_token" },
                //{ "scope", "xxxx" },
                {"refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
            var jsonContent = await response.Content.ReadAsStringAsync();
            AccessToken tok = JsonConvert.DeserializeObject<AccessToken>(jsonContent);

            string access_token = tok.access_token;
            var ExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(tok.expires_in);
            var authenticationInfo = await currentContext.AuthenticateAsync("Cookies");
            authenticationInfo.Properties.UpdateTokenValue("expires_at", ExpiresAt.ToString("o", CultureInfo.InvariantCulture));
            authenticationInfo.Properties.UpdateTokenValue("access_token", tok.access_token);
            authenticationInfo.Properties.UpdateTokenValue("refresh_token", tok.refresh_token);
            await currentContext.SignInAsync("Cookies", authenticationInfo.Principal, authenticationInfo.Properties);
            return tok;
        }


        public async Task<IActionResult> TryApiSend(Models.ApiViewModel api)
        {

            var cert = GetCertificate();
            var handler = new HttpClientHandler();
            if (cert != null){
                handler.ClientCertificates.Add(cert);
            }

            var result = "";
            var url = api.Url;
            var errorMessage = "";
            dynamic statusCode = "";
            dynamic data = "";
            try {
                var httpClient = new HttpClient(handler);

                //************************************
                var defaultRequetHeaders = httpClient.DefaultRequestHeaders;
                if (defaultRequetHeaders.Accept == null || !defaultRequetHeaders.Accept.Any(m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                }
                //********************************
                //httpClient.DefaultRequestHeaders.Add("Authorization", String.Format("Bearer {0}", api.Token));
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + api.Token);


                if (!string.IsNullOrEmpty(api.Subscriptionkey))                {

                    httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", String.Format("{0}", api.Subscriptionkey));
                }


                HttpResponseMessage response = await httpClient.GetAsync(url);

                result = await response.Content.ReadAsStringAsync();
                 data = JsonDocument.Parse(result);
                statusCode = response.StatusCode;
                var ReasonPhrase = response.ReasonPhrase;

            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }
  
         
            return Json(new { result = data, infoMessage = errorMessage, statusCode= statusCode });

        }

        public IActionResult TryApiSend2(Models.ApiViewModel api)
        {

            var result = "";
            var url = api.Url;
            var errorMessage = "";
            dynamic httpRequest ="";
            try {
                 httpRequest = (HttpWebRequest)WebRequest.Create(url);


                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "Bearer " + api.Token;
                if(string.IsNullOrEmpty(api.Subscriptionkey))
                { httpRequest.Headers["Ocp-Apim-Subscription-Key"] = api.Subscriptionkey; }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

            }
            catch(Exception ex)
            {
                errorMessage = ex.Message;
            }


            return Json(new { result = result, infoMessage = errorMessage });

        }


        public IActionResult TryApiSend3(Models.ApiViewModel api)
        {

            var result = "";
            var url = api.Url;
        
            var subscriptionId = "6fba467a-c738-4c66-9590-4021ecff8d3c";
            var resourceGroupName = "API-TEST-RG";
            var serviceName = "apiedmond";
            url = "https://"+ serviceName +".management.azure-api.net:3443/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis?api-version=2021-08-01";

           // url = "https://management.azure.com/subscriptions/6fba467a-c738-4c66-9590-4021ecff8d3c/resourceGroups/API-TEST-RG/providers/Microsoft.ApiManagement/service/apiedmond/apis?api-version=2021-08-01";

            //***************************************************
            string id = "integration";
            // key - either the primary or secondary key from that same tab.
            string key = "QvXU2sE1SIJDjgW3JJL0BmJx30MZ6/MtMG0ms/ApfQhnOZ6d+9i9fETsmdA8YMZBGbGkIdpttmteEQ3XhbYZSg==";
            // expiry - the expiration date and time of the generated access token. In this example
            //          the expiry is one day from the time the sample is run.
            DateTime expiry = DateTime.UtcNow.AddDays(1);

            // To programmatically create the access token so that we can authenticate and call the REST APIs,
            // call this method. If you pasted in the access token from the publisher portal then you can
            // comment out this line.
          var  sharedAccessSignature = CreateSharedAccessToken(id, key, expiry);
           // sharedAccessSignature = "integration&202205261827&AKtMCLKwMK3vr0JiPA+nTmwxqpVGEMSg2M17asOLfZgEqkQoNI918+ktCIpH+JuYE5FRz38OXxg+rgvS3T13ug==";
            //*********************************************
            var errorMessage = "";
            dynamic httpRequest = "";
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);

                var shass = sharedAccessSignature;
                shass = sharedAccessSignature;
                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "SharedAccessSignature  " + shass;
                //if (string.IsNullOrEmpty(api.Subscriptionkey))
                //{ httpRequest.Headers["Ocp-Apim-Subscription-Key"] = api.Subscriptionkey; }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }


            return Json(new { result = result, infoMessage = errorMessage });

        }



        public IActionResult TryApiSend4(Models.ApiViewModel api)
        {

            var result = "";
            var url = api.Url;

            var subscriptionId = "6fba467a-c738-4c66-9590-4021ecff8d3c";
            var resourceGroupName = "API-TEST-RG";
            var serviceName = "apiedmond";
            url = "https://management.azure.com/subscriptions/" + subscriptionId + "/resourceGroups/" + resourceGroupName + "/providers/Microsoft.ApiManagement/service/" + serviceName + "/apis?api-version=2021-08-01";

            url = "https://management.azure.com/subscriptions/6fba467a-c738-4c66-9590-4021ecff8d3c/resourceGroups/API-TEST-RG/providers/Microsoft.ApiManagement/service/apiedmond/apis?api-version=2021-08-01";

            //***************************************************
            string id = "integration";
            // key - either the primary or secondary key from that same tab.
            string key = "QvXU2sE1SIJDjgW3JJL0BmJx30MZ6/MtMG0ms/ApfQhnOZ6d+9i9fETsmdA8YMZBGbGkIdpttmteEQ3XhbYZSg==";
            // expiry - the expiration date and time of the generated access token. In this example
            //          the expiry is one day from the time the sample is run.
            DateTime expiry = DateTime.UtcNow.AddDays(1);

            // To programmatically create the access token so that we can authenticate and call the REST APIs,
            // call this method. If you pasted in the access token from the publisher portal then you can
            // comment out this line.
            var sharedAccessSignature = CreateSharedAccessToken(id, key, expiry);
            sharedAccessSignature = "integration&202205261827&AKtMCLKwMK3vr0JiPA+nTmwxqpVGEMSg2M17asOLfZgEqkQoNI918+ktCIpH+JuYE5FRz38OXxg+rgvS3T13ug==";
            //*********************************************
            var errorMessage = "";
            dynamic httpRequest = "";
            try
            {
                httpRequest = (HttpWebRequest)WebRequest.Create(url);

                var shass = sharedAccessSignature;
                shass = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6ImpTMVhvMU9XRGpfNTJ2YndHTmd2UU8yVnpNYyIsImtpZCI6ImpTMVhvMU9XRGpfNTJ2YndHTmd2UU8yVnpNYyJ9.eyJhdWQiOiJodHRwczovL21hbmFnZW1lbnQuY29yZS53aW5kb3dzLm5ldCIsImlzcyI6Imh0dHBzOi8vc3RzLndpbmRvd3MubmV0L2NkMWI5YmVlLTRkNzUtNDMxMS1iZTZiLTZjZGI2ZTdjYTUzMS8iLCJpYXQiOjE2NTA5OTc2NjEsIm5iZiI6MTY1MDk5NzY2MSwiZXhwIjoxNjUxMDAyMjc0LCJhY3IiOiIxIiwiYWlvIjoiQVdRQW0vOFRBQUFBYk5CV0xVMi9lKzdnd3VOVzgrcHE4Q0dudnZYRXdQRGY5eVN0SHliRW9leWdYMU04eXFsSmhnTWY3b3pGT2V4QVh1dHhvMmVVYUJmWFFPUzlZa0xiU0N2QklZVTNod1NVS3UzbnlqZlc0N2t1VjlHZWxUaWVsckpwT3JiQ0hvRWIiLCJhbHRzZWNpZCI6IjE6bGl2ZS5jb206MDAwMzAwMDA0MEY3RURBNSIsImFtciI6WyJwd2QiLCJtZmEiXSwiYXBwaWQiOiIxOGZiY2ExNi0yMjI0LTQ1ZjYtODViMC1mN2JmMmIzOWIzZjMiLCJhcHBpZGFjciI6IjAiLCJlbWFpbCI6Im1zZG5lbnQzNUBsZ3MuY29tIiwiZmFtaWx5X25hbWUiOiJFVE9VIiwiZ2l2ZW5fbmFtZSI6IkVkbW9uZCIsImdyb3VwcyI6WyJiZDNjY2FlNS01MDI2LTRjZTUtODI4Yy0xMWFkMzQ2NmQ2NjkiXSwiaWRwIjoibGl2ZS5jb20iLCJpcGFkZHIiOiIyMy4yMzMuMjAxLjEzNCIsIm5hbWUiOiJFZG1vbmQgRVRPVSIsIm9pZCI6IjMyM2NkZWQ3LTZlMTktNDk3ZC05MDRmLTlkYjdlZDUwMTRlMSIsInB1aWQiOiIxMDAzMjAwMThBMEUyNENEIiwicmgiOiIwLkFWQUE3cHNielhWTkVVTy1hMnpiYm55bE1VWklmM2tBdXRkUHVrUGF3ZmoyTUJOUUFDUS4iLCJzY3AiOiJ1c2VyX2ltcGVyc29uYXRpb24iLCJzdWIiOiItWmZpMzk0NHRfUkdyb0tySlpIXzZFTHhaUWxPcHNEUndpS3RfRnRpb0xjIiwidGlkIjoiY2QxYjliZWUtNGQ3NS00MzExLWJlNmItNmNkYjZlN2NhNTMxIiwidW5pcXVlX25hbWUiOiJsaXZlLmNvbSNtc2RuZW50MzVAbGdzLmNvbSIsInV0aSI6Im0yQ2UzajV2MmttM1Mwc3lvLUFNQUEiLCJ2ZXIiOiIxLjAiLCJ3aWRzIjpbIjYyZTkwMzk0LTY5ZjUtNDIzNy05MTkwLTAxMjE3NzE0NWUxMCIsImI3OWZiZjRkLTNlZjktNDY4OS04MTQzLTc2YjE5NGU4NTUwOSJdLCJ4bXNfdGNkdCI6MTYzMjM2OTA1OX0.Jw7VtNR3AhP1mG_PmNeEXP2NbPRLZBfaeUnW92Pm3zK0k8L5nbCCTRfeqAY0wZy41WtEg9WBmrNLL_-Np-Lng3fl1ufbYd9MCMm1iSqMrWbxhu6i-HQTBIx3ZDwcVvV_ywfvdHeYJapPm6g0jaG88f8D5F_em3vaexkzqXKQJIWAu0q1FaQFKiXnlVRVN52qYZlztm-G-Ys0DUIKhsurY4apbE4HrMNtbfF-XFym-XQEJi2vDkjwh3KZxMbU8ZjvYG5AKuffjg5c1GUnu-76T8-TU5lZziufpx4yzlBNXW3_794enavjm4jg0U3ufTDRRAzM1DsUW2g6YF4klPEKbw";
                httpRequest.Accept = "application/json";
                httpRequest.Headers["Authorization"] = "Bearer  " + shass;
                //if (string.IsNullOrEmpty(api.Subscriptionkey))
                //{ httpRequest.Headers["Ocp-Apim-Subscription-Key"] = api.Subscriptionkey; }

                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }


            return Json(new { result = result, infoMessage = errorMessage });

        }
        static private string CreateSharedAccessToken(string id, string key, DateTime expiry)
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

        public async Task<IActionResult> refreshAccessTokenAsync(Models.ApiViewModel api)
        {

            AccessToken accessToken = new AccessToken();
             accessToken= await RenewTokens();
            var Expires = DateTime.Now.AddSeconds(accessToken.expires_in);
            var u = Expires.ToLongTimeString();
            ViewBag.Expires = Expires;
           var infoMessage = "Le token a été réactualisé avec success";
            //return Json(accessToken);
            return Json(new { accessToken = accessToken, expirees = u , infoMessage = infoMessage });
        }
        public X509Certificate2  GetCertificate()
        {
            X509Certificate2 cer =null;
            try {
                X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                cer= certStore.Certificates.OfType<X509Certificate2>().FirstOrDefault();
                cert = cer;
                certStore.Close();
            } catch (Exception ex)
            {
               
            }

            return cer;
        }

        public IActionResult Certificat()
        {

            //https://azure.microsoft.com/fr-fr/blog/using-certificates-in-azure-websites-applications/

              ViewBag.FriendlyName = "je suis videe";

               X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                ViewBag.certStore = certStore.Certificates.OfType<X509Certificate2>().FirstOrDefault();
                
            X509Certificate2Collection certCollection = certStore.Certificates.Find(
                                         X509FindType.FindByThumbprint,
                                   // Replace below with your cert's thumbprint
                                   "456AAB1833DF842152605DF6C2B1DB2BBA29380D",
                                         false);
            // Get the first cert with the thumbprint

            X509Certificate2 cert1 = certCollection.OfType<X509Certificate2>().FirstOrDefault();

            ViewBag.cert = cert1;
            if (certCollection.Count > 0)
            {
                X509Certificate2 cert = certCollection[0];
                // Use certificate
                /*      Console.WriteLine(cert.FriendlyName)*/
                ;
                ViewBag.FriendlyName = cert.Thumbprint;

            }
            certStore.Close();
            ViewBag.FriendlyName = cert;
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
