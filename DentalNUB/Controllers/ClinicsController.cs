using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DentalNUB.Entities.Models;
using DentalNUB.Entities;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ClinicsController : ControllerBase
{
    private readonly DBContext _context;

    public ClinicsController(DBContext context)
    {
        _context = context;
    }

    [HttpPost("AddClinic")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddClinic([FromBody] CreateClinicRequest request)
    {
        var existingClinic = await _context.Clinics
            .FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName);

        if (existingClinic != null)
        {
            return BadRequest("العيادة دي موجودة بالفعل.");
        }

        var newClinic = new Clinic
        {
            ClinicName = request.ClinicName,
            MaxStudent = request.MaxStudent,
            Schedule = request.Schedule,
            AllowedYear = request.AllowedYear // 👈 خدنا القيمة من الـ request
        };

        _context.Clinics.Add(newClinic);
        await _context.SaveChangesAsync();

        return Ok("تمت إضافة العيادة بنجاح.");
    }


    [HttpPut("UpdateClinic/{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateClinic(int id, [FromBody] CreateClinicRequest request)
    {
        var clinic = await _context.Clinics.FindAsync(id);
        if (clinic == null)
            return NotFound("العيادة غير موجودة.");

        // نتأكد مفيش عيادة بنفس الاسم غير اللي بنعدلها
        var duplicate = await _context.Clinics
            .FirstOrDefaultAsync(c => c.ClinicName == request.ClinicName && c.ClinicID != id);
        if (duplicate != null)
            return BadRequest("في عيادة تانية بنفس الاسم.");

        clinic.ClinicName = request.ClinicName;
        clinic.MaxStudent = request.MaxStudent;
        clinic.Schedule = request.Schedule;

        await _context.SaveChangesAsync();
        return Ok("تم تحديث بيانات العيادة بنجاح.");
    }

   
    [HttpDelete("DeleteClinic/{id}")]
    //[Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteClinic(int id)
    {
        var clinic = await _context.Clinics
            .Include(c => c.ClinicSections)
                .ThenInclude(cs => cs.Doctors) // علشان نعرف لو في طلاب جوا السكاشن
            .FirstOrDefaultAsync(c => c.ClinicID == id);

        if (clinic == null)
            return NotFound("العيادة غير موجودة.");

        // لو أي سيكشن فيه طلاب، نمنع الحذف
        if (clinic.ClinicSections.Any(section => section.Doctors != null && section.Doctors.Any()))
            return BadRequest("لا يمكن حذف العيادة لأنها تحتوي على سكاشن بها طلاب.");

        // نحذف السكاشن الفاضية (اللي مفيهاش طلاب)
        if (clinic.ClinicSections.Any())
            _context.ClinicSections.RemoveRange(clinic.ClinicSections);

        // نحذف العيادة نفسها
        _context.Clinics.Remove(clinic);

        await _context.SaveChangesAsync();

        return Ok("تم حذف العيادة والسكاشن الفارغة المرتبطة بها بنجاح.");
    }



    [HttpGet("GetAll")]
    //[Authorize(Roles = "Admin,Doctor,Consultant")]
    public async Task<ActionResult<List<ClinicResponse>>> GetAllClinics()
    {
        var clinics = await _context.Clinics.ToListAsync();

        var response = clinics.Select(c => new ClinicResponse
        {
            ClinicName = c.ClinicName,
            MaxStudent = c.MaxStudent,
            AllowedYear = c.AllowedYear,
            ScheduleDays = c.Schedule.Split(',').Select(day => day.Trim()).ToList()
        }).ToList();

        return Ok(response);
    }
}
