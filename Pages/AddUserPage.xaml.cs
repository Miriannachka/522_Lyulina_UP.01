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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _522_Lyulina.Pages
{
    // ! Предполагается наличие: using _522_Lyulina.Model; и классов Users, Entities.
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();
            if (selectedUser != null)
                _currentUser = selectedUser;
            DataContext = _currentUser;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");

            // Проверка роли
            if ((_currentUser.Role == null) || (cmbRole.SelectedItem == null))
            {
                errors.AppendLine("Выберите роль!");
            }
            else
            {
                // Устанавливаем роль из выбранного элемента ComboBox
                _currentUser.Role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            }

            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Если ID == 0, значит, это новый пользователь
            if (_currentUser.ID == 0)
                Entities.GetContext().User.Add(_currentUser);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                // Возвращаемся на предыдущую страницу (UsersTabPage)
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем только поля, а не сам _currentUser, чтобы сохранить контекст редактирования
            TBLogin.Text = string.Empty;
            TBPass.Text = string.Empty;
            cmbRole.SelectedIndex = -1; // Сброс выбора
            TBFio.Text = string.Empty;
            TBPhoto.Text = string.Empty;
        }
    }
}