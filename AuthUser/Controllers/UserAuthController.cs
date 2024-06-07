using AuthUser.DTOs;
using AuthUser.Models;
using AuthUser.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthUser.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthController : ControllerBase
    {
        private readonly IUserAuthService _userAuthService;
        public UserAuthController(IUserAuthService userAuthService)
        {
            _userAuthService = userAuthService;
        }
        [HttpPost]
        public async Task<ActionResult<User>> UserRegister(UserDTOs request)
        {
            var results = await _userAuthService.RegisterUser(request);
            return Ok(results);
        }
        [HttpPost("login")]
        public async Task<ActionResult<User>> UserLogin(UserDTOs request)
        {
            var results = await _userAuthService.UserLogin(request);
            if (results.Success)
            {
                return Ok(results);

            }
            return BadRequest(results.Message);
        }
        [HttpGet("Admin"),Authorize(Roles ="Admin")]
        public ActionResult<string> Prachanda()
        {
            return Ok("hello you are authenticate and authorized person ");
        }
        [HttpPost("reftesh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var response = await _userAuthService.RefreshToken();
            if(response.Success)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }
        
    }
}
