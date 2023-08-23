using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models.QuizModel
{
    public class UserQuiz
    {
        public int UserId { get; set; }
        public User? User { get; set; }
        public int QuizId { get; set; }
        public Quiz? Quiz { get; set; }
        public bool Passing { get; set; }
    }
}
