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
using System.Windows.Forms.DataVisualization.Charting; // 9. Для Chart
using Word = Microsoft.Office.Interop.Word;              // 9. Для Word
using Excel = Microsoft.Office.Interop.Excel;            // 9. Для Excel

namespace _522_Lyulina.Pages
{
    public partial class DiagrammPage : Page
    {
        // 10. Поле для контекста базы данных
        private Entities _context = new Entities(); // ! Проверьте, что имя вашего контекста - Entities

        public DiagrammPage()
        {
            InitializeComponent();

            // 10. Создаем область построения диаграммы и добавляем её
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            // 10. Добавляем наборы данных (серию)
            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true,
                ChartArea = "Main"
            };
            ChartPayments.Series.Add(currentSeries);

            // 11. Загрузка данных в ComboBox'ы
            CmbUser.ItemsSource = _context.User.ToList(); // Используем Users

            // Типы диаграммы
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            // Устанавливаем начальные значения
            if (CmbUser.Items.Count > 0)
                CmbUser.SelectedIndex = 0;

            CmbDiagram.SelectedIndex = 3; // Column (Столбчатая)

            // Вызываем для начального построения
            if (CmbUser.SelectedItem != null)
                UpdateChart(null, null);
        }

        // 11. Метод для обновления и построения диаграммы
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();

                if (currentSeries == null) return;

                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear(); // Очищаем старые точки

                // Заголовок диаграммы
                ChartPayments.Titles.Clear();
                ChartPayments.Titles.Add($"Платежи по категориям для: {currentUser.FIO}");

                // Получаем список категорий
                var categoriesList = _context.Category.ToList();

                // Извлекаем ID текущего пользователя для использования в запросе
                int currentUserId = currentUser.ID;

