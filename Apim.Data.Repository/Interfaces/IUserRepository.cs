using Apim.Data.Repository.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apim.Data.Repository.Interfaces
{
  public  interface IUserRepository
    {
        List<User> Users();

    }
}
