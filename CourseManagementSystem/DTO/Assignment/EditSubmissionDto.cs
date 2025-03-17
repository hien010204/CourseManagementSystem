using System.ComponentModel.DataAnnotations;

namespace CourseManagementSystem.DTO.Assignment
{
    public class EditSubmissionDto
    {
        [StringLength(500, ErrorMessage = "Submission link cannot exceed 500 characters.")]
        public string? SubmissionLink { get; set; }
    }
}
