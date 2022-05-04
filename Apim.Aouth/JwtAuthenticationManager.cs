using Apim.Aouth.Interface;
using Apim.Business.Service.Interfaces;
using Apim.Data.Repository.Models.Cle;
//using Apim.Business.Service.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Apim.Aouth
{

    public class JwtAuthenticationManager : IJwtAuthenticationManager
    {
        private readonly string tokenKey;
        public IUserService userService { get; set; }
        public JwtAuthenticationManager(IUserService _UserService)
        {
            userService = _UserService;
            tokenKey = Secretkey.TokenKey;
            
        }


        public string Authenticate(string username, string password)
        {

            if (!userService.UsersCred().Any(u => u.Username == username && u.Password == password))
            {
                return null;
            }
            var userCred = userService.UsersCred().Where(x => x.Username == username && x.Password == password).SingleOrDefault();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(tokenKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {

                Subject = new ClaimsIdentity(new Claim[]
              {
                    new Claim(ClaimTypes.Name, userCred.Username),
                    //new Claim(ClaimTypes.Role,userCred.Role )
              }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                  new SymmetricSecurityKey(key),
                  SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
