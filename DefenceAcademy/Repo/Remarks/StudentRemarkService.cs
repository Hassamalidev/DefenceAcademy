using Dapper;
using System.Data;
using DefenceAcademy.Model;
using DefenceAcademy.Repo.Remarks;

namespace DefenceAcademy.Repo.Remarks
{
    public class StudentRemarkRepository : IStudentRemarkRepository
    {
        private readonly DapperContext _context;

        public StudentRemarkRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StudentRemark>> GetAllRemarksAsync()
        {
            var sql = "SELECT * FROM StudentRemarks ORDER BY CreatedAt DESC";
            using (var connection = await _context.createConnection())
            {
                return await connection.QueryAsync<StudentRemark>(sql);
            }
        }

        public async Task<IEnumerable<StudentRemark>> GetApprovedRemarksAsync()
        {
            var sql = "SELECT * FROM StudentRemarks WHERE IsApproved = 1 ORDER BY CreatedAt DESC";
            using (var connection = await _context.createConnection())
            {
                return await connection.QueryAsync<StudentRemark>(sql);
            }
        }

        public async Task<StudentRemark> GetRemarkByIdAsync(int id)
        {
            var sql = "SELECT * FROM StudentRemarks WHERE Id = @Id";
            using (var connection = await _context.createConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<StudentRemark>(sql, new { Id = id });
            }
        }

        public async Task<int> CreateRemarkAsync(StudentRemark remark)
        {
            var sql = @"INSERT INTO StudentRemarks (StudentName, Remark, Status, CreatedAt, IsApproved) 
                       VALUES (@StudentName, @Remark, @Status, @CreatedAt, @IsApproved)";

            remark.CreatedAt = DateTime.UtcNow;
            remark.IsApproved = false;

            using (var connection = await _context.createConnection())
            {
                return await connection.ExecuteAsync(sql, remark);
            }
        }

        public async Task<int> DeleteRemarkAsync(int id)
        {
            var sql = "DELETE FROM StudentRemarks WHERE Id = @Id";
            using (var connection = await _context.createConnection())
            {
                return await connection.ExecuteAsync(sql, new { Id = id });
            }
        }

        public async Task<int> ApproveRemarkAsync(int id)
        {
            var sql = "UPDATE StudentRemarks SET IsApproved = 1 WHERE Id = @Id";
            using (var connection = await _context.createConnection())
            {
                return await connection.ExecuteAsync(sql, new { Id = id });
            }
        }
    }
}