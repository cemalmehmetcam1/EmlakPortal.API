using EmlakPortal.API.DTOs;
using EmlakPortal.API.Models;
using EmlakPortal.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmlakPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new AppUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Sistemde "User" ve "Admin" rolleri yoksa oluştur
                string[] roles = { "Admin", "User" };
                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        await _roleManager.CreateAsync(new AppRole { Name = role });
                    }
                }

                // Dışarıdan kaydolan herkes varsayılan olarak "User" (Normal kullanıcı) olur
                await _userManager.AddToRoleAsync(user, "User");

                return Ok(new ResultDto { Status = true, Message = "Kayıt Başarılı. Hesabınız standart 'User' yetkisiyle oluşturuldu." });
            }

            return BadRequest(new ResultDto { Status = false, Message = "Kayıt sırasında bir hata oluştu.", Data = result.Errors });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                // Şifre doğruysa Token üret ve ver
                var token = await _tokenService.GenerateToken(user);
                return Ok(new ResultDto { Status = true, Message = "Giriş Başarılı", Data = token });
            }

            return Unauthorized(new ResultDto { Status = false, Message = "Kullanıcı adı veya şifre hatalı!" });
        }
    }
}