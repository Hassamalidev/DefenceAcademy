using Microsoft.AspNetCore.Mvc;
using DefenceAcademy.Model;
using DefenceAcademy.Repo.Notes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class NotesController : ControllerBase
{
    private readonly INote _noteService;

    public NotesController(INote noteService)
    {
        _noteService = noteService;
    }

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

    [HttpGet("single/{id}")]
    public async Task<IActionResult> GetNoteById(int id)
    {
        var note = await _noteService.GetNoteByIdAsync(id);
        if (note == null)
        {
            return NotFound();
        }
        return Ok(note);
    }
    [Authorize("Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] Note note)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var id = await _noteService.CreateNoteAsync(note);
        return CreatedAtAction(
            nameof(GetNoteById),
            new { id = id },
            note
        );
    }
    [Authorize("Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingNote = await _noteService.GetNoteByIdAsync(id);
        if (existingNote == null)
        {
            return NotFound();
        }
        note.Id = id; 
        var success = await _noteService.UpdateNoteAsync(note);
        if (!success)
        {
            return StatusCode(500, "Error updating note");
        }

        var updatedNote = await _noteService.GetNoteByIdAsync(id);
        return Ok(updatedNote);
    }

    [Authorize("Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNote(int id)
    {
        var existingNote = await _noteService.GetNoteByIdAsync(id);
        if (existingNote == null)
        {
            return NotFound();
        }

        var success = await _noteService.DeleteNoteAsync(id);
        if (!success)
        {
            return StatusCode(500, "Error deleting note");
        }

        return Ok(existingNote); 
    }

}