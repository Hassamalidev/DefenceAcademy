using DefenceAcademy.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DefenceAcademy.Repo.Notes
{
    public interface INote
    {
        Task<IEnumerable<Note>> GetNotesBySubjectAsync(string subject, int page, int pageSize);
        Task<int> GetTotalNotesCountAsync(string subject);
        Task<int> CreateNoteAsync(Note note);
        Task<bool> UpdateNoteAsync(Note note);
        Task<bool> DeleteNoteAsync(int id);
        Task<Note> GetNoteByIdAsync(int id);
    }
}