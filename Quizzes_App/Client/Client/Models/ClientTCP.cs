using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Linq;
using Server.Models.QuizModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Client.ViewModels;
using Client.Other;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Windows.Media;

namespace Client
{
    /// <summary>
    /// Модель клиента
    /// </summary>
    public class ClientTCP
    {
        private IPEndPoint ipEndPoint; // Адрес и порт сервера
        private string login;          // Логин клиента
        private Socket tcpClient;   // Связь с сервером
        private NetworkStream stream;  // Для передачи сообщений
        private CancellationTokenSource serverMessagingToken; // Остановка прослушивания сообщений от сервера
        private Mutex mutex;
        public event EventHandler StopServer;               // Событие остановки сервера
        public event EventHandler<string> AddToLog;         // Событие записи в лог
        public ObservableCollection<QuizVM> QuizList { get; set; }         // Список тестов для ServerWindow
        public ObservableCollection<Question> QuestionList { get; set; }  // Список вопросов для ServerWindow

        /// <summary>
        /// Инициализация настроек клиента
        /// </summary>
        /// <param name="ipAddress">IP-адрес сервера</param>
        /// <param name="port">номер порта сервера</param>
        /// <param name="login">логин пользователя</param>
        public ClientTCP(string ipAddress, int port, string login)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            this.login = login;
            tcpClient = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            QuizList = new ObservableCollection<QuizVM>();
            QuestionList = new ObservableCollection<Question>();
            serverMessagingToken = new CancellationTokenSource();
            mutex = new Mutex();
        }

        /// <summary>
        /// Выполняет подключение к серверу
        /// </summary>
        public async Task ConnectAsync()
        {
            await tcpClient.ConnectAsync(ipEndPoint);
            stream = new NetworkStream(tcpClient);

            await SendMessageAsync(login);
            _ = Task.Run(() => ServerMessaging());
            AddToLog?.Invoke(this, "Conection successful");
        }

        /// <summary>
        /// Выполняет отключение от сервера
        /// </summary>
        public async void DisconnectAsync()
        {
            await SendMessageAsync("disconnect_request");
            serverMessagingToken.Cancel();
            stream.Close();
            tcpClient.Shutdown(SocketShutdown.Both);
            AddToLog(this, "Disconnected");
        }

        /// <summary>
        /// Отсылает сообщение на сервер через NetworkStream
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public async Task SendMessageAsync(string message)
        {
            byte[] buff = Encoding.UTF8.GetBytes(message);
            await tcpClient.SendAsync(buff, SocketFlags.None);
        }

        /// <summary>
        /// Принимает сообщение от сервера через NetworkStream
        /// </summary>
        public async Task<string> ReceiveMessageAsync()
        {
            StringBuilder str = new StringBuilder();

            if (tcpClient.Poll(1000000, SelectMode.SelectRead))
            {
                while (stream.DataAvailable)
                {
                    byte[] buff = new byte[1024 * 1024];
                    var bytes = await tcpClient.ReceiveAsync(buff, SocketFlags.None);
                    str.Append(Encoding.UTF8.GetString(buff, 0, bytes));
                }
            }
            return str.ToString();
        }

        /// <summary>
        /// Обработка сообщений от сервера
        /// </summary>
        private async void ServerMessaging()
        {
            string command = "";
            while (!serverMessagingToken.Token.IsCancellationRequested)
            {
                if (tcpClient.Poll(10000000, SelectMode.SelectRead)) // Изменение на сокете
                {
                    if (stream.DataAvailable) // Есть какие-то данные
                    {
                        if (!serverMessagingToken.Token.IsCancellationRequested) // Если в процессе проверки сокета не было отмены
                        {
                            command = await ReceiveMessageAsync();
                            switch (command)
                            {
                                case "stop_server":
                                    StopClient();
                                    break;
                                default:
                                    AddToLog(this, command);
                                    break;
                            }
                        }
                    }
                    else // Если нет, то произошел обрыв связи
                    {
                        StopClient();
                    }
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Остановка клиента при остановке сервера
        /// </summary>
        private void StopClient()
        {
            serverMessagingToken.Cancel();
            stream.Close();
            tcpClient.Shutdown(SocketShutdown.Both);
            StopServer(this, new EventArgs());
            AddToLog(this, "Server is stopped");
        }

        /// <summary>
        /// Запрос на получение тестов
        /// </summary>
        /// <returns></returns>
        public async Task RequestQuizzesAsync()
        {
            mutex.WaitOne();

            Task.Run(() => serverMessagingToken.Cancel()).Wait();

            await SendMessageAsync("get_quizzes_request");

            string result = string.Empty;

            while (result == string.Empty)
            {
                result = await ReceiveMessageAsync();

                if (result != string.Empty)
                {
                    QuizList.Clear();

                    foreach (var quizVM in JsonSerializer.Deserialize<List<QuizVM>>(result)!)
                    {
                        if (quizVM.Passing == "False") { quizVM.Passing = "Не пройден"; quizVM.Brush = new SolidColorBrush(Colors.Orange); }
                        else { quizVM.Passing = "Пройден"; quizVM.Brush = new SolidColorBrush(Colors.GreenYellow); }

                        QuizList.Add(quizVM);
                    }
                }
            }

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Запрос на получение вопросов теста
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <returns></returns>
        public async Task RequestQuestionsQuiz(object quiz)
        {
            mutex.WaitOne();

            if (quiz is QuizVM quizVM)
            {               
                Task.Run(() => serverMessagingToken.Cancel()).Wait(); // Отключаем обработку сообщений от сервера для синхронизации
                
                await SendMessageAsync("get_questions_of_quiz_request");

                await ReceiveMessageAsync();

                await SendMessageAsync(JsonSerializer.Serialize(quizVM, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) }));

                var result = await ReceiveMessageAsync();

                if (result != string.Empty)
                {
                    QuestionList.Clear();

                    foreach (var qt in JsonSerializer.Deserialize<List<Question>>(result)!)
                    {
                        qt.Answers.ToList().ForEach(x => x.IsCorrect = false);
                        QuestionList.Add(qt);
                    }
                }
            }

            mutex.ReleaseMutex();
        }

        /// <summary>
        /// Отправка результатов тестирования
        /// </summary>
        /// <param name="quiz">тест</param>
        /// <returns></returns>
        public async Task SendResultQuiz(object quiz)
        {
            mutex.WaitOne();

            if (quiz is QuizVM qVM)
            {
                Task.Run(() => serverMessagingToken.Cancel()).Wait();

                await SendMessageAsync("send_answers_of_quiz_request");

                await ReceiveMessageAsync();

                var sendQuiz = new Quiz() { Id = qVM.Id, Questions = qVM.Questions, Summary = qVM.Summary, Title = qVM.Title, Users = qVM.Users };

                await SendMessageAsync(JsonSerializer.Serialize(sendQuiz, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) }));

                var result = await ReceiveMessageAsync();

                qVM.Passing = result;

                var examQuiz = QuizList.FirstOrDefault(x => x.Title == qVM.Title);

                if(examQuiz != null)
                    if (result == "True")
                    {
                        examQuiz.Passing = "Пройден";
                        examQuiz.Brush = new SolidColorBrush(Colors.GreenYellow);
                    }
                    else
                    {
                        examQuiz.Passing = "Не пройден";
                        examQuiz.Brush = new SolidColorBrush(Colors.OrangeRed);
                    }
            }

            mutex.ReleaseMutex();
        }
    }
}
