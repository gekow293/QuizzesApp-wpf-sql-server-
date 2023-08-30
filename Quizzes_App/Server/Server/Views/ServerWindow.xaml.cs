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
//using System.Collections.ObjectModel;

namespace Server.Views
{
    /// <summary>
    /// Логика интерфейса ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        List<UIElement> listUI; // Список ui элементов для смены их в grid
        ServerVM serverVM;

        public object? openedQuiz { get; set; }

        /// <summary>
        /// Настройка окна ServerWindows
        /// </summary>
        public ServerWindow()
        {
            AppConfigManager.CreateConfigParameters("ip_address", "port");

            if (Authorization(out string address, out int port))
            {
                serverVM = new ServerVM(address, port);
                serverVM.OpenQuestionsEvent += ServerVM_OpenQuestionsEvent;
                serverVM.BackClickEvent += ServerVM_BackClickEvent;
                DataContext = serverVM;
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

        #region События

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

        private void ServerVM_OpenQuestionsEvent(object? sender, EventArgs e)
        {
            dataGrid.Children.Clear();
            dataGrid.Children.Add(questionsGrid);
            backBtn.Visibility = Visibility.Visible;
        }

        private void ServerVM_BackClickEvent(object? sender, EventArgs e)
        {
            int index = listViewMenu.SelectedIndex;
            dataGrid.Children.Clear();
            dataGrid.Children.Add(listUI[index]);
            backBtn.Visibility = Visibility.Collapsed;
        }

        #endregion
    }
}
