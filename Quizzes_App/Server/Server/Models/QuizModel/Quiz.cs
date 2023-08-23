using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Server.Models.QuizModel
{
    public class Quiz
    {
        public Quiz()
        {
            Questions = new ObservableCollection<Question>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Summary { get; set; }
        public ObservableCollection<Question> Questions { get; set; }
        public ICollection<User>? Users { get; set; }
    }
}
