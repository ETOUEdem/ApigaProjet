using Apim.Business.Service.Interfaces;
using Apim.Business.Service.Services;
using Apim.Data.Repository.Extensions;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models.Cle;
using Apim.Data.Repository.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Business.Service.Extensions
{
   public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddBusinessLibrary(this IServiceCollection services)
        {


            services.AddRepositoryLibrary();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserRepository, UserRepository>();       
            services.AddScoped<IOrderRepository, OrderRepository>();
            return services;
        }

        public static IServiceCollection AddVerifitokenLibrary(this IServiceCollection services)
        {
            var tokenKey = Secretkey.TokenKey;
            var key = Encoding.ASCII.GetBytes(tokenKey);
          
     
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
          .AddJwtBearer(x =>
          {
              x.RequireHttpsMetadata = false;
              x.SaveToken = true;
              x.TokenValidationParameters = new TokenValidationParameters
              {
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(key),
                  ValidateIssuer = false,
                  ValidateAudience = false
              };
          });
            return services;
        }
    }
}
