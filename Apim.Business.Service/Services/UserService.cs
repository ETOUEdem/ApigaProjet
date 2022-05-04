using Apim.Business.Service.Interfaces;
using Apim.Data.Repository.Interfaces;
using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Business.Service.Services
{
    public class UserService : IUserService
    {
       public IUserRepository userRepository { get; set; }
        public UserService(IUserRepository UserRepository)
        {
            userRepository = UserRepository;
        }
        public List<User> UsersCred()
        {
           return userRepository.Users();
        }

    }
}
