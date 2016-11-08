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
        NotifyIcon notifyIcon = new NotifyIcon();  //Для трея
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
            contextMenu1.MenuItems.AddRange(new MenuItem[] { this.miExit });
            notifyIcon.ContextMenu = this.contextMenu1;
            notifyIcon.Click += new EventHandler(this.notifyIcon1_DoubleClick);

            reload();
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
        private void buttonExtend_Click(object sender, RoutedEventArgs e)
        {
            //Режим расширения экранов
            Process.Start("DisplaySwitch.exe", "/extend");
        }

        //Кнопка включения
        private async void buttonOn_Click(object sender, RoutedEventArgs e)
        {
            Console.Write(projector.IP);
            
            prgProjector.Visibility = Visibility.Visible;
            buttonOn.Visibility = Visibility.Hidden;
            buttonOff.Visibility = Visibility.Visible;
            buttonFreeze.Visibility = Visibility.Visible;

            System.Windows.Controls.ProgressBar pr = prgProjector;
            Thread myThread = new Thread(projector.turnOn); //Создаем новый объект потока (Thread)
            myThread.IsBackground = true;
            myThread.Start(); //запускаем поток

            (new Thread((s) =>
            {
                while (true)
                {
                    if (!myThread.IsAlive)
                    {
                        Thread.Sleep(5000);
                        Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
                        break;
                    }
                }
            })).Start();

            //Console.WriteLine("Статус: " + projector.getStatus());
        }

        //Кнопка выключения
        private void buttonOff_Click(object sender, RoutedEventArgs e)
        {
            //Выключени проектора
            prgProjector.Visibility = Visibility.Visible;
            buttonOn.Visibility = Visibility.Visible;
            buttonFreeze.Visibility = Visibility.Hidden;
            buttonOff.Visibility = Visibility.Hidden;

            String statusPr = "";
            System.Windows.Controls.ProgressBar pr = prgProjector;

            Thread statusThread = new Thread((s) =>
            {
                projector.turnOff();
                Thread.Sleep(2000);
                statusPr = projector.getStatus();
                while (!statusPr.StartsWith("PWSTATUS=01"))
                {
                    Thread.Sleep(2000);
                    statusPr = projector.getStatus();
                    projector.turnOff();


                }
                Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
            });
            statusThread.IsBackground = true;
            statusThread.Start(); //запускаем поток
            //Process.Start("DisplaySwitch.exe", "/internal");

        }

        //Кнопка "настройки"
        private void buttonSettings_Click(object sender, RoutedEventArgs e)
        {
            settings_windows.Visibility = Visibility.Visible;
        }

        //Кнопка обновить проектор
        private void buttonReload_Click(object sender, RoutedEventArgs e)
        {
            prgProjector.Visibility = Visibility.Visible;
            reload();
        }
        
        //Создание/обновление проектора
        private void reload()
        {
            try
            {
                String statusPr = "";
                string prType="", prIP="";

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

                if (prType == "0")
                    projector = new EpsonProjector();
                else if (prType == "1")
                    projector = new BenqProjector();
                else
                    throw new Exception();

                try
                {
                    projector.IP = System.Net.IPAddress.Parse(prIP);
                    
                    //Console.WriteLine("Статус: " + projector.IP);

                    Thread myThread = new Thread((s) => { statusPr = projector.getStatus(); }) {IsBackground = true};
                    myThread.Start(); //запускаем поток
                    System.Windows.Controls.ProgressBar pr = prgProjector;


                    (new Thread((s) =>
                    {
                        Thread.Sleep(3000);

                        if (myThread.IsAlive)
                        {
                            Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = false));
                            Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Visible));
                            myThread.Abort();
                        }
                        else if (statusPr.StartsWith("PWSTATUS=01"))
                        {
                            Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => buttonOn.IsEnabled = true));
                            Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));

                        }
                        else if (statusPr.StartsWith("PWSTATUS=03"))
                        {
                            Dispatcher.BeginInvoke((Action)(() => prgProjector.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => buttonOn.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => buttonOff.Visibility = Visibility.Visible));
                            Dispatcher.BeginInvoke((Action)(() => buttonFreeze.Visibility = Visibility.Visible));
                            Dispatcher.BeginInvoke((Action)(() => buttonReload.Visibility = Visibility.Hidden));
                            Dispatcher.BeginInvoke((Action)(() => lblError.Visibility = Visibility.Hidden));
                        }
                    })).Start();

                }
                catch (IndexOutOfRangeException)
                {
                    System.Windows.MessageBox.Show("Ошибка чтения файла. Пустой файл");
                    lblError.Visibility = Visibility.Visible;
                    prgProjector.Visibility = Visibility.Hidden;
                    buttonOn.IsEnabled = false;
                    buttonReload.Visibility = Visibility.Visible;
                }
                catch (FormatException)
                {
                    System.Windows.MessageBox.Show("Ошибка чтения файла. Неправильно указан IP адрес!");
                    lblError.Visibility = Visibility.Visible;
                    prgProjector.Visibility = Visibility.Hidden;
                    buttonOn.IsEnabled = false;
                    buttonReload.Visibility = Visibility.Visible;

                }
            }
            catch (System.IO.FileNotFoundException)
            {
                System.IO.File.CreateText(FILE_SETTINGS);
                lblError.Visibility = Visibility.Visible;
                prgProjector.Visibility = Visibility.Hidden;
                buttonOn.IsEnabled = false;
                buttonReload.Visibility = Visibility.Visible;
                System.Windows.MessageBox.Show("Ошибка чтения файла.");


            }
        }

        private void buttonFreeze_Click(object sender, RoutedEventArgs e)
        {
            Thread myThread = new Thread(projector.freeze); //Создаем новый объект потока (Thread)
            myThread.IsBackground = true;
            myThread.Start(); //запускаем поток

        }
    }

}
