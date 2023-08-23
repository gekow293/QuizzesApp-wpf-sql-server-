using System.ComponentModel.DataAnnotations;

namespace Server.Models.QuizModel
{
    public class Answer
    {
        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        [Required]
        public bool IsCorrect { get; set; }
        [Required]
        public string? AnswerText { get; set; }
        public Question? Question { get; set; }
    }
}