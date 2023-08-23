using System.Windows;
using Client.Views;
using Client.Other;
using System.Windows.Input;
using System;
using System.Windows.Threading;
using System.Collections.Generic;
using Microsoft.Win32;
using System.IO;
using Server.ViewModels;
using System.Reflection;
using System.Windows.Controls;
using Client.ViewModels;
using System.Threading;

namespace Client
{
    /// <summary>
    /// Логика интерфейса ClientWindow
    /// </summary>
    public partial class MainWindow : Window
    {
        private ClientTCP client;
        private ClientVM ClientVM;
        private Dispatcher dispatcher;
        private List<UIElement> listUI; // Список ui элементов для смены их в grid
        private QuizVM quizVM;

        /// <summary>
        /// Настройка окна ClientWindow 
        /// </summary>
        public MainWindow()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            AppConfigManager.CreateConfigParameters("ip_address", "port", "login");
            Authorization();
            InitializeComponent();
            listUI = new List<UIElement>
            {
                quizzesGrid,
                logTextBox
            };
            dataGrid.Children.Clear();
            dataGrid.Children.Add(listUI[0]);
            listViewMenu.SelectedIndex = 0;

            ClientVM = new ClientVM(client);
            DataContext = ClientVM;

        }

        /// <summary>
        /// Вызов модального окна для ввода IP-адресса, порта и логина
        /// </summary>
        private void Authorization()
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            if (authorizationWindow.ShowDialog() == true)
            {
                client = authorizationWindow.ReadyClient;
                client.StopServer += ServerStatus_Changed;
                client.AddToLog += LogList_AddData;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        #region События

        /// <summary>
        /// Добавить запись в лог
        /// </summary>
        private void LogList_AddData(object sender, string str)
        {
            string text = $"{DateTime.Now.ToLongTimeString()} {str}";
            dispatcher.BeginInvoke(new Action(() => logTextBox.Text += $"{text}\n"));
        }

        /// <summary>
        /// Обработка изменения статуса сервера
        /// </summary>
        private void ServerStatus_Changed(object sender, EventArgs e)
        {
            dispatcher.BeginInvoke(new Action(() => Authorization()));
        }

        /// <summary>
        /// Переключение по вкладкам меню
        /// </summary>
        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = listViewMenu.SelectedIndex;
            dataGrid.Children.Clear();
            dataGrid.Children.Add(listUI[index]);

            backBtn.Visibility = Visibility.Collapsed;
        }

        #endregion События

        /// <summary>
        /// Пройти тест
        /// </summary>
        private async void GoTest_Click(object sender, RoutedEventArgs e)
        {
            var quiz = (sender as Button)!.DataContext;
            
            if(quiz is QuizVM qVM)
            {
                quizVM = qVM;
            }
            await client.RequestQuestionsQuiz(quizVM);

            dataGrid.Children.Clear();
            dataGrid.Children.Add(questionsGrid);

            backBtn.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Получить тесты с сервера
        /// </summary>
        private async void GetQuizzes_Click(object sender, RoutedEventArgs e)
        {
            await client.RequestQuizzesAsync();

            dataGrid.Children.Clear();
            dataGrid.Children.Add(quizzesGrid);
        }

        /// <summary>
        /// Отправить результаты тестирования на сервер
        /// </summary>
        private async void SendResultQuiz_Click(object sender, RoutedEventArgs e)
        {
            if (quizVM != null)
            {
                quizVM.Questions = ClientVM.QuestionList;
                await client.SendResultQuiz(quizVM);
            }

            dataGrid.Children.Clear();
            dataGrid.Children.Add(quizzesGrid);

            backBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Событие нажатия кнопки "Назад"
        /// </summary>
        private async void backBtn_Click(object sender, RoutedEventArgs e)
        {
            await client.RequestQuizzesAsync();

            dataGrid.Children.Clear();
            dataGrid.Children.Add(quizzesGrid);

            backBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Выйти из аккаунта
        /// </summary>
        private void GoOutAccount_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            App.Current.Shutdown();
        }

    }
}
