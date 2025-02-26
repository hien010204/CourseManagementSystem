
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
        public Task<List<StudentSubmissionDto>> GetUngradedSubmissions();
        public Task<List<StudentSubmissionDto>> GetNoFeedbackSubmissions();
        public Task<StudentSubmissionDto> GetGradeAndFeedback(int submissionId);
        public Task<List<StudentSubmissionDto>> GetGradedAndFeedbackSubmissions();
        public Task<List<Assignment>> FilterAssignmentsByDueDate(int courseId, DateOnly? dueDate);
        public Task<List<User>> GetStudentsMissingSubmission(int assignmentId);
        public Task<AssignmentSubmission> EditGradeAndFeedback(int submissionId, GradeAssignmentDto gradeDto);
    }
}
