using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.IO;
using System.Text;
using System.Threading;
using Server.Models.QuizModel;
using Client;
using System.Windows;
using Client.ViewModels;

namespace Server.ViewModels
{
    /// <summary>
    /// Модель представления, связывающая MainView и ServerTCP
    /// </summary>
    public class ClientVM : BaseVM
    {
        private Mutex quizMutex; // Для синхронизации 
        private Mutex questionMutex; // Для синхронизации 
        private ClientTCP client; // Клиент
        public ObservableCollection<QuizVM> QuizList { get; set; }         // Список тестов для ServerWindow
        public ObservableCollection<Question> QuestionList { get; set; }  // Список вопросов для ServerWindow

        /// <summary>
        /// Инициализация необходимых для MainView данных
        /// </summary>
        public ClientVM(ClientTCP client)
        {
            QuizList = new ObservableCollection<QuizVM>();
            QuestionList = new ObservableCollection<Question>();
            quizMutex = new Mutex();
            questionMutex = new Mutex();
            this.client = client;

            client.QuizList.CollectionChanged += QuizList_CollectionChanged;
            client.QuestionList.CollectionChanged += QuestionList_CollectionChanged;
        }

        /// <summary>
        /// Обновление свойства QuestionList => обновление данных в MainView
        /// </summary>
        private void QuestionList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            questionMutex.WaitOne();
            QuestionList = client.QuestionList;
            OnPropertyChanged("QuestionList");
            questionMutex.ReleaseMutex();
        }

        /// <summary>
        /// Обновление свойства QuizList => обновление данных в MainView
        /// </summary>
        private void QuizList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            quizMutex.WaitOne();
            QuizList = client.QuizList;
            OnPropertyChanged("QuizList");
            quizMutex.ReleaseMutex();
        }
    }
}
