using EventManagement.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EventManagmentApp
{
    public partial class Authentication : RoundedWindow
    {
        public Authentication()
        {
            InitializeComponent();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (ApplicationContext context = new ApplicationContext())
            {
                bool bdConnection = context.Database.CanConnect();
                if (bdConnection) tbDataBaseCon.Text = "Успешно!";
                else tbDataBaseCon.Text = "Отсутствует!";
            }
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string login = tbLogin.Text;
            string pass = tbPassword.Password;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show($"Заполните поля ввода данных!");
            }
            else
            {
                using (ApplicationContext context = new ApplicationContext())
                {
                    var user = context.Users.FirstOrDefault(u => u.Login == login);
                    if (user != null)
                    {
                        if (user.Password != pass)
                        {
                            MessageBox.Show($"Неверный пароль!");
                        }
                        else
                        {
                            int roleId = context.Users
                                .Where(u => u.Login == login)
                                .Select(u => u.RoleId)
                                .FirstOrDefault();
                            switch (roleId)
                            {
                                case 1:
                                    {
                                        data.roleAuth = 0;
                                        EnterMainWindow();
                                    }
                                    break;
                                case 2:
                                    {
                                        MessageBox.Show($"Вход в приложение ограничен!");
                                    }
                                    break;
                                case 3:
                                    {
                                        data.roleAuth = 1;
                                        EnterMainWindow();
                                    }
                                    break;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Пользователь не найден!");
                    }
                }
            }
        }

        public void EnterMainWindow()
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
    }
}
