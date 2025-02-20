
using CourseManagementSystem.DTO;
using CourseManagementSystem.DTO.Assignment;
using CourseManagementSystem.Models;

namespace CourseManagementSystem.Services.Assignments
{
    public interface IAssignmentService
    {
        public Task<List<Assignment>> GetAssignmentsByCourseId(int courseId);
        public Task<Assignment> GetAssignmentById(int assignmentId);
        public Task<Assignment> CreateAssignment(CreateAssignmentDto assignmentDto);
        public Task<Assignment> EditAssignment(int assignmentId, EditAssignmentDto assignmentDto);
        public Task<bool> DeleteAssignment(int assignmentId);
        public Task<AssignmentSubmissionDto> SubmitAssignment(int assignmentId, SubmitAssignmentDto submissionDto);
        public Task<List<StudentSubmissionDto>> GetSubmissionsByAssignmentId(int assignmentId);
        public Task<AssignmentSubmission> GradeAssignment(int submissionId, GradeAssignmentDto gradeDto);
    }
}
