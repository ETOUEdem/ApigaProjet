using Apim.Data.Repository.DAL;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Apim.Data.Repository.Repository
{

    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDBContext _context;

        public UserRepository(ApplicationDBContext context)
        {
            _context = context;
        }
        public List<User> Users()
        {
            //ApplicationDBContext _context = new ApplicationDBContext();
            return _context.Users.ToList();
            //return null;

        }
        //public List<User> Users()
        //{
        //    List<User> users = new List<User>() {
        //     new User
        //    {
        //        Username ="test1",
        //        Password = "password1",
        //        Role ="Admin"

        //    },
        //    new User
        //    {
        //        Username ="test1",
        //        Password = "password2",
        //        Role ="Admin"

        //    },

        //    };


        //    return users;
        //}
    }
}
