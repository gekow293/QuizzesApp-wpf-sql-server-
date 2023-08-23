using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.IO;
using System.Text;
using System.Threading;
using Server.Models.QuizModel;
using Server.Database;

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
        public BaseCommand StartServerCommand => startServerCommand ??
                    (startServerCommand = new BaseCommand(obj => server.Start()));

        #endregion Команды
    }
}
