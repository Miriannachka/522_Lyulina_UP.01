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
using System.Windows.Threading;
using System.ComponentModel;
using _522_Lyulina.Pages;

namespace _522_Lyulina
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // **********************************************
            // ГЛАВНЫЙ ШАГ: ЗАПУСК ПЕРВОЙ СТРАНИЦЫ
            // **********************************************

            // 1. Установите стартовую страницу (обычно это AuthPage)
            MainFrame.Navigate(new AuthPage());

            // 2. Если вы хотите, чтобы Frame был доступен в других частях программы (для общих операций)
            Manager.MainFrame = MainFrame;
            // ! Если вы используете статический класс Manager, как это часто делается в обучающих проектах,
            // ! эта строка должна быть здесь.
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1); // Интервал: 1 секунда
            timer.IsEnabled = true;

            // Обработчик Tick: обновляет TextBlock
            timer.Tick += (o, t) => {
                DateTimeNow.Text = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"); // Улучшенный формат
            };

            timer.Start();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
        
        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Показываем диалоговое окно с вопросом
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите закрыть окно?",
                "Подтверждение закрытия",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // Если пользователь выбрал "Нет"
            if (result == MessageBoxResult.No)
            {
                // Отменяем закрытие окна
                e.Cancel = true;
            }
            // Если пользователь выбрал "Да" (или нажал крестик и не появилось окно, что маловероятно
            // с MessageBox, но для полноты)
            else
            {
                // Позволяем окну закрыться (по умолчанию оно бы закрылось, 
                // но явное указание e.Cancel = false не помешает)
                e.Cancel = false;
            }
        }

        private void StyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string styleFileName = "";

            // Определяем, какой элемент был выбран
            switch (StyleComboBox.SelectedIndex)
            {
                case 0: // Выбран первый элемент: "Стиль по Руководству"
                    styleFileName = "Dictionary.xaml";
                    break;
                case 1: // Выбран второй элемент: "Темно-зеленая тема"
                    styleFileName = "DarkGreenTheme.xaml";
                    break;
                default:
                    return; // На всякий случай
            }

            // 1. Определяем путь к файлу ресурсов
            var uri = new Uri(styleFileName, UriKind.Relative);

            // 2. Загружаем словарь ресурсов
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            // Проверка на успешную загрузку
            if (resourceDict == null) return;

            // 3. Очищаем коллекцию ресурсов приложения
            Application.Current.Resources.MergedDictionaries.Clear();

            // 4. Добавляем загруженный словарь ресурсов
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }
    }
}