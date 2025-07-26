using DefenceAcademy.Model;

namespace DefenceAcademy.Repo
{
    public interface IQuestionCategory
    {
        Task<int> CreateCategoryAsync(QuestionsCategory category);
        Task<IEnumerable<QuestionsCategory>> GetAllCategoriesAsync();
        Task<QuestionsCategory?> GetCategoryByIdAsync(int id);
    }
}
