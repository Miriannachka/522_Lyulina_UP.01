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
using System.ComponentModel; // Для Users и Entities (если они здесь)

namespace _522_Lyulina.Pages
{
    // ! Убедитесь, что 'Entities' и 'Users' доступны в этом пространстве имен
    // или используйте правильную директиву using, например:
    // using _522_Lyulina.Model; 

    public partial class AuthPage : Page
    {
        // Переменные, необходимые по заданию
        private int failedAttempts = 0;
        

        public AuthPage()
        {
            InitializeComponent();

            // Опционально: сразу генерировать код капчи при загрузке страницы, 
            // чтобы он был готов к показу.
            CaptchaChange();
        }

        // --- ЛОГИКА АВТОРИЗАЦИИ И БЕЗОПАСНОСТИ ---

        // Хэширование пароля (SHA1)
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x => x.ToString("X2")));
            }
        }

        // Метод включения/отключения капчи
        public void CaptchaSwitch()
        {
            switch (captcha.Visibility)
            {
                case Visibility.Visible:
                    // Скрыть элементы капчи
                    captcha.Visibility = Visibility.Hidden;
                    captchaInput.Visibility = Visibility.Hidden;
                    labelCaptcha.Visibility = Visibility.Hidden;
                    submitCaptcha.Visibility = Visibility.Hidden;

                    // Показать элементы авторизации (предполагая, что labelLogin существует)
                    labelLogin.Visibility = Visibility.Visible;
                    labelPass.Visibility = Visibility.Visible;
                    TextBoxLogin.Visibility = Visibility.Visible;
                    // txtHintLogin.Visibility = Visibility.Visible; // Если используете Hint
                    PasswordBox.Visibility = Visibility.Visible;
                    // txtHintPass.Visibility = Visibility.Visible;   // Если используете Hint

                    ButtonChangePassword.Visibility = Visibility.Visible;
                    ButtonEnter.Visibility = Visibility.Visible;
                    ButtonReg.Visibility = Visibility.Visible;

                    TextBoxLogin.Clear();
                    PasswordBox.Clear();
                    return;

                case Visibility.Hidden:
                    // Показать элементы капчи
                    CaptchaChange(); // Генерируем новый код
                    captcha.Visibility = Visibility.Visible;
                    captchaInput.Visibility = Visibility.Visible;
                    labelCaptcha.Visibility = Visibility.Visible;
                    submitCaptcha.Visibility = Visibility.Visible;

                    // Скрыть элементы авторизации
                    labelLogin.Visibility = Visibility.Hidden;
                    labelPass.Visibility = Visibility.Hidden;
                    TextBoxLogin.Visibility = Visibility.Hidden;
                    // txtHintLogin.Visibility = Visibility.Hidden;
                    PasswordBox.Visibility = Visibility.Hidden;
                    // txtHintPass.Visibility = Visibility.Hidden;

                    ButtonChangePassword.Visibility = Visibility.Hidden;
                    ButtonEnter.Visibility = Visibility.Hidden;
                    ButtonReg.Visibility = Visibility.Hidden;
                    return;
            }
        }

        // Код обновления/генерации капчи
        public void CaptchaChange()
        {
            String allowchar = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z";
            allowchar += ",a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z";
            allowchar += ",1,2,3,4,5,6,7,8,9,0";

            char[] a = { ',' };
            String[] ar = allowchar.Split(a);
            String pwd = "";
            Random r = new Random();

            for (int i = 0; i < 6; i++)
            {
                pwd += ar[(r.Next(0, ar.Length))];
            }
            captcha.Text = pwd;
            captchaInput.Text = ""; // Очищаем поле ввода капчи
        }

        // Запрет копирования/вставки капчи
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        // --- ОБРАБОТЧИКИ СОБЫТИЙ ---

        // Нажатие на кнопку "Вход" (ButtonEnter)
        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) ||
                string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин или пароль");
                return;
            }

            // Если капча активна, нельзя входить через эту кнопку
            if (captcha.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Сначала подтвердите код капчи, нажав кнопку 'Подтвердить'.");
                return;
            }

            string hashedPassword = GetHash(PasswordBox.Password);

            // ! Замените 'Entities' на имя вашего контекста базы данных
            using (var db = new Entities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text &&
                                         u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    failedAttempts++;

                    if (failedAttempts >= 3)
                    {
                        if (captcha.Visibility != Visibility.Visible)
                        {
                            MessageBox.Show("Превышено количество попыток. Введите капчу.");
                            CaptchaSwitch(); // Показать капчу
                        }
                        CaptchaChange(); // Сменить код капчи, если уже видна
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Пользователь успешно найден!");

                    // Сброс счетчика при успешной авторизации
                    failedAttempts = 0;

                    switch (user.Role)
                    {
                        case "User":
                            NavigationService?.Navigate(new Pages.UserPage());
                            break;
                        case "Admin":
                            NavigationService?.Navigate(new Pages.AdminPage());
                            break;
                    }
                }
            }
        }

        // Нажатие на кнопку "Подтвердить" (submitCaptcha)
        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                failedAttempts = 0; // Сбрасываем счетчик после успешного ввода
            }
        }

        // Навигация: Регистрация
        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        // Навигация: Смена пароля
        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        // Обработчики, упомянутые в XAML (могут быть пустыми, если не требуется доп. логика)
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Логика отслеживания изменений логина
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Логика отслеживания изменений пароля
        }

        // Обработчики для перехода фокуса (если вы добавили элементы txtHintLogin/txtHintPass)
        // private void txtHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        // {
        //     TextBoxLogin.Focus();
        // }
        // private void txtHintPass_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        // {
        //     PasswordBox.Focus();
        // }
    }
}