                foreach (var category in categoriesList)
                {
                    int currentCategoryId = category.ID;

                    // *************************************************************
                    // ИСПРАВЛЕНИЕ NotSupportedException и InvalidOperationException
                    // *************************************************************

                    // 1. Используем ID в Where для LINQ to Entities
                    decimal? totalSumNullable = _context.Payment
                        .Where(p => p.UserID == currentUserId && p.CategoryID == currentCategoryId)
                        .Sum(p => (decimal?)(p.Price * p.Num)); // Суммируем nullable decimal

                    // 2. Присваиваем 0, если результат Sum() был null (т.е. нет платежей)
                    decimal totalSum = totalSumNullable ?? 0.00m;

                    // Добавляем точку только если сумма больше 0
                    if (totalSum > 0)
                    {
                        currentSeries.Points.AddXY(category.Name, totalSum);
                    }
                }
            }
        }

        // --- Обработчики экспорта ---

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Объявляем переменную Excel.Application ВНЕ блока try
            Excel.Application application = null;

            try
            {
                // Получаем список пользователей с одновременной сортировкой по ФИО
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();

                if (allUsers.Count == 0)
                {
                    MessageBox.Show("В базе данных отсутствуют пользователи для экспорта.", "Ошибка");
                    return;
                }

                // Инициализация Excel
                application = new Excel.Application();

                // Указываем количество листов равным количеству пользователей
                application.SheetsInNewWorkbook = allUsers.Count();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                // Переменная для накопления общего итога
                decimal grandTotal = 0;

                // 2. Запускаем цикл по пользователям
                for (int i = 0; i < allUsers.Count(); i++)
                {
                    int startRowIndex = 1;

                    // Выбираем текущий лист рабочей книги
                    Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                    worksheet.Name = allUsers[i].FIO;

                    // === 3. ДОБАВЛЕНИЕ И ФОРМАТИРОВАНИЕ ЗАГОЛОВКОВ КОЛОНОК ===
                    worksheet.Cells[1][startRowIndex] = "Дата платежа";
                    worksheet.Cells[2][startRowIndex] = "Название";
                    worksheet.Cells[3][startRowIndex] = "Стоимость";
                    worksheet.Cells[4][startRowIndex] = "Количество";
                    worksheet.Cells[5][startRowIndex] = "Сумма";

                    Excel.Range columlHeaderRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][1]];
                    columlHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columlHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    // === 4. ГРУППИРОВКА И СОРТИРОВКА ПЛАТЕЖЕЙ ===
                    var userCategories = allUsers[i].Payment
                        .OrderBy(u => u.Date)
                        .GroupBy(u => u.Category)
                        .OrderBy(u => u.Key.Name);

                    // Переменная для запоминания начала таблицы для установки границ
                    int dataStartRow = startRowIndex;

                    // 5. Вложенный цикл по категориям платежей
                    foreach (var groupCategory in userCategories)
                    {
                        // --- Название категории ---
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[5][startRowIndex]];
                        headerRange.Merge();
                        headerRange.Value = groupCategory.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        // --- Цикл по платежам внутри категории ---
                        int categoryStartRow = startRowIndex; // Запоминаем начало категории для формулы ИТОГО

                        foreach (var payment in groupCategory)
                        {
                            worksheet.Cells[1][startRowIndex] = payment.Date.ToString("dd.MM.yyyy");
                            worksheet.Cells[2][startRowIndex] = payment.Name;

                            // Цена
                            worksheet.Cells[3][startRowIndex] = payment.Price;
                            (worksheet.Cells[3][startRowIndex] as Excel.Range).NumberFormat = "0.00";

                            // Количество
                            worksheet.Cells[4][startRowIndex] = payment.Num;

                            // Сумма (Формула: Стоимость * Количество)
                            worksheet.Cells[5][startRowIndex].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "0.00";

                            startRowIndex++;
                        } // Завершение цикла по платежам

                        // --- ИТОГО по категории ---
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[4][startRowIndex]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                        // Формула для ИТОГО категории
                        worksheet.Cells[5][startRowIndex].Formula = $"=SUM(E{categoryStartRow}:E{startRowIndex - 1})";

                        // Форматирование ИТОГО
                        sumRange.Font.Bold = worksheet.Cells[5][startRowIndex].Font.Bold = true;

                        // Добавляем сумму категории к общему итогу
                        decimal categoryTotal = (decimal)((Excel.Range)worksheet.Cells[5][startRowIndex]).Value2;
                        grandTotal += categoryTotal;

                        startRowIndex++;
                    } // Завершение цикла по категориям

                    // === 6. ДОБАВЛЕНИЕ ГРАНИЦ ТАБЛИЦЫ ===
                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][startRowIndex - 1]];

                    Excel.XlLineStyle lineStyle = Excel.XlLineStyle.xlContinuous;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = lineStyle;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = lineStyle;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = lineStyle;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = lineStyle;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = lineStyle;
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = lineStyle;

                    // === 7. АВТОШИРИНА СТОЛБЦОВ ===
                    worksheet.Columns.AutoFit();

                } // Завершение цикла по пользователям

                // === 8. ДОБАВЛЕНИЕ ЛИСТА "ОБЩИЙ ИТОГ" (После обработки всех пользователей) ===
                Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                summarySheet.Name = "Общий итог";

                // Запись заголовка и значения
                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = grandTotal;

                // Форматирование: красный цвет и жирный шрифт
                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;

                // Форматирование суммы
                (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "0.00";

                // Автоподбор ширины столбцов
                summarySheet.Columns.AutoFit();

                // === 9. ОТОБРАЖЕНИЕ ===
                application.Visible = true;

                MessageBox.Show("Экспорт в Excel завершен. Файлы сохранены и открыты.", "Успех");
            }
            catch (Exception ex)
            {
                // Блок catch: теперь переменная application доступна
                MessageBox.Show($"Произошла ошибка при экспорте в Excel. Убедитесь, что Microsoft Excel установлен и вы добавили COM-ссылку.\nОшибка: {ex.Message}", "Ошибка экспорта");

                if (application != null)
                {
                    application.Visible = true;
                    application.Quit(); // Очистка (закрытие Excel)
                }
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            // 1. Объявляем переменную Word.Application ВНЕ блока try
            Word.Application application = null;

            try
            {
                // Получаем список пользователей и категорий из базы данных
                var allUsers = _context.User.ToList();
                var allCategories = _context.Category.ToList();

                if (allUsers.Count == 0 || allCategories.Count == 0)
                {
                    MessageBox.Show("В базе данных отсутствуют пользователи или категории для экспорта.", "Ошибка");
                    return;
                }

                // 2. Инициализируем Word.Application ВНУТРИ блока try
                application = new Word.Application();
                Word.Document document = application.Documents.Add();

                // 3. Запускаем цикл по пользователям
                foreach (var user in allUsers)
                {
                    // === ЗАГОЛОВОК СТРАНИЦЫ (ФИО пользователя) ===
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style("Заголовок 1");
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add();

                    // === СОЗДАНИЕ ТАБЛИЦЫ ===
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;

                    Word.Table paymentsTable = document.Tables.Add(tableRange,
                        allCategories.Count() + 1, 2);

                    paymentsTable.Borders.InsideLineStyle =
                    paymentsTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // === НАЗВАНИЯ КОЛОНОК ===
                    Word.Range cellRange;

                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // === ЗАПОЛНЕНИЕ ТАБЛИЦЫ ДАННЫМИ ===
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];

                        // Столбец 1: Категория
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        // Безопасное суммирование для LINQ to Objects
                        decimal categorySum = user.Payment.ToList()
                            .Where(p => p.Category == currentCategory)
                            .Sum(p => (decimal?)(p.Num * p.Price) ?? 0.00m);

                        // Столбец 2: Сумма расходов
                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = categorySum.ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                        cellRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphRight;
                    }
                    document.Paragraphs.Add();

                    // === МАКСИМАЛЬНЫЙ ПЛАТЕЖ (КРАСНЫЙ) ===
                    Payment maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")}" +
                                               $" руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";

                        maxPaymentParagraph.set_Style("Подзаголовок");
                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter();
                    }
                    document.Paragraphs.Add();

                    // === МИНИМАЛЬНЫЙ ПЛАТЕЖ (ЗЕЛЕНЫЙ) ===
                    Payment minPayment = user.Payment.OrderBy(u => u.Price * u.Num).FirstOrDefault();

                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")}" +
                                               $" руб. от {minPayment.Date.ToString("dd.MM.yyyy")}";

                        minPaymentParagraph.set_Style("Подзаголовок");
                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter();
                    }

                    document.Paragraphs.Add();

                    // === РАЗРЫВ СТРАНИЦЫ ===
                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);

                } // Завершение цикла по пользователям

                // === КОЛОНТИТУЛЫ ===

                // Верхний колонтитул (Дата)
                foreach (Word.Section section in document.Sections)
                {
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = DateTime.Now.ToString("dd.MM.yyyy") + " (Отчет о платежах)";
                }

                // Нижний колонтитул (Номер страницы)
                foreach (Word.Section section in document.Sections)
                {
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);
                }

                // Разрешаем отображение документа
                application.Visible = true;

                // Сохранение документа (Измените путь при необходимости)
                string docxPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Payments_Report.docx";
                string pdfPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Payments_Report.pdf";

                document.SaveAs2(docxPath);
                document.SaveAs2(pdfPath, Word.WdExportFormat.wdExportFormatPDF);

                MessageBox.Show($"Экспорт завершен. Файлы сохранены на рабочем столе:\n{docxPath}\n{pdfPath}", "Успех");
            }
            catch (Exception ex)
            {
                // Блок catch: теперь переменная application доступна
                MessageBox.Show($"Произошла ошибка при экспорте в Word. Убедитесь, что Microsoft Word установлен и вы добавили COM-ссылку.\nОшибка: {ex.Message}", "Ошибка экспорта");

                if (application != null)
                {
                    application.Visible = true;
                    application.Quit(); // Очистка (закрытие Word)
                }
            }
        }
    }
}