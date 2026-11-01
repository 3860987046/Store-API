using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoreAPI.Data;
using StoreAPI.DTOs;
using StoreAPI.Models;
using StoreAPI.Services;
using System.Security.Cryptography;
using System.Text;

namespace StoreAPI.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;

        public UserController(AppDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/users/register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            // بررسی وجود کاربر
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return BadRequest("Username already exists");

            // هش کردن رمز عبور
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        // POST: api/users/login
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null)
                return Unauthorized("Invalid username or password");


            bool isValid = BCrypt.Net.BCrypt.Verify(
                loginDto.Password,    //  پسورد خام (hash نشده)
                user.PasswordHash     //  هش ذخیره شده در دیتابیس
            );
            if (!isValid)
                return Unauthorized("Invalid username or password");

            var token = _jwtService.GenerateToken(user);

            //return an Object
            return Ok(new { message = "Login successful", 
                token = token, userId = user.Id , username = user.Username});
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();
            //retuen user
            return Ok(new
            {
                user.Id,
                user.Username,
                CreatedAt = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
