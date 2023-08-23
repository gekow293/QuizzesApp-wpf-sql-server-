using System.Threading;
using System.Net.Sockets;

namespace Server
{
    /// <summary>
    /// Модель клиента
    /// </summary>
    public class ConnectedClient
    {
        public Socket TcpClient { get; set; }
        public string Login { get; set; }
        public CancellationTokenSource ClientToken { get; set; }
        public NetworkStream NetworkStream { get; set; }

        /// <summary>
        /// Инициализая подключенного клиента
        /// </summary>
        /// <param name="tcpClient">tcpClient, через который установлена связь с клиентом</param>
        /// <param name="login">Логин подключенного клиента</param>
        /// <param name="token">Токен для завершения задачи обработки сообщений</param>
        public ConnectedClient(Socket tcpClient, NetworkStream networkStream, string login, CancellationTokenSource token)
        {
            TcpClient = tcpClient;
            NetworkStream = networkStream;
            Login = login;
            ClientToken = token;
        }
    }
}
