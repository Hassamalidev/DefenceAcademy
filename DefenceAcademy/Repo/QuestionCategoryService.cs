using Dapper;
using System.Data;
using DefenceAcademy.Model;

namespace DefenceAcademy.Repo
{
    public class QuestionCategoryService : IQuestionCategory
    {
        private readonly DapperContext _context;

        public QuestionCategoryService(DapperContext context)
        {
            _context = context;
        }

        public async Task<int> CreateCategoryAsync(QuestionsCategory category)
        {
            var sql = @"INSERT INTO QuestionsCategories (name, description) 
                        VALUES (@name, @description)";

            using (var connection = await _context.createConnection())
            {
                var result = await connection.ExecuteAsync(sql, category);
                return result;
            }
        }

        public async Task<IEnumerable<QuestionsCategory>> GetAllCategoriesAsync()
        {
            var sql = "SELECT * FROM QuestionsCategories";

            using (var connection = await _context.createConnection())
            {
                var result = await connection.QueryAsync<QuestionsCategory>(sql);
                return result;
            }
        }

        public async Task<QuestionsCategory?> GetCategoryByIdAsync(int id)
        {
            var sql = "SELECT * FROM QuestionsCategories WHERE id = @id";

            using (var connection = await _context.createConnection())
            {
                var result = await connection.QueryFirstOrDefaultAsync<QuestionsCategory>(sql, new { id });
                return result;
            }
        }
    }
}
