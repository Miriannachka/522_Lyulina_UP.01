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
    // ! Предполагается наличие: using _522_Lyulina.Model; и классов Payment, Entities, Users, Category.
    public partial class AddPaymentPage : Page
    {
        private Payment _currentPayment = new Payment();

        public AddPaymentPage(Payment selectedPayment)
        {
            InitializeComponent();

            // Загрузка данных для ComboBox'ов
            CBCategory.ItemsSource = Entities.GetContext().Category.ToList();
            CBCategory.DisplayMemberPath = "Name"; // Отображаем название категории
            CBUser.ItemsSource = Entities.GetContext().User.ToList();
            CBUser.DisplayMemberPath = "FIO";      // Отображаем ФИО пользователя

            if (selectedPayment != null)
                _currentPayment = selectedPayment;
            DataContext = _currentPayment;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            // Проверки на пустые/неверные данные
            if (string.IsNullOrWhiteSpace(TBDate.Text))
                errors.AppendLine("Укажите дату!");

            // Num (Количество) и Price (Сумма) должны быть числами
            if (!decimal.TryParse(TBAmount.Text, out decimal price) || price <= 0)
                errors.AppendLine("Укажите корректную сумму!");
            if (!int.TryParse(TBCount.Text, out int num) || num <= 0)
                errors.AppendLine("Укажите корректное количество!");

            // Проверка привязанных ComboBox'ов (ID)
            if (_currentPayment.UserID == 0 || CBUser.SelectedValue == null)
                errors.AppendLine("Укажите клиента!");
            if (_currentPayment.CategoryID == 0 || CBCategory.SelectedValue == null)
                errors.AppendLine("Укажите категорию!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            // Если ID == 0, добавляем новую запись
            if (_currentPayment.ID == 0)
                Entities.GetContext().Payment.Add(_currentPayment);

            try
            {
                Entities.GetContext().SaveChanges();
                MessageBox.Show("Данные успешно сохранены!");
                // Возвращаемся на предыдущую страницу (PaymentTabPage)
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем поля
            TBPaymentName.Text = string.Empty;
            TBAmount.Text = string.Empty;
            TBCount.Text = string.Empty;
            TBDate.Text = string.Empty;
            // Сбрасываем ComboBox'ы
            CBCategory.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
        }
    }
}