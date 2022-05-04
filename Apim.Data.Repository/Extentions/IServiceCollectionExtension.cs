
using Apim.Data.Repository.DAL;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models.Cle;
using Apim.Data.Repository.Repository;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Extensions
{
   public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddRepositoryLibrary(this IServiceCollection services)
        {
            //********************************
            //string id = string.Format("{0}.db", Guid.NewGuid().ToString());
            string id = string.Format("{0}.db", "data".ToString());

            var builder = new SqliteConnectionStringBuilder()
            {
                DataSource = id,
                Mode = SqliteOpenMode.Memory,
                Cache = SqliteCacheMode.Shared
            };
            var connection = new SqliteConnection(builder.ConnectionString);
            connection.Open();
            connection.EnableExtensions(true);
            services.AddDbContext<ApplicationDBContext>(options => options.UseSqlite(connection));
            //*****************************


            return services;
        }

    
    }
}
