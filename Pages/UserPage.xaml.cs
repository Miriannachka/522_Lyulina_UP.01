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
    // ! Убедитесь, что классы 'Entities' и 'Users' доступны в этом пространстве имен 
    // или используйте правильную директиву using, например:
    // using _522_Lyulina.Model; 

    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            // 6. Инициализация и загрузка данных при запуске страницы
            // var currentUsers = Entities.GetContext().Users.ToList();
            // ListUser.ItemsSource = currentUsers;

            // Вместо прямого присвоения, вызываем UpdateUsers для применения начальной сортировки
            UpdateUsers();
        }

        // 8. Метод для обновления и применения всех фильтров
        private void UpdateUsers()
        {
            // Проверка инициализации, чтобы избежать ошибок при загрузке
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                // Получаем ВСЕ данные из контекста БД
                List<User> currentUsers = Entities.GetContext().User.ToList();

                // Филтрация по фамилии (часть ФИО)
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    string filterText = fioFilterTextBox.Text.ToLower();
                    currentUsers = currentUsers.Where(x =>
                        x.FIO != null && x.FIO.ToLower().Contains(filterText)).ToList();
                }

                // Фильтрация по роли (Только администраторы)
                if (onlyAdminCheckBox.IsChecked.HasValue && onlyAdminCheckBox.IsChecked.Value)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                }

                // Фильтрация по убыванию/возрастанию
                if (sortComboBox.SelectedIndex == 0) // По возрастанию (A -> Z)
                {
                    ListUser.ItemsSource = currentUsers.OrderBy(x => x.FIO).ToList();
                }
                else // По убыванию (Z -> A)
                {
                    ListUser.ItemsSource = currentUsers.OrderByDescending(x => x.FIO).ToList();
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок БД или данных
                MessageBox.Show($"Ошибка при загрузке или фильтрации данных: {ex.Message}", "Ошибка");
            }
        }

        // 8. Обработчик изменения текста в поле ФИО
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        // 8. Обработчик изменения выбора в ComboBox сортировки
        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        // 8. Обработчик установки флажка "Только администраторы"
        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        // 8. Обработчик снятия флажка "Только администраторы"
        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        // 7. Логика кнопки для очищения фильтров
        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0; // По возрастанию
            onlyAdminCheckBox.IsChecked = false;

            // Вызов UpdateUsers() не требуется, так как установка свойств Text и IsChecked 
            // автоматически вызовет соответствующие обработчики, которые вызовут UpdateUsers().
            // Однако, чтобы быть уверенным:
            // UpdateUsers(); 
        }
    }
}