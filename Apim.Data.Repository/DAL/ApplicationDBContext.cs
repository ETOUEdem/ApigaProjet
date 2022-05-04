using Apim.Data.Repository.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.DAL
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
    : base(options)
        {
            //Database.EnsureCreated();
            //Database.Migrate();
        
        }

        public DbSet<User> Users { get; set; }

    }
}
