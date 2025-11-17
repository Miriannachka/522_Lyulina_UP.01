using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    // ! Убедитесь, что классы 'Users' и 'Entities' доступны в этом пространстве имен
    // или используйте правильную директиву using, например:
    // using _522_Lyulina.Model; 

    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();

            // Настройка роли по умолчанию (Пользователь)
            comboBxRole.SelectedIndex = 0;
        }

        // --- ЛОГИКА "ЗАПОЛНИТЕЛЕЙ" (PLACEHOLDERS) ---

        // Обработчик нажатия на метку-подсказку (для логина)
        private void lblLogHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxLog.Focus();
        }

        // Обработчик изменения текста (для логина): скрывает/показывает подсказку
        private void txtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Используем оператор '?' для безопасного доступа (если метка была бы null)
            lblLogHitn.Visibility = txtbxLog.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // Обработчик нажатия на метку-подсказку (для первого пароля)
        private void lblPassHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxFrst.Focus();
        }

        // Обработчик изменения пароля (для первого пароля)
        private void passBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassHitn.Visibility = passBxFrst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // Обработчик нажатия на метку-подсказку (для второго пароля)
        private void lblPassSecHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            passBxScnd.Focus();
        }

        // Обработчик изменения пароля (для второго пароля)
        private void passBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassSecHitn.Visibility = passBxScnd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // Обработчик нажатия на метку-подсказку (для ФИО)
        private void lblFioHitn_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtbxFIO.Focus();
        }

        // Обработчик изменения текста (для ФИО)
        private void txtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblFioHitn.Visibility = txtbxFIO.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // --- ЛОГИКА РЕГИСТРАЦИИ ---

        // Напоминание: Вам потребуется метод GetHash() из AuthPage.xaml.cs, 
        // чтобы хэшировать пароль перед сохранением (хотя в примере ниже этого нет)
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }


        private void regButton_Click(object sender, RoutedEventArgs e)
        {
            // 6.1. Проверка заполнения полей
            if (string.IsNullOrEmpty(txtbxLog.Text) ||
                string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка");
                return;
            }

            // 6.2. Проверка наличия логина в БД
            // ! Замените 'Entities' на имя вашего контекста базы данных
            using (var db = new Entities())
            {
                var existingUser = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == txtbxLog.Text);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка");
                    return;
                }
            } // Закрываем контекст перед продолжением

            // 6.3. Проверка формата пароля
            string password = passBxFrst.Password;
            bool isPasswordValid = true; // Используем флаг для агрегирования ошибок
            bool en = true;
            bool number = false;

            if (password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка");
                isPasswordValid = false;
            }
            else
            {
                for (int i = 0; i < password.Length; i++)
                {
                    // Проверка на цифру
                    if (password[i] >= '0' && password[i] <= '9')
                        number = true;
                    // Проверка на английскую раскладку
                    else if (!((password[i] >= 'A' && password[i] <= 'Z') || (password[i] >= 'a' && password[i] <= 'z')))
                        en = false;
                }

                if (!en)
                {
                    MessageBox.Show("Используйте только английскую раскладку!", "Ошибка");
                    isPasswordValid = false;
                }
                else if (!number)
                {
                    MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка");
                    isPasswordValid = false;
                }
            }

            // Если проверки на длину, раскладку и цифру не прошли, завершаем
            if (!isPasswordValid)
            {
                return;
            }

            // 6.4. Проверка на совпадение паролей
            if (passBxFrst.Password != passBxScnd.Password)
            {
                MessageBox.Show("Пароли не совпадают!", "Ошибка");
                return;
            }

            // 6.5. Все проверки прошли успешно: запись в БД
            try
            {
                using (var db = new Entities())
                {
                    // Хэширование пароля перед сохранением
                    string hashedPassword = GetHash(passBxFrst.Password);

                    User userObject = new User
                    {
                        FIO = txtbxFIO.Text,
                        Login = txtbxLog.Text,
                        // Сохраняем хэшированный пароль
                        Password = hashedPassword,
                        Role = (comboBxRole.SelectedItem as ComboBoxItem)?.Content.ToString()
                               ?? "User" // Берем текст из выбранного ComboBoxItem
                    };

                    db.User.Add(userObject);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован! Вы можете войти.", "Успех");

                    // Очистка полей
                    txtbxLog.Clear();
                    passBxFrst.Clear();
                    passBxScnd.Clear();
                    txtbxFIO.Clear();
                    comboBxRole.SelectedIndex = 0; // Возвращаем роль по умолчанию (User)

                    // Опционально: Переход на страницу авторизации
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.Message}", "Критическая ошибка");
            }
        }
    }
}