using Apim.Data.Repository.Models.Cle;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Apim.Aouth.Filter
{
    public sealed class SpecialAuthorizeAttribute :Attribute, IAuthorizationFilter
    {
        private  string certificat = Certificat.Certificatkey;
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            
            if (context != null)
            {
                var authCertificatKey = context.HttpContext.Request.Form["Certificat"];
     
                if (string.IsNullOrEmpty(authCertificatKey))
                {
                    context.Result = new RedirectResult("/Home/Certificat");
                }
                if (certificat.Equals((authCertificatKey)))
                {
             
                    return;
                }
                else
                {
                    context.Result = new RedirectResult("/Home/Certificat");
                }

            }
        }
    }
}
