using System.Text;
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
using AuthInTaimer.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthInTaimer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly AuthITaimerContext _context;
        private const int MaxFailed = 2;
        private DispatcherTimer _lockTimer;
        private int _remainingSeconds = 60;
        public MainWindow()
        {
            InitializeComponent();
            _context = new AuthITaimerContext();
            _lockTimer = new DispatcherTimer();
            _lockTimer.Interval = TimeSpan.FromSeconds(1);
            _lockTimer.Tick += LockTimer_Tick;
        }
        private void LockTimer_Tick(object sender, EventArgs e)
        {
            _remainingSeconds--;
            LogBtn.Content = $"Заблокировано ({_remainingSeconds} сек)";

            if (_remainingSeconds <= 0)
            {
                _lockTimer.Stop();
                LogBtn.IsEnabled = true;
                LogBtn.Content = "Войти";
                _remainingSeconds = 60;
            }
        }

        private void LogBtn_Click(object sender, RoutedEventArgs e)
        {
            string userName = LogTxb.Text;
            string passName = PasswordPsb.Password;

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(passName))
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            var optionsBuilder = new DbContextOptionsBuilder<AuthITaimerContext>();
            optionsBuilder.UseSqlServer("Data Source=LAPTOP-2MRBCQD7;Initial Catalog=AuthITaimer;Integrated Security=True;Encrypt=False");

            using (var context = new AuthITaimerContext(optionsBuilder.Options))
            {
                var user = context.Users.FirstOrDefault(x => x.Login == userName);
                if (user != null)
                {
                    if (user.Password != passName)
                    {
                        user.FailedAttempts++;
                        if (user.FailedAttempts > MaxFailed)
                        {
                            user.IsAccountLocked = true;
                            MessageBox.Show("Ваш аккаунт был заблокирован на 1 минуту");
                            LogBtn.IsEnabled = false;
                            _lockTimer.Start();
                        }
                        else
                        {
                            MessageBox.Show($"Неверный пароль. Осталось попыток: {MaxFailed - user.FailedAttempts + 1}");
                        }
                        context.SaveChanges();
                    }
                    else
                    {
                        
                        user.FailedAttempts = 0;
                        _context.SaveChanges();
                        
                        MessageBox.Show($"Добро пожаловать, {user.Name1}", "Успешный вход", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Пользователь не найден");
                }
            }

        
            
        }
    }
}