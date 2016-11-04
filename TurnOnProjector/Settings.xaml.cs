using System;
using System.Threading;
using System.Windows;


namespace TurnOnProjector
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        System.Net.IPAddress prIPAddress = System.Net.IPAddress.Parse("0.0.0.0");
        public Settings()
        {
            InitializeComponent();
        }

        private void testProjector_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                prPing.Visibility = Visibility.Visible;
                prIPAddress = System.Net.IPAddress.Parse((tbIP1.Text + "." + tbIP2.Text + "." + tbIP3.Text + "." + tbIP4.Text));
                string name = "";
                Thread myThread = new Thread((s) =>
                {
                    name = Projector.getName(prIPAddress);

                });
                myThread.IsBackground = true;
                myThread.Start();

                System.Windows.Controls.ProgressBar pr = prPing;
                
                (new Thread((s) =>
                {
                    Thread.Sleep(4000);
                    if (myThread.IsAlive)
                    {
                        Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
                        MessageBox.Show("Неправильно указан IP адрес!");
                        myThread.Abort();
                    }
                    else if (name != "")
                    {
                        Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
                        Dispatcher.BeginInvoke((Action)(() => btnSave.IsEnabled = true));
                        Dispatcher.BeginInvoke((Action)(() => lblStatus.Content = "Подключено к " + name));
                        MainWindow.projector = new Projector(prIPAddress);
                    }
                    else
                    {
                        Dispatcher.BeginInvoke((Action)(() => pr.Visibility = Visibility.Hidden));
                        MessageBox.Show("Неправильно указан IP адрес!");
                    }
                })).Start();
            }
            catch (FormatException)
            {
                MessageBox.Show("Неправильно указан IP адрес!");
            }

        }

        private void buttonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(MainWindow.FILE_SETTINGS))
            {
                sw.WriteLine(prIPAddress);
            }            
        }

        private void tbIP_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (btnSave != null)
            {
                btnSave.IsEnabled = false;
            }
        }
    }
}
