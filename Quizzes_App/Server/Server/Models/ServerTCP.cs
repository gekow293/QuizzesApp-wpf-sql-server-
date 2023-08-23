using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;
using Server.Models.QuizModel;
using Server.Database;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Client.ViewModels;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Server
{
    /// <summary>
    /// Модель сервера
    /// </summary>
    public class ServerTCP
    {
        private IPEndPoint ipEndPoint; // Адрес и порт сервера
        private readonly Dispatcher dispatcher;  // Для изменения ObservableCollection из других потоков
        private readonly CancellationTokenSource listenToken; // Отмена прослушивания 
        private Controller controller;
        public ObservableCollection<ConnectedClient> ClientList { get; set; } // Список клиентов
        public event EventHandler<string> AddToLog; // Событие записи в лог
        private Socket server;

        /// <summary>
        /// Инициализация настроек сервера
        /// </summary>
        public ServerTCP(String ipAddress, int serverPort)
        {
            ipEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), serverPort);
            this.server = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this.server.Bind(ipEndPoint);
            dispatcher = Dispatcher.CurrentDispatcher;
            listenToken = new CancellationTokenSource();
            ClientList = new ObservableCollection<ConnectedClient>();
            controller = new Controller();
        }

        #region Public methods

        /// <summary>
        /// Запуск прослушивания входящих соединений
        /// </summary>
        public async void Start()
        {
            this.server.Listen(100);
            _ = Task.Run(() => ListeningIncomingConnetions());
            AddToLog(this, "Server is running");
        }

        /// <summary>
        /// Остановка сервера 
        /// </summary>
        public Task Stop()
        {
            return Task.Run(() =>
            {
                listenToken.Cancel();
                foreach (var client in ClientList)
                {
                    CloseClient(client);
                }
                ClientList.Clear();
                server.Shutdown(SocketShutdown.Both);
            });
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Закрытие соединения с клиентом по инициативе сервера
        /// </summary>
        /// <param name="client">клиент</param>
        private async void CloseClient(ConnectedClient client)
        {
            client.ClientToken.Cancel();
            await SendMessage("stop_server", client);
            client.TcpClient.Shutdown(SocketShutdown.Both);
            _ = dispatcher.BeginInvoke(new Action(() => ClientList.Remove(client)));
            AddToLog(this, $"{client.Login}: disconnected");
        }

        /// <summary>
        /// Прослушивание входящих соединений
        /// </summary>
        private async void ListeningIncomingConnetions()
        {
            Socket newClient;
            while (!listenToken.Token.IsCancellationRequested)
            {
                newClient = await server.AcceptAsync();
                NetworkStream networkStream = new NetworkStream(newClient);

                string clientName = await ReceiveMessage(newClient); // Получаем имя клиента и добавляем его в список
                ConnectedClient client = new ConnectedClient(newClient, networkStream, clientName, new CancellationTokenSource());

                await controller.SaveUserToDatabase(client);

                _ = dispatcher.BeginInvoke(new Action(() => ClientList.Add(client)));

                _ = Task.Run(() => ClientMessaging(client));

                AddToLog(this, $"{client.Login} successful connected");
            }
        }

        /// <summary>
        /// Обработка сообщений от клиента
        /// </summary>
        /// <param name="client">клиент</param>
        private async void ClientMessaging(ConnectedClient client)
        {
            string command = "";
            while (!client.ClientToken.Token.IsCancellationRequested) // Обрабатываем, пока клиент не отключится
            {
                if (client.TcpClient.Poll(1000000, SelectMode.SelectRead)) // Изменение на сокете
                {
                    if (client.NetworkStream.DataAvailable) // Есть какие-то данные
                    {
                        command = await ReceiveMessage(client.TcpClient);
                        switch (command)
                        {
                            case "disconnect_request":
                                ClientDisconnection(client);
                                break;
                            case "get_quizzes_request":
                                await SendQuizzes(client);
                                break;
                            case "get_questions_of_quiz_request":
                                await SendQuestionsQuiz(client);
                                break;
                            case "send_answers_of_quiz_request":
                                await ReceiveResultsQuiz(client);
                                break;
                            default:
                                AddToLog(this, $"{client.Login}: {command}");
                                break;
                        }
                    }
                    else // Если нет, то произошел обрыв связи
                    {
                        ClientDisconnection(client);
                    }
                }
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Получение результатов тестирования клиента
        /// </summary>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        private async Task ReceiveResultsQuiz(ConnectedClient client)
        {
            try
            {
                string result = string.Empty;

                await SendMessage("", client);

                string quizStr = await ReceiveMessage(client.TcpClient);

                if (quizStr != string.Empty)
                {
                    var quiz = JsonSerializer.Deserialize<Quiz>(quizStr)!;

                    result = await controller.ExamenationQuiz(quiz, client);
                }

                await SendMessage(result, client);
            }
            catch (Exception e)
            {
                AddToLog(this, $"{client.Login}: {e.Message}");
            }
        }

        /// <summary>
        /// Отправка вопросов теста клиенту
        /// </summary>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        private async Task SendQuestionsQuiz(ConnectedClient client)
        {
            try
            {
                await SendMessage("", client);

                string quizStr = await ReceiveMessage(client.TcpClient);

                var questions = await controller.GetQuestionsFromQuiz(JsonSerializer.Deserialize<Quiz>(quizStr)!, client);

                questions.ForEach(x => x.Answers.ToList().ForEach(y => y.Question = null!));

                await SendMessage(JsonSerializer.Serialize(questions, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) }), client);
            }
            catch (Exception e)
            {
                AddToLog(this, $"{client.Login}: {e.Message}");
            }
        }

        /// <summary>
        /// Отправка тестов клиенту
        /// </summary>
        /// <param name="client">клиент</param>
        /// <returns></returns>
        private async Task SendQuizzes(ConnectedClient client)
        {
            try
            {
                var quizes = await controller.LoadQuizzesForClient(client);

                await SendMessage(JsonSerializer.Serialize(quizes, new JsonSerializerOptions() { Encoder = JavaScriptEncoder.Create(UnicodeRanges.All) }), client);
            }
            catch (Exception e)
            {
                AddToLog(this, $"{client.Login}: {e.Message}");
            }
        }

        /// <summary>
        /// Отсылает сообщение клиенту
        /// </summary>
        /// <param name="client">клиент</param>
        /// <param name="message">текст сообщения</param>
        private async Task SendMessage(string message, ConnectedClient client)
        {
            byte[] buff = Encoding.UTF8.GetBytes(message);
            await client.TcpClient.SendAsync(buff, 0);
        }

        /// <summary>
        /// Принимает сообщение от клиента
        /// </summary>
        /// <param name="networkStream">поток данных для доступа к сети клиента</param>
        private async Task<string> ReceiveMessage(Socket client)
        {
            byte[] data = new byte[1024 * 1024];
            int recv = await client.ReceiveAsync(data, SocketFlags.None);
            return Encoding.UTF8.GetString(data, 0, recv);
        }

        /// <summary>
        /// Закрытие соединения с клиентом по инициативе клиента
        /// </summary>
        /// <param name="client">клиент</param>
        private void ClientDisconnection(ConnectedClient client)
        {
            client.TcpClient.Shutdown(SocketShutdown.Both);
            client.ClientToken.Cancel();
            dispatcher.BeginInvoke(new Action(() => ClientList.Remove(client)));
            AddToLog(this, $"{client.Login}: disconnected");
        }

        #endregion
    }
}
