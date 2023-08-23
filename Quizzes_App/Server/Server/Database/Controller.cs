﻿using Client.ViewModels;
using Microsoft.EntityFrameworkCore;
using Server.Models.QuizModel;
using Server.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Server.Database
{
    /// <summary>
    /// Класс действий с базой данных
    /// </summary>
    public class Controller
    {
        public event EventHandler<string> AddToLog; // Событие записи в лог

        public Controller()
        {
        }

        //обработка тестов ==================================================>

        /// <summary>
        /// Обновление или добавление
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <returns></returns>
        public async Task UpdateQuiz(object quiz)
        {
            if(quiz != null)
                await Task.Run( async () =>
                {
                    using (DataContext db = new DataContext())
                    {
                        if (quiz is Quiz q)
                        {
                            if (q.Id != 0)
                            {
                                db.Quizzes.Update(q);
                            }
                            else
                            {
                                await db.Quizzes.AddAsync(q);
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                });
        }

        /// <summary>
        /// Удаление
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <returns></returns>
        public async Task DeleteQuiz(object quiz)
        {
            if(quiz != null)
                await Task.Run(async () =>
                {
                    if (quiz is Quiz q && q.Id != 0)
                        using (DataContext db = new DataContext())
                        {
                            _ = db.Quizzes.Remove(q);
                            await db.SaveChangesAsync();
                        }
                });
        }

        /// <summary>
        /// Выгрузка для клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        public async Task<List<QuizVM>> LoadQuizzesForClient(ConnectedClient client)
        {
            var list = new List<QuizVM>();

            await Task.Run( async () => 
            {
                using (DataContext db = new DataContext())
                {
                    var quizList = new List<Quiz>();

                    foreach (var q in db.Quizzes)
                    {
                        quizList.Add(q);
                    }

                    foreach (var ql in quizList)
                    {
                        var userQuiz = await db.UserQuizzes.FirstOrDefaultAsync(x => x.QuizId == ql.Id && x.User.UserName == client.Login);

                        QuizVM qVM;

                        if(userQuiz != null)
                        {
                            qVM = new QuizVM(ql) { Passing = userQuiz?.Passing };
                        }
                        else
                        {
                            qVM = new QuizVM(ql) { Passing = "False" };
                        }

                        list.Add(qVM);
                    }
                }
            });

            return list;
        }

        /// <summary>
        /// Выгрузка в интерфейс программы
        /// </summary>
        /// <returns></returns>
        public async Task<List<Quiz>> LoadQuizzes()
        {
            var quizList = new List<Quiz>();

            await Task.Run(() =>
            {
                using (DataContext db = new DataContext())
                {
                    foreach (var q in db.Quizzes)
                    {
                        quizList.Add(q);
                    }  
                }
            });

            return quizList;
        }

        /// <summary>
        /// Проверка результатов
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        public async Task<string> ExamenationQuiz(Quiz quiz, ConnectedClient client)
        {
            var origQuestions = await GetQuestionsFromQuiz(quiz);

            string passing;

            foreach (var sentQuestion in quiz.Questions)
            {
                var question = origQuestions.FirstOrDefault(x => x.QuestionText == sentQuestion.QuestionText);

                if (question != null)

                    foreach (var sentAnswer in sentQuestion.Answers)
                    {
                        var answer = question.Answers.First(x => x.AnswerText == sentAnswer.AnswerText);

                        if (answer.IsCorrect != sentAnswer.IsCorrect)
                        {
                            passing = "False";

                            await SaveQuizResultsForUser(client.Login, quiz, passing);

                            return passing;
                        }
                    }
            }

            passing = "True";

            await SaveQuizResultsForUser(client.Login, quiz, passing);

            return passing;
        }

        //Обработка вопросов ==================================================>

        /// <summary>
        /// Выгрузка на интерфейс или для клиента
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        public async Task<List<Question>> GetQuestionsFromQuiz(object quiz, ConnectedClient? client = null)
        {
            List<Question> listq = null!;

            if(quiz != null)
                await Task.Run(async () =>
                {
                    if (quiz is Quiz qz)
                    {
                        using (DataContext db = new DataContext())
                        {
                            listq = db.Questions.Where(i => i.QuizId == qz.Id).Include(q => q.Answers).ToList();
                        }

                        if (client != null)
                        {
                            User? user;

                            using (DataContext db = new DataContext())
                            {
                                user = await db.Users.FirstOrDefaultAsync(x => x.UserName == client.Login);
                            }

                            if(user != null)
                                using (DataContext db = new DataContext())
                                {
                                    var userQuiz = new UserQuiz() { QuizId = qz.Id, UserId = user.Id };

                                    if (!db.UserQuizzes.Contains(new UserQuiz() { QuizId = qz.Id, UserId = user.Id }))
                                    {
                                        if (userQuiz.Passing == "True")
                                        {
                                            userQuiz.Passing = "False";
                                            _ = await db.UserQuizzes.AddAsync(userQuiz);
                                            await db.SaveChangesAsync();
                                        }
                                    }
                                }
                        }
                    }
                });

            return listq;
        }

        /// <summary>
        /// Обновление
        /// </summary>
        /// <param name="question">вопрос</param>
        /// <returns></returns>
        public async Task UpdateQuestionsQuiz(object question)
        {
            if(question != null)
                await Task.Run(async() =>
                {
                    if (question is Question q && q.Answers.Any(x => x.IsCorrect == true && q.Answers.All(x => x.AnswerText != null)))
                    {
                        using (DataContext db = new DataContext())
                        {
                            db.Questions.Update(q);
                            await db.SaveChangesAsync();
                        }
                        using (DataContext db = new DataContext())
                        {
                            var userQuiz = await db.UserQuizzes.FirstOrDefaultAsync(x => x.QuizId == q.QuizId);

                            if (userQuiz != null)
                            {
                                userQuiz.Passing = "False";

                                db.Entry(userQuiz).State = EntityState.Modified;
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ответы не должны быть пустыми");
                    }
                });
        }

        /// <summary>
        /// Добавление к тесту
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <returns></returns>
        public async Task AddQuestionsQuiz(object quiz)
        {
            if(quiz != null)
                await Task.Run(async () =>
                {
                    if (quiz is Quiz qz)
                        using (DataContext db = new DataContext())
                        {
                            var q = new Question()
                            {
                                QuestionText = "",
                                QuizId = qz.Id,
                                Answers = new ObservableCollection<Answer>()
                            };
                            _ = await db.Questions.AddAsync(q);
                            await db.SaveChangesAsync();
                        }
                });
        }

        /// <summary>
        /// Удаление
        /// </summary>
        /// <param name="question">вопрос</param>
        /// <returns></returns>
        public async Task DeleteQuestionQuiz(object question)
        {
            if(question != null)
                await Task.Run(async () =>
                {
                    if (question is Question q)
                        using (DataContext db = new DataContext())
                        {
                            _ = db.Questions.Remove(q);
                            await db.SaveChangesAsync();
                        }
                });
        }

        /// <summary>
        /// Удаление вариантов ответа
        /// </summary>
        /// <param name="answer">ответ</param>
        /// <returns></returns>
        public async Task DeleteAnswerQuestion(object answer)
        {
            if (answer != null)
                await Task.Run(async () =>
                {
                    if (answer is Answer a && a.Id != 0)
                    {
                        using (DataContext db = new DataContext())
                        {
                            var question = await db.Questions.FirstOrDefaultAsync(q => q.Id == a.Question!.Id);
                            _ = question?.Answers.Remove(a);
                            await db.SaveChangesAsync();
                        } 
                    }
                });
        }

        //обработка действий с клиентами ==================================================>

        /// <summary>
        /// Сохранение результатов тестирования клиента
        /// </summary>
        /// <param name="login">логин клинта</param>
        /// <param name="quiz">тест</param>
        /// <param name="passing">факт прохождения теста</param>
        /// <returns></returns>
        private async Task SaveQuizResultsForUser(string login, Quiz quiz, string passing)
        {
            if (login != string.Empty)
                await Task.Run(async () =>
                {
                    User? user;
                    UserQuiz? findUQ;

                    using (DataContext db = new DataContext())
                    {
                        user = await db.Users.FirstOrDefaultAsync(x => x.UserName == login);   
                    }

                    using (DataContext db = new DataContext())
                        findUQ = await db.UserQuizzes.FirstOrDefaultAsync(x => x.UserId == user!.Id && x.QuizId == quiz.Id);

                    using (DataContext db = new DataContext())
                    {
                        if (user != null)
                        {
                            var newUserQuiz = new UserQuiz() { UserId = user.Id, QuizId = quiz.Id, Passing = passing };

                            if (findUQ == null)
                                _ = await db.UserQuizzes.AddAsync(newUserQuiz);
                            else
                                db.Entry(newUserQuiz).State = EntityState.Modified;

                            await db.SaveChangesAsync();
                        }
                    }
                });
        }

        /// <summary>
        /// Добавление нового клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        public async Task SaveUserToDatabase(ConnectedClient client)
        {
            if (client != null)
                await Task.Run(async () =>
                {
                    var user = new User() { UserName = client.Login };

                    using (DataContext db = new DataContext())
                    {
                        var userFind = await db.Users.FirstOrDefaultAsync(x => x.UserName == client.Login);

                        if (userFind == null)
                        {
                            _ = await db.Users.AddAsync(user);
                            await db.SaveChangesAsync();
                        }
                    }
                });
        }
    }
}
