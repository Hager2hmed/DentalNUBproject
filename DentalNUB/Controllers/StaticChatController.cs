using Microsoft.AspNetCore.Mvc;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DentalNUB.Entities.Models;
using DentalNUB.Entities;

namespace DentalNUB.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Patient")]
public class StaticChatController : ControllerBase
{
    private readonly DBContext _context;

    public StaticChatController(DBContext context)
    {
        _context = context;
    }

    // Endpoint 1: لعرض الأسئلة
    [Authorize(Roles = "Patient")]

    [HttpGet("GetQuestions")]
    public async Task<ActionResult<IEnumerable<QuestionResponse>>> GetQuestions()
    {
        var questions = await _context.Questions
                                      .Select(q => q.Adapt<QuestionResponse>())
                                      .ToListAsync();

        return Ok(questions);
    }

    // Endpoint 2: لعرض الإجابة الخاصة بسؤال معين
    [HttpGet("{id}/answer")]
    public async Task<ActionResult<AnswerResponse>> GetAnswer([FromRoute]int id)
    {
        var answer = await _context.Answers
                                   .Where(a => a.QuestID == id)
                                   .FirstOrDefaultAsync();

        if (answer == null)
        {
            return NotFound("الإجابة مش موجودة.");
        }

        return Ok(answer.Adapt<AnswerResponse>());
    }
}

