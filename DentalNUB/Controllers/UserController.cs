using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using DentalNUB.Entities.Models;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]

public class UserController : ControllerBase
{
    private readonly DBContext _context;

    public UserController(DBContext context)
    {
        _context = context;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetUserProfile()
    {
        // جيب الـ UserId من الـ JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // غالبًا بيرجع Id كـ string
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized("Invalid user ID.");
        }

        // جيب الـ Role من الـ token
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        // لو الـ Role هي Admin، ارجع Forbidden
        if (role == "Admin")
        {
            return Forbid("Admins are not allowed to access this endpoint.");
        }

        // جيب بيانات الـ User من الـ database باستخدام Id بدل UserId
        var user = await _context.Users
            .Where(u => u.Id == userId) // غيرت من u.UserId لـ u.Id
            .Select(u => new
            {
                u.FullName,
                u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // ارجع الـ response
        return Ok(new
        {
            FullName = user.FullName,
            Email = user.Email
        });
    }
}