using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

using System.Windows.Forms; // NotifyIcon control
using System.Drawing; // Icon
using System.Xml;

namespace TurnOnProjector
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Projector projector;
        public const string FILE_SETTINGS = @"C:\ProgramData\IP_PROJECTOR.xml";
        Settings settings_windows = new Settings(); // Окно настроек
        NotifyIcon notifyIcon = new NotifyIcon(); //Для трея
        private ContextMenu contextMenu1 = new ContextMenu(); //Для трея
        private MenuItem miExit;

        public MainWindow()
        {
            InitializeComponent();
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
            prgProjector.Visibility = Visibility.Visible;

            notifyIcon.BalloonTipText = "Управление проектором";
            notifyIcon.Text = "Управление проектором";
            notifyIcon.Icon = Properties.Resources.ProjectorICO;
            notifyIcon.Visible = true;
            miExit = new MenuItem("Выход", this.MenuExit);
            contextMenu1.MenuItems.AddRange(new MenuItem[] {this.miExit});
            notifyIcon.ContextMenu = this.contextMenu1;
            notifyIcon.Click += new EventHandler(this.notifyIcon1_DoubleClick);

            OpenSettringsFile();
        }

        public void OpenSettringsFile()
        {
            try //Открыть файл, получить данные о проекторе
            {
                string prType = "", prIP = "";

                using (XmlReader reader = XmlReader.Create(FILE_SETTINGS))
                {
                    while (reader.Read())
                    {
                        switch (reader.Name.ToString())
                        {
                            case "pr":
                                prType = reader.ReadString();
                                break;
                            case "ippr":
                                prIP = reader.ReadString();
                                break;
                        }

                    }
                }

                switch (prType)
                {
                    case "0":
                        projector = new EpsonProjector();
                        projector.IP = System.Net.IPAddress.Parse(prIP);
                        LoadProjectors();
                        break;
                    case "1":
                        projector = new BenqProjector();
                        projector.IP = System.Net.IPAddress.Parse(prIP);
                        LoadProjectors();
                        break;
                    default:
                        System.Windows.MessageBox.Show("Ошибка чтения файла. В файле кривые настройки");
                        break;
                }

            }
            catch (XmlException)
            {
                System.Windows.MessageBox.Show("Ошибка чтения файла. Пустой файл настроек");
                System.IO.File.CreateText(FILE_SETTINGS);
                this.lblError.Visibility = Visibility.Visible;
                prgProjector.Visibility = Visibility.Hidden;
                buttonOn.IsEnabled = false;
                buttonReload.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                Console.WriteLine("MEGAEROROR");
                System.Windows.MessageBox.Show("Ошибка чтения файла. Невозможно открыть/создать файл");
                System.IO.File.CreateText(FILE_SETTINGS);
                lblError.Visibility = Visibility.Visible;
                prgProjector.Visibility = Visibility.Hidden;
                buttonOn.IsEnabled = false;
                buttonReload.Visibility = Visibility.Visible;

            }
        }

        //Выход из приложения
        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            notifyIcon.ShowBalloonTip(100);
        }

        private void MenuExit(object Sender, EventArgs e)
        {
            settings_windows.Close();
            this.Close();
        }

        private void notifyIcon1_DoubleClick(object Sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
                this.Visibility = Visibility.Visible;
            else
                this.Visibility = Visibility.Hidden;
        }

        //Кнопка клонировать экраны
        private void buttonClone_Click(object sender, RoutedEventArgs e)
        {
            //Режим клонирования экранов
            Process.Start("DisplaySwitch.exe", "/clone");
        }

        //Кнопка расширить экраны
        private async void buttonExtend_Click(object sender, RoutedEventArgs e)
        {
            //Режим расширения экранов

            Console.WriteLine("Новый статус проектора: " + await projector.getStatus());

            //Process.Start("DisplaySwitch.exe", "/extend");
        }

        //Кнопка включения
        private void buttonOn_Click(object sender, RoutedEventArgs e)
        {
            prgProjector.Visibility = Visibility.Visible;
            buttonOn.Visibility = Visibility.Hidden;
            buttonOff.Visibility = Visibility.Visible;
            buttonFreeze.Visibility = Visibility.Visible;

            (new Thread(async (s) =>
            {
                Console.WriteLine("Включается: " + await projector.turnOn());
                Thread.Sleep(2000);
                string stat = await projector.getStatus();
                stat = stat.Split(" "[0])[0];
                while (stat == "%1POWR=3\r" || stat == "PWSTATUS=01")
                {
                    Console.WriteLine("Ждем полного включения");
                    Thread.Sleep(2000);
                    stat = await projector.getStatus();
                }
                await Dispatcher.BeginInvoke((Action)(() => buttonOn.Visibility = Visibility.Hidden));
                await Dispatcher.BeginInvoke((Action)(() => buttonOff.Visibility = Visibility.Visible));
                await Dispatcher.BeginInvoke((Action)(() => buttonFreeze.Visibility = Visibility.Visible));
                await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));

            })).Start();

        }

        //Кнопка выключения
        private void buttonOff_Click(object sender, RoutedEventArgs e)
        {
            prgProjector.Visibility = Visibility.Visible;

            (new Thread(async (s) =>
            {
                Console.WriteLine("Выключается: " + await projector.turnOff());
                Thread.Sleep(2000);
                string stat = await projector.getStatus();
                stat = stat.Split(" "[0])[0];
                while (stat == "%1POWR=1\r" || stat == "PWSTATUS=01")
                {
                    Console.WriteLine("Пытаемся выключить: " + await projector.turnOff());
                    Thread.Sleep(2000);
                    stat = await projector.getStatus();
                }
                
                while (stat == "%1POWR=2\r" || stat == "PWSTATUS=03")
                {
                    Console.WriteLine("Ждем полного выклчюения");
                    Thread.Sleep(5000);
                    stat = await projector.getStatus();
                }
                await Dispatcher.BeginInvoke((Action)(() => buttonOff.Visibility = Visibility.Hidden));
                await Dispatcher.BeginInvoke((Action)(() => buttonFreeze.Visibility = Visibility.Hidden));
                await Dispatcher.BeginInvoke((Action)(() => buttonOn.Visibility = Visibility.Visible));
                await Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = true));
                await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));

            })).Start();

        }

        //Кнопка "настройки"
        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            settings_windows.Visibility = Visibility.Visible;
        }

        //Кнопка обновить проектор
        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            OpenSettringsFile();
        }

        //Создание/обновление проектора
        private async void LoadProjectors()
        {

            string statProjector = await projector.getStatus();
            statProjector = statProjector.Split(" "[0])[0];
            Console.WriteLine("Текущий статус проектора: " + statProjector);

            switch (statProjector)
            {
                case "PWSTATUS=01": //Выключен
                    await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = true));
                    await Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                    break;
                case "PWSTATUS=03": //Включен
                    await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOn.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOff.Visibility = Visibility.Visible));
                    await Dispatcher.BeginInvoke((Action)(() => buttonFreeze.Visibility = Visibility.Visible));
                    await Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                    break;
                case "%1POWR=0\r": //Выключен
                    await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = true));
                    await Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                    break;
                case "%1POWR=2\r": //Остывает?
                    await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = true));
                    await Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                    break;
                case "%1POWR=1\r": //Выключен
                    await Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOn.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => buttonOff.Visibility = Visibility.Visible));
                    await Dispatcher.BeginInvoke((Action)(() => buttonFreeze.Visibility = Visibility.Visible));
                    await Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                    await Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                    break;
            }

        }
     
        private async void buttonFreeze_Click(object sender, RoutedEventArgs e)
        {

            (new Thread((s) =>
            {
                projector.freeze();
            })).Start();

        }
    }

}
