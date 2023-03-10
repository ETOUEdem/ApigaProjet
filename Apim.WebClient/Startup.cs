using Apim.WebClient.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


using System.Net.Http;
using System.Security.Authentication;
using System.Net;

namespace Apim.WebClient
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var proxy = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = null,
                DefaultProxyCredentials = CredentialCache.DefaultNetworkCredentials
            };
            //**************************/
            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            //services.AddCertificateForwarding(options =>
            //{
            //    options.CertificateHeader = "X-ARR-ClientCert";
            //    options.HeaderConverter = (headerValue) =>
            //    {
            //        var clientCertificate = new X509Certificate2(System.Web.HttpUtility.UrlDecodeToBytes(headerValue));
            //        return clientCertificate;
            //    };
            //});
            //****************************/
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            //IdentityModelEventSource.ShowPII = true;
            services.AddControllersWithViews();
            services.AddMvc();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddHttpContextAccessor();
            services.AddAuthentication(options =>
            {
                // Store the session to cookies
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // OpenId authentication
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
     .AddCookie("Cookies")
     .AddOpenIdConnect(options =>
     {
         // URL of the Keycloak server
         //options.Authority = "http://localhost:8080/auth/realms/master";
       //  options.Authority = Configuration["Authority"];
           //options.Authority = "http://keycloakvm.azure-api.net:8080/auth/realms/master";
          
         // Client configured in the Keycloak
         options.ClientId = Configuration["ClientId"];

         // For testing we disable https (should be true for production)
         options.RequireHttpsMetadata = false;
         options.SaveTokens = true;

         // Client secret shared with Keycloak
         options.ClientSecret = Configuration["ClientSecret"];
         options.GetClaimsFromUserInfoEndpoint = true;
        
         options.MetadataAddress = "https://webappkeycloak.azurewebsites.net/auth/realms/master/.well-known/openid-configuration";
         /* options.BackchannelHttpHandler = new HttpClientHandler
          {
              ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
          };*/
         //ou

         /*   options.BackchannelHttpHandler = new HttpClientHandler
            {
                UseProxy = false,
                UseDefaultCredentials = true
            };*/
         // ou

         //options.BackchannelHttpHandler = GetHandler();
         //ou
         options.BackchannelHttpHandler = proxy;
         // OpenID flow to use

         options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
     }
     
     );
        }


        private static HttpClientHandler GetHandler()
        {
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.SslProtocols = SslProtocols.Tls12;
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            return handler;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Add this line to ensure authentication is enabled
            app.UseAuthentication();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseForwardedHeaders(new ForwardedHeadersOptions
            //{
            //    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            //});
            //app.UseCertificateForwarding();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
    

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
