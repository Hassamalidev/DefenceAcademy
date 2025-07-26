using Dapper;
using DefenceAcademy.Model;

namespace DefenceAcademy.Repo
{
    public class QuestionService:IQuestion
    {
        private readonly DapperContext _context;
        public QuestionService(DapperContext context)
        {
            _context = context;
        }


      
        public async Task<int> CreateQuestionAsync(Question question)
        {
            using (var connection = await _context.createConnection())
            {
                var sql = @"INSERT INTO Questions 
                        (QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption, CategoryId)
                        VALUES 
                        (@QuestionText, @OptionA, @OptionB, @OptionC, @OptionD, @CorrectOption, @CategoryId)";


                var result = await connection.ExecuteAsync(sql, question);
                return result;
            }
        }
        public async Task<IEnumerable<Question>> GetQuestionsByCategoryAsync(int categoryId)
        {
            using (var connection = await _context.createConnection())
            {
                var sql = "SELECT * FROM Questions WHERE CategoryId = @CategoryId";
                return await connection.QueryAsync<Question>(sql, new { CategoryId = categoryId });
            }
        }

        public async Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count, int categoryId)
        {
            using (var connection = await _context.createConnection())
            {
                var sql = @"SELECT TOP (@Count) * 
                        FROM Questions 
                        WHERE CategoryId = @CategoryId 
                        ORDER BY NEWID()";


                return await connection.QueryAsync<Question>(sql, new { Count = count, CategoryId = categoryId });
            }


        }
    }
}