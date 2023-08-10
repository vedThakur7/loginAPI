using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LoginAPI.Data;
using LoginAPI.Models;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using LinqToDB;

namespace LoginAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        
        private readonly IConfiguration _configuration;
        private readonly LoginAPIContext _dataContext;
        public AuthController(IConfiguration configuration, LoginAPIContext dbContext)
        {
            _configuration = configuration;
            _dataContext = dbContext;
        }

        [HttpPost("register")]
        public async  Task<ActionResult<User>> Register(UserDto request) {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            user.Username = request.UserName;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _dataContext.Add(user);
            _dataContext.SaveChanges();
           




            return Ok(user);
        }

        [HttpGet("loggedusers")]
        public async Task<List<User>> GetAllUsers()
        {
            var loggedUsers = await _dataContext.Users.ToListAsync();
            return loggedUsers;
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            if(user.Username != request.UserName)
            {
                return BadRequest("User not found.");
            }
            if(!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest("Wrong Password.");
            }
            string token = CreateToken(user);
            return Ok(token);   
        }

        private string CreateToken(User user)
        {

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) 
        {
            using(var hmac = new HMACSHA512()) 
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);    
            }
        }
    }
}
