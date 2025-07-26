using Microsoft.AspNetCore.Mvc;
using DefenceAcademy.Model;
using DefenceAcademy.Repo;

namespace DefenceAcademy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionCategoryController : ControllerBase
    {
        private readonly IQuestionCategory _categoryService;

        public QuestionCategoryController(IQuestionCategory categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] QuestionsCategory category)
        {
            if (category == null)
                return BadRequest("Category is null");

            var result = await _categoryService.CreateCategoryAsync(category);
            if (result > 0)
                return Ok(new { message = "Category created successfully" });

            return StatusCode(500, "Failed to create category");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }
    }
}
