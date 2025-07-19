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

    // Add this DTO class to your project
    public class PaginatedResponse<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
    [HttpGet("{subject}")]
    public async Task<IActionResult> GetNotesBySubject(
        string subject,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var notes = await _noteService.GetNotesBySubjectAsync(subject, page, pageSize);
        var totalCount = await _noteService.GetTotalNotesCountAsync(subject);

        var response = new PaginatedResponse<Note>
        {
            Items = notes,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(response);
    }

    [HttpPost]
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