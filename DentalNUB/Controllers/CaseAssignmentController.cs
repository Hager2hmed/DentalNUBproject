using DentalNUB.Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CaseAssignmentController : ControllerBase
{
    private readonly DBContext _context;

    public CaseAssignmentController(DBContext context)
    {
        _context = context;
    }


    [HttpPost("UploadDoctorRankingExcel")]
    public async Task<IActionResult> UploadDoctorRankingExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("يرجى اختيار ملف Excel صالح 📄");

        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();
            var rows = worksheet.RowsUsed().Skip(1); // نعدي الـ header

            foreach (var row in rows)
            {
                var doctorId = int.Parse(row.Cell(1).Value.ToString());
                var section = int.Parse(row.Cell(2).Value.ToString());
                var order = int.Parse(row.Cell(3).Value.ToString());

                // Check: لو الدكتور ده موجود بالفعل في نفس السيكشن
                var exists = await _context.DoctorSectionRankings.AnyAsync(r =>
                    r.DoctorID == doctorId && r.SectionNumber == section);

                // Check: لو فيه حد بنفس الترتيب في نفس السيكشن
                var duplicateOrder = await _context.DoctorSectionRankings.AnyAsync(r =>
                    r.SectionNumber == section && r.OrderInSection == order);

                if (exists || duplicateOrder)
                    continue; // تجاهل الصف

                var ranking = new DoctorSectionRanking
                {
                    DoctorID = doctorId,
                    SectionNumber = section,
                    OrderInSection = order
                };

                _context.DoctorSectionRankings.Add(ranking);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "تم رفع الملف وتسجيل الترتيب بنجاح ✅" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, "حصلت مشكلة أثناء قراءة الملف ❌: " + ex.Message);
        }
    }
}