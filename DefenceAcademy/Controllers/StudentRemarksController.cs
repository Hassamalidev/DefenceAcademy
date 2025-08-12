using Microsoft.AspNetCore.Mvc;
using DefenceAcademy.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefenceAcademy.Repo.Remarks;

namespace DefenceAcademy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentRemarksController : ControllerBase
    {
        private readonly IStudentRemarkRepository _studentRemarkRepository;

        public StudentRemarksController(IStudentRemarkRepository studentRemarkRepository)
        {
            _studentRemarkRepository = studentRemarkRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentRemark>>> GetAllRemarks()
        {
            var remarks = await _studentRemarkRepository.GetAllRemarksAsync();
            return Ok(remarks);
        }

        [HttpGet("approved")]
        public async Task<ActionResult<IEnumerable<StudentRemark>>> GetApprovedRemarks()
        {
            var remarks = await _studentRemarkRepository.GetApprovedRemarksAsync();
            return Ok(remarks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StudentRemark>> GetRemark(int id)
        {
            var remark = await _studentRemarkRepository.GetRemarkByIdAsync(id);
            if (remark == null)
            {
                return NotFound();
            }
            return Ok(remark);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateRemark(StudentRemark remark)
        {
            if (string.IsNullOrWhiteSpace(remark.StudentName) ||
                string.IsNullOrWhiteSpace(remark.Remark) ||
                string.IsNullOrWhiteSpace(remark.Status))
            {
                return BadRequest("Student name, remark, and status are required");
            }

            var result = await _studentRemarkRepository.CreateRemarkAsync(remark);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<int>> DeleteRemark(int id)
        {
            var result = await _studentRemarkRepository.DeleteRemarkAsync(id);
            if (result == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpPut("approve/{id}")]
        public async Task<ActionResult<int>> ApproveRemark(int id)
        {
            var result = await _studentRemarkRepository.ApproveRemarkAsync(id);
            if (result == 0)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}