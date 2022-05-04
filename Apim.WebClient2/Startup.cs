using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Apim.WebClient2
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
            services.AddControllersWithViews();
            services.AddMvc();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
         options.Authority = "http://localhost:8080/auth/realms/master";
         //options.Authority = "https://keycloakuri.canadacentral.azurecontainer.io/auth/realms/master";

         // Client configured in the Keycloak
         options.ClientId = "WebApp";

         // For testing we disable https (should be true for production)
         options.RequireHttpsMetadata = false;
         options.SaveTokens = true;

         // Client secret shared with Keycloak
         options.ClientSecret = "AgYyUCvS8uzCxde8qkIuoXQTvkvv05ZY";
         options.GetClaimsFromUserInfoEndpoint = true;

         // OpenID flow to use
         options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
     });
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
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
