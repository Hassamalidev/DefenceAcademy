using DefenceAcademy.Model;
using DefenceAcademy.Repo;
using Microsoft.AspNetCore.Mvc;

namespace DefenceAcademy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestion _questionService;

        public QuestionController(IQuestion questionService)
        {
            _questionService = questionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] Question question)
        {
            if (question == null)
                return BadRequest("Question is null");

            var result = await _questionService.CreateQuestionAsync(question);
            if (result > 0)
                return Ok(new { message = "Question added successfully" });

            return StatusCode(500, "Failed to create question");
        }

        [HttpGet("byCategory/{categoryId}")]
        public async Task<IActionResult> GetQuestionsByCategoryId([FromRoute] int categoryId)
        {
            var questions = await _questionService.GetQuestionsByCategoryAsync(categoryId);
            if (questions == null || !questions.Any())
                return NotFound("No questions found for this category.");

            return Ok(questions);
        }


        [HttpGet("test")]
        public async Task<IActionResult> GetRandomTestQuestions([FromQuery] int count = 30, [FromQuery] int categoryId = 1)
        {
            var questions = await _questionService.GetRandomQuestionsAsync(count, categoryId);
            return Ok(questions);
        }
    }
}
