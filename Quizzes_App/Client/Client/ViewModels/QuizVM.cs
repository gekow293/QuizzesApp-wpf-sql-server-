using Server.Models.QuizModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Client.ViewModels
{
    /// <summary>
    /// Модель тестов
    /// </summary>
    public class QuizVM
    {
        public QuizVM(Quiz quiz)
        {
            Id = quiz.Id;
            Title = quiz.Title;
            Summary = quiz.Summary;
            Questions = quiz.Questions;
            Users = quiz.Users;
        }

        public QuizVM() { }

        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public ObservableCollection<Question> Questions { get; set; }
        public ICollection<User>? Users { get; set; }
        public string? Passing { get; set; }
        [JsonIgnore]
        public Brush Brush { get; set; }
    }
}
