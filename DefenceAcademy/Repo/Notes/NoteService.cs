using Dapper;
using DefenceAcademy.Model;
using DefenceAcademy.Repo.Notes;
using System.Data;

namespace DefenceAcademy.Repo
{
    public class NoteService : INote
    {
        private readonly DapperContext _context;

        public NoteService(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Note>> GetNotesBySubjectAsync(string subject, int page, int pageSize)
        {
            using (var connection = await _context.createConnection())
            {
                string sql = @"SELECT * FROM Notes 
                       WHERE Subject = @Subject 
                       ORDER BY CreatedAt DESC
                       OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                return await connection.QueryAsync<Note>(sql, new
                {
                    Subject = subject,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                });
            }
        }

        public async Task<int> CreateNoteAsync(Note note)
        {
            using (var connection = await _context.createConnection())
            {
                string sql = @"INSERT INTO Notes (Title, Answer, Explanation, Subject)
                       VALUES (@Title, @Answer, @Explanation, @Subject)";

                return await connection.ExecuteAsync(sql, note);
            }
        }
    }

    }
