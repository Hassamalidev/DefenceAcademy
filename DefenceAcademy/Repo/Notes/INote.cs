using DefenceAcademy.Model;

namespace DefenceAcademy.Repo.Notes
{
    public interface INote
    {
        Task<IEnumerable<Note>> GetNotesBySubjectAsync(string subject, int page, int pageSize);
        Task<int> CreateNoteAsync(Note note);

    }
}
