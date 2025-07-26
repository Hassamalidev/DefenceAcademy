using DefenceAcademy.Model;

namespace DefenceAcademy.Repo
{
    public interface IQuestion
    {
        Task<int> CreateQuestionAsync(Question question);
        Task<IEnumerable<Question>> GetQuestionsByCategoryAsync(int categoryId);
        Task<IEnumerable<Question>> GetRandomQuestionsAsync(int count, int categoryId);

    }
}
