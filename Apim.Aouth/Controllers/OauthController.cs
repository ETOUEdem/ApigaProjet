
using Apim.Aouth.Interface;
using Apim.Data.Repository.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Apim.Auth.Controllers
{
    [Route("server/[controller]")]
    [ApiController]
    public class OauthController : ControllerBase
    {
        private readonly IJwtAuthenticationManager jWTAuthenticationManager;
        public OauthController(IJwtAuthenticationManager jwtAuthenticationManager)
        {
            this.jWTAuthenticationManager = jwtAuthenticationManager;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] UserCred userCred)
        {
            var token = jWTAuthenticationManager.Authenticate(userCred.Username, userCred.Password);
            if (token == null)
                return Unauthorized();
            return Ok(token);
        }

    }
}
