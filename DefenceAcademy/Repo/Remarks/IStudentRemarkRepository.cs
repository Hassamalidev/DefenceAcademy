using DefenceAcademy.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DefenceAcademy.Repo.Remarks
{
    public interface IStudentRemarkRepository
    {
        Task<IEnumerable<StudentRemark>> GetAllRemarksAsync();
        Task<IEnumerable<StudentRemark>> GetApprovedRemarksAsync();
        Task<StudentRemark> GetRemarkByIdAsync(int id);
        Task<int> CreateRemarkAsync(StudentRemark remark);
        Task<int> DeleteRemarkAsync(int id);
        Task<int> ApproveRemarkAsync(int id);
        Task<IEnumerable<StudentRemark>> GetRemarksByApprovalStatusAsync(bool isApproved);
    }
}