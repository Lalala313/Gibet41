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

namespace Гибет41размер
{
    /// <summary>
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        private string currentCaptcha; //текущая сгенеренная капча
        private int failedAttempts = 0; //уол-во неудачных попыток
        private DateTime? blockUntil = null; //время разюлокировки системы, сначала устанавливает значение на null
        public AuthPage()
        {
            InitializeComponent();
            //скрываем капчу
            captchOneWord.Visibility = Visibility.Collapsed;
            captchTwoWord.Visibility = Visibility.Collapsed;
            captchThreeWord.Visibility = Visibility.Collapsed;
            captchFourWord.Visibility = Visibility.Collapsed;
            CaptchaTB.Visibility = Visibility.Collapsed;

        }
        private string GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        private void DisplayCaptcha(string captcha)
        {
            TextBlock[] captchaBlocks = { captchOneWord, captchTwoWord, captchThreeWord, captchFourWord }; //массив текстблоков
           
            Random random = new Random();
            for (int i = 0; i < captchaBlocks.Length; i++)
            {
                captchaBlocks[i].Visibility = Visibility.Visible; //видимый элемент
                captchaBlocks[i].Text = captcha[i].ToString();//символ капчи
                //смещение
                int leftMargin = i == 0 ? 30 + random.Next(-5, 5) : random.Next(-5, 5);
                captchaBlocks[i].Margin = new Thickness(leftMargin, random.Next(-5, 5), 0, 0);

                //перечеркивания
                captchaBlocks[i].TextDecorations = random.Next(2) == 0 ? TextDecorations.Strikethrough : null;
            }
           
        }

        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            if (blockUntil.HasValue && DateTime.Now < blockUntil.Value)
            {
                MessageBox.Show($"Система заблокирована. Попробуйте через {(int)(blockUntil.Value - DateTime.Now).TotalSeconds} секунд");
                return;
            }
            string login = LogInTB.Text;
            string password = PassTB.Text;
            if(login== "" || password=="")
            {
                MessageBox.Show("Есть пустые поля!");
                return;
            }
            TextBlock[] captchaBlocks = { captchOneWord, captchTwoWord, captchThreeWord, captchFourWord };

            if (failedAttempts > 0)
            {

                CaptchaTB.Visibility = Visibility.Visible;
                CaptchaTB.Focus();

                if (CaptchaTB.Visibility != Visibility.Visible || string.IsNullOrEmpty(CaptchaTB.Text))
                {
                    MessageBox.Show("Введите капчу!");
                    return;
                }
                if (CaptchaTB.Text != currentCaptcha)
                {
                    MessageBox.Show("Неверная капча!");
                    blockUntil = DateTime.Now.AddSeconds(10); // блокирует систему на 10 сек
                    CaptchaTB.Text = "";
                    currentCaptcha = GenerateCaptcha();
                    DisplayCaptcha(currentCaptcha);
                    MessageBox.Show("Система заблокирована на 10 секунд!");
                    return;
                }
            }


            User user = Gibet41Entities.GetContext().User.ToList().Find(p => p.UserLogin == login && p.UserPassword == password);
            if(user!=null)
            {
                failedAttempts = 0;
                CaptchaTB.Visibility = Visibility.Collapsed;
                for (int i = 0; i < captchaBlocks.Length; i++)
                {
                    captchaBlocks[i].Visibility = Visibility.Collapsed;
                }
                CaptchaTB.Text = "";
                Manager.MainFrame.Navigate(new ProductPage(user));
                LogInTB.Text = "";
                PassTB.Text = "";
            }
            else
            {
                failedAttempts++;
                if (failedAttempts > 0)
                {
                    currentCaptcha = GenerateCaptcha();
                    DisplayCaptcha(currentCaptcha);
                    CaptchaTB.Visibility = Visibility.Visible;
                    CaptchaTB.Text = "";
                    CaptchaTB.Focus();
                }
                MessageBox.Show("Введены неверные данные!");
                
            }
        }
        private void LogInButtonGuest_Click(object sender, RoutedEventArgs e)
        {
            TextBlock[] captchaBlocks = { captchOneWord, captchTwoWord, captchThreeWord, captchFourWord };

            User guestUser = new User
            {
                UserSurname = "Гость",
                UserName = "",
                UserPatronymic = "",
                UserRole = 4 //роль гостя будет 4
            };
            //очистка капчи
            failedAttempts = 0;
            CaptchaTB.Visibility = Visibility.Collapsed;
            for (int i = 0; i < captchaBlocks.Length; i++)
            {
                captchaBlocks[i].Visibility = Visibility.Collapsed;
            }
            CaptchaTB.Text = "";
            Manager.MainFrame.Navigate(new ProductPage(guestUser));
            LogInTB.Text = "";
            PassTB.Text = "";
        }
    }
}
