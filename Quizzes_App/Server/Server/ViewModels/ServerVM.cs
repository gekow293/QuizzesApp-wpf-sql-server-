using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.IO;
using System.Text;
using System.Threading;
using Server.Models.QuizModel;
using Server.Database;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Server.ViewModels
{
    /// <summary>
    /// Модель представления, связывающая MainView и ServerTCP
    /// </summary>
    public class ServerVM : BaseVM
    {
        private Mutex clientMutex; // Для синхронизации 
        private Dispatcher dispatcher; // Для обновления свойств, на которые есть Binding
        private ServerTCP server; // Сервер
        public ObservableCollection<ConnectedClient> ClientList { get; set; } // Список клиентов для ServerWindow
        public ObservableCollection<Quiz> QuizList { get; set; } // Список тестов для ServerWindow
        public ObservableCollection<Question> QuestionList { get; set; } // Список вопросов для ServerWindow

        public string Log { get; set; } // Лог для ServerWindow

        public event EventHandler OpenQuestionsEvent;
        public event EventHandler BackClickEvent;

        private Controller controller;

        private Quiz openedQuiz;
        
        /// <summary>
        /// Инициализация необходимых для MainView данных
        /// </summary>
        public ServerVM(string ipAddress, int port)
        {
            File.Delete("Log.txt");
            dispatcher = Dispatcher.CurrentDispatcher;
            clientMutex = new Mutex();
            server = new ServerTCP(ipAddress, port);
            QuizList = new ObservableCollection<Quiz>();
            QuestionList = new ObservableCollection<Question>();
            server.ClientList.CollectionChanged += ClientList_CollectionChanged;
            server.AddToLog += Server_AddToLog;
            server.Start();

            controller = new Controller();
        }

        #region События

        /// <summary>
        /// Обновление свойства-коллекции ClientList => обновление данных в MainView
        /// </summary>
        private void ClientList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            clientMutex.WaitOne();
            ClientList = server.ClientList;
            OnPropertyChanged("ClientList");
            clientMutex.ReleaseMutex();
        }

        
        /// <summary>
        /// Обновление свойства Log => обновление данных в MainView
        /// </summary>
        private void Server_AddToLog(object? sender, string str)
        {
            clientMutex.WaitOne();
            string text = $"{DateTime.Now.ToLongTimeString()} {str}";
            using (StreamWriter sw = new StreamWriter("Log.txt", true, Encoding.Default))
            {
                sw.WriteLine(text);
            }
            dispatcher.BeginInvoke(new Action(() => Log += $"{text}\n"));
            OnPropertyChanged("Log");
            clientMutex.ReleaseMutex();
        }

        #endregion События

        #region Команды

        private BaseCommand startServerCommand;
        private BaseCommand saveQuizCommand;
        private BaseCommand loadQuizzesCommand;
        private BaseCommand deleteQuizCommand;
        private BaseCommand openQuizCommand;
        private BaseCommand saveQuestionsCommand;
        private BaseCommand deleteQuestionsCommand;
        private BaseCommand addQuestionsCommand;
        private BaseCommand backButtonCommand;
        private BaseCommand deleteAnswerCommand;

        /// <summary>
        /// Старт сервета
        /// </summary>
        public BaseCommand StartServerCommand => startServerCommand ??
                    (startServerCommand = new BaseCommand(obj => server.Start()));
        /// <summary>
        /// Удаление варианта ответа
        /// </summary>
        public BaseCommand DeleteAnswerCommand => deleteAnswerCommand ?? (deleteAnswerCommand = new BaseCommand(async obj =>
        {
            var i = await controller.DeleteAnswerQuestion(obj);

            if (i > 0)
            {
                QuestionList.Clear();
                (await controller.GetQuestionsFromQuiz(openedQuiz)).ForEach(x => QuestionList.Add(x));
            }
        }));
        /// <summary>
        /// По нажатию кнопки "Назад"
        /// </summary>
        public BaseCommand BackButtonCommand => backButtonCommand ?? (backButtonCommand = new BaseCommand(async obj =>
        {
            QuizList.Clear();
            (await controller.LoadQuizzes()).ForEach(x => QuizList.Add(x));

            BackClickEvent(this, null);
        }));
        /// <summary>
        /// Добавить вопрос к тесту
        /// </summary>
        public BaseCommand AddQuestionsCommand => addQuestionsCommand ?? (addQuestionsCommand = new BaseCommand(async obj =>
        {
            if (openedQuiz != null)
            {
                var i = await controller.AddQuestionsQuiz(openedQuiz);

                if (i > 0)
                {
                    QuestionList.Clear();
                    (await controller.GetQuestionsFromQuiz(openedQuiz)).ForEach(x => QuestionList.Add(x));
                }
            }
        }));
        /// <summary>
        /// Удаление вопроса из теста
        /// </summary>
        public BaseCommand DeleteQuestionsCommand => deleteQuestionsCommand ?? (deleteQuestionsCommand = new BaseCommand(async obj =>
        {
            var i = await controller.DeleteQuestionQuiz(obj);

            if (i > 0)
            {
                QuestionList.Clear();
                (await controller.GetQuestionsFromQuiz(openedQuiz)).ForEach(x => QuestionList.Add(x));
            }
        }));
        /// <summary>
        /// Сохранить изменения в вопросе
        /// </summary>
        public BaseCommand SaveQuestionsCommand => saveQuestionsCommand ?? (saveQuestionsCommand = new BaseCommand(async obj =>
        {
            var i = await controller.UpdateQuestionsQuiz(obj);

            if(i > 0)
            {
                QuestionList.Clear();
                (await controller.GetQuestionsFromQuiz(openedQuiz)).ForEach(x => QuestionList.Add(x));
            }
        }));
        /// <summary>
        /// Открыть тест
        /// </summary>
        public BaseCommand OpenQuizCommand => openQuizCommand ?? (openQuizCommand = new BaseCommand(async obj =>
        {
            if (obj is Quiz quiz)
            {
                openedQuiz = quiz;
                QuestionList.Clear();
                (await controller.GetQuestionsFromQuiz(openedQuiz)).ForEach(x => QuestionList.Add(x));

                OpenQuestionsEvent(this, null);
            }
        }));
        /// <summary>
        /// Удалить тест
        /// </summary>
        public BaseCommand DeleteQuizCommand => deleteQuizCommand ?? (deleteQuizCommand = new BaseCommand(async obj =>
        {
            var i = await controller.DeleteQuiz(obj);
            if(i > 0)
            {
                QuizList.Clear();
                (await controller.LoadQuizzes()).ForEach(x => QuizList.Add(x));
            }
        }));
        /// <summary>
        /// Сохранить тест
        /// </summary>
        public BaseCommand SaveQuizCommand => saveQuizCommand ?? (saveQuizCommand = new BaseCommand(async obj =>
        {
            if (obj is Quiz quiz)
            {
                var i = await controller.UpdateQuiz(quiz);

                if(i > 0)
                {
                    QuizList.Clear();
                    (await controller.LoadQuizzes()).ForEach(x => QuizList.Add(x));
                }
            }
        }));
        /// <summary>
        /// Загрузить все тесты
        /// </summary>
        public BaseCommand LoadQuizzesCommand => loadQuizzesCommand ?? (loadQuizzesCommand = new BaseCommand(async obj =>
        {
            QuizList.Clear();
            (await controller.LoadQuizzes()).ForEach(x => QuizList.Add(x));
        }));

        #endregion Команды
    }
}
