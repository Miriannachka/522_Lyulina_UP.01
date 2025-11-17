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
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        // 3. Переход на страницу "Таблица пользователей"
        private void BtnTab1_Click(object sender, RoutedEventArgs e)
        {
            // ! Убедитесь, что класс UsersTabPage существует
            NavigationService?.Navigate(new UsersTabPage());
        }

        // 3. Переход на страницу "Таблица категорий"
        private void BtnTab2_Click(object sender, RoutedEventArgs e)
        {
            // ! Убедитесь, что класс CategoryTabPage существует
            NavigationService?.Navigate(new CategoryTabPage());
        }

        // 3. Переход на страницу "Таблица платежей"
        private void BtnTab3_Click(object sender, RoutedEventArgs e)
        {
            // ! Убедитесь, что класс PaymentTabPage существует
            NavigationService?.Navigate(new PaymentTabPage());
        }

        // 3. Переход на страницу "Диаграммы"
        private void BtnTab4_Click(object sender, RoutedEventArgs e)
        {
            // ! Убедитесь, что класс DiagrammPage существует
            NavigationService?.Navigate(new DiagrammPage());
        }
    }
}