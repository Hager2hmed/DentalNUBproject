using DentalNUB.Entities;
using DentalNUB.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
                                    

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly DBContext _context;

    public AdminController(DBContext context)
    {
        _context = context;
    }

    [HttpPost("CreateAdmin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminRequest request)
    {
        // التأكد إذا كان الأدمن موجود في النظام
        var adminExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (adminExists)
            return BadRequest("الـ Admin ده موجود بالفعل");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var admin = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = hashedPassword,
       
            Role = "Admin"
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        return Ok(new { message = "تم إضافة الأدمن بنجاح ✅" });
    }
}
