using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DataStore.Auth;
using DataStore.ConfigOptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DataStore.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IUserManager _userManager;
        private readonly NodeAppSettingsOptions _appSettingsOptions;

        public AuthController(
            IUserManager userManager, 
            IOptions<NodeAppSettingsOptions> appSettingsOptions)
        {
            _userManager = userManager;
            _appSettingsOptions = appSettingsOptions.Value;
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult RequestToken([FromBody] TokenRequest request)
        {
            var claims = _userManager.GetUser(request.Username, request.Password);
            if (claims == null)
            {
                return BadRequest("Could not verify username and password");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettingsOptions.SecurityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
            
            var token = new JwtSecurityToken(
                issuer: "yourdomain.com",
                audience: "yourdomain.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }

    public class TokenRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}