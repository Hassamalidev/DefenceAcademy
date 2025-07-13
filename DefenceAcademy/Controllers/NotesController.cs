using Microsoft.AspNetCore.Mvc;
using DefenceAcademy.Model;
using DefenceAcademy.Repo.Notes;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INote _noteService;

    public NotesController(INote noteService)
    {
        _noteService = noteService;
    }

    [HttpGet("{subject}")]
    public async Task<IActionResult> GetNotesBySubject(
        string subject,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var notes = await _noteService.GetNotesBySubjectAsync(subject, page, pageSize);
        var totalCount = await _noteService.GetTotalNotesCountAsync(subject);

        Response.Headers.Add("X-Total-Count", totalCount.ToString());
        return Ok(notes);
    }

    [HttpPost]
    // [Authorize(Roles = "Admin")]  // Uncomment when auth is implemented
    public async Task<IActionResult> CreateNote([FromBody] Note note)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _noteService.CreateNoteAsync(note);
        return CreatedAtAction(
            nameof(GetNotesBySubject),
            new { subject = note.Subject },
            note
        );
    }
}