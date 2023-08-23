using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Server.Views;
using Server.ViewModels;
using Server.Other;
using Server.Models.QuizModel;
using Server.Database;
using MaterialDesignThemes.Wpf;
using System.Windows.Data;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Server.Views
{
    /// <summary>
    /// Логика интерфейса ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        List<UIElement> listUI; // Список ui элементов для смены их в grid

        Controller controller; // контроллер - работа сервера с базой данных

        public object? openedQuiz { get; set; }

        /// <summary>
        /// Настройка окна ServerWindows
        /// </summary>
        public ServerWindow()
        {
            AppConfigManager.CreateConfigParameters("ip_address", "port");

            if (Authorization(out string address, out int port))
            {
                DataContext = new ServerVM(address, port);
            }
            else
            {
                Application.Current.Shutdown();
            }

            InitializeComponent();

            listUI = new List<UIElement>
            {
                clientsListBox,
                quizzesGrid,
                logTextBox
            };
            dataGrid.Children.Clear();
            dataGrid.Children.Add(listUI[0]);
            listViewMenu.SelectedIndex = 0;

            controller = new Controller();
        }

        

        /// <summary>
        /// Вызов модального окна для ввода IP-адресса и порта сервера
        /// </summary>
        public bool Authorization(out string address, out int port)
        {
            AuthorizationWindow authorizationWindow = new AuthorizationWindow();
            if (authorizationWindow.ShowDialog() == true)
            {
                address = authorizationWindow.Address;
                port = int.Parse(authorizationWindow.Port);
                return true;
            }
            else
            {
                address = "";
                port = -1;
                return false;
            }
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

        /// <summary>
        /// Сохранение или редактирование теста
        /// </summary>
        private async void SaveQuiz_Click(object sender, RoutedEventArgs e)
        {
            await controller.UpdateQuiz(quizzesListBox.SelectedValue);
            quizzesListBox.ItemsSource = await controller.LoadQuizzes();
        }
        /// <summary>
        /// Удаление теста
        /// </summary>
        private async void DeleteQuiz_Click(object sender, RoutedEventArgs e)
        {
            var quiz = (sender as Button)!.DataContext;

            if(quiz != null)
            {
                await controller.DeleteQuiz(quiz);
                quizzesListBox.ItemsSource = await controller.LoadQuizzes();
            }
        }

        /// <summary>
        /// Выгрузка тестов в интерфейс
        /// </summary>
        private async void QuizzesListBox_Loaded(object sender, RoutedEventArgs e)
        {
            quizzesListBox.ItemsSource = await controller.LoadQuizzes();
        }

        /// <summary>
        /// Открытие теста
        /// </summary>
        private async void OpenQuiz_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            openedQuiz = (sender as Button)!.DataContext;

            if (openedQuiz != null)
            {
                dataGrid.Children.Clear();
                dataGrid.Children.Add(questionsGrid);

                questionsList.ItemsSource = await controller.GetQuestionsFromQuiz(openedQuiz);

                backBtn.Visibility = Visibility.Visible;
            }     
        }
        
        /// <summary>
        /// Сохранение изменение вопроса
        /// </summary>
        private async void SaveQuestions_Click(object sender, RoutedEventArgs e)
        {
            await controller.UpdateQuestionsQuiz((sender as Button)!.DataContext);
        }
        
        /// <summary>
        /// Добавление вопроса
        /// </summary>
        private async void AddQuestions_Click(object sender, RoutedEventArgs e)
        {
            if (openedQuiz != null)
            {
                await controller.AddQuestionsQuiz(openedQuiz);

                questionsList.ItemsSource = await controller.GetQuestionsFromQuiz(openedQuiz);
            }
        }
        
        /// <summary>
        /// Удаление вопроса
        /// </summary>
        private async void DeleteQuestions_Click(object sender, RoutedEventArgs e)
        {
            var question = (sender as Button)!.DataContext;

            if (question != null)
            {
                await controller.DeleteQuestionQuiz(question);

                if (openedQuiz != null)
                    questionsList.ItemsSource = await controller.GetQuestionsFromQuiz(openedQuiz);
            }
        }

        /// <summary>
        /// Событие по нажатию кнопки "Назад"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void backBtn_Click(object sender, RoutedEventArgs e)
        {
            int index = listViewMenu.SelectedIndex;
            dataGrid.Children.Clear();
            dataGrid.Children.Add(listUI[index]);

            quizzesListBox.ItemsSource = await controller.LoadQuizzes();

            backBtn.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Удаление варианта ответа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DelAnswer_Click(object sender, RoutedEventArgs e)
        {
            var answer = (sender as PackIcon)!.DataContext;

            if (answer != null && answer is Answer a)
            {
                a.Question?.Answers.Remove(a);

                await controller.DeleteAnswerQuestion(answer);
            }
        }
    }
}
