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
    // ! Предполагается наличие: using _522_Lyulina.Model; и классов Users, Entities, AuthPage.
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        // Вспомогательный метод для хэширования (дублируется из AuthPage для удобства)
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // 2. Метод для обработки нажатия на кнопку сохранения
        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // 3. Проверка заполнения ВСЕХ полей
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Ошибка");
                return;
            }

            // Проверка совпадения нового пароля и подтверждения
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show("Новый пароль и его подтверждение не совпадают!", "Ошибка");
                return;
            }

            // 4. Проверка на правильно введенные данные аккаунта (Логин и Текущий Пароль)
            string hashedCurrentPass = GetHash(CurrentPasswordBox.Password);

            // ! Замените Entities.GetContext() на имя вашего контекста, если оно другое
            var user = Entities.GetContext().User
                .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedCurrentPass);

            if (user == null)
            {
                MessageBox.Show("Текущий пароль/Логин неверный!", "Ошибка аутентификации");
                return;
            }

            // 5. Проверка корректности нового пароля (по аналогии с регистрацией)
            string newPassword = NewPasswordBox.Password;
            bool en = true; // Английская раскладка
            bool number = false; // Наличие цифры

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!", "Ошибка");
                return;
            }

            for (int i = 0; i < newPassword.Length; i++)
            {
                if (newPassword[i] >= '0' && newPassword[i] <= '9')
                    number = true;
                // Проверяем, что символ является английской буквой (A-Z, a-z)
                else if (!((newPassword[i] >= 'A' && newPassword[i] <= 'Z') ||
                           (newPassword[i] >= 'a' && newPassword[i] <= 'z')))
                    en = false;
            }

            if (!en)
            {
                MessageBox.Show("Используйте только английскую раскладку!", "Ошибка");
                return;
            }

            if (!number)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!", "Ошибка");
                return;
            }

            // 6. Если все проверки пройдены успешно, сохраняем новый хэш пароля
            if (en && number)
            {
                try
                {
                    user.Password = GetHash(NewPasswordBox.Password);
                    Entities.GetContext().SaveChanges();

                    MessageBox.Show("Пароль успешно изменен! Вы будете перенаправлены на страницу входа.", "Успех");
                    NavigationService?.Navigate(new AuthPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка БД");
                }
            }
        }
    }
}