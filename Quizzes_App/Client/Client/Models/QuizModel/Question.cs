using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.QuizModel
{
    public class Question
    {
        public Question() 
        {
            Answers = new ObservableCollection<Answer>();
        }

        [Key]
        public int Id { get; set; }
        public int QuizId { get; set; }
        [Required]
        public string? Level { get; set; }
        [Required]
        public Quiz? Quiz { get; set; }
        [Required]
        public ObservableCollection<Answer> Answers { get; set; }
        [Required]
        public string? QuestionText { get; set; }
    }
}
