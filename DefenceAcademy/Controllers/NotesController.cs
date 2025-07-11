using DefenceAcademy.Model;
using DefenceAcademy.Repo;
using DefenceAcademy.Repo.Notes;
using Microsoft.AspNetCore.Mvc;

namespace DefenceAcademy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly INote _notesService;

        public NotesController(INote notesService)
        {
            _notesService = notesService;
        }

        [HttpGet("{subject}")]
        public async Task<IActionResult> GetNotesBySubject(string subject, int page = 1, int pageSize = 10)
        {
            var notes = await _notesService.GetNotesBySubjectAsync(subject, page, pageSize);
            return Ok(notes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note note)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _notesService.CreateNoteAsync(note);
            if (result > 0)
                return Ok(new { message = "Note created successfully" });

            return StatusCode(500, "An error occurred while creating the note");
        }
    }
}
