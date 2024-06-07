using AuthUser.Data;
using AuthUser.DTOs;
using AuthUser.Models;
using AuthUser.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AuthUser.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly UserDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _context;
        public UserAuthService(UserDbContext db,IConfiguration configuration,IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _configuration = configuration;
            _context = httpContextAccessor;
        }

        public async Task<AuthResponseDTOs> UserLogin(UserDTOs request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (user == null)
            {
                return new AuthResponseDTOs { Message = "invalid username" };
            }

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return new AuthResponseDTOs { Message = "wrong password" };
            }
            string token = CreateToken(user);
            var  refreshtoken = CreateRefreshToken();
            setRefreshToken(refreshtoken, user);


            return new AuthResponseDTOs { 
                Success = true,
                Token=token ,
                RefreshToken = refreshtoken.Token,
                TokenExpires=refreshtoken.Expires
            };
        }
        public async Task<User> RegisterUser(UserDTOs request)
        {
            CreatePasswordHash(request.Password, out byte[] PasswordHash, out byte[] PasswordSalt);
            var user = new User { UserName = request.UserName
                , PasswordHash = PasswordHash,
                PasswordSalt = PasswordSalt
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }
        private bool VerifyPasswordHash(string password,  byte[] PasswordHash,  byte[] PasswordSalt)
        {
            using (var hmac = new HMACSHA512(PasswordSalt))
            {
              
                var ComputedPasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return ComputedPasswordHash.SequenceEqual(PasswordHash);

            }
        }
        private void CreatePasswordHash(string password, out byte[] PasswordHash,out byte[] PasswordSalt)
        {
            using(var hmac = new HMACSHA512())
            {
               PasswordSalt = hmac.Key;
                PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)); 

            }
        }
        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.UserName),
                new Claim(ClaimTypes.Role,user.UserRole)

            };
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var token = new JwtSecurityToken(
                claims: claims,

                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
                ); 
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);
            return jwt;
        }

       

      private RefreshToken CreateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires= DateTime.Now.AddDays(7),
                Created = DateTime.Now
               

            };
            return refreshToken;
        }

        private async void setRefreshToken(RefreshToken refreshToken,User user){

            var cookiesoption = new CookieOptions
            {
                HttpOnly = true,
                Expires = refreshToken.Expires
            };
            _context.HttpContext.Response.Cookies.Append("refreshToken", refreshToken.Token, cookiesoption);
            user.RefreshToken = refreshToken.Token;
            user.TokenCreated = refreshToken.Created;
            user.TokenExpires = refreshToken.Expires;
           await _db.SaveChangesAsync();

        }

        public async Task<AuthResponseDTOs> RefreshToken()
        {
            var refreshtoken = _context.HttpContext.Request.Cookies["refreshToken"];
            var user = await _db.Users.FirstOrDefaultAsync(u=>u.RefreshToken== refreshtoken);
            if(user == null)
            {
                return new AuthResponseDTOs { Message="invalid refresh token"};
            }
            else if(user.TokenExpires<DateTime.Now){
                return new AuthResponseDTOs { Message = "token expired" };

            }
            string token = CreateToken(user);
            var newrefreshtoken = CreateRefreshToken();
            setRefreshToken(newrefreshtoken, user);
            return new AuthResponseDTOs
            {
                Success = true,
                Token = token,
                RefreshToken = newrefreshtoken.Token,
                TokenExpires = newrefreshtoken.Expires,

            };
        }
    }
}